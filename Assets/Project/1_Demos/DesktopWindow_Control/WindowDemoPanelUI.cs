using UnityEngine;

/// <summary>
/// 데모 디버그 패널
/// - 각 창 기능(투명·항상위·클릭스루·정보바·도킹)을 런타임에 On/Off 하며 효과를 검증.
/// - uGUI Button/Toggle의 OnClick 이벤트에 아래 public 메서드를 연결해 사용한다.
/// - 빌드에서만 실제 창이 반응하므로, 에디터에서는 토글 상태만 바뀐다.
/// </summary>
public class WindowDemoPanelUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WindowController windowController; // 창 제어 본체
    [SerializeField] private InfoBarToggleUI  infoBar;          // 정보바 토글

    [Header("Dock Settings - 작업표시줄 도킹 시 창 크기")]
    [SerializeField] private int dockWidth  = 800; // 도킹 시 가로
    [SerializeField] private int dockHeight = 200; // 도킹 시 세로

    // ─── 버튼 연결용 public 메서드 ───

    public void OnToggleTransparent(bool enable)
    {
        if (windowController != null)
            windowController.SetTransparent(enable);
    }

    public void OnToggleTopmost(bool enable)
    {
        if (windowController != null)
            windowController.SetTopmost(enable);
    }

    public void OnToggleClickThrough(bool enable)
    {
        if (windowController != null)
            windowController.SetClickThrough(enable);
    }

    public void OnToggleInfoBar()
    {
        if (infoBar != null)
            infoBar.ToggleInfoBar();
    }

    public void OnDockToTaskbar()
    {
        if (windowController != null)
            windowController.DockToTaskbar(dockWidth, dockHeight);
    }

    public void OnToggleDynamicClickThrough(bool enable)
    {
        if (windowController != null)
            windowController.SetDynamicClickThrough(enable);
    }
}
