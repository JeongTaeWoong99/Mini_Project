using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 정보바(타이틀바) 토글 + OS 창 드래그 트리거
/// - 평소엔 정보바가 꺼져 있어 순수 투명 창 상태.
/// - 정보바를 켜면 상단 바가 보이고, 그 바를 마우스로 누르면 OS 창 자체를 드래그 이동.
/// - 이 컴포넌트는 "정보바" UI 오브젝트(Image 등 RectTransform)에 붙인다.
/// - 정보바 위에 마우스가 있을 땐 클릭 스루가 풀려야 하므로 enter/exit를 함께 처리.
/// </summary>
public class InfoBarToggleUI : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private WindowController windowController; // 창 제어 본체
    [SerializeField] private GameObject       infoBarRoot;      // 정보바 UI 루트(토글 대상)

    [Header("State")]
    [SerializeField] private bool infoBarVisible = false; // 정보바 표시 여부(기본 꺼짐)

    void Start()
    {
        ApplyVisibility();
    }

    /// <summary>정보바 On/Off 토글 (디버그 패널 버튼 등에서 호출).</summary>
    public void ToggleInfoBar()
    {
        infoBarVisible = !infoBarVisible;
        ApplyVisibility();
    }

    public void SetInfoBar(bool visible)
    {
        infoBarVisible = visible;
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (infoBarRoot != null)
            infoBarRoot.SetActive(infoBarVisible);
    }

    // ─── 정보바를 눌러 드래그 시작 ───
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!infoBarVisible || windowController == null)
            return;

        windowController.StartWindowDrag();
    }

    // ─── 정보바 위에 마우스가 올라오면 클릭을 받도록 강제 ───
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (windowController != null)
            windowController.SetForceInteractive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (windowController != null)
            windowController.SetForceInteractive(false);
    }
}
