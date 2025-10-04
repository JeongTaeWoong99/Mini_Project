using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class DOTweenUIAnimations : MonoBehaviour
{
    [Header("UI 요소들")]
    [SerializeField] private Image           fadeImage;
    [SerializeField] private Button          scaleButton;
    [SerializeField] private TextMeshProUGUI animatedText;
    [SerializeField] private RectTransform   slidePanel;
    [SerializeField] private Image           colorChangeImage;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float   animationDuration = 1f;
    [SerializeField] private Vector2 slideDistance = new Vector2(300f, 0f);
    [SerializeField] private Color   targetColor = Color.red;
    
    [Header("카운팅 애니메이션 설정")]
    [SerializeField] private int   countIncrease = 100;
    [SerializeField] private float countDuration = 1.5f;
    [SerializeField] private Color countAnimColor = Color.green;
    
    private Vector2  originalSlidePosition;
    private Color    originalColor;
    private Color    originalTextColor;
    private int      currentNumber = 0;
    private Tween    countingTween;
    private Sequence countingSequence;
    
    void Start()
    {
        // 초기 값 저장
        if (slidePanel != null)
            originalSlidePosition = slidePanel.anchoredPosition;
        
        if (colorChangeImage != null)
            originalColor = colorChangeImage.color;
        
        // 초기 텍스트 숫자 설정
        if (animatedText != null)
        {
            // 원래 텍스트 색상 저장
            originalTextColor = animatedText.color;
            
            // 현재 텍스트에서 숫자 추출 시도
            if (int.TryParse(animatedText.text, out int parsedNumber))
            {
                currentNumber = parsedNumber;
            }
            else
            {
                currentNumber = 0;
                animatedText.text = currentNumber.ToString();
            }
        }
        
        // 버튼 이벤트 연결
        if (scaleButton != null)
        {
            scaleButton.onClick.AddListener(PlayButtonScaleAnimation);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            FadeInOut();
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SlideAnimation();
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
            TextTypewriterEffect();
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
            ColorAnimation();
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
            PlayAllUIAnimations();
        
        if (Input.GetKeyDown(KeyCode.Alpha6))
            PlayCountingAnimation();
    }
    
    public void FadeInOut()
    {
        if (fadeImage == null) return;
        
        fadeImage.DOFade(0f, animationDuration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => {
                fadeImage.DOFade(1f, animationDuration)
                    .SetEase(Ease.InOutQuad);
            });
    }
    
    public void SlideAnimation()
    {
        if (slidePanel == null) return;
        
        slidePanel.DOAnchorPos(originalSlidePosition + slideDistance, animationDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                slidePanel.DOAnchorPos(originalSlidePosition, animationDuration)
                    .SetEase(Ease.InBack);
            });
    }
    
    public void TextTypewriterEffect()
    {
        if (animatedText == null) return;
        
        string originalText = animatedText.text;
        StartCoroutine(TypewriterCoroutine(originalText));
    }
    
    private System.Collections.IEnumerator TypewriterCoroutine(string targetText)
    {
        animatedText.text = "";
        
        for (int i = 0; i <= targetText.Length; i++)
        {
            animatedText.text = targetText.Substring(0, i);
            yield return new WaitForSeconds((animationDuration * 2) / targetText.Length);
        }
    }
    
    public void ColorAnimation()
    {
        if (colorChangeImage == null) return;
        
        colorChangeImage.DOColor(targetColor, animationDuration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                colorChangeImage.DOColor(originalColor, animationDuration)
                    .SetEase(Ease.InOutSine);
            });
    }
    
    public void PlayCountingAnimation()
    {
        if (animatedText == null) return;
        
        // 먼저 목표값을 즉시 증가 (이렇게 해야 연속 입력 시 정확한 누적)
        currentNumber += countIncrease;
        
        // 기존 애니메이션 중지 (즉시 완료 처리됨)
        StopCountingAnimation();
        
        // 현재 화면에 표시된 값을 시작점으로 사용
        int displayedValue = 0;
        if (int.TryParse(animatedText.text, out int parsedValue))
        {
            displayedValue = parsedValue;
        }
        
        int startValue = displayedValue;
        int endValue = currentNumber; // 누적된 최종 목표값까지
        
        // 시퀀스 생성
        countingSequence = DOTween.Sequence();
        
        // 1. 숫자 카운팅 애니메이션
        countingTween = DOTween.To(() => startValue, x => {
            if (animatedText != null) // null 체크 추가
                animatedText.text = x.ToString();
        }, endValue, countDuration)
        .SetEase(Ease.OutQuart);
        
        countingSequence.Join(countingTween);
        
        // 2. 텍스트 스케일 펀치 효과
        countingSequence.Join(animatedText.transform.DOPunchScale(Vector3.one * 0.3f, countDuration * 0.6f, 3, 0.5f)
            .SetEase(Ease.OutElastic));
        
        // 3. 텍스트 색상 변화 애니메이션 (동시 실행)
        Sequence colorSeq = DOTween.Sequence();
        colorSeq.Append(animatedText.DOColor(countAnimColor, countDuration * 0.3f)
            .SetEase(Ease.OutQuad));
        colorSeq.Append(animatedText.DOColor(originalTextColor, countDuration * 0.7f)
            .SetEase(Ease.InQuad));
        
        countingSequence.Join(colorSeq);
        
        // 완료 콜백
        countingSequence.OnComplete(() => {
            // 완료 후 확실히 원래 상태로 복원
            if (animatedText != null)
            {
                animatedText.transform.localScale = Vector3.one;
                animatedText.color = originalTextColor;
                animatedText.text = currentNumber.ToString(); // 최종값 확실히 표시
            }
            countingTween = null;
            countingSequence = null;
        });
        
        countingSequence.Play();
        
        Debug.Log($"숫자가 {startValue}에서 {endValue}로 증가합니다! (목표: {currentNumber})");
    }
    
    public void StopCountingAnimation()
    {
        // 기존 애니메이션을 즉시 완료 상태로 만들기
        if (countingTween != null && countingTween.IsActive())
        {
            countingTween.Complete(); // Kill 대신 Complete 사용
            countingTween = null;
        }
        
        if (countingSequence != null && countingSequence.IsActive())
        {
            countingSequence.Complete(); // Kill 대신 Complete 사용
            countingSequence = null;
        }
        
        // 스케일과 색상을 원래 상태로 복원
        if (animatedText != null)
        {
            animatedText.transform.DOKill();
            animatedText.DOKill();
            
            // 즉시 원래 상태로 복원
            animatedText.transform.localScale = Vector3.one;
            animatedText.color = originalTextColor; // 원래 색상으로 복원
        }
    }
    
    public void PlayButtonScaleAnimation()
    {
        if (scaleButton == null) return;
        
        scaleButton.transform.DOScale(1.2f, 0.1f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => {
                scaleButton.transform.DOScale(1f, 0.1f)
                    .SetEase(Ease.InQuad);
            });
    }
    
    public void PlayAllUIAnimations()
    {
        // UI 애니메이션들을 시퀀스로 실행
        Sequence uiSequence = DOTween.Sequence();
        
        if (fadeImage != null)
        {
            uiSequence.Append(fadeImage.DOFade(0.5f, 0.5f));
        }
        
        if (slidePanel != null)
        {
            uiSequence.Join(slidePanel.DOAnchorPos(originalSlidePosition + slideDistance, 0.5f)
                .SetEase(Ease.OutBack));
        }
        
        if (colorChangeImage != null)
        {
            uiSequence.Join(colorChangeImage.DOColor(targetColor, 0.5f));
        }
        
        if (scaleButton != null)
        {
            uiSequence.Join(scaleButton.transform.DOScale(1.2f, 0.5f));
        }
        
        // 카운팅 애니메이션 추가 (전체 애니메이션용 - 안전하게 처리)
        if (animatedText != null)
        {
            // 목표값 먼저 증가
            currentNumber += (countIncrease / 2); // 전체 애니메이션에서는 절반만
            
            // 기존 카운팅 애니메이션 중지
            StopCountingAnimation();
            
            // 현재 화면 값에서 시작
            int displayedValue = 0;
            if (int.TryParse(animatedText.text, out int parsedValue))
            {
                displayedValue = parsedValue;
            }
            
            int startValue = displayedValue;
            int endValue = currentNumber; // 누적된 목표값까지
            
            uiSequence.Join(DOTween.To(() => startValue, x => {
                if (animatedText != null)
                    animatedText.text = x.ToString();
            }, endValue, 0.5f));
        }
        
        // 원래대로 되돌리기
        uiSequence.AppendInterval(0.5f);
        
        if (fadeImage != null)
        {
            uiSequence.Append(fadeImage.DOFade(1f, 0.5f));
        }
        
        if (slidePanel != null)
        {
            uiSequence.Join(slidePanel.DOAnchorPos(originalSlidePosition, 0.5f)
                .SetEase(Ease.InBack));
        }
        
        if (colorChangeImage != null)
        {
            uiSequence.Join(colorChangeImage.DOColor(originalColor, 0.5f));
        }
        
        if (scaleButton != null)
        {
            uiSequence.Join(scaleButton.transform.DOScale(1f, 0.5f));
        }
        
        uiSequence.Play();
    }
    
    // 펄스 효과 (계속 반복)
    public void StartPulseEffect()
    {
        if (scaleButton != null)
        {
            scaleButton.transform.DOScale(1.1f, 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }
    
    public void StopPulseEffect()
    {
        if (scaleButton != null)
        {
            scaleButton.transform.DOKill();
            scaleButton.transform.DOScale(1f, 0.2f);
        }
    }
    
    // 추가 유틸리티 메서드
    public void ResetCountingValue(int newValue = 0)
    {
        StopCountingAnimation();
        currentNumber = newValue;
        if (animatedText != null)
        {
            animatedText.text = currentNumber.ToString();
            animatedText.transform.localScale = Vector3.one;
            animatedText.color = originalTextColor;
        }
    }
    
    public int GetCurrentCountingValue()
    {
        return currentNumber;
    }
    
    void OnDestroy()
    {
        // 메모리 누수 방지
        StopCountingAnimation();
        DOTween.Kill(this);
    }
} 