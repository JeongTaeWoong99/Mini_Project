using System;
using System.Runtime.InteropServices;

/// <summary>
/// Win32 / DWM API P/Invoke 모음
/// - Task Bar Hero 스타일의 창 제어(투명·보더리스·항상위·클릭스루·드래그)에 필요한
///   네이티브 함수, 상수, 구조체를 한곳에 모아둔 클래스.
/// - 실제 호출은 WindowController에서 담당하며, 여기서는 선언만 한다.
/// </summary>
public static class Win32Native
{
    // ─── 윈도우 스타일 인덱스 (SetWindowLong nIndex) ───
    public const int GWL_STYLE   = -16; // 기본 윈도우 스타일
    public const int GWL_EXSTYLE = -20; // 확장 윈도우 스타일

    // ─── 기본 스타일 플래그 (GWL_STYLE) ───
    public const uint WS_POPUP   = 0x80000000; // 팝업 창 (타이틀바·테두리 없음)
    public const uint WS_VISIBLE = 0x10000000; // 창 보이기

    // ─── 확장 스타일 플래그 (GWL_EXSTYLE) ───
    public const uint WS_EX_LAYERED     = 0x00080000; // 레이어드 창 (투명도/색상키 지원)
    public const uint WS_EX_TRANSPARENT = 0x00000020; // 클릭 통과 (입력을 뒤 창으로 전달)

    // ─── SetWindowPos 용 hWndInsertAfter 값 ───
    public static readonly IntPtr HWND_TOPMOST   = new IntPtr(-1); // 항상 위
    public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2); // 항상 위 해제

    // ─── SetWindowPos uFlags ───
    public const uint SWP_NOSIZE     = 0x0001; // 크기 유지
    public const uint SWP_NOMOVE     = 0x0002; // 위치 유지
    public const uint SWP_NOACTIVATE = 0x0010; // 활성화 안 함
    public const uint SWP_SHOWWINDOW = 0x0040; // 창 표시

    // ─── 창 드래그용 메시지 ───
    public const int WM_SYSCOMMAND     = 0x0112; // 시스템 명령 메시지
    public const int SC_MOVE_HTCAPTION = 0xF012; // 타이틀바 드래그(SC_MOVE | HTCAPTION)

    // ─── SetLayeredWindowAttributes dwFlags ───
    public const uint LWA_ALPHA    = 0x00000002; // 알파값 적용
    public const uint LWA_COLORKEY = 0x00000001; // 색상키 적용

    /// <summary>창 여백 구조체 (DwmExtendFrameIntoClientArea 용)</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MARGINS
    {
        public int leftWidth;    // 왼쪽 여백
        public int rightWidth;   // 오른쪽 여백
        public int topHeight;    // 위쪽 여백
        public int bottomHeight; // 아래쪽 여백
    }

    /// <summary>창 사각 영역 구조체 (GetWindowRect 용)</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;   // 좌
        public int top;    // 상
        public int right;  // 우
        public int bottom; // 하
    }

    /// <summary>화면 좌표 구조체 (GetCursorPos 용)</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int x; // 데스크톱 X (좌상단 0, 오른쪽 +)
        public int y; // 데스크톱 Y (좌상단 0, 아래쪽 +)
    }

    // ─── user32.dll ───
    [DllImport("user32.dll")]
    public static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll")]
    public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    public static extern bool SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);

    [DllImport("user32.dll")]
    public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);

    // ─── dwmapi.dll ───
    [DllImport("dwmapi.dll")]
    public static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
}
