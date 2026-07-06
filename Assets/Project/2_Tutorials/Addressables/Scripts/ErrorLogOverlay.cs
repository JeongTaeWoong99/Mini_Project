using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ErrorLogOverlay
/// - 런타임/빌드에서 발생하는 로그·경고·에러(예외 포함)를 화면 위 오버레이로 수집·표시하는 진단용 컴포넌트.
/// - Application.logMessageReceivedThreaded 로 "모든 스레드"의 로그를 큐에 모은 뒤,
///   메인 스레드(Update)에서 주기적으로 TMP 텍스트에 반영한다(스레드 안전 처리).
/// - 옵션 : 연속 중복 로그 접기(x2·x3…), 로그 타입별 필터, 새 에러 발생 시 화면 토스트 알림.
/// - 에디터/개발 빌드에서는 핫키(기본 F1)로 오버레이를 숨김/표시할 수 있다.
/// - DontDestroyOnLoad 로 씬 전환에도 살아남아 계속 로그를 수집한다.
/// </summary>
[AddComponentMenu("Diagnostics/Error Log Overlay")]
public sealed class ErrorLogOverlay : MonoBehaviour
{
    [CenterHeader("UI References")]
    [SerializeField] private TextMeshProUGUI logText;    // 스크롤뷰 안의 본문 텍스트 (필수)
    [SerializeField] private ScrollRect      scrollRect; // 선택 : 로그 뷰의 ScrollRect. 넣으면 새 로그마다 자동으로 맨 아래로 스크롤 / 없으면 자동 스크롤만 안 됨(로그 표시는 정상)
    [SerializeField] private CanvasGroup     toastGroup; // 선택 : 토스트 패널의 CanvasGroup. 넣으면 에러 시 패널이 잠깐 떴다 페이드아웃 / 없으면 토스트 미표시(로그창엔 그대로 남음)
    [SerializeField] private TextMeshProUGUI toastText;  // 선택 : 토스트에 표시할 메시지 텍스트. toastGroup 과 "둘 다" 있어야 토스트가 동작
    // ※ "토스트(toast)" = 모바일 알림처럼 화면에 잠깐 떴다 자동으로 사라지는 짧은 팝업 알림 (안드로이드 Toast 에서 유래)

    [CenterHeader("Behavior")]
    [Tooltip("표시 최대 줄 수(오래된 로그 자동 삭제)")]
    [SerializeField] private int   maxLines           = 300;
    [Tooltip("스레드에서 수신한 로그를 메인 스레드에서 UI로 반영하는 주기(초)")]
    [SerializeField] private float uiRefreshInterval  = 0.1f;
    [Tooltip("같은 연속 메시지를 1줄로 접기")]
    [SerializeField] private bool  collapseDuplicates = true;
    [Tooltip("스택트레이스 표시")]
    [SerializeField] private bool  includeStackTrace;
    [Tooltip("새 에러가 오면 화면 토스트 표시")]
    [SerializeField] private bool  showToastOnError   = true;
    [SerializeField] private float toastSeconds       = 4f;

    [CenterHeader("Filter")]
    public bool showLog       = true;
    public bool showWarning   = true;
    public bool showError     = true;
    public bool showAssert    = true;
    public bool showException = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [CenterHeader("Hotkeys (Editor/Dev)")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;   // 오버레이 숨김/표시
#endif

    struct LogRecord
    {
        public string   Condition;
        public string   Stack;
        public LogType  Type;
        public DateTime Time;
    }

    private readonly ConcurrentQueue<LogRecord> _queue  = new();
    private readonly LinkedList<LogRecord>      _buffer = new(); // 링버퍼처럼 사용
    private readonly StringBuilder              _sb     = new(16 * 1024);

    private float     _nextUiAt;
    private bool      _dirty;
    private Coroutine _toastCo;

//----------------------------------------------------------------------------------------------------------------------

    // logText 참조가 비어 있으면 같은 오브젝트에서 자동 획득 + 씬 전환에도 유지(DontDestroyOnLoad).
    private void Awake()
    {
        if (!logText)
        {
            TryGetComponent(out logText);
        }

        DontDestroyOnLoad(gameObject);
    }

    // 활성화 시 로그 수신 콜백(모든 스레드) 등록.
    private void OnEnable()
    {
        Application.logMessageReceivedThreaded += OnLogThreaded; // 모든 스레드
        // 필요 시 : Application.logMessageReceived += OnLogMainThread;
    }

    // 비활성화 시 로그 수신 콜백 해제(중복/누수 방지).
    private void OnDisable()
    {
        Application.logMessageReceivedThreaded -= OnLogThreaded;
    }

    // 파괴 시 로그 수신 콜백 해제(안전망).
    private void OnDestroy()
    {
        Application.logMessageReceivedThreaded -= OnLogThreaded;
    }

    // 매 프레임 : (에디터/개발) 핫키 토글 + 큐 비우기 + 주기적으로 UI 텍스트 갱신.
    private void Update()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
        {
            ToggleVisible();
        }
#endif
        // 큐 비우기 & UI 주기 반영
        DrainQueueToBuffer();
        if (Time.unscaledTime >= _nextUiAt && _dirty)
        {
            _nextUiAt = Time.unscaledTime + uiRefreshInterval;
            RebuildText();
            _dirty = false;
        }
    }

    // 스레드에서 들어온 로그 1건을 필터링 후 큐에 적재(UI 반영은 메인 스레드 Update 에서).
    private void OnLogThreaded(string condition, string stackTrace, LogType type)
    {
        // 필터 : 비용 줄이기 위해 조기 리턴
        if (!PassFilter(type))
        {
            return;
        }

        _queue.Enqueue(new LogRecord {
            Condition = condition,
            Stack     = includeStackTrace ? stackTrace : null,
            Type      = type,
            Time      = DateTime.Now
        });
    }

    // 로그 타입별 표시 여부(showLog/showWarning/…) 필터 판정.
    private bool PassFilter(LogType t) => t switch
    {
        LogType.Log       => showLog,
        LogType.Warning   => showWarning,
        LogType.Error     => showError,
        LogType.Assert    => showAssert,
        LogType.Exception => showException,
        _                 => true
    };

    // 큐에 쌓인 로그를 표시 버퍼로 옮긴다 : 연속 중복은 (xN)으로 접고, 최대 줄 수 유지, 에러면 토스트 트리거.
    private void DrainQueueToBuffer()
    {
        while (_queue.TryDequeue(out var rec))
        {
            // collapse : 직전과 동일 메시지면 1줄로 접기
            if (collapseDuplicates && _buffer.Last != null)
            {
                var last = _buffer.Last.Value;
                if (last.Type == rec.Type && last.Condition == rec.Condition && last.Stack == rec.Stack)
                {
                    // 마지막 줄에 (x2), (x3)… 카운트 접미어
                    // 간단 구현 : condition 끝에 카운트 표기
                    // 생산용이면 별도 카운트 필드 관리 권장
                    if (!last.Condition.EndsWith(")"))
                    {
                        last.Condition += " (x2)";
                    }
                    else
                    {
                        // (xN) N 증가
                        var idx = last.Condition.LastIndexOf("(x", StringComparison.Ordinal);
                        if (idx >= 0 && int.TryParse(last.Condition.AsSpan(idx + 2, last.Condition.Length - idx - 3), out var n))
                        {
                            last.Condition = last.Condition[..idx] + $"(x{n + 1})";
                        }
                    }
                    _buffer.RemoveLast();
                    _buffer.AddLast(last);
                    _dirty = true;
                    continue;
                }
            }

            _buffer.AddLast(rec);
            while (_buffer.Count > maxLines)
            {
                _buffer.RemoveFirst();
            }

            _dirty = true;

            // 에러 토스트
            if (showToastOnError && (rec.Type == LogType.Error || rec.Type == LogType.Exception || rec.Type == LogType.Assert))
            {
                ShowToastSafe(rec);
            }
        }
    }

    // 버퍼의 모든 로그를 TMP 텍스트로 다시 조립하고, ScrollRect 가 있으면 맨 아래로 자동 스크롤.
    private void RebuildText()
    {
        if (!logText)
        {
            return;
        }

        _sb.Clear();
        foreach (var rec in _buffer)
        {
            _sb.Append(FormatOne(rec)).Append('\n');
        }
        logText.text = _sb.ToString();

        // 자동 스크롤 (선택)
        if (scrollRect)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    // 로그 1건을 타입별 색상 태그(TMP RichText) + 시각 + 본문(+스택)으로 문자열 포맷.
    private string FormatOne(in LogRecord r)
    {
        // 색상 태그(TMP RichText)
        string textStyle = r.Type switch
        {
            LogType.Error     => "<color=#ff5555>[ERROR]</color>",
            LogType.Exception => "<color=#ff4444>[EXCEPTION]</color>",
            LogType.Assert    => "<color=#ff8844>[ASSERT]</color>",
            LogType.Warning   => "<color=#ffaa00>[WARN]</color>",
            _                 => "<color=#cccccc>[LOG]</color>"
        };

        var time = r.Time.ToString("HH:mm:ss.fff");
        if (!string.IsNullOrEmpty(r.Stack))
        {
            return $"{textStyle} {time} {r.Condition}\n<color=#DDDDDD>{r.Stack}</color>";
        }
        else
        {
            return $"{textStyle} {time} {r.Condition}";
        }
    }

    // 에러성 로그가 오면 토스트 패널에 메시지를 띄운다(toastGroup·toastText 둘 다 있을 때만). 메인 스레드에서 실행.
    private void ShowToastSafe(LogRecord rec)
    {
        if (!toastGroup || !toastText)
        {
            return;
        }

        var msg = $"{rec.Type} : {rec.Condition}";
        // 메인 스레드에서만 UI
        UnityMainThread(Action);

        void Action()
        {
            if (_toastCo != null)
            {
                StopCoroutine(_toastCo);
            }
            toastText.text = msg;
            _toastCo = StartCoroutine(FadeToast());
        }
    }

    // 토스트를 즉시 표시했다가 toastSeconds 후 자동으로 숨기는 코루틴.
    private System.Collections.IEnumerator FadeToast()
    {
        toastGroup.alpha = 1f;
        toastGroup.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(toastSeconds);
        toastGroup.alpha = 0f;
        toastGroup.gameObject.SetActive(false);
    }

    // 메인 스레드에서 실행되어야 하는 UI 작업 실행 래퍼(현재는 Update 경유라 바로 호출).
    private void UnityMainThread(Action a)
    {
        // Update에서만 호출하므로 현재는 메인 스레드.
        // 다른 스레드에서 들어올 수 있으니 큐잉하고 Update에서 처리하는 구조로 단순화하는 편이 더 안전합니다.
        a();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    // (에디터/개발 빌드) 핫키로 오버레이(토스트/스크롤뷰) 표시·숨김 토글.
    private void ToggleVisible()
    {
        var on = !(toastGroup ? toastGroup.gameObject.activeSelf : gameObject.activeSelf);
        if (toastGroup)
        {
            toastGroup.gameObject.SetActive(on);
        }
        on = !(scrollRect ? scrollRect.gameObject.activeSelf : gameObject.activeSelf);
        if (scrollRect)
        {
            scrollRect.gameObject.SetActive(on);
        }
    }
#endif
}
