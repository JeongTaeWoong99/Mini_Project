using UnityEngine;

namespace MiniGame2D3DCollision
{
    /// <summary>
    /// 총알 스크립트
    /// - 일정 속도로 전진
    /// - 3초 후 자동 파괴
    /// - 상어와 충돌 시 파괴
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Settings")]
        [SerializeField] private float speed           = 15f;  // 총알 속도
        [SerializeField] private float lifetime        = 3f;   // 생존 시간 (초)

        [Header("Collision Settings")]
        [SerializeField] private string sharkTag       = "Shark"; // 상어 태그
        [SerializeField] private int    damage         = 1;       // 데미지
        [SerializeField] private bool   showDebugLog   = false;   // 디버그 로그 표시

        // Private variables
        private Rigidbody2D rb2D;
        private float       destroyTimer = 0f;

        private void Update()
        {
            transform.Translate(Vector3.up * (speed * Time.deltaTime), Space.Self);
            
            // 생존 시간 체크
            destroyTimer += Time.deltaTime;
            if (destroyTimer >= lifetime)
            {
                if (showDebugLog)
                {
                    Debug.Log($"Bullet : {lifetime}초 경과로 파괴됨");
                }
                Destroy(gameObject);
            }
        }

        // 2D 충돌 이벤트 (Collision)
        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleCollision(collision.gameObject);
        }

        // 2D 충돌 이벤트 (Trigger)
        private void OnTriggerEnter2D(Collider2D collision)
        {
            HandleCollision(collision.gameObject);
        }

        // 충돌 처리
        private void HandleCollision(GameObject hitObject)
        {
            // 상어와 충돌했는지 확인
            if (hitObject.CompareTag(sharkTag))
            {
                // 상어에게 데미지 전달
                SharkController sharkController = hitObject.GetComponent<SharkController>();
                if (sharkController != null)
                {
                    sharkController.TakeDamage(damage);

                    if (showDebugLog)
                    {
                        Debug.Log($"Bullet : 상어 '{hitObject.name}'에게 데미지 {damage} 전달! 총알 파괴");
                    }
                }
                else
                {
                    if (showDebugLog)
                    {
                        Debug.LogWarning($"Bullet : 상어 '{hitObject.name}'에 SharkController가 없습니다!");
                    }
                }

                // 총알 파괴
                Destroy(gameObject);
            }
        }
    }
}