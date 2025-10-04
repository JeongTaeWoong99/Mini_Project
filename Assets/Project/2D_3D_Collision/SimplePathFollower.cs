using UnityEngine;

public class SimplePathFollower : MonoBehaviour
{
    [Header("경로 설정")]
    public Transform[] waypoints;  // 경로 포인트들
    
    [Header("Movement Settings - RocketController 3D 버전")]
    [SerializeField] private float thrustPower = 5f;     // 추진력 (RocketController와 동일)
    [SerializeField] private float maxSpeed = 10f;       // 최대 속도 (RocketController와 동일)
    [SerializeField] private float drag = 0.95f;         // 우주에서의 감속 (RocketController와 동일)
    [SerializeField] private float arrivalDistance = 1f; // 도착 판정 거리
    
    [Header("Rotation Settings - RocketController 3D 버전")]
    [SerializeField] private bool smoothRotation = true;   // 부드러운 회전 (RocketController와 동일)
    [SerializeField] private float rotationDamping = 5f;   // 회전 감쇠 (RocketController와 동일)
    [SerializeField] private float inputThreshold = 0.1f;  // 입력 임계값 (RocketController와 동일)
    [SerializeField] private Vector3 rotationOffset = new Vector3(0, 90, 0); // 회전 오프셋 (RocketController의 -90f와 동일 개념)
    
    [Header("디버그")]
    public bool showDebug = true;         // 디버그 정보 표시
    
    // RocketController와 동일한 변수들 (3D 버전)
    private Rigidbody rb;                    // 3D Rigidbody (RocketController는 2D)
    private Vector3   movementInput;           // 이동 입력 (경로 방향으로 자동 계산)
    private Vector3   velocity;                // 현재 속도 (RocketController와 동일)
    private float     currentThrust;             // 현재 추진력 (RocketController와 동일)
    
    // PathFollower 전용 변수들
    private int currentWaypointIndex = 0;    // 현재 목표 포인트 인덱스
    private bool isMoving = false;
    
    void Start()
    {
        // RocketController와 동일한 Rigidbody 설정 (3D 버전)
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        // 우주 환경 설정 (RocketController와 동일)
        rb.useGravity = false;        // 중력 없음 (2D의 gravityScale = 0f와 동일)
        rb.linearDamping = 0f;        // Rigidbody의 드래그 사용 안함 (직접 제어)
        rb.angularDamping = 0f;       // 각속도 드래그 사용 안함
        
        if (waypoints.Length > 0)
        {
            isMoving = true;
            velocity = Vector3.zero;  // 초기 속도 설정
        }
    }
    
    void Update()
    {
        if (!isMoving || waypoints.Length == 0) return;
        
        HandleInput();      // 키 입력 대신 경로 방향 계산 (RocketController 구조와 동일)
        HandleRotation();   // 회전 (RocketController와 동일)
        CheckArrival();     // 도착 체크 (PathFollower 전용)
    }
    
    void FixedUpdate()
    {
        HandleMovement();   // 물리 이동 (RocketController와 동일)
    }
    
    private void HandleInput()
    {
        // RocketController의 HandleInput()과 동일한 구조
        // 플레이어 입력 대신 경로 방향을 계산
        if (currentWaypointIndex >= waypoints.Length) 
        {
            movementInput = Vector3.zero;
            currentThrust = 0f;
            return;
        }
        
        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;
        
        // 경로 방향을 movementInput으로 설정 (RocketController의 키 입력과 동일한 역할)
        movementInput = direction;
        
        // 추진력 계산 (RocketController와 동일 - 입력이 있을 때만)
        currentThrust = movementInput.magnitude > inputThreshold ? thrustPower : 0f;
    }
    
    private void HandleMovement()
    {
        // RocketController의 HandleMovement()와 완전히 동일한 로직 (3D 버전)
        if (movementInput.magnitude > inputThreshold)
        {
            // 입력 방향으로 추진력 적용 (RocketController와 동일)
            Vector3 thrustDirection = movementInput.normalized;
            velocity += thrustDirection * (currentThrust * Time.fixedDeltaTime);
            
            // 최대 속도 제한 (RocketController와 동일)
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
            }
        }
        
        // 우주에서의 감속 (RocketController와 완전히 동일)
        // drag가 0.95면, 1 fixed 프레임 마다, 95%로 속도가 감소함...
        velocity *= drag;
        
        // 3D Rigidbody에 속도 적용 (RocketController는 2D)
        rb.linearVelocity = velocity;
    }
    
    private void HandleRotation()
    {
        // RocketController의 HandleRotation()을 3D로 변환
        if (movementInput.magnitude > inputThreshold)
        {
            if (smoothRotation)
            {
                // 부드러운 회전 - RocketController의 2D 로직을 3D로 변환
                // RocketController: Atan2로 목표 각도 계산 → 3D: LookRotation으로 목표 회전 계산
                Quaternion targetRotation = Quaternion.LookRotation(movementInput.normalized);
                
                // RocketController의 -90f 오프셋과 동일한 개념으로 rotationOffset 적용
                targetRotation *= Quaternion.Euler(rotationOffset);
                
                // RocketController: DeltaAngle로 각도 차이 계산 → 3D: Slerp로 부드러운 회전
                // rotationDamping 값 그대로 사용
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationDamping * Time.deltaTime);
            }
            else
            {
                // 즉시 회전 - RocketController와 동일한 개념
                Quaternion targetRotation = Quaternion.LookRotation(movementInput.normalized);
                targetRotation *= Quaternion.Euler(rotationOffset);
                transform.rotation = targetRotation;
            }
        }
    }
    
    private void CheckArrival()
    {
        // PathFollower 전용 메서드 - 경로 포인트 도착 체크
        if (currentWaypointIndex >= waypoints.Length) return;
        
        Transform target = waypoints[currentWaypointIndex];
        float distance = Vector3.Distance(transform.position, target.position);
        
        if (distance < arrivalDistance)
        {
            // 다음 포인트로 이동
            currentWaypointIndex++;
            
            // 마지막 포인트에 도달하면 처음부터 다시 시작
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }
    
    // 외부에서 제어할 수 있는 메서드들 (RocketController 스타일)
    public void SetVelocity(Vector3 newVelocity)
    {
        velocity = newVelocity;
    }
    
    public Vector3 GetVelocity()
    {
        return velocity;
    }
    
    public void StopMovement()
    {
        velocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        isMoving = false;
    }
    
    public void ResumeMovement()
    {
        isMoving = true;
    }
    
    // PathFollower 전용 메서드들
    public void SetSpeed(float newMaxSpeed)
    {
        maxSpeed = newMaxSpeed;
    }
    
    public void SetCurrentWaypoint(int index)
    {
        if (index >= 0 && index < waypoints.Length)
        {
            currentWaypointIndex = index;
        }
    }
    
    public int GetCurrentWaypointIndex()
    {
        return currentWaypointIndex;
    }
    
    // 디버그용 기즈모 그리기 (RocketController 스타일 + PathFollower 전용)
    private void OnDrawGizmos()
    {
        if (!showDebug) return;
        
        // 경로 라인 그리기 (PathFollower 전용)
        if (waypoints != null && waypoints.Length > 1)
        {
            Gizmos.color = Color.yellow;
            for (int i = 0; i < waypoints.Length; i++)
            {
                if (waypoints[i] == null) continue;
                
                // 포인트 표시
                Gizmos.DrawWireSphere(waypoints[i].position, 0.5f);
                
                // 다음 포인트와 연결선
                int nextIndex = (i + 1) % waypoints.Length;
                if (waypoints[nextIndex] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[nextIndex].position);
                }
            }
        }
        
        if (Application.isPlaying)
        {
            // RocketController와 동일한 디버그 표시
            // 속도 벡터 표시 (RocketController와 동일)
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + velocity);
            
            // 입력 방향 표시 (RocketController와 동일 - movementInput)
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + movementInput * 2f);
            
            // PathFollower 전용 디버그
            // 현재 목표 포인트 강조
            if (waypoints.Length > 0 && currentWaypointIndex < waypoints.Length)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(waypoints[currentWaypointIndex].position, 0.8f);
            }
            
            // 도착 판정 거리 표시
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, arrivalDistance);
        }
    }
} 