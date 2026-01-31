using UnityEngine;

namespace DesignPatterns.StatePattern
{
    // 간단한 FPS 컨트롤러 (FPS Starter의 로직)
    [RequireComponent(typeof(PlayerInput), typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] 
        private PlayerInput              playerInput;
        private SimplePlayerStateMachine playerStateMachine;

        [Header("Movement")]
        [Tooltip("수평 이동 속도")]
        [SerializeField] private float moveSpeed     = 5f;
        [Tooltip("이동 속도 변화율")]
        [SerializeField] private float acceleration  = 10f;
        [Tooltip("최대 점프 높이")]
        [SerializeField] private float jumpHeight    = 1.25f;

        [Tooltip("플레이어 커스텀 중력")]
        [SerializeField] private float gravity       = -15f;
        [Tooltip("점프 사이의 대기 시간")]
        [SerializeField] private float jumpTimeout   = 0.1f;

        [SerializeField] private bool      isGrounded     = true;
        [SerializeField] private float     groundedRadius = 0.5f;
        [SerializeField] private float     groundedOffset = 0.15f;
        [SerializeField] private LayerMask groundLayers;

        public CharacterController       CharController       => charController;
        public bool                      IsGrounded           => isGrounded;
        public SimplePlayerStateMachine  PlayerStateMachine   => playerStateMachine;

        private CharacterController charController;
        private float               targetSpeed;
        private float               verticalVelocity;
        private float               jumpCooldown;

        private void Awake()
        {
            playerInput    = GetComponent<PlayerInput>();
            charController = GetComponent<CharacterController>();

            // 상태 머신 초기화
            playerStateMachine = new SimplePlayerStateMachine(this);
        }

        private void Start()
        {
            playerStateMachine.Initialize(playerStateMachine.idleState);
        }

        private void Update()
        {
            // 현재 상태 업데이트
            playerStateMachine.Execute();
        }

        private void LateUpdate()
        {
            CalculateVertical();
            Move();
        }

        private void Move()
        {
            Vector3 inputVector = playerInput.InputVector;

            // 이동 입력이 없으면 목표 속도를 0으로 설정
            if (inputVector == Vector3.zero)
            {
                targetSpeed = 0;
            }

            // 목표 속도에 도달하지 않은 경우 (허용 범위 밖), 목표 속도로 보간
            float currentHorizontalSpeed = new Vector3(charController.velocity.x, 0.0f, charController.velocity.z).magnitude;
            float tolerance              = 0.1f;

            // 목표 속도에 도달하지 않은 경우 (허용 범위 밖), 부드러운 전환을 위해 목표 속도로 보간
            if (currentHorizontalSpeed < targetSpeed - tolerance || currentHorizontalSpeed > targetSpeed + tolerance)
            {
                targetSpeed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed, Time.deltaTime * acceleration);
                targetSpeed = Mathf.Round(targetSpeed * 1000f) / 1000f;
            }
            else
            {
                targetSpeed = moveSpeed;
            }
            
            // 플레이어 이동
            charController.Move((inputVector.normalized * targetSpeed * Time.deltaTime) + new Vector3(0f, verticalVelocity, 0f) * Time.deltaTime);
        }

        private void CalculateVertical()
        {
            if (isGrounded)
            {
                if (verticalVelocity < 0f)
                {
                    verticalVelocity = -2f;
                }

                if (playerInput.IsJumping && jumpCooldown <= 0f)
                {
                    verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
                }

                if (jumpCooldown >= 0f)
                {
                    jumpCooldown -= Time.deltaTime;
                }
            }
            else
            {
                jumpCooldown         = jumpTimeout;
                playerInput.IsJumping = false;
            }

            verticalVelocity += gravity * Time.deltaTime;

            // 바닥 접촉 확인
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z);
            isGrounded             = Physics.CheckSphere(spherePosition, 0.5f, groundLayers, QueryTriggerInteraction.Ignore);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed   = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (isGrounded) Gizmos.color = transparentGreen;
            else Gizmos.color            = transparentRed;

            // 선택 시 접지 콜라이더의 위치와 반지름에 맞는 기즈모를 그림
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y + groundedOffset, transform.position.z), groundedRadius);
        }
    }
}
