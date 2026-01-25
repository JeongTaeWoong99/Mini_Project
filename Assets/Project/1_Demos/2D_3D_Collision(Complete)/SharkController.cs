using System.Collections;
using UnityEngine;

namespace MiniGame2D3DCollision
{
    /// <summary>
    /// 상어 컨트롤러
    /// - HP 시스템 (기본 30)
    /// - 총알 충돌 시 데미지 및 빨간색 깜빡임
    /// - HP 0이 되면 파괴
    /// </summary>
    public class SharkController : MonoBehaviour
    {
        [Header("HP Settings")]
        [SerializeField] private int   maxHP            = 30;   // 최대 HP
        [SerializeField] private bool  showHPDebug      = false; // HP 디버그 로그

        [Header("Hit Flash Settings")]
        [SerializeField] private float hitFlashDuration = 0.1f;         // 피격 시 깜빡임 지속 시간 (초)
        [SerializeField] private Color hitFlashColor    = Color.red; // 피격 시 색상

        [Header("Renderer Settings")]
        [SerializeField] private bool         autoFindRenderer = true;  // 자동으로 Renderer 찾기
        [SerializeField] private MeshRenderer targetRenderer   = null;  // 대상 Renderer (null이면 자동 검색)
        [SerializeField] private bool         showMaterialDebug = false; // 메터리얼 디버그 로그

        // Private variables
        private int       currentHP;
        private Material  materialInstance;   // Material 인스턴스 (개별 할당)
        private Color     originalColor;      // 원래 색상
        private bool      isFlashing = false; // 깜빡임 중 여부
        private Renderer  cachedRenderer;     // 캐싱된 Renderer

        private void Start()
        {
            // HP 초기화
            currentHP = maxHP;

            // Renderer 자동 찾기
            if (autoFindRenderer && targetRenderer == null)
            {
                targetRenderer = GetComponent<MeshRenderer>();
                if (targetRenderer == null)
                {
                    targetRenderer = GetComponentInChildren<MeshRenderer>();
                }
            }

            // Renderer 캐싱
            cachedRenderer = targetRenderer;

            // Material 인스턴스 생성 (개별 할당 - sharedMaterial을 복사)
            if (cachedRenderer != null)
            {
                // IMPORTANT : sharedMaterial을 직접 사용하지 말고, material 프로퍼티로 인스턴스 생성
                // Unity는 .material 접근 시 자동으로 인스턴스를 생성함
                materialInstance = new Material(cachedRenderer.sharedMaterial);
                cachedRenderer.material = materialInstance;

                if (showMaterialDebug)
                {
                    Debug.Log($"[SharkController] [{gameObject.name}] Material 인스턴스 생성 완료 : {materialInstance.name}");
                    Debug.Log($"[SharkController] [{gameObject.name}] Shader : {materialInstance.shader.name}");
                }

                // 원래 색상 저장 (URP Lit 메터리얼은 _BaseColor 사용)
                if (materialInstance.HasProperty("_BaseColor"))
                {
                    originalColor = materialInstance.GetColor("_BaseColor");

                    if (showMaterialDebug)
                    {
                        Debug.Log($"[SharkController] [{gameObject.name}] _BaseColor 발견! 원래 색상 : {originalColor}");
                    }
                }
                else if (materialInstance.HasProperty("_Color"))
                {
                    originalColor = materialInstance.GetColor("_Color");

                    if (showMaterialDebug)
                    {
                        Debug.Log($"[SharkController] [{gameObject.name}] _Color 발견! 원래 색상 : {originalColor}");
                    }
                }
                else if (materialInstance.HasProperty("_MainColor"))
                {
                    originalColor = materialInstance.GetColor("_MainColor");

                    if (showMaterialDebug)
                    {
                        Debug.Log($"[SharkController] [{gameObject.name}] _MainColor 발견! 원래 색상 : {originalColor}");
                    }
                }
                else
                {
                    originalColor = Color.white;

                    if (showMaterialDebug)
                    {
                        Debug.LogWarning($"[SharkController] [{gameObject.name}] Color 프로퍼티를 찾을 수 없습니다! 사용 가능한 프로퍼티 :");

                        // 모든 프로퍼티 출력
                        for (int i = 0; i < materialInstance.shader.GetPropertyCount(); i++)
                        {
                            string propName = materialInstance.shader.GetPropertyName(i);
                            Debug.Log($"  - {propName}");
                        }
                    }
                }
            }
            else
            {
                if (showMaterialDebug)
                {
                    Debug.LogWarning($"[SharkController] [{gameObject.name}] Renderer를 찾을 수 없습니다!");
                }
            }

            if (showHPDebug)
            {
                Debug.Log($"SharkController [{gameObject.name}] : HP 초기화 완료 (HP : {currentHP}/{maxHP})");
            }
        }

        /// <summary>
        /// 데미지를 받는 메서드
        /// </summary>
        public void TakeDamage(int damage)
        {
            // 이미 죽은 경우 무시
            if (currentHP <= 0) return;

            // HP 감소
            currentHP -= damage;
            currentHP =  Mathf.Max(0, currentHP); // 음수 방지

            if (showHPDebug)
            {
                Debug.Log($"SharkController [{gameObject.name}] : 데미지 {damage} 받음! (HP : {currentHP}/{maxHP})");
            }

            // 피격 깜빡임
            if (!isFlashing && materialInstance != null)
            {
                StartCoroutine(HitFlash());
            }

            // HP가 0 이하면 파괴
            if (currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 상어 파괴 처리
        /// </summary>
        private void Die()
        {
            if (showHPDebug)
            {
                Debug.Log($"SharkController [{gameObject.name}] : HP 0! 파괴됨");
            }

            // SharkManager에서 제거
            if (SharkManager.Instance != null)
            {
                SharkManager.Instance.RemoveShark(transform);
            }

            // GameObject 파괴
            Destroy(gameObject);
        }

        /// <summary>
        /// 피격 시 색상 변경 코루틴
        /// </summary>
        private IEnumerator HitFlash()
        {
            if (materialInstance == null)
            {
                if (showMaterialDebug)
                {
                    Debug.LogWarning($"[SharkController] [{gameObject.name}] HitFlash : materialInstance가 null입니다!");
                }
                yield break;
            }

            isFlashing = true;

            // 빨간색으로 변경
            SetMaterialColor(hitFlashColor);

            if (showMaterialDebug)
            {
                Debug.Log($"[SharkController] [{gameObject.name}] HitFlash : 빨간색으로 변경 → {hitFlashColor}");
            }

            // 지정된 시간 대기
            yield return new WaitForSeconds(hitFlashDuration);

            // 원래 색상으로 복구
            SetMaterialColor(originalColor);

            if (showMaterialDebug)
            {
                Debug.Log($"[SharkController] [{gameObject.name}] HitFlash : 원래 색상으로 복구 → {originalColor}");
            }

            isFlashing = false;
        }

        /// <summary>
        /// Material Color 설정 헬퍼 메서드 (URP Lit 메터리얼은 _BaseColor 사용)
        /// </summary>
        private void SetMaterialColor(Color color)
        {
            if (materialInstance == null)
            {
                if (showMaterialDebug)
                {
                    Debug.LogWarning($"[SharkController] [{gameObject.name}] SetMaterialColor : materialInstance가 null입니다!");
                }
                return;
            }

            bool colorSet = false;

            // URP Lit 메터리얼은 _BaseColor 우선 시도
            if (materialInstance.HasProperty("_BaseColor"))
            {
                materialInstance.SetColor("_BaseColor", color);
                colorSet = true;

                if (showMaterialDebug)
                {
                    Debug.Log($"[SharkController] [{gameObject.name}] SetMaterialColor : _BaseColor 설정 → {color}");
                }
            }
            else if (materialInstance.HasProperty("_Color"))
            {
                materialInstance.SetColor("_Color", color);
                colorSet = true;

                if (showMaterialDebug)
                {
                    Debug.Log($"[SharkController] [{gameObject.name}] SetMaterialColor : _Color 설정 → {color}");
                }
            }
            else if (materialInstance.HasProperty("_MainColor"))
            {
                materialInstance.SetColor("_MainColor", color);
                colorSet = true;

                if (showMaterialDebug)
                {
                    Debug.Log($"[SharkController] [{gameObject.name}] SetMaterialColor : _MainColor 설정 → {color}");
                }
            }

            if (!colorSet && showMaterialDebug)
            {
                Debug.LogWarning($"[SharkController] [{gameObject.name}] SetMaterialColor : Color 프로퍼티를 찾을 수 없습니다!");
            }
        }

        /// <summary>
        /// 현재 HP 반환
        /// </summary>
        public int GetCurrentHP()
        {
            return currentHP;
        }

        /// <summary>
        /// 최대 HP 반환
        /// </summary>
        public int GetMaxHP()
        {
            return maxHP;
        }

        // Material 인스턴스 정리
        private void OnDestroy()
        {
            if (materialInstance != null)
            {
                Destroy(materialInstance);
            }
        }
    }
}
