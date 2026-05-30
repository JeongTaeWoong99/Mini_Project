using UnityEngine;
using TMPro;
using System.Text;

/// <summary>
/// 유니티 텍스트 출력 방법 비교 데모
/// 각 방법의 내부 동작과 GC 할당 차이를 시각적으로 확인
/// </summary>
public class TextBoxingCompare : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TMP_Text text1_Concat;        // 방법 1 : 문자열 연결 (박싱 발생)
    [SerializeField] private TMP_Text text2_ToString;      // 방법 2 : ToString() 직접 호출
    [SerializeField] private TMP_Text text3_Cached;        // 방법 3 : 문자열 캐싱
    [SerializeField] private TMP_Text text4_SetText;       // 방법 4 : TMP SetText
    [SerializeField] private TMP_Text text5_StringBuilder; // 방법 5 : StringBuilder

    [Header("FPS Range Settings")]
    [SerializeField] private int minFPS = 30;  // 캐싱 최소 범위
    [SerializeField] private int maxFPS = 240; // 캐싱 최대 범위

    // 방법 3 : Start에서 미리 생성해 두는 문자열 캐시 배열
    private string[] _fpsCache;

    // 방법 5 : 매 프레임 재사용할 StringBuilder 인스턴스
    private StringBuilder _sb;

    private int   _currentFPS = 60;
    private float _timer      = 0f;

    void Start()
    {
        // 방법 3 : 범위 내 모든 문자열을 Start에서 한 번만 생성
        _fpsCache = new string[maxFPS + 1];
        for (int i = minFPS; i <= maxFPS; i++)
        {
            _fpsCache[i] = "FPS : " + i.ToString();
        }

        // 방법 5 : StringBuilder는 한 번만 new, 이후 Clear()로 재사용
        _sb = new StringBuilder(32);
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer < 0.5f) return;

        _timer      = 0f;
        _currentFPS = Random.Range(minFPS, maxFPS + 1);

        Method1_Concat(_currentFPS);
        Method2_ToString(_currentFPS);
        Method3_Cached(_currentFPS);
        Method4_SetText(_currentFPS);
        Method5_StringBuilder(_currentFPS);
    }

    // ─────────────────────────────────────────────────────────────
    // 방법 1 : 문자열 연결 (Boxing 발생)
    // IL : box System.Int32 → String.Concat(object, object)
    // 할당 : 박싱된 int 객체 1회 + 연결된 string 1회 = 2회
    // ─────────────────────────────────────────────────────────────
    private void Method1_Concat(int fps)
    {
        text1_Concat.text = "FPS : " + fps;
    }

    // ─────────────────────────────────────────────────────────────
    // 방법 2 : ToString() 직접 호출
    // IL : constrained. System.Int32 → callvirt ToString() (박싱 없음)
    //      → String.Concat(string, string)
    // 할당 : ToString() 결과 string 1회 + 연결된 string 1회 = 2회 (박싱 없음)
    // ─────────────────────────────────────────────────────────────
    private void Method2_ToString(int fps)
    {
        text2_ToString.text = "FPS : " + fps.ToString();
    }

    // ─────────────────────────────────────────────────────────────
    // 방법 3 : 문자열 캐싱
    // 런타임 변환 없음, 미리 만든 string 참조를 그대로 사용
    // 할당 : 0회 (범위 내에 있을 경우)
    // ─────────────────────────────────────────────────────────────
    private void Method3_Cached(int fps)
    {
        if (fps < minFPS || fps > maxFPS) return;
        text3_Cached.text = _fpsCache[fps];
    }

    // ─────────────────────────────────────────────────────────────
    // 방법 4 : TMP_Text.SetText()
    // TMP 내부 char[] 버퍼에 직접 숫자 → 문자 변환 (string 생성 없음)
    // 포맷 문자열은 리터럴이므로 추가 할당 없음
    // 할당 : 0회
    // ─────────────────────────────────────────────────────────────
    private void Method4_SetText(int fps)
    {
        text4_SetText.SetText("FPS : {0}", fps);
    }

    // ─────────────────────────────────────────────────────────────
    // 방법 5 : StringBuilder
    // Append(int) 오버로드 → 박싱 없이 내부 char[] 에 직접 기록
    // 단, sb.ToString() 시점에 최종 string 1회 할당 발생
    // 할당 : 1회 (ToString() 결과)
    // ─────────────────────────────────────────────────────────────
    private void Method5_StringBuilder(int fps)
    {
        _sb.Clear();
        _sb.Append("FPS : ");
        _sb.Append(fps);
        text5_StringBuilder.text = _sb.ToString();
    }
}
