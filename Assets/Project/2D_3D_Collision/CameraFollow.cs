using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("따라갈 대상")]
    public Transform target;  // 따라갈 플레이어 오브젝트
    
    [Header("카메라 설정")]
    [SerializeField] private float smoothTime     = 0.3f;           // 부드러운 이동 시간 (낮을수록 빠름)
    [SerializeField] private float maxSpeed       = Mathf.Infinity; // 최대 이동 속도
    [SerializeField] private bool  followX        = true;           // X축 따라가기
    [SerializeField] private bool  followY        = true;           // Y축 따라가기
    [SerializeField] private bool  followZ        = false;          // Z축 따라가기 (기본 꺼짐)
    [SerializeField] private bool  useFixedUpdate = true;           // FixedUpdate 사용 (Rigidbody 플레이어용)
    
    [Header("오프셋 설정")]
    [SerializeField] private Vector3 offset = Vector3.zero; // 카메라 오프셋
    
    [Header("디버그")]
    [SerializeField] private bool showDebug = true; // 디버그 정보 표시
    
    // SmoothDamp용 velocity 변수들
    private Vector3 velocity        = Vector3.zero;
    private Vector3 initialPosition = Vector3.zero; // 초기 위치 저장
    
    void Start()
    {
        // 초기 위치 저장
        initialPosition = transform.position;
        
        // 타겟이 없으면 경고
        if (target == null)
        {
            Debug.LogWarning("CameraFollow: 타겟이 설정되지 않았습니다!");
        }
        
        // 타겟이 Rigidbody를 사용하는 경우 Interpolation 체크
        if (target != null)
        {
            Rigidbody targetRb = target.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                if (targetRb.interpolation == RigidbodyInterpolation.None)
                {
                    Debug.LogWarning("CameraFollow: 타겟의 Rigidbody Interpolation이 None입니다. " + "부드러운 카메라 움직임을 위해 Interpolate 또는 Extrapolate로 설정하거나 " + "카메라의 Use Fixed Update를 켜주세요.");
                }
            }
        }
    }
    
    void LateUpdate()
    {
        // FixedUpdate 사용 시 건너뛰기
        if (useFixedUpdate) return;
        
        UpdateCameraPosition();
    }
    
    void FixedUpdate()
    {
        // LateUpdate 사용 시 건너뛰기
        if (!useFixedUpdate) return;
        
        UpdateCameraPosition();
    }
    
    void UpdateCameraPosition()
    {
        // 타겟이 없으면 동작하지 않음
        if (target == null) return;
        
        // 목표 위치 계산
        Vector3 targetPosition = CalculateTargetPosition();
        
        // SmoothDamp로 부드러운 이동 (버벅거림 해결)
        float deltaTime = useFixedUpdate ? Time.fixedDeltaTime : Time.deltaTime;
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime, maxSpeed, deltaTime);
    }
    
    Vector3 CalculateTargetPosition()
    {
        // 현재 카메라 위치
        Vector3 currentPos = transform.position;
        
        // 타겟 위치 + 오프셋
        Vector3 targetPos = target.position + offset;
        
        // 축별로 따라갈지 결정
        Vector3 newPosition = new Vector3(
            followX ? targetPos.x : currentPos.x,  // X축 따라가기
            followY ? targetPos.y : currentPos.y,  // Y축 따라가기
            followZ ? targetPos.z : currentPos.z   // Z축 따라가기
        );
        
        return newPosition;
    }
    
    // 외부에서 제어할 수 있는 메서드들
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetSmoothTime(float newSmoothTime)
    {
        smoothTime = newSmoothTime;
    }
    
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
    
    public void SetFollowAxis(bool x, bool y, bool z)
    {
        followX = x;
        followY = y;
        followZ = z;
    }
    
    public void SetUseFixedUpdate(bool useFixed)
    {
        useFixedUpdate = useFixed;
        velocity = Vector3.zero;  // 설정 변경 시 velocity 초기화
    }
    
    public void ResetToInitialPosition()
    {
        transform.position = initialPosition;
        velocity = Vector3.zero;  // velocity도 초기화
    }
    
    public void StopFollowing()
    {
        velocity = Vector3.zero;  // 즉시 정지
    }
    
    // 디버그용 기즈모 그리기
    void OnDrawGizmos()
    {
        if (!showDebug || target == null) return;
        
        // 타겟 위치 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position, 0.5f);
        
        // 오프셋 적용된 목표 위치 표시
        Vector3 targetWithOffset = target.position + offset;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(targetWithOffset, 0.3f);
        
        // 카메라와 타겟 사이의 연결선
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, targetWithOffset);
        
        // 현재 카메라 위치 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.5f);
        
        // 현재 속도 벡터 표시 (디버그용)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + velocity);
            
            // 업데이트 방식 표시
            Gizmos.color = useFixedUpdate ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.1f);
        }
    }
} 