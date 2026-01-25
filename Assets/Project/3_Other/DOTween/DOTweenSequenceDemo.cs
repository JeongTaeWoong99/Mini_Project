using UnityEngine;
using DG.Tweening;
using System.Collections;

public class DOTweenSequenceDemo : MonoBehaviour
{
    [Header("시퀀스 대상 오브젝트들")]
    [SerializeField] private Transform[] sequenceTargets;
    
    [Header("시퀀스 설정")]
    [SerializeField] private float sequenceDuration = 2f;
    [SerializeField] private float delayBetweenTargets = 0.2f;
    
    [Header("애니메이션 설정")]
    [SerializeField] private Vector3 jumpHeight = new Vector3(0f, 3f, 0f);
    [SerializeField] private Vector3 rotationAmount = new Vector3(0f, 360f, 0f);
    [SerializeField] private Vector3 scaleAmount = new Vector3(1.5f, 1.5f, 1.5f);
    
    private Sequence  currentSequence;
    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    
    void Start()
    {
        // 초기 위치와 스케일 저장
        if (sequenceTargets != null && sequenceTargets.Length > 0)
        {
            originalPositions = new Vector3[sequenceTargets.Length];
            originalScales = new Vector3[sequenceTargets.Length];
            
            for (int i = 0; i < sequenceTargets.Length; i++)
            {
                if (sequenceTargets[i] != null)
                {
                    originalPositions[i] = sequenceTargets[i].position;
                    originalScales[i] = sequenceTargets[i].localScale;
                }
            }
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha6))
            PlayWaveSequence();
        
        if (Input.GetKeyDown(KeyCode.Alpha7))
            PlayJumpSequence();
        
        if (Input.GetKeyDown(KeyCode.Alpha8))
            PlayComplexSequence();
        
        if (Input.GetKeyDown(KeyCode.Alpha9))
            PlayLoopingSequence();
        
        if (Input.GetKeyDown(KeyCode.Alpha0))
            StopCurrentSequence();
    }
    
    public void PlayWaveSequence()
    {
        if (sequenceTargets == null || sequenceTargets.Length == 0) return;
        
        StopCurrentSequence();
        
        currentSequence = DOTween.Sequence();
        
        for (int i = 0; i < sequenceTargets.Length; i++)
        {
            if (sequenceTargets[i] == null) continue;
            
            // 각 오브젝트를 순차적으로 위아래로 움직이는 웨이브 효과
            currentSequence.Insert(i * delayBetweenTargets, 
                sequenceTargets[i].DOMoveY(originalPositions[i].y + jumpHeight.y, sequenceDuration / 2)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo));
        }
        
        currentSequence.OnComplete(() => {
            Debug.Log("웨이브 시퀀스 완료!");
        });
        
        currentSequence.Play();
    }
    
    public void PlayJumpSequence()
    {
        if (sequenceTargets == null || sequenceTargets.Length == 0) return;
        
        StopCurrentSequence();
        
        currentSequence = DOTween.Sequence();
        
        for (int i = 0; i < sequenceTargets.Length; i++)
        {
            if (sequenceTargets[i] == null) continue;
            
            // 점프와 회전을 조합한 시퀀스
            currentSequence.Insert(i * delayBetweenTargets,
                sequenceTargets[i].DOJump(
                    originalPositions[i] + new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-2f, 2f)),
                    jumpHeight.y,
                    1,
                    sequenceDuration)
                .SetEase(Ease.OutQuad));
            
            currentSequence.Insert(i * delayBetweenTargets,
                sequenceTargets[i].DORotate(rotationAmount, sequenceDuration, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine));
        }
        
        // 원래 위치로 되돌리기
        currentSequence.AppendInterval(0.5f);
        
        for (int i = 0; i < sequenceTargets.Length; i++)
        {
            if (sequenceTargets[i] == null) continue;
            
            currentSequence.Insert(currentSequence.Duration(),
                sequenceTargets[i].DOMove(originalPositions[i], 1f)
                .SetEase(Ease.InOutQuad));
        }
        
        currentSequence.Play();
    }
    
    public void PlayComplexSequence()
    {
        if (sequenceTargets == null || sequenceTargets.Length == 0) return;
        
        StopCurrentSequence();
        
        currentSequence = DOTween.Sequence();
        
        // 1단계: 모든 오브젝트 스케일 확대
        for (int i = 0; i < sequenceTargets.Length; i++)
        {
            if (sequenceTargets[i] == null) continue;
            
            currentSequence.Join(
                sequenceTargets[i].DOScale(scaleAmount, 0.5f)
                .SetEase(Ease.OutBack));
        }
        
        // 2단계: 순차적으로 점프
        currentSequence.AppendInterval(0.2f);
        
        for (int i = 0; i < sequenceTargets.Length; i++)
        {
            if (sequenceTargets[i] == null) continue;
            
            currentSequence.Append(
                sequenceTargets[i].DOMoveY(originalPositions[i].y + jumpHeight.y, 0.3f)
                .SetEase(Ease.OutQuad)
                .SetLoops(2, LoopType.Yoyo));
        }
        
        // 3단계: 동시에 회전하면서 원래 크기로
        for (int i = 0; i < sequenceTargets.Length; i++)
        {
            if (sequenceTargets[i] == null) continue;
            
            currentSequence.Join(
                sequenceTargets[i].DORotate(rotationAmount, 1f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.InOutSine));
            
            currentSequence.Join(
                sequenceTargets[i].DOScale(originalScales[i], 1f)
                .SetEase(Ease.InBack));
        }
        
        // 콜백 이벤트들
        currentSequence.OnStart(() => Debug.Log("복합 시퀀스 시작!"));
        currentSequence.OnUpdate(() => {
            // 시퀀스 진행률 표시 (옵션)
            // Debug.Log($"진행률: {currentSequence.ElapsedPercentage():F2}");
        });
        currentSequence.OnComplete(() => Debug.Log("복합 시퀀스 완료!"));
        
        currentSequence.Play();
    }
    
    public void PlayLoopingSequence()
    {
        if (sequenceTargets == null || sequenceTargets.Length == 0) return;
        
        StopCurrentSequence();
        
        currentSequence = DOTween.Sequence();
        
        for (int i = 0; i < sequenceTargets.Length; i++)
        {
            if (sequenceTargets[i] == null) continue;
            
            // 각 오브젝트를 원형으로 회전시키는 무한 루프
            Vector3 centerPos = originalPositions[i];
            float radius = 2f;
            float angleStep = 360f / sequenceTargets.Length;
            float startAngle = i * angleStep;
            
            currentSequence.Join(
                sequenceTargets[i].DORotate(new Vector3(0, 360, 0), 2f, RotateMode.LocalAxisAdd)
                .SetEase(Ease.Linear)
                .SetLoops(-1));
            
            // 원형 궤도 이동
            currentSequence.Join(
                DOTween.To(() => 0f, x => {
                    float angle = (startAngle + x) * Mathf.Deg2Rad;
                    Vector3 newPos = centerPos + new Vector3(
                        Mathf.Cos(angle) * radius,
                        0f,
                        Mathf.Sin(angle) * radius);
                    sequenceTargets[i].position = newPos;
                }, 360f, 4f)
                .SetEase(Ease.Linear)
                .SetLoops(-1));
        }
        
        currentSequence.Play();
        
        Debug.Log("무한 루프 시퀀스 시작! (0키를 눌러 중지)");
    }
    
    public void StopCurrentSequence()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
            Debug.Log("현재 시퀀스 중지됨");
            
            // 모든 오브젝트를 원래 상태로 복원
            StartCoroutine(ResetAllTargets());
        }
    }
    
    private IEnumerator ResetAllTargets()
    {
        yield return new WaitForSeconds(0.1f); // 잠시 대기
        
        for (int i = 0; i < sequenceTargets.Length; i++)
        {
            if (sequenceTargets[i] == null) continue;
            
            sequenceTargets[i].DOKill(); // 개별 트윈 중지
            sequenceTargets[i].DOMove(originalPositions[i], 0.5f);
            sequenceTargets[i].DOScale(originalScales[i], 0.5f);
            sequenceTargets[i].DORotate(Vector3.zero, 0.5f);
        }
    }
    
    void OnDestroy()
    {
        StopCurrentSequence();
    }
    
    void OnDisable()
    {
        StopCurrentSequence();
    }
} 