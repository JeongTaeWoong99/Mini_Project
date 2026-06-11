# Desktop Window Control (Task Bar Hero 스타일)

## 목차
- [📋 개요](#-개요)
- [🎮 참고 게임 분석 : TBH Task Bar Hero](#-참고-게임-분석--tbh-task-bar-hero)
- [🧩 구현한 창 기능](#-구현한-창-기능)
- [🔧 Win32 / DWM 호출 매핑](#-win32--dwm-호출-매핑)
- [🛠️ Unity 에디터 세팅 (수동)](#️-unity-에디터-세팅-수동)
- [🚀 빌드 및 테스트 방법](#-빌드-및-테스트-방법)
- [🐞 검은 배경 버그 트러블슈팅](#-검은-배경-버그-트러블슈팅)

---

## 📋 개요

**기간 :** 2026.06.11 ~

바탕화면 위에서 **타이틀바 없는 투명 창**으로 동작하는 데스크톱 앱의 핵심 기능들을 검증하는 데모입니다.

최근 출시된 **TBH : Task Bar Hero** (Nugem Studio, Unity 제작)가 창모드로 바탕화면 위에서 방치형 게임을 진행하는 컨셉이 신기하여, **게임 로직은 제외하고 "창 관련 기능"만** 독립적으로 구현·검증해 보았습니다.

> ⚠️ **중요 :** 투명창·클릭스루·항상위·창드래그는 **Win32 API 호출**이라 **에디터 Play 모드에서는 동작하지 않습니다.** 반드시 **Windows 빌드(.exe)** 로 실행해서 확인해야 합니다.

---

## 🎮 참고 게임 분석 : TBH Task Bar Hero

| 항목 | 내용 |
|------|------|
| 개발사 | Nugem Studio / Tesseract Studio |
| 출시 | 2026.05.27 |
| 엔진 | Unity |
| 장르 | 데스크톱 컴패니언 방치형 RPG |

### 🔧 관찰된 핵심 기능
- 🔸 **타이틀바 없는 투명 창** : 화면 위에 떠 있지만 정보바·테두리가 없음
- 🔸 **바탕화면 비침** : 창의 빈 영역으로 뒤 바탕화면이 그대로 보임
- 🔸 **클릭 통과** : 빈 영역 클릭은 바탕화면으로 전달되어 게임이 작업을 방해하지 않음
- 🔸 **항상 위** : 다른 창을 띄워도 항상 최상단에 표시
- 🔸 **검은 배경 버그** : URP의 알파 출력 문제로 투명 대신 검은 화면이 나오는 사례가 보고됨 (본 데모의 트러블슈팅 항목 참조)

---

## 🧩 구현한 창 기능

이 데모의 핵심 스크립트는 다음 4개입니다.

| 파일 | 역할 |
|------|------|
| `Win32Native.cs` | Win32 / DWM P/Invoke 선언 · 상수 · 구조체 모음 |
| `WindowController.cs` | 창 기능 제어 본체 (투명 · 보더리스 · 항상위 · 클릭스루 · 드래그 · 위치/크기) |
| `InfoBarToggleUI.cs` | 정보바 토글 + 정보바를 잡고 OS 창 드래그 |
| `WindowDemoPanelUI.cs` | 각 기능을 런타임에 On/Off 하는 디버그 패널 |

### 기능 목록
1. ✅ **투명 배경** : DWM 프레임을 창 전체로 확장 + 카메라 알파 0 → 바탕화면 비침
2. ✅ **보더리스** : 타이틀바·테두리 제거 (`WS_POPUP`)
3. ✅ **항상 위** : 다른 창 위에 고정 (`HWND_TOPMOST`)
4. ✅ **클릭 스루** : 빈 영역 클릭을 바탕화면으로 통과, 콘텐츠 위에선 클릭 받기 (동적 토글)
5. ✅ **정보바 토글** : 평소엔 꺼진 상태, 켜면 상단 바 표시
6. ✅ **창 드래그** : 정보바를 켠 상태에서 바를 잡고 OS 창 자체를 이동
7. ✅ **위치/크기 지정** : 작업표시줄 부근 도킹 등

---

## 🔧 Win32 / DWM 호출 매핑

| 기능 | 핵심 호출 | 비고 |
|------|-----------|------|
| 창 핸들 확보 | `GetActiveWindow()` | 시작 시 1회 |
| 보더리스 | `SetWindowLong(GWL_STYLE, WS_POPUP \| WS_VISIBLE)` | 타이틀바 제거 |
| 투명 배경 | `DwmExtendFrameIntoClientArea(MARGINS = -1)` | 프레임을 창 전체로 확장 |
| 항상 위 | `SetWindowPos(HWND_TOPMOST, ...)` | NOTOPMOST로 해제 |
| 클릭 스루 ON | `SetWindowLong(GWL_EXSTYLE, WS_EX_LAYERED \| WS_EX_TRANSPARENT)` + `SetLayeredWindowAttributes` | 입력 통과 |
| 클릭 스루 OFF | `WS_EX_TRANSPARENT` 플래그 제거 | 다시 클릭 받음 |
| 창 드래그 | `ReleaseCapture()` + `SendMessage(WM_SYSCOMMAND, 0xF012, 0)` | OS에 이동 위임 |
| 위치/크기 | `SetWindowPos(x, y, w, h)` | 도킹/리사이즈 |

### 동적 클릭 스루 원리
```csharp
// 마우스가 UI 또는 2D 스프라이트(콜라이더) 위면 콘텐츠로 간주 → 클릭 받기
// 빈 영역이면 클릭 스루 ON → 바탕화면으로 통과
bool pointerOverContent = forceInteractive || IsPointerOverContent();
SetClickThrough(!pointerOverContent);
```
- `EventSystem.IsPointerOverGameObject()` : uGUI UI 판정
- `Physics2D.OverlapPoint()` : 2D 스프라이트 콜라이더 판정
- 정보바처럼 항상 클릭을 받아야 하는 UI는 `SetForceInteractive(true)`로 예외 처리

---

## 🛠️ Unity 에디터 세팅 (수동)

스크립트만으로는 동작하지 않으며, 아래 씬·프로젝트 설정이 필요합니다.

### 1️⃣ 씬 구성 (`DesktopWindow_Control.unity`)
- 🔸 빈 GameObject `WindowManager` 생성 → `WindowController` 부착
- 🔸 `Main Camera` → Background Type = **Solid Color**, 색상 = **(0, 0, 0, 0)** (알파 0)
- 🔸 Canvas 생성 후 :
  - **정보바 Panel** (상단) → `InfoBarToggleUI` 부착, `infoBarRoot`에 바 본체 연결
  - **디버그 패널** (Toggle/Button 모음) → `WindowDemoPanelUI` 부착, 각 버튼 OnClick에 메서드 연결

### 2️⃣ Player Settings
- 🔸 **Resolution & Presentation** → Fullscreen Mode = **Windowed**
- 🔸 **Other Settings** → **Use Flip Model Swapchain 체크 해제** ⚠️ *(알파 보존 핵심)*
- 🔸 **Other Settings** → Graphics APIs = **Direct3D11** 우선 (투명창 안정성 ↑)

### 3️⃣ URP 점검
- 🔸 알파를 덮어쓰는 후처리(불투명 배경 처리)가 있으면 데모 씬에선 최소화
- 🔸 필요 시 데모 전용 URP Renderer를 만들어 후처리 OFF

---

## 🚀 빌드 및 테스트 방법

1. **File → Build Settings** → `DesktopWindow_Control.unity` 씬만 추가
2. Platform = **Windows / x64** 로 Build
3. 생성된 `.exe` 실행 후 아래 항목 확인 :

| # | 검증 항목 | 기대 결과 |
|---|-----------|-----------|
| 1 | 투명 / 보더리스 | 타이틀바·테두리 없이 뜨고 바탕화면이 비침 |
| 2 | 항상 위 | 브라우저 등 다른 창을 띄워도 항상 위에 표시 |
| 3 | 클릭 스루 | 빈 영역 클릭 → 바탕화면 아이콘 반응 / 콘텐츠 위 클릭 → 데모에 먹힘 |
| 4 | 정보바 토글 | 버튼으로 정보바 On/Off |
| 5 | 창 드래그 | 정보바 켠 뒤 바를 잡고 끌면 OS 창이 따라 이동 |
| 6 | 도킹 | 작업표시줄 부근으로 창 이동/리사이즈 |

---

## 🐞 검은 배경 버그 트러블슈팅

투명 대신 **검은 화면**이 나온다면 아래를 순서대로 점검합니다.

1. 🔸 **Use Flip Model Swapchain** 체크가 해제되어 있는가? *(가장 흔한 원인)*
2. 🔸 Main Camera Background 알파가 **0**인가?
3. 🔸 Graphics API가 **Direct3D11**인가? (`-force-d3d11` 실행 인자로도 강제 가능)
4. 🔸 URP 후처리가 알파를 1로 덮어쓰고 있지 않은가?
5. 🔸 `DwmExtendFrameIntoClientArea`의 MARGINS가 **-1**(전체 확장)로 설정되었는가?

> 💡 **팁 :** Task Bar Hero에서도 동일 증상이 보고되었으며, 공식 안내는 "호환성 탭에서 전체 화면 최적화 사용 안 함 체크"였습니다. 근본 원인은 Flip Model Swapchain의 알파 처리이므로, 빌드 단계에서 위 설정을 맞추는 것이 정석입니다.
