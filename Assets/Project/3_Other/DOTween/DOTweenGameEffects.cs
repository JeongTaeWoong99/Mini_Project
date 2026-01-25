using UnityEngine;
using DG.Tweening;
using System.Collections;

public class DOTweenGameEffects : MonoBehaviour
{
    [Header("효과 대상들")]
    [SerializeField] private Transform[]      gameObjects;
    [SerializeField] private SpriteRenderer[] spriteRenderers;
    [SerializeField] private MeshRenderer[]   meshRenderers;
    
    [Header("등장/퇴장 효과 설정")]
    [SerializeField] private float   appearDuration = 1f;
    [SerializeField] private float   disappearDuration = 0.5f;
    [SerializeField] private Vector3 appearFromScale = Vector3.zero;
    [SerializeField] private Vector3 appearFromPosition = Vector3.down * 5f;
    
    [Header("수집/획득 효과 설정")]
    [SerializeField] private Transform collectTarget; // 수집될 위치 (예: 플레이어 위치)
    [SerializeField] private float     collectDuration = 1f;
    [SerializeField] private float     collectPunchScale = 1.2f;
    
    [Header("데미지 효과 설정")]
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private float damageDuration = 0.2f;
    [SerializeField] private float damageShakeStrength = 0.3f;
    
    [Header("반복 효과 설정")]
    [SerializeField] private float idleBobHeight = 0.5f;
    [SerializeField] private float idleBobSpeed = 2f;
    [SerializeField] private float rotationSpeed = 30f;
    
    private Vector3[] originalPositions;
    private Vector3[] originalScales;
    private Color[]   originalColors;
    private bool[]    isEffectActive;
    
    void Start()
    {
        InitializeOriginalValues();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            PlayAppearEffect();
        
        if (Input.GetKeyDown(KeyCode.F2))
            PlayDisappearEffect();
        
        if (Input.GetKeyDown(KeyCode.F3))
            PlayCollectEffect();
        
        if (Input.GetKeyDown(KeyCode.F4))
            PlayDamageEffect();
        
        if (Input.GetKeyDown(KeyCode.F5))
            ToggleIdleAnimation();
        
        if (Input.GetKeyDown(KeyCode.F6))
            PlayPopupEffect();
        
        if (Input.GetKeyDown(KeyCode.F7))
            PlayBounceEffect();
        
        if (Input.GetKeyDown(KeyCode.F8))
            PlayFadeInOut();
        
        if (Input.GetKeyDown(KeyCode.F9))
            PlayItemSpawnEffect();
        
        if (Input.GetKeyDown(KeyCode.F10))
            ResetAllEffects();
    }
    
    private void InitializeOriginalValues()
    {
        if (gameObjects == null || gameObjects.Length == 0) return;
        
        originalPositions = new Vector3[gameObjects.Length];
        originalScales = new Vector3[gameObjects.Length];
        originalColors = new Color[gameObjects.Length];
        isEffectActive = new bool[gameObjects.Length];
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] != null)
            {
                originalPositions[i] = gameObjects[i].position;
                originalScales[i] = gameObjects[i].localScale;
                
                // 색상 정보 저장
                if (spriteRenderers != null && i < spriteRenderers.Length && spriteRenderers[i] != null)
                {
                    originalColors[i] = spriteRenderers[i].color;
                }
                else if (meshRenderers != null && i < meshRenderers.Length && meshRenderers[i] != null)
                {
                    originalColors[i] = meshRenderers[i].material.color;
                }
            }
        }
    }
    
    public void PlayAppearEffect()
    {
        if (gameObjects == null) return;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            
            // 초기 상태 설정
            gameObjects[i].localScale = appearFromScale;
            gameObjects[i].position = originalPositions[i] + appearFromPosition;
            
            // 나타나는 애니메이션
            gameObjects[i].DOScale(originalScales[i], appearDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(i * 0.1f); // 순차적으로 나타나기
            
            gameObjects[i].DOMove(originalPositions[i], appearDuration)
                .SetEase(Ease.OutBounce)
                .SetDelay(i * 0.1f);
        }
        
        Debug.Log("등장 효과 재생!");
    }
    
    public void PlayDisappearEffect()
    {
        if (gameObjects == null) return;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            
            // 사라지는 애니메이션
            gameObjects[i].DOScale(Vector3.zero, disappearDuration)
                .SetEase(Ease.InBack)
                .SetDelay(i * 0.05f);
            
            gameObjects[i].DOMoveY(originalPositions[i].y + 2f, disappearDuration)
                .SetEase(Ease.InQuad)
                .SetDelay(i * 0.05f);
        }
        
        Debug.Log("퇴장 효과 재생!");
    }
    
    public void PlayCollectEffect()
    {
        if (gameObjects == null || collectTarget == null) return;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            
            Sequence collectSequence = DOTween.Sequence();
            
            // 1단계: 펀치 스케일 효과
            collectSequence.Append(gameObjects[i].DOPunchScale(Vector3.one * collectPunchScale, 0.3f));
            
            // 2단계: 타겟으로 이동하면서 축소
            collectSequence.Append(gameObjects[i].DOMove(collectTarget.position, collectDuration)
                .SetEase(Ease.InQuad));
            
            collectSequence.Join(gameObjects[i].DOScale(Vector3.zero, collectDuration)
                .SetEase(Ease.InQuad));
            
            // 3단계: 수집 완료 후 원래 위치로 리셋 (옵션)
            collectSequence.OnComplete(() => {
                gameObjects[i].position = originalPositions[i];
                gameObjects[i].localScale = originalScales[i];
            });
            
            collectSequence.SetDelay(i * 0.1f);
            collectSequence.Play();
        }
        
        Debug.Log("수집 효과 재생!");
    }
    
    public void PlayDamageEffect()
    {
        if (gameObjects == null) return;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            
            // 흔들기 + 색상 변화
            gameObjects[i].DOShakePosition(damageDuration, damageShakeStrength, 10, 90, false, true);
            
            // 색상 변화 효과
            if (spriteRenderers != null && i < spriteRenderers.Length && spriteRenderers[i] != null)
            {
                spriteRenderers[i].DOColor(damageColor, damageDuration / 2)
                    .SetLoops(2, LoopType.Yoyo);
            }
            else if (meshRenderers != null && i < meshRenderers.Length && meshRenderers[i] != null)
            {
                meshRenderers[i].material.DOColor(damageColor, damageDuration / 2)
                    .SetLoops(2, LoopType.Yoyo);
            }
        }
        
        Debug.Log("데미지 효과 재생!");
    }
    
    public void ToggleIdleAnimation()
    {
        if (gameObjects == null) return;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            
            if (isEffectActive[i])
            {
                // 아이들 애니메이션 중지
                gameObjects[i].DOKill();
                gameObjects[i].DOMove(originalPositions[i], 0.5f);
                gameObjects[i].DORotate(Vector3.zero, 0.5f);
                isEffectActive[i] = false;
            }
            else
            {
                // 아이들 애니메이션 시작
                // 위아래로 부드럽게 움직이기
                gameObjects[i].DOMoveY(originalPositions[i].y + idleBobHeight, 1f / idleBobSpeed)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
                
                // 천천히 회전하기
                gameObjects[i].DORotate(new Vector3(0, 360, 0), 360f / rotationSpeed, RotateMode.LocalAxisAdd)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1);
                
                isEffectActive[i] = true;
            }
        }
        
        Debug.Log($"아이들 애니메이션 {(isEffectActive[0] ? "시작" : "중지")}!");
    }
    
    public void PlayPopupEffect()
    {
        if (gameObjects == null) return;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            
            // 팝업 효과 (빠르게 커졌다가 원래 크기로)
            gameObjects[i].DOPunchScale(Vector3.one * 0.3f, 0.5f, 1, 0.5f)
                .SetDelay(i * 0.1f);
        }
        
        Debug.Log("팝업 효과 재생!");
    }
    
    public void PlayBounceEffect()
    {
        if (gameObjects == null) return;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            
            // 탄성 점프 효과
            gameObjects[i].DOPunchPosition(Vector3.up * 2f, 1f, 3, 0.5f)
                .SetEase(Ease.OutBounce)
                .SetDelay(i * 0.15f);
        }
        
        Debug.Log("바운스 효과 재생!");
    }
    
    public void PlayFadeInOut()
    {
        if (gameObjects == null) return;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            
            // 페이드 효과
            if (spriteRenderers != null && i < spriteRenderers.Length && spriteRenderers[i] != null)
            {
                spriteRenderers[i].DOFade(0f, 0.5f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetDelay(i * 0.1f);
            }
            else if (meshRenderers != null && i < meshRenderers.Length && meshRenderers[i] != null)
            {
                meshRenderers[i].material.DOFade(0f, 0.5f)
                    .SetLoops(2, LoopType.Yoyo)
                    .SetDelay(i * 0.1f);
            }
        }
        
        Debug.Log("페이드 효과 재생!");
    }
    
    public void PlayItemSpawnEffect()
    {
        if (gameObjects == null) return;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            
            // 아이템 생성 효과 (땅에서 튀어나오는 느낌)
            Vector3 spawnPos = originalPositions[i] + Vector3.down * 3f;
            gameObjects[i].position = spawnPos;
            gameObjects[i].localScale = Vector3.zero;
            
            Sequence spawnSequence = DOTween.Sequence();
            
            // 위로 튀어오르면서 크기 증가
            spawnSequence.Append(gameObjects[i].DOMove(originalPositions[i] + Vector3.up * 1f, 0.3f)
                .SetEase(Ease.OutQuad));
            
            spawnSequence.Join(gameObjects[i].DOScale(originalScales[i] * 1.2f, 0.3f)
                .SetEase(Ease.OutBack));
            
            // 원래 위치로 떨어지면서 원래 크기로
            spawnSequence.Append(gameObjects[i].DOMove(originalPositions[i], 0.2f)
                .SetEase(Ease.InQuad));
            
            spawnSequence.Join(gameObjects[i].DOScale(originalScales[i], 0.2f)
                .SetEase(Ease.OutBounce));
            
            spawnSequence.SetDelay(i * 0.2f);
            spawnSequence.Play();
        }
        
        Debug.Log("아이템 생성 효과 재생!");
    }
    
    public void ResetAllEffects()
    {
        if (gameObjects == null) return;
        
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] == null) continue;
            
            // 모든 트윈 중지
            gameObjects[i].DOKill();
            
            // 원래 상태로 복원
            gameObjects[i].DOMove(originalPositions[i], 0.5f);
            gameObjects[i].DOScale(originalScales[i], 0.5f);
            gameObjects[i].DORotate(Vector3.zero, 0.5f);
            
            // 색상 복원
            if (spriteRenderers != null && i < spriteRenderers.Length && spriteRenderers[i] != null)
            {
                spriteRenderers[i].DOColor(originalColors[i], 0.5f);
            }
            else if (meshRenderers != null && i < meshRenderers.Length && meshRenderers[i] != null)
            {
                meshRenderers[i].material.DOColor(originalColors[i], 0.5f);
            }
            
            isEffectActive[i] = false;
        }
        
        Debug.Log("모든 효과 리셋!");
    }
    
    void OnDestroy()
    {
        // 메모리 누수 방지
        DOTween.Kill(this);
    }
} 