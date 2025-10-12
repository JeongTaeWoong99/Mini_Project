using System.Collections;
using UnityEngine;

namespace MiniGame2D3DCollision
{
    /// <summary>
    /// 플레이어 추적 스크립트
    /// - RocketController.instance를 추적
    /// - RocketController와 동일한 물리 시스템 사용
    /// - 도착 판정 시 2초 대기 후 다시 추적
    /// </summary>
    public class ChasePlayer : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float thrustPower     = 5f;    // 추진력
        [SerializeField] private float maxSpeed        = 10f;   // 최대 속도
        [SerializeField] private float drag            = 0.95f; // 우주에서의 감속
        [SerializeField] private float arrivalDistance = 1f;    // 도착 판정 거리

        [Header("Arrival Settings")]
        [SerializeField] private float arrivalWaitTime = 2f;    // 도착 후 대기 시간 (초)

        [Header("Rotation Settings")]
        [SerializeField] private bool    smoothRotation  = true;                    // 부드러운 회전
        [SerializeField] private float   rotationDamping = 5f;                      // 회전 감쇠
        [SerializeField] private float   inputThreshold  = 0.1f;                    // 입력 임계값
        [SerializeField] private Vector3 rotationOffset  = new Vector3(0, 90, 0);  // 회전 오프셋

        [Header("Debug")]
        [SerializeField] private bool showDebug    = false; // 디버그 정보 표시
        [SerializeField] private bool showDebugLog = false; // 디버그 로그 표시

        // Private variables
        private Vector3 movementInput;     // 이동 입력 (플레이어 방향으로 자동 계산)
        private Vector3 velocity;          // 현재 속도
        private float   currentThrust;     // 현재 추진력
        private bool    isMoving  = false; // 이동 여부
        private bool    isWaiting = false; // 도착 후 대기 중 여부

        void Start()
        {
            // Transform 기반 이동 - Rigidbody 사용 안함
            velocity = Vector3.zero; // 초기 속도 설정
            isMoving = true;         // 이동 시작

            if (showDebugLog)
            {
                Debug.Log($"ChasePlayer [{gameObject.name}] : 초기화 완료");
            }
        }

        void Update()
        {
            // 플레이어가 없으면 정지
            if (RocketController.instance == null)
            {
                if (showDebugLog && isMoving)
                {
                    Debug.LogWarning($"ChasePlayer [{gameObject.name}] : RocketController.instance가 null입니다!");
                    isMoving = false;
                }
                return;
            }

            if (!isMoving)
            {
                // 플레이어가 생성되면 다시 이동 시작
                isMoving = true;
                if (showDebugLog)
                {
                    Debug.Log($"ChasePlayer [{gameObject.name}] : 플레이어 발견! 추적 시작");
                }
            }

            HandleInput();    // 플레이어 방향 계산
            HandleMovement(); // Transform 기반 이동
            HandleRotation(); // 회전
        }

        private void HandleInput()
        {
            // 플레이어가 없으면 입력 없음
            if (RocketController.instance == null)
            {
                movementInput = Vector3.zero;
                currentThrust = 0f;
                return;
            }

            // 대기 중이면 입력 없음
            if (isWaiting)
            {
                movementInput = Vector3.zero;
                currentThrust = 0f;
                return;
            }

            // 플레이어 방향 계산
            Transform target           = RocketController.instance.transform;
            float     distanceToTarget = Vector3.Distance(transform.position, target.position);

            // 도착 판정 : arrivalDistance 안에 들어오면 2초 대기
            if (distanceToTarget <= arrivalDistance)
            {
                if (showDebugLog)
                {
                    Debug.Log($"ChasePlayer [{gameObject.name}] : 플레이어 도착! {arrivalWaitTime}초 대기 시작");
                }

                StartCoroutine(WaitAtArrival());
                return;
            }

            Vector3 direction = (target.position - transform.position).normalized;

            // 플레이어 방향을 movementInput으로 설정
            movementInput = direction;

            // 추진력 계산 (입력이 있을 때만)
            currentThrust = movementInput.magnitude > inputThreshold ? thrustPower : 0f;
        }

        /// <summary>
        /// 도착 후 대기 코루틴
        /// </summary>
        private IEnumerator WaitAtArrival()
        {
            isWaiting     = true;
            movementInput = Vector3.zero;
            currentThrust = 0f;

            // 대기 시간
            yield return new WaitForSeconds(arrivalWaitTime);

            isWaiting = false;

            if (showDebugLog)
            {
                Debug.Log($"ChasePlayer [{gameObject.name}] : 대기 종료! 다시 추적 시작");
            }
        }

        private void HandleMovement()
        {
            if (movementInput.magnitude > inputThreshold)
            {
                // 입력 방향으로 추진력 적용
                Vector3 thrustDirection = movementInput.normalized;
                velocity               += thrustDirection * (currentThrust * Time.deltaTime);

                // 최대 속도 제한
                if (velocity.magnitude > maxSpeed)
                {
                    velocity = velocity.normalized * maxSpeed;
                }
            }

            // 우주에서의 감속 (매우 적은 저항)
            // drag가 0.95면, 1 프레임마다 95%로 속도가 감소함
            velocity *= drag;

            // Transform으로 직접 이동 (Rigidbody 없음)
            transform.position += velocity * Time.deltaTime;

            if (showDebugLog)
            {
                Debug.Log($"ChasePlayer [{gameObject.name}] : 속도 = {velocity.magnitude:F2}, 위치 = {transform.position}");
            }
        }

        private void HandleRotation()
        {
            if (movementInput.magnitude > inputThreshold)
            {
                if (smoothRotation)
                {
                    // 부드러운 회전
                    Quaternion targetRotation = Quaternion.LookRotation(movementInput.normalized);

                    // 회전 오프셋 적용
                    targetRotation *= Quaternion.Euler(rotationOffset);

                    // 부드러운 회전
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                        rotationDamping * Time.deltaTime);
                }
                else
                {
                    // 즉시 회전
                    Quaternion targetRotation = Quaternion.LookRotation(movementInput.normalized);
                    targetRotation           *= Quaternion.Euler(rotationOffset);
                    transform.rotation        = targetRotation;
                }
            }
        }

        // 디버그용 기즈모 그리기
        private void OnDrawGizmos()
        {
            if (!showDebug) return;

            if (Application.isPlaying)
            {
                // 속도 벡터 표시
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, transform.position + velocity);

                // 입력 방향 표시
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + movementInput * 2f);

                // 플레이어로 향하는 선 표시
                if (RocketController.instance != null)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(transform.position, RocketController.instance.transform.position);
                }

                // 도착 판정 거리 표시
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, arrivalDistance);
            }
        }
    }
}
