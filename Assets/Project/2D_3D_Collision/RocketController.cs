using UnityEngine;

public class RocketController : MonoBehaviour
{
    [Header("Movement Settings")]
    private Rigidbody2D rb;
    private Vector2     movementInput;
    private Vector2     velocity;
    private float       currentThrust;  // 현재 추진력
    
    [Header("Movement Settings")]
    [SerializeField] private float thrustPower = 5f;     // 추진력
    [SerializeField] private float maxSpeed    = 10f;    // 최대 속도
    [SerializeField] private float drag        = 0.95f;  // 우주에서의 감속 (매우 적음)
    
    [Header("Rotation Settings")]
    // [SerializeField] private float rotationSpeed   = 200f;  // 회전 속도
    [SerializeField] private bool  smoothRotation  = true;  // 부드러운 회전
    [SerializeField] private float rotationDamping = 5f;    // 회전 감쇠
    
    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem thrusterEffect; // 추진기 이펙트 (선택사항)
    [SerializeField] private AudioSource    thrusterSound;  // 추진기 사운드 (선택사항)
    
    private void Start()
    {
        // 없으면 넣어주기
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        // 우주 환경 설정
        rb.gravityScale   = 0f;  // 중력 없음
        rb.linearDamping  = 0f;  // Rigidbody의 드래그 사용 안함 (직접 제어)
        rb.angularDamping = 0f;  // 각속도 드래그 사용 안함
    }
    
    private void Update()
    {
        HandleInput();          // 키 입력
        HandleRotation();       // 회전
        // HandleVisualEffects();  // 이펙트
    }
    
    private void FixedUpdate()
    {
        HandleMovement();   // 물리 이동
    }
    
    private void HandleInput()
    {
        // 방향키 입력 받기
        movementInput.x = Input.GetAxis("Horizontal");
        movementInput.y = Input.GetAxis("Vertical");
        
        // 추진력 계산 (입력이 있을 때만)
        currentThrust = movementInput.magnitude > 0.1f ? thrustPower : 0f;
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
                float angle = Mathf.Atan2(movementInput.y, movementInput.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
    }
    
    // private void HandleVisualEffects()
    // {
    //     // 추진기 이펙트 제어
    //     if (thrusterEffect != null)
    //     {
    //         if (currentThrust > 0f && !thrusterEffect.isPlaying)
    //         {
    //             thrusterEffect.Play();
    //         }
    //         else if (currentThrust <= 0f && thrusterEffect.isPlaying)
    //         {
    //             thrusterEffect.Stop();
    //         }
    //         
    //         // 추진력에 따라 이펙트 강도 조절
    //         var emission = thrusterEffect.emission;
    //         emission.rateOverTime = currentThrust * 10f;
    //     }
    //     
    //     // 추진기 사운드 제어
    //     if (thrusterSound != null)
    //     {
    //         if (currentThrust > 0f && !thrusterSound.isPlaying)
    //         {
    //             thrusterSound.Play();
    //         }
    //         else if (currentThrust <= 0f && thrusterSound.isPlaying)
    //         {
    //             thrusterSound.Stop();
    //         }
    //         
    //         // 추진력에 따라 볼륨 조절
    //         thrusterSound.volume = Mathf.Lerp(0f, 1f, currentThrust / thrustPower);
    //     }
    // }
    
    // 외부에서 속도를 설정할 수 있는 메서드
    // public void SetVelocity(Vector2 newVelocity)
    // {
    //     velocity = newVelocity;
    // }
    //
    // // 현재 속도를 가져오는 메서드
    // public Vector2 GetVelocity()
    // {
    //     return velocity;
    // }
    //
    // // 속도 초기화 메서드
    // public void StopMovement()
    // {
    //     velocity          = Vector2.zero;
    //     rb.linearVelocity = Vector2.zero;
    // }
    
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