using System;
using System.Collections;
using UnityEngine;

namespace MiniGame2D3DCollision
{
    public class RocketController : MonoBehaviour
    {
        public static RocketController instance;
        
        [Header("Movement Settings")] [SerializeField]
        private float thrustPower = 5f; // 추진력
    
        [SerializeField] private float maxSpeed = 10f;   // 최대 속도
        [SerializeField] private float drag     = 0.95f; // 우주에서의 감속 (매우 적음)

        [Header("Rotation Settings")] [SerializeField]
        private bool smoothRotation = true; // 부드러운 회전

        [SerializeField] private float rotationDamping  = 5f;    // 회전 감쇠
        [SerializeField] private float hitFlashDuration = 0.2f; // 충돌 시 깜빡임 지속 시간 (초)

        [Header("HP Settings")]
        [SerializeField] private int  maxHP       = 3;     // 최대 HP
        [SerializeField] private bool showHPDebug = false; // HP 디버그 로그

        [Header("Body Settings")] [SerializeField]
        private Transform bodyTransform = null; // Body 자식 오브젝트 Transform

        [SerializeField] private bool    autoFindBody        = true;                    // 시작 시 자동으로 Body 찾기
        [SerializeField] private bool    bodyLookAtShark     = true;                    // Body가 가장 가까운 상어를 바라봄
        [SerializeField] private float   bodyRotationDamping = 5f;                      // Body 회전 감쇠
        [SerializeField] private Vector3 bodyRotationOffset  = new Vector3(0, 0, -90f); // Body 회전 오프셋 (Z축)

        [Header("Bullet Settings")] [SerializeField]
        private GameObject bulletPrefab      = null; // 총알 프리팹

        [SerializeField] private Transform bulletSpawnPoint      = null;  // 총알 발사 위치 (null이면 Body 사용)
        [SerializeField] private bool      autoFireEnabled       = true;  // 자동 발사 활성화
        [SerializeField] private float     fireRate              = 0.5f;  // 발사 간격 (초)
        [SerializeField] private bool      checkScreenVisibility = true;  // 화면에 보이는 상어만 공격
        [SerializeField] private bool      showFireDebug         = false; // 발사 디버그 로그

        // Private variables
        private Rigidbody2D    rb;
        private Vector2        movementInput;
        private Vector2        velocity;
        private float          currentThrust;     // 현재 추진력
        private SpriteRenderer spriteRenderer;    // 메인 SpriteRenderer (색상 변경용)
        private Color          originalColor;     // 원래 색상
        private bool           isHit = false;     // 충돌 중 여부

        // Body 관련 변수
        private SpriteRenderer bodyRenderer;      // Body의 SpriteRenderer
        private Color          bodyOriginalColor; // Body의 원래 색상

        // Bullet 관련 변수
        private float nextFireTime = 0f; // 다음 발사 가능 시간

        // HP 관련 변수
        private int  currentHP;      // 현재 HP
        private bool isDead = false; // 사망 여부

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            // HP 초기화
            currentHP = maxHP;

            if (showHPDebug)
            {
                Debug.Log($"RocketController : HP 초기화 완료 (HP : {currentHP}/{maxHP})");
            }

            // 없으면 넣어주기
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }

            // 우주 환경 설정
            rb.gravityScale   = 0f; // 중력 없음
            rb.linearDamping  = 0f; // Rigidbody의 드래그 사용 안함 (직접 제어)
            rb.angularDamping = 0f; // 각속도 드래그 사용 안함

            // 메인 SpriteRenderer 가져오기 (이 오브젝트의 스프라이트 렌더러)
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // 원래 색상 저장
                originalColor = spriteRenderer.color;
            }

            // Body 자식 오브젝트 찾기
            if (autoFindBody && bodyTransform == null)
            {
                Transform foundBody = transform.Find("Body");
                if (foundBody != null)
                {
                    bodyTransform = foundBody;
                }
            }

            // Body의 SpriteRenderer 가져오기
            if (bodyTransform != null)
            {
                bodyRenderer = bodyTransform.GetComponent<SpriteRenderer>();
                if (bodyRenderer != null)
                {
                    // Body 원래 색상 저장
                    bodyOriginalColor = bodyRenderer.color;
                }
            }
        }

        private void Update()
        {
            HandleInput();        // 키 입력
            HandleRotation();     // 회전
            HandleBodyRotation(); // Body 회전 (가장 가까운 상어를 향함)
            HandleAutoFire();     // 자동 발사
        }

        private void FixedUpdate()
        {
            HandleMovement(); // 물리 이동
        }

        private void HandleInput()
        {
            // 방향키 입력 받기
            movementInput.x = Input.GetAxis("Horizontal");
            movementInput.y = Input.GetAxis("Vertical");

            // 추진력 계산 (입력이 있을 때만)
            currentThrust   = movementInput.magnitude > 0.1f ? thrustPower : 0f;
        }

        private void HandleMovement()
        {
            if (movementInput.magnitude > 0.1f)
            {
                // 입력 방향으로 추진력 적용
                Vector2 thrustDirection = movementInput.normalized;
                velocity += thrustDirection * (currentThrust * Time.fixedDeltaTime);

                // 최대 속도 제한
                if (velocity.magnitude > maxSpeed)
                {
                    velocity = velocity.normalized * maxSpeed;
                }
            }

            // 우주에서의 감속 (매우 적은 저항)
            // drag가 0.95면, 1 fixed 프레임 마다, 95%로 속도가 감소함...
            velocity *= drag;

            // Rigidbody2D에 속도 적용
            rb.linearVelocity = velocity;
        }

        private void HandleRotation()
        {
            if (movementInput.magnitude > 0.1f)
            {
                if (smoothRotation)
                {
                    // 부드러운 회전 - 이동 방향을 향해 천천히 회전
                    float targetAngle  = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg - 90f;
                    float currentAngle = transform.eulerAngles.z;

                    // 각도 차이 계산 (-180 ~ 180 범위로 정규화)
                    float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);

                    // 부드럽게 회전
                    float rotationAmount = rotationDamping * Time.deltaTime * angleDifference;
                    transform.Rotate(0, 0, rotationAmount);
                }
                else
                {
                    // 즉시 회전 - 이동 방향을 바로 바라봄
                    float angle            = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg - 90f;
                    transform.rotation     = Quaternion.AngleAxis(angle, Vector3.forward);
                }
            }
        }

        private void HandleBodyRotation()
        {
            // Body가 없거나, 기능이 꺼져있으면 건너뛰기
            if (bodyTransform == null || !bodyLookAtShark) return;

            // SharkManager에서 가장 가까운 활성 상어 가져오기
            if (MiniGame2D3DCollision.SharkManager.Instance == null) return;

            Transform closestShark = MiniGame2D3DCollision.SharkManager.Instance.GetClosestActiveShark(transform.position);

            // 가장 가까운 상어가 없으면 건너뛰기
            if (closestShark == null) return;

            // 상어를 향하는 방향 계산
            Vector3 direction     = closestShark.position - bodyTransform.position;
            float   targetAngle   = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 오프셋 적용
            targetAngle += bodyRotationOffset.z;

            // 부드러운 회전
            float currentAngle    = bodyTransform.eulerAngles.z;
            float angleDifference = Mathf.DeltaAngle(currentAngle, targetAngle);
            float rotationAmount  = bodyRotationDamping * Time.deltaTime * angleDifference;

            bodyTransform.Rotate(0, 0, rotationAmount);
        }

        private void HandleAutoFire()
        {
            // 자동 발사가 꺼져있으면 건너뛰기
            if (!autoFireEnabled) return;

            // 총알 프리팹이 없으면 건너뛰기
            if (bulletPrefab == null)
            {
                if (showFireDebug)
                {
                    Debug.LogWarning("RocketController : 총알 프리팹이 설정되지 않았습니다!");
                }
                return;
            }

            // SharkManager에서 가장 가까운 활성 상어 가져오기
            if (MiniGame2D3DCollision.SharkManager.Instance == null) return;

            Transform closestShark = MiniGame2D3DCollision.SharkManager.Instance.GetClosestActiveShark(transform.position);

            // 가장 가까운 상어가 없으면 건너뛰기
            if (closestShark == null) return;

            // 화면 가시성 체크 (옵션)
            if (checkScreenVisibility && !IsSharkOnScreen(closestShark))
            {
                if (showFireDebug)
                {
                    Debug.Log($"RocketController : 타겟 상어 [{closestShark.name}]가 화면 밖에 있어서 발사 취소");
                }
                return;
            }

            // 발사 쿨타임 체크
            if (Time.time < nextFireTime) return;

            // 총알 발사
            FireBullet();

            // 다음 발사 시간 설정
            nextFireTime = Time.time + fireRate;
        }

        /// <summary>
        /// 상어가 화면에 보이는지 체크 (Z축 범위 + 화면 안)
        /// </summary>
        private bool IsSharkOnScreen(Transform shark)
        {
            if (shark == null) return false;

            // 메인 카메라 가져오기
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return false;

            // 월드 좌표 → 스크린 좌표
            Vector3 screenPos = mainCamera.WorldToScreenPoint(shark.position);

            // Z축 체크 : 카메라 앞에 있어야 함 (screenPos.z > 0)
            if (screenPos.z < 0) return false;

            // 화면 안 체크
            if (screenPos.x < 0 || screenPos.x > Screen.width) return false;
            if (screenPos.y < 0 || screenPos.y > Screen.height) return false;

            return true;
        }

        private void FireBullet()
        {
            // 발사 위치 결정 (bulletSpawnPoint가 있으면 사용, 없으면 Body 사용)
            Transform spawnTransform = bulletSpawnPoint != null ? bulletSpawnPoint : bodyTransform;

            if (spawnTransform == null)
            {
                if (showFireDebug)
                {
                    Debug.LogWarning("RocketController : 총알 발사 위치를 찾을 수 없습니다!");
                }
                return;
            }

            // 총알 생성
            GameObject bullet = Instantiate(bulletPrefab, spawnTransform.position, spawnTransform.rotation);

            if (showFireDebug)
            {
                Debug.Log($"RocketController : 총알 발사! 위치 : {spawnTransform.position}, 회전 : {spawnTransform.rotation.eulerAngles}");
            }
        }

        // 2D 충돌 이벤트 처리
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 이미 죽었으면 무시
            if (isDead) return;

            // 상어와 충돌 시 데미지
            if (collision.gameObject.CompareTag("Shark"))
            {
                TakeDamage(1);
            }

            // 충돌 시 색상 변경
            if (!isHit)
            {
                StartCoroutine(HitFlash());
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // 이미 죽었으면 무시
            if (isDead) return;

            // 상어와 충돌 시 데미지
            if (collision.gameObject.CompareTag("Shark"))
            {
                TakeDamage(1);
            }

            // 충돌 시 색상 변경
            if (!isHit)
            {
                StartCoroutine(HitFlash());
            }
        }

        // 충돌 시 색상 변경 코루틴
        private IEnumerator HitFlash()
        {
            isHit = true;

            // 메인 스프라이트를 빨간색으로 변경
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
            }

            // Body도 빨간색으로 변경
            if (bodyRenderer != null)
            {
                bodyRenderer.color = Color.red;
            }

            // 지정된 시간 대기
            yield return new WaitForSeconds(hitFlashDuration);

            // 메인 스프라이트를 원래 색상으로 복구
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }

            // Body도 원래 색상으로 복구
            if (bodyRenderer != null)
            {
                bodyRenderer.color = bodyOriginalColor;
            }

            isHit = false;
        }

        /// <summary>
        /// 데미지를 받는 메서드
        /// </summary>
        public void TakeDamage(int damage)
        {
            // 이미 죽었으면 무시
            if (isDead) return;

            // HP 감소
            currentHP -= damage;
            currentHP =  Mathf.Max(0, currentHP); // 음수 방지

            if (showHPDebug)
            {
                Debug.Log($"RocketController : 데미지 {damage} 받음! (HP : {currentHP}/{maxHP})");
            }

            // HP가 0 이하면 게임 종료
            if (currentHP <= 0)
            {
                Die();
            }
        }

        /// <summary>
        /// 플레이어 사망 처리
        /// </summary>
        private void Die()
        {
            isDead = true;

            if (showHPDebug)
            {
                Debug.Log("RocketController : HP 0! 게임 종료");
            }

            // 게임 종료 처리
            GameOver();
        }

        /// <summary>
        /// 게임 종료 처리
        /// </summary>
        private void GameOver()
        {
            Debug.Log("═══════════════════════════════════════");
            Debug.Log("           GAME OVER!                  ");
            Debug.Log("═══════════════════════════════════════");

            // UIManager에 게임 오버 텍스트 표시
            if (UIManager.instance != null)
            {
                UIManager.instance.ShowGameOverText();
            }

            // 시간 정지
            Time.timeScale = 0f;
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


        // 디버그용 기즈모 그리기
        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                // 속도 벡터 표시
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)velocity);

                // 입력 방향 표시
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + (Vector3)movementInput * 2f);
            }
        }
    }
}