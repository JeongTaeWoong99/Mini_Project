using UnityEngine;
using DG.Tweening;
using TMPro;

public class DOTweenDemoManager : MonoBehaviour
{
    [Header("DOTween 컴포넌트들")]
    [SerializeField] private DOTweenBasicAnimations  basicAnimations;
    [SerializeField] private DOTweenUIAnimations     uiAnimations;
    [SerializeField] private DOTweenSequenceDemo     sequenceDemo;
    [SerializeField] private DOTweenCameraController cameraController;
    [SerializeField] private DOTweenGameEffects      gameEffects;
    
    [Header("UI 요소들")]
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Canvas          instructionCanvas;
    [SerializeField] private bool            showInstructions = true;
    
    [Header("DOTween 전역 설정")]
    [SerializeField] private bool         autoInitialize = true;
    [SerializeField] private int          maxTweeners = 200;
    [SerializeField] private int          maxSequences = 50;
    [SerializeField] private LogBehaviour logBehaviour = LogBehaviour.Default;
    
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeDOTween();
        SetupInstructionText();
        
        if (instructionCanvas != null)
            instructionCanvas.gameObject.SetActive(showInstructions);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
            ToggleInstructions();
        
        if (Input.GetKeyDown(KeyCode.Escape))
            StopAllDOTweenAnimations();
        
        if (Input.GetKeyDown(KeyCode.F12))
            ShowDOTweenDebugInfo();
        
        // UI 애니메이션 직접 제어 (중복 방지용)
        if (Input.GetKeyDown(KeyCode.Alpha6) && uiAnimations != null)
            uiAnimations.PlayCountingAnimation();
    }
    
    private void InitializeDOTween()
    {
        if (isInitialized) return;
        
        if (autoInitialize)
        {
            // DOTween 초기화 설정
            DOTween.Init(false, true, logBehaviour);
            DOTween.SetTweensCapacity(maxTweeners, maxSequences);
            
            // 전역 설정
            DOTween.defaultEaseType = Ease.OutQuad;
            DOTween.defaultAutoPlay = AutoPlay.All;
            DOTween.defaultAutoKill = true;
            
            Debug.Log("DOTween이 초기화되었습니다!");
        }
        
        isInitialized = true;
    }
    
    private void SetupInstructionText()
    {
        if (instructionText == null) return;
        
        string instructions = @"<size=24><color=yellow>DOTween 데모 컨트롤 가이드</color></size>

<size=18><color=cyan>== 기본 Transform 애니메이션 ==</color></size>
Q: 위치 애니메이션
W: 스케일 애니메이션  
E: 회전 애니메이션
R: 모든 애니메이션 동시 실행
T: Transform 리셋

 <size=18><color=cyan>== UI 애니메이션 ==</color></size>
 1: 페이드 인/아웃
 2: 슬라이드 애니메이션
 3: 텍스트 타이핑 효과
 4: 색상 변화 애니메이션
 5: 모든 UI 애니메이션
 6: 숫자 카운팅 애니메이션 (+100, 연속입력 완벽지원)

<size=18><color=cyan>== 시퀀스 데모 ==</color></size>
6: 웨이브 시퀀스
7: 점프 시퀀스
8: 복합 시퀀스
9: 무한 루프 시퀀스
0: 시퀀스 중지

<size=18><color=cyan>== 카메라 컨트롤 ==</color></size>
C: 카메라 위치 이동
V: 카메라 흔들기
B: 줌 인
N: 줌 아웃
M: 타겟 따라가기 토글
K: 카메라 시퀀스
L: 카메라 리셋

<size=18><color=cyan>== 게임 효과 ==</color></size>
F1: 등장 효과
F2: 퇴장 효과
F3: 수집 효과
F4: 데미지 효과
F5: 아이들 애니메이션 토글
F6: 팝업 효과
F7: 바운스 효과
F8: 페이드 효과
F9: 아이템 생성 효과
F10: 모든 효과 리셋

<size=18><color=cyan>== 전역 컨트롤 ==</color></size>
H: 이 가이드 토글
ESC: 모든 DOTween 애니메이션 중지
F12: DOTween 디버그 정보 표시";

        instructionText.text = instructions;
    }
    
    private void ToggleInstructions()
    {
        if (instructionCanvas != null)
        {
            showInstructions = !showInstructions;
            instructionCanvas.gameObject.SetActive(showInstructions);
            
            Debug.Log($"사용 가이드 {(showInstructions ? "표시" : "숨김")}");
        }
    }
    
    private void StopAllDOTweenAnimations()
    {
        // 모든 DOTween 애니메이션 즉시 중지
        DOTween.KillAll();
        
        Debug.Log("모든 DOTween 애니메이션이 중지되었습니다!");
    }
    
    private void ShowDOTweenDebugInfo()
    {
        // DOTween 현재 상태 정보 출력
        int activeTweens = DOTween.TotalActiveTweens();
        int playingTweens = DOTween.TotalPlayingTweens();
        
        string debugInfo = $@"
=== DOTween 디버그 정보 ===
활성 트윈 수: {activeTweens}
재생 중인 트윈 수: {playingTweens}
최대 트위너 용량: {maxTweeners}
최대 시퀀스 용량: {maxSequences}
초기화 상태: {isInitialized}";
        
        Debug.Log(debugInfo);
    }
    
    // 외부에서 호출할 수 있는 유틸리티 메서드들
    public void PlayRandomBasicAnimation()
    {
        if (basicAnimations == null) return;
        
        int randomChoice = Random.Range(0, 4);
        switch (randomChoice)
        {
            case 0: basicAnimations.AnimatePosition(); break;
            case 1: basicAnimations.AnimateScale(); break;
            case 2: basicAnimations.AnimateRotation(); break;
            case 3: basicAnimations.AnimateAll(); break;
        }
    }
    
    public void PlayRandomUIAnimation()
    {
        if (uiAnimations == null) return;
        
        int randomChoice = Random.Range(0, 6);
        switch (randomChoice)
        {
            case 0: uiAnimations.FadeInOut(); break;
            case 1: uiAnimations.SlideAnimation(); break;
            case 2: uiAnimations.TextTypewriterEffect(); break;
            case 3: uiAnimations.ColorAnimation(); break;
            case 4: uiAnimations.PlayAllUIAnimations(); break;
            case 5: uiAnimations.PlayCountingAnimation(); break;
        }
    }
    
    public void PlayRandomSequence()
    {
        if (sequenceDemo == null) return;
        
        int randomChoice = Random.Range(0, 4);
        switch (randomChoice)
        {
            case 0: sequenceDemo.PlayWaveSequence(); break;
            case 1: sequenceDemo.PlayJumpSequence(); break;
            case 2: sequenceDemo.PlayComplexSequence(); break;
            case 3: sequenceDemo.PlayLoopingSequence(); break;
        }
    }
    
    public void PlayRandomCameraEffect()
    {
        if (cameraController == null) return;
        
        int randomChoice = Random.Range(0, 4);
        switch (randomChoice)
        {
            case 0: cameraController.MoveCameraToNextPosition(); break;
            case 1: cameraController.ShakeCamera(); break;
            case 2: cameraController.ZoomIn(); break;
            case 3: cameraController.ZoomOut(); break;
        }
    }
    
    public void PlayRandomGameEffect()
    {
        if (gameEffects == null) return;
        
        int randomChoice = Random.Range(0, 7);
        switch (randomChoice)
        {
            case 0: gameEffects.PlayAppearEffect(); break;
            case 1: gameEffects.PlayDisappearEffect(); break;
            case 2: gameEffects.PlayCollectEffect(); break;
            case 3: gameEffects.PlayDamageEffect(); break;
            case 4: gameEffects.PlayPopupEffect(); break;
            case 5: gameEffects.PlayBounceEffect(); break;
            case 6: gameEffects.PlayItemSpawnEffect(); break;
        }
    }
    
    // 모든 컴포넌트 리셋
    public void ResetAllComponents()
    {
        basicAnimations?.ResetTransform();
        sequenceDemo?.StopCurrentSequence();
        cameraController?.ResetCamera();
        gameEffects?.ResetAllEffects();
        
        Debug.Log("모든 DOTween 컴포넌트가 리셋되었습니다!");
    }
    
    // 자동 데모 모드 (모든 효과를 순차적으로 보여줌)
    public void StartAutoDemoMode()
    {
        StartCoroutine(AutoDemoCoroutine());
    }
    
    private System.Collections.IEnumerator AutoDemoCoroutine()
    {
        Debug.Log("자동 데모 모드 시작!");
        
        yield return new WaitForSeconds(1f);
        
        // 기본 애니메이션들
        PlayRandomBasicAnimation();
        yield return new WaitForSeconds(3f);
        
        // UI 애니메이션들
        PlayRandomUIAnimation();
        yield return new WaitForSeconds(3f);
        
        // 시퀀스 데모
        PlayRandomSequence();
        yield return new WaitForSeconds(5f);
        
        // 카메라 효과
        PlayRandomCameraEffect();
        yield return new WaitForSeconds(3f);
        
        // 게임 효과
        PlayRandomGameEffect();
        yield return new WaitForSeconds(3f);
        
        // 리셋
        ResetAllComponents();
        
        Debug.Log("자동 데모 모드 완료!");
    }
    
    void OnDestroy()
    {
        // DOTween 정리
        DOTween.KillAll();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            // 앱이 일시정지될 때 DOTween도 일시정지
            DOTween.PauseAll();
        }
        else
        {
            // 앱이 재개될 때 DOTween도 재개
            DOTween.PlayAll();
        }
    }
} 