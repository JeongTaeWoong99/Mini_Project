using UnityEngine;
using DG.Tweening;

public class DOTweenBasicAnimations : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [SerializeField] private float   animationDuration = 1f;
    [SerializeField] private Vector3 moveTarget = new Vector3(5f, 0f, 0f);
    [SerializeField] private Vector3 scaleTarget = new Vector3(2f, 2f, 2f);
    [SerializeField] private Vector3 rotationTarget = new Vector3(0f, 0f, 360f);
    
    [Header("컨트롤")]
    [SerializeField] private bool  autoPlay = true;
    [SerializeField] private float autoPlayDelay = 2f;
    
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Vector3 originalRotation;
    
    void Start()
    {
        // 초기 값 저장
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.eulerAngles;
        
        if (autoPlay)
        {
            InvokeRepeating(nameof(PlayRandomAnimation), autoPlayDelay, autoPlayDelay);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            AnimatePosition();
        
        if (Input.GetKeyDown(KeyCode.W))
            AnimateScale();
        
        if (Input.GetKeyDown(KeyCode.E))
            AnimateRotation();
        
        if (Input.GetKeyDown(KeyCode.R))
            AnimateAll();
        
        if (Input.GetKeyDown(KeyCode.T))
            ResetTransform();
    }
    
    public void AnimatePosition()
    {
        transform.DOMove(originalPosition + moveTarget, animationDuration)
            .SetEase(Ease.OutBounce)
            .OnComplete(() => {
                transform.DOMove(originalPosition, animationDuration)
                    .SetEase(Ease.InOutQuad);
            });
    }
    
    public void AnimateScale()
    {
        transform.DOScale(scaleTarget, animationDuration)
            .SetEase(Ease.OutElastic)
            .OnComplete(() => {
                transform.DOScale(originalScale, animationDuration)
                    .SetEase(Ease.InOutQuad);
            });
    }
    
    public void AnimateRotation()
    {
        transform.DORotate(rotationTarget, animationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.InOutSine);
    }
    
    public void AnimateAll()
    {
        // 모든 애니메이션을 동시에 실행
        transform.DOMove(originalPosition + moveTarget, animationDuration)
            .SetEase(Ease.OutQuad);
        
        transform.DOScale(scaleTarget, animationDuration)
            .SetEase(Ease.OutQuad);
        
        transform.DORotate(rotationTarget, animationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                // 모든 애니메이션을 원래대로 되돌리기
                transform.DOMove(originalPosition, animationDuration);
                transform.DOScale(originalScale, animationDuration);
            });
    }
    
    public void ResetTransform()
    {
        transform.DOKill(); // 모든 트윈 중지
        
        transform.DOMove(originalPosition, 0.5f);
        transform.DOScale(originalScale, 0.5f);
        transform.DORotate(originalRotation, 0.5f);
    }
    
    private void PlayRandomAnimation()
    {
        int randomChoice = Random.Range(0, 4);
        
        switch (randomChoice)
        {
            case 0:
                AnimatePosition();
                break;
            case 1:
                AnimateScale();
                break;
            case 2:
                AnimateRotation();
                break;
            case 3:
                AnimateAll();
                break;
        }
    }
    
    void OnDestroy()
    {
        // 메모리 누수 방지
        transform.DOKill();
    }
} 