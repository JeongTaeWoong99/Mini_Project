using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
// [실험] 유니티에서 Async/Await 실행 순서 실험
// ============================================================
//
// [이 코드가 무슨 실험인가?]
// 이 스크립트는 async/await가 유니티 생명주기 함수
// (Awake → Start)의 실행 순서에 어떤 영향을 미치는지
// 시각적으로 보여주는 실험 코드입니다.
//
// [왜 Awake의 나머지 코드보다 Start가 먼저 실행되는가?]
// 'await' 키워드를 만나는 순간, 현재 메서드는 실행을 일시 중단하고
// 제어권을 호출자(유니티 메인 스레드)에게 반환합니다.
// 유니티는 Awake가 끝난 것으로 간주하고,
// 대기 중인 작업이 완료되길 기다리지 않고 곧바로 Start를 실행합니다.
// 이후 비동기 작업이 완료되면, Awake는 await 이후 지점부터 재개됩니다.
//
//   실행 흐름:
//   Awake()  → 실행되다가 'await' 도달
//              → 일시 중단, 제어권을 유니티에 반환
//   Start()  → Awake 완료를 기다리지 않고 즉시 실행
//   --- 3초 후 ---
//   SumAsync() → 비동기 작업 완료
//   Awake()  → await 이후 지점부터 재개, 최종 결과 출력
//
// [왜 유니티 생명주기 순서를 무시하는 것처럼 보이는가?]
// 실제로 유니티의 생명주기 순서(Awake → Start)는 여전히 지켜집니다.
// 두 메서드 모두 올바른 순서로 호출됩니다.
// 단, 'async void'는 반환값이 없기 때문에 유니티가 완료를 기다릴 수 없습니다.
// 유니티는 Awake가 첫 번째 'await'를 만나는 순간 "완료"로 판단하고
// Start로 넘어갑니다.
// 이것은 버그가 아니라 async/await의 의도된 동작입니다.
//
// [핵심 정리]
// 생명주기 함수(Awake, Start)에서 async void를 사용하면
// 예상치 못한 실행 순서 문제가 발생할 수 있습니다.
// 유니티에서 더 안전한 비동기 처리를 위해서는
// UniTask 또는 코루틴 사용을 권장합니다.
// ============================================================

public class AsyncAwaitTest : MonoBehaviour
{
    [Header("UI")]
    public Button startButton;
    public TextMeshProUGUI logText;

    [Header("3D Object")]
    public Renderer cubeRenderer;

    private Color colorAwakeStart  = Color.yellow;
    private Color colorWaiting     = Color.gray;
    private Color colorSumAsync    = Color.cyan;
    private Color colorAwakeEnd    = Color.green;
    private Color colorStart       = Color.blue;

    void Awake()
    {
        logText.text = "";
        startButton.onClick.AddListener(OnStartButtonClicked);
        SetCubeColor(Color.white);
    }

    void OnStartButtonClicked()
    {
        startButton.interactable = false;
        logText.text = "";
        RunAsync();
    }

    async void RunAsync()
    {
        AddLog("[Awake] Main thread started");
        SetCubeColor(colorAwakeStart);

        int result = await SumAsync(10, 20);

        AddLog("[Awake] Async result: " + result);
        SetCubeColor(colorAwakeEnd);

        startButton.interactable = true;
    }

    void SimulatedStart()
    {
        AddLog("[Start] Start log output <- executed immediately after await");
        SetCubeColor(colorStart);
    }

    async Task<int> SumAsync(int num1, int num2)
    {
        SimulatedStart();

        AddLog("[SumAsync] Waiting for 3 seconds...");
        SetCubeColor(colorWaiting);

        await Task.Delay(3000);

        AddLog("[SumAsync] Task completed");
        SetCubeColor(colorSumAsync);

        return num1 + num2;
    }

    void AddLog(string message)
    {
        logText.text += message + "\n";
        Debug.Log(message);
    }

    void SetCubeColor(Color color)
    {
        if (cubeRenderer != null)
            cubeRenderer.material.color = color;
    }
}