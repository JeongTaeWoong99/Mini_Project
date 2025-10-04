using UnityEngine;
using DG.Tweening;

public class DOTweenCameraController : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private Camera       targetCamera;
    [SerializeField] private Transform[]  cameraPositions;
    [SerializeField] private Transform    followTarget;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float moveDuration = 2f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeStrength = 1f;
    [SerializeField] private float zoomDuration = 1f;
    [SerializeField] private float zoomOutSize = 10f;
    [SerializeField] private float zoomInSize = 3f;
    
    [Header("카메라 효과")]
    [SerializeField] private bool  smoothFollow = true;
    [SerializeField] private float followSpeed = 2f;
    
    private Vector3 originalCameraPosition;
    private float   originalCameraSize;
    private Vector3 originalCameraRotation;
    private bool    isFollowing = false;
    private Tween   followTween;
    
    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
        
        if (targetCamera != null)
        {
            originalCameraPosition = targetCamera.transform.position;
            originalCameraRotation = targetCamera.transform.eulerAngles;
            
            if (targetCamera.orthographic)
                originalCameraSize = targetCamera.orthographicSize;
            else
                originalCameraSize = targetCamera.fieldOfView;
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            MoveCameraToNextPosition();
        
        if (Input.GetKeyDown(KeyCode.V))
            ShakeCamera();
        
        if (Input.GetKeyDown(KeyCode.B))
            ZoomIn();
        
        if (Input.GetKeyDown(KeyCode.N))
            ZoomOut();
        
        if (Input.GetKeyDown(KeyCode.M))
            ToggleFollowTarget();
        
        if (Input.GetKeyDown(KeyCode.K))
            PlayCameraSequence();
        
        if (Input.GetKeyDown(KeyCode.L))
            ResetCamera();
    }
    
    private int currentPositionIndex = 0;
    
    public void MoveCameraToNextPosition()
    {
        if (cameraPositions == null || cameraPositions.Length == 0 || targetCamera == null) return;
        
        StopFollowing();
        
        currentPositionIndex = (currentPositionIndex + 1) % cameraPositions.Length;
        Transform targetPos = cameraPositions[currentPositionIndex];
        
        if (targetPos == null) return;
        
        targetCamera.transform.DOMove(targetPos.position, moveDuration)
            .SetEase(Ease.InOutQuad);
        
        targetCamera.transform.DORotate(targetPos.eulerAngles, moveDuration)
            .SetEase(Ease.InOutQuad);
        
        Debug.Log($"카메라가 위치 {currentPositionIndex}로 이동 중...");
    }
    
    public void ShakeCamera()
    {
        if (targetCamera == null) return;
        
        // 카메라 흔들기 효과
        targetCamera.transform.DOShakePosition(shakeDuration, shakeStrength, 10, 90, false, true)
            .SetEase(Ease.InOutQuad);
        
        // 회전도 같이 흔들기 (옵션)
        targetCamera.transform.DOShakeRotation(shakeDuration, new Vector3(2f, 2f, 1f), 10, 90, true)
            .SetEase(Ease.InOutQuad);
        
        Debug.Log("카메라 흔들기 효과!");
    }
    
    public void ZoomIn()
    {
        if (targetCamera == null) return;
        
        if (targetCamera.orthographic)
        {
            targetCamera.DOOrthoSize(zoomInSize, zoomDuration)
                .SetEase(Ease.InOutQuad);
        }
        else
        {
            targetCamera.DOFieldOfView(zoomInSize * 10, zoomDuration)
                .SetEase(Ease.InOutQuad);
        }
        
        Debug.Log("줌 인!");
    }
    
    public void ZoomOut()
    {
        if (targetCamera == null) return;
        
        if (targetCamera.orthographic)
        {
            targetCamera.DOOrthoSize(zoomOutSize, zoomDuration)
                .SetEase(Ease.InOutQuad);
        }
        else
        {
            targetCamera.DOFieldOfView(zoomOutSize * 6, zoomDuration)
                .SetEase(Ease.InOutQuad);
        }
        
        Debug.Log("줌 아웃!");
    }
    
    public void ToggleFollowTarget()
    {
        if (followTarget == null || targetCamera == null) return;
        
        if (isFollowing)
        {
            StopFollowing();
        }
        else
        {
            StartFollowing();
        }
    }
    
    private void StartFollowing()
    {
        if (followTarget == null || targetCamera == null) return;
        
        isFollowing = true;
        
        if (smoothFollow)
        {
            // 부드러운 따라가기 - Update에서 직접 처리
            followTween = DOTween.To(() => 0f, x => {
                if (followTarget != null && targetCamera != null)
                {
                    Vector3 targetPos = followTarget.position + Vector3.back * 10f;
                    targetCamera.transform.position = Vector3.Lerp(
                        targetCamera.transform.position, 
                        targetPos, 
                        Time.deltaTime * followSpeed);
                }
            }, 1f, float.MaxValue)
            .SetEase(Ease.Linear);
        }
        else
        {
            // 즉시 따라가기 - Update에서 직접 처리
            followTween = DOTween.To(() => 0f, x => {
                if (followTarget != null && targetCamera != null)
                {
                    Vector3 targetPos = followTarget.position + Vector3.back * 10f;
                    targetCamera.transform.position = targetPos;
                }
            }, 1f, float.MaxValue)
            .SetEase(Ease.Linear);
        }
        
        Debug.Log("타겟 따라가기 시작!");
    }
    
    private void StopFollowing()
    {
        isFollowing = false;
        
        if (followTween != null)
        {
            followTween.Kill();
            followTween = null;
        }
        
        Debug.Log("타겟 따라가기 중지!");
    }
    
    public void PlayCameraSequence()
    {
        if (targetCamera == null) return;
        
        StopFollowing();
        
        Sequence cameraSequence = DOTween.Sequence();
        
        // 1단계: 줌 인
        if (targetCamera.orthographic)
        {
            cameraSequence.Append(targetCamera.DOOrthoSize(zoomInSize, 1f));
        }
        else
        {
            cameraSequence.Append(targetCamera.DOFieldOfView(30f, 1f));
        }
        
        // 2단계: 회전
        cameraSequence.Append(targetCamera.transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InOutSine));
        
        // 3단계: 흔들기
        cameraSequence.AppendCallback(() => ShakeCamera());
        cameraSequence.AppendInterval(shakeDuration);
        
        // 4단계: 줌 아웃하면서 원래 위치로
        if (targetCamera.orthographic)
        {
            cameraSequence.Append(targetCamera.DOOrthoSize(originalCameraSize, 1f));
        }
        else
        {
            cameraSequence.Append(targetCamera.DOFieldOfView(originalCameraSize, 1f));
        }
        
        cameraSequence.Join(targetCamera.transform.DOMove(originalCameraPosition, 1f));
        cameraSequence.Join(targetCamera.transform.DORotate(originalCameraRotation, 1f));
        
        cameraSequence.OnComplete(() => Debug.Log("카메라 시퀀스 완료!"));
        cameraSequence.Play();
    }
    
    public void ResetCamera()
    {
        if (targetCamera == null) return;
        
        StopFollowing();
        targetCamera.transform.DOKill();
        
        targetCamera.transform.DOMove(originalCameraPosition, 1f)
            .SetEase(Ease.InOutQuad);
        
        targetCamera.transform.DORotate(originalCameraRotation, 1f)
            .SetEase(Ease.InOutQuad);
        
        if (targetCamera.orthographic)
        {
            targetCamera.DOOrthoSize(originalCameraSize, 1f)
                .SetEase(Ease.InOutQuad);
        }
        else
        {
            targetCamera.DOFieldOfView(originalCameraSize, 1f)
                .SetEase(Ease.InOutQuad);
        }
        
        Debug.Log("카메라 리셋!");
    }
    
    void OnDestroy()
    {
        StopFollowing();
        if (targetCamera != null)
            targetCamera.transform.DOKill();
    }
    
    void OnDisable()
    {
        StopFollowing();
    }
    
    // 게임 이벤트에서 호출할 수 있는 유틸리티 메서드들
    public void QuickShake() => ShakeCamera();
    public void QuickZoomIn() => ZoomIn();
    public void QuickZoomOut() => ZoomOut();
    
    // 특정 위치로 카메라 이동 (외부에서 호출 가능)
    public void MoveCameraTo(Vector3 position, float duration = 2f)
    {
        if (targetCamera == null) return;
        
        StopFollowing();
        targetCamera.transform.DOMove(position, duration)
            .SetEase(Ease.InOutQuad);
    }
    
    // 특정 타겟을 바라보도록 카메라 회전
    public void LookAtTarget(Transform target, float duration = 1f)
    {
        if (targetCamera == null || target == null) return;
        
        Vector3 direction = target.position - targetCamera.transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        
        targetCamera.transform.DORotateQuaternion(rotation, duration)
            .SetEase(Ease.InOutQuad);
    }
} 