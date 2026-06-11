using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 데스크톱 창 제어 핵심 클래스 (Task Bar Hero 스타일)
/// - 투명 배경, 보더리스, 항상 위, 클릭 스루, OS 창 드래그, 위치/크기 지정을 담당.
/// - 모든 Win32 호출은 #if !UNITY_EDITOR 로 감싼다 (에디터에서 메인 윈도우 핸들을
///   건드리면 에디터가 불안정해지므로 빌드(.exe)에서만 동작).
/// </summary>
public class WindowController : MonoBehaviour
{
    [Header("Window Settings - 시작 시 적용할 창 상태")]
    [SerializeField] private bool startTransparent  = true;  // 시작 시 투명 배경
    [SerializeField] private bool startBorderless   = true;  // 시작 시 보더리스(타이틀바 제거)
    [SerializeField] private bool startTopmost      = true;  // 시작 시 항상 위
    [SerializeField] private bool startClickThrough = true;  // 시작 시 클릭 스루 활성

    [Header("Click-Through Settings - 동적 클릭 통과")]
    [SerializeField] private bool   dynamicClickThrough = true; // 마우스 위치에 따라 자동 토글
    [SerializeField] private Camera raycastCamera;              // 스프라이트 판정용 카메라(미지정 시 Camera.main)

    // ─── Private variables ───
    private IntPtr hWnd            = IntPtr.Zero; // 현재 창 핸들
    private bool   isClickThrough  = false;       // 현재 클릭 스루 상태
    private bool   isTopmost       = false;       // 현재 항상 위 상태
    private bool   initialized     = false;       // 초기화 완료 여부

    // 정보바 등 "항상 클릭을 받아야 하는" UI가 마우스 아래에 있는지 외부에서 강제 지정
    private bool forceInteractive = false;

    void Start()
    {
        if (raycastCamera == null)
            raycastCamera = Camera.main;

        InitializeWindow();
    }

    /// <summary>창 핸들 확보 후 시작 상태(보더리스/투명/항상위/클릭스루)를 적용한다.</summary>
    private void InitializeWindow()
    {
#if !UNITY_EDITOR
        hWnd = Win32Native.GetActiveWindow();

        if (startBorderless)
            ApplyBorderless();

        if (startTransparent)
            SetTransparent(true);

        SetTopmost(startTopmost);
        SetClickThrough(startClickThrough);
#endif
        initialized = true;
    }

    void Update()
    {
        // 동적 클릭 스루 : 마우스가 UI/스프라이트 위면 클릭 받기, 빈 영역이면 통과
        if (!dynamicClickThrough || !initialized)
            return;

        bool pointerOverContent = forceInteractive || IsPointerOverContent();
        SetClickThrough(!pointerOverContent);
    }

    // ──────────────────────────────────────────────
    // 1. 보더리스 (타이틀바 / 테두리 제거)
    // ──────────────────────────────────────────────
    private void ApplyBorderless()
    {
#if !UNITY_EDITOR
        Win32Native.SetWindowLong(hWnd, Win32Native.GWL_STYLE, Win32Native.WS_POPUP | Win32Native.WS_VISIBLE);
#endif
    }

    // ──────────────────────────────────────────────
    // 2. 투명 배경 (DWM 프레임을 클라이언트 영역 전체로 확장)
    // ──────────────────────────────────────────────
    public void SetTransparent(bool enable)
    {
#if !UNITY_EDITOR
        // margin -1 : 프레임을 창 전체로 확장 → 카메라 알파 0 영역이 그대로 비침
        Win32Native.MARGINS margins = new Win32Native.MARGINS
        {
            leftWidth    = enable ? -1 : 0,
            rightWidth   = enable ? -1 : 0,
            topHeight    = enable ? -1 : 0,
            bottomHeight = enable ? -1 : 0
        };
        Win32Native.DwmExtendFrameIntoClientArea(hWnd, ref margins);
#endif
    }

    // ──────────────────────────────────────────────
    // 3. 항상 위 (Always On Top)
    // ──────────────────────────────────────────────
    public void SetTopmost(bool enable)
    {
        isTopmost = enable;
#if !UNITY_EDITOR
        IntPtr insertAfter = enable ? Win32Native.HWND_TOPMOST : Win32Native.HWND_NOTOPMOST;
        uint   flags       = Win32Native.SWP_NOMOVE | Win32Native.SWP_NOSIZE | Win32Native.SWP_SHOWWINDOW;
        Win32Native.SetWindowPos(hWnd, insertAfter, 0, 0, 0, 0, flags);
#endif
    }

    // ──────────────────────────────────────────────
    // 4. 클릭 스루 (빈 영역 클릭을 뒤 창으로 통과)
    // ──────────────────────────────────────────────
    public void SetClickThrough(bool enable)
    {
        if (enable == isClickThrough)
            return; // 상태가 같으면 불필요한 호출 생략

        isClickThrough = enable;
#if !UNITY_EDITOR
        uint exStyle = Win32Native.GetWindowLong(hWnd, Win32Native.GWL_EXSTYLE);

        if (enable)
        {
            // 레이어드 + 투명 플래그 추가 → 입력이 뒤 창으로 통과
            exStyle |= Win32Native.WS_EX_LAYERED | Win32Native.WS_EX_TRANSPARENT;
            Win32Native.SetWindowLong(hWnd, Win32Native.GWL_EXSTYLE, exStyle);
            Win32Native.SetLayeredWindowAttributes(hWnd, 0, 255, Win32Native.LWA_ALPHA);
        }
        else
        {
            // 투명 플래그만 제거 → 창이 다시 클릭을 받음 (레이어드는 유지)
            exStyle &= ~Win32Native.WS_EX_TRANSPARENT;
            Win32Native.SetWindowLong(hWnd, Win32Native.GWL_EXSTYLE, exStyle);
        }
#endif
    }

    // ──────────────────────────────────────────────
    // 5. OS 창 드래그 (정보바를 잡고 끌 때 호출)
    // ──────────────────────────────────────────────
    public void StartWindowDrag()
    {
#if !UNITY_EDITOR
        // 마우스 캡처를 풀고, 타이틀바를 잡은 것처럼 시스템에 위임 → OS가 창을 끌어줌
        Win32Native.ReleaseCapture();
        Win32Native.SendMessage(hWnd, Win32Native.WM_SYSCOMMAND, Win32Native.SC_MOVE_HTCAPTION, 0);
#endif
    }

    // ──────────────────────────────────────────────
    // 6. 창 위치 / 크기 지정 (작업표시줄 부근 도킹 등)
    // ──────────────────────────────────────────────
    public void MoveWindow(int x, int y)
    {
#if !UNITY_EDITOR
        uint flags = Win32Native.SWP_NOSIZE | Win32Native.SWP_NOACTIVATE | Win32Native.SWP_SHOWWINDOW;
        IntPtr after = isTopmost ? Win32Native.HWND_TOPMOST : Win32Native.HWND_NOTOPMOST;
        Win32Native.SetWindowPos(hWnd, after, x, y, 0, 0, flags);
#endif
    }

    public void ResizeWindow(int width, int height)
    {
#if !UNITY_EDITOR
        uint flags = Win32Native.SWP_NOMOVE | Win32Native.SWP_NOACTIVATE | Win32Native.SWP_SHOWWINDOW;
        IntPtr after = isTopmost ? Win32Native.HWND_TOPMOST : Win32Native.HWND_NOTOPMOST;
        Win32Native.SetWindowPos(hWnd, after, 0, 0, width, height, flags);
#endif
    }

    /// <summary>작업표시줄 바로 위(화면 하단 중앙)로 창을 이동시킨다.</summary>
    public void DockToTaskbar(int width, int height)
    {
#if !UNITY_EDITOR
        int x = (Screen.currentResolution.width  - width)  / 2;
        int y =  Screen.currentResolution.height - height - 48; // 작업표시줄 높이만큼 위
        ResizeWindow(width, height);
        MoveWindow(x, y);
#endif
    }

    // ──────────────────────────────────────────────
    // 마우스가 콘텐츠(UI / 스프라이트) 위에 있는지 판정
    // ★ 클릭 스루 ON 상태에선 OS가 마우스 입력을 창에 안 보내므로,
    //   Unity의 Input.mousePosition / IsPointerOverGameObject 는 못 쓴다.
    //   → Win32 GetCursorPos 로 전역 커서를 직접 폴링해 판정한다.
    // ──────────────────────────────────────────────
    private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();

    private bool IsPointerOverContent()
    {
        Vector2 screenPos = GetCursorScreenPosition();

        // 1) uGUI UI 위에 있으면 콘텐츠로 간주 (커서 위치를 직접 넣어 수동 레이캐스트)
        if (EventSystem.current != null)
        {
            PointerEventData pointer = new PointerEventData(EventSystem.current) { position = screenPos };
            raycastResults.Clear();
            EventSystem.current.RaycastAll(pointer, raycastResults);
            if (raycastResults.Count > 0)
                return true;
        }

        // 2) 2D 스프라이트(콜라이더) 위에 있으면 콘텐츠로 간주
        if (raycastCamera != null)
        {
            Vector3   worldPoint = raycastCamera.ScreenToWorldPoint(screenPos);
            Collider2D hit        = Physics2D.OverlapPoint(worldPoint);
            if (hit != null)
                return true;
        }

        return false;
    }

    /// <summary>
    /// 전역 커서(데스크톱) 좌표를 Unity 화면 좌표(좌하단 0,0)로 변환해 반환한다.
    /// 클릭 스루 ON 상태에서도 마우스 위치를 얻기 위함.
    /// </summary>
    private Vector2 GetCursorScreenPosition()
    {
#if !UNITY_EDITOR
        if (hWnd != IntPtr.Zero
            && Win32Native.GetCursorPos(out Win32Native.POINT cursor)
            && Win32Native.GetWindowRect(hWnd, out Win32Native.RECT rect))
        {
            // 데스크톱 좌표 → 창 클라이언트 좌표 (보더리스 popup이라 client ≈ window)
            float localX = cursor.x - rect.left;
            float localY = cursor.y - rect.top;
            // Y축 뒤집기 (Win32는 위가 0, Unity는 아래가 0)
            return new Vector2(localX, Screen.height - localY);
        }
#endif
        return Input.mousePosition; // 에디터 / 폴백
    }

    /// <summary>정보바 등 특정 UI가 마우스 아래일 때 외부에서 강제로 클릭을 받게 한다.</summary>
    public void SetForceInteractive(bool value)
    {
        forceInteractive = value;
    }

    /// <summary>동적 클릭 스루(마우스 위치 자동 판정)를 켜고 끈다.</summary>
    public void SetDynamicClickThrough(bool value)
    {
        dynamicClickThrough = value;
    }

    // 외부(디버그 패널)에서 현재 상태를 읽기 위한 프로퍼티
    public bool IsClickThrough => isClickThrough;
    public bool IsTopmost      => isTopmost;
}
