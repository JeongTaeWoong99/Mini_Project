using System.Collections;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float thrustPower = 5f;    // 추진력
    [SerializeField] private float maxSpeed    = 10f;   // 최대 속도
    [SerializeField] private float drag        = 0.95f; // 우주에서의 감속 (매우 적음)

    [Header("Rotation Settings")]
    [SerializeField] private bool  smoothRotation  = true; // 부드러운 회전
    [SerializeField] private float rotationDamping = 5f;   // 회전 감쇠

    [Header("Collision Settings")]
    [SerializeField] private bool  showCollisionDebug = false; // 충돌 디버그 표시
    [SerializeField] private float hitFlashDuration   = 0.2f;  // 충돌 시 깜빡임 지속 시간 (초)

    // Private variables
    private Rigidbody2D rb;
    private Vector2     movementInput;
    private Vector2     velocity;
    private float       currentThrust;     // 현재 추진력
    private Renderer    objectRenderer;    // 렌더러 (3D/2D 모두 지원)
    private Material    materialInstance;  // Material 인스턴스
    private Color       originalColor;     // 원래 색상
    private bool        isHit = false;     // 충돌 중 여부
    
    private void Start()
    {
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

        // Renderer 가져오기 (3D/2D 모두 지원)
        // 2D: SpriteRenderer, 3D: MeshRenderer, SkinnedMeshRenderer 등
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            // Material의 인스턴스 생성 (원본 보호)
            materialInstance = objectRenderer.material;

            // Shader Graph 등 커스텀 셰이더 대응: 여러 가능한 Color 프로퍼티 시도
            if (materialInstance.HasProperty("_Color"))
            {
                originalColor = materialInstance.GetColor("_Color");
            }
            else if (materialInstance.HasProperty("_BaseColor"))
            {
                originalColor = materialInstance.GetColor("_BaseColor");
            }
            else if (materialInstance.HasProperty("_MainColor"))
            {
                originalColor = materialInstance.GetColor("_MainColor");
            }
            else
            {
                // Color 프로퍼티가 없으면 기본값 사용
                originalColor = Color.white;
                Debug.LogWarning($"RocketController: Material '{materialInstance.name}'에서 Color 프로퍼티를 찾을 수 없습니다. 색상 변경 기능이 작동하지 않을 수 있습니다.");
            }
        }
    }
    
    private void Update()
    {
        HandleInput();          // 키 입력
        HandleRotation();       // 회전
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
    
    // 2D 충돌 이벤트 처리
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // if (showCollisionDebug)
        // {
        //     Debug.Log($"RocketController: {collision.gameObject.name}와 충돌! (Collision)");
        // }

        // 충돌 시 색상 변경
        if (!isHit)
        {
            StartCoroutine(HitFlash());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if (showCollisionDebug)
        // {
        //     Debug.Log($"RocketController: {collision.gameObject.name}와 충돌! (Trigger)");
        // }

        // 충돌 시 색상 변경
        if (!isHit)
        {
            StartCoroutine(HitFlash());
        }
    }

    // 충돌 시 색상 변경 코루틴
    private IEnumerator HitFlash()
    {
        if (materialInstance == null) yield break;

        isHit = true;

        // 빨간색으로 변경 (여러 가능한 프로퍼티 시도)
        SetMaterialColor(Color.red);

        // 지정된 시간 대기
        yield return new WaitForSeconds(hitFlashDuration);

        // 원래 색상으로 복구
        SetMaterialColor(originalColor);

        isHit = false;
    }

    // Material Color 설정 헬퍼 메서드
    private void SetMaterialColor(Color color)
    {
        if (materialInstance == null) return;

        // 여러 가능한 Color 프로퍼티 시도
        if (materialInstance.HasProperty("_Color"))
        {
            materialInstance.SetColor("_Color", color);
        }
        else if (materialInstance.HasProperty("_BaseColor"))
        {
            materialInstance.SetColor("_BaseColor", color);
        }
        else if (materialInstance.HasProperty("_MainColor"))
        {
            materialInstance.SetColor("_MainColor", color);
        }
        // 프로퍼티가 없으면 무시 (경고는 Start에서 이미 출력됨)
    }

    // Material 인스턴스 정리
    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
        }
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