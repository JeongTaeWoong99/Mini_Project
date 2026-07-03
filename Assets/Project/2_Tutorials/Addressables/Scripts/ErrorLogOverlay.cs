using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Diagnostics/Error Log Overlay")]
public sealed class ErrorLogOverlay : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI logText;              // 스크롤뷰 안의 본문 텍스트
    [SerializeField] private ScrollRect scrollRect;                 // 선택 (자동 스크롤)
    [SerializeField] private CanvasGroup toastGroup;                // 선택 (토스트 패널)
    [SerializeField] private TextMeshProUGUI toastText;             // 선택 (토스트 텍스트)

    [Header("Behavior")]
    [Tooltip("표시 최대 줄 수(오래된 로그 자동 삭제)")]
    [SerializeField] private int maxLines = 300;
    [Tooltip("스레드에서 수신한 로그를 메인 스레드에서 UI로 반영하는 주기(초)")]
    [SerializeField] private float uiRefreshInterval = 0.1f;
    [Tooltip("같은 연속 메시지를 1줄로 접기")]
    [SerializeField] private bool collapseDuplicates = true;
    [Tooltip("스택트레이스 표시")]
    [SerializeField] private bool includeStackTrace;
    [Tooltip("새 에러가 오면 화면 토스트 표시")]
    [SerializeField] private bool showToastOnError = true;
    [SerializeField] private float toastSeconds = 4f;

    [Header("Filter")]
    public bool showLog = true;
    public bool showWarning = true;
    public bool showError = true;
    public bool showAssert = true;
    public bool showException = true;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("Hotkeys (Editor/Dev)")]
    [SerializeField] private KeyCode toggleKey = KeyCode.F1;   // 오버레이 숨김/표시
#endif

    struct LogRecord
    {
        public string Condition;
        public string Stack;
        public LogType Type;
        public DateTime Time;
    }

    private readonly ConcurrentQueue<LogRecord> _queue = new();
    private readonly LinkedList<LogRecord> _buffer = new(); // 링버퍼처럼 사용
    private readonly StringBuilder _sb = new(16 * 1024);

    private float _nextUiAt;
    private bool _dirty;
    private Coroutine _toastCo;

    private void Awake()
    {
        if (!logText) TryGetComponent(out logText);
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        Application.logMessageReceivedThreaded += OnLogThreaded; // 모든 스레드
        // 필요 시: Application.logMessageReceived += OnLogMainThread;
    }

    private void OnDisable()
    {
        Application.logMessageReceivedThreaded -= OnLogThreaded;
    }

    private void OnDestroy()
    {
        Application.logMessageReceivedThreaded -= OnLogThreaded;
    }

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

    private void OnLogThreaded(string condition, string stackTrace, LogType type)
    {
        // 필터: 비용 줄이기 위해 조기 리턴
        if (!PassFilter(type)) return;

        _queue.Enqueue(new LogRecord {
            Condition = condition,
            Stack = includeStackTrace ? stackTrace : null,
            Type = type,
            Time = DateTime.Now
        });
    }

    private bool PassFilter(LogType t) => t switch
    {
        LogType.Log => showLog,
        LogType.Warning => showWarning,
        LogType.Error => showError,
        LogType.Assert => showAssert,
        LogType.Exception => showException,
        _ => true
    };

    private void DrainQueueToBuffer()
    {
        while (_queue.TryDequeue(out var rec))
        {
            // collapse: 직전과 동일 메시지면 1줄로 접기
            if (collapseDuplicates && _buffer.Last != null)
            {
                var last = _buffer.Last.Value;
                if (last.Type == rec.Type && last.Condition == rec.Condition && last.Stack == rec.Stack)
                {
                    // 마지막 줄에 (x2), (x3)… 카운트 접미어
                    // 간단 구현: condition 끝에 카운트 표기
                    // 생산용이면 별도 카운트 필드 관리 권장
                    if (!last.Condition.EndsWith(")"))
                        last.Condition += " (x2)";
                    else
                    {
                        // (xN) N 증가
                        var idx = last.Condition.LastIndexOf("(x", StringComparison.Ordinal);
                        if (idx >= 0 && int.TryParse(last.Condition.AsSpan(idx + 2, last.Condition.Length - idx - 3), out var n))
                            last.Condition = last.Condition[..idx] + $"(x{n + 1})";
                    }
                    _buffer.RemoveLast();
                    _buffer.AddLast(last);
                    _dirty = true;
                    continue;
                }
            }

            _buffer.AddLast(rec);
            while (_buffer.Count > maxLines)
                _buffer.RemoveFirst();

            _dirty = true;

            // 에러 토스트
            if (showToastOnError && (rec.Type == LogType.Error || rec.Type == LogType.Exception || rec.Type == LogType.Assert))
                ShowToastSafe(rec);
        }
    }

    private void RebuildText()
    {
        if (!logText) return;

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

    private string FormatOne(in LogRecord r)
    {
        // 색상 태그(TMP RichText)
        string textStyle = r.Type switch
        {
            LogType.Error => "<color=#ff5555>[ERROR]</color>",
            LogType.Exception => "<color=#ff4444>[EXCEPTION]</color>",
            LogType.Assert => "<color=#ff8844>[ASSERT]</color>",
            LogType.Warning => "<color=#ffaa00>[WARN]</color>",
            _ => "<color=#cccccc>[LOG]</color>"
        };

        var time = r.Time.ToString("HH:mm:ss.fff");
        if (!string.IsNullOrEmpty(r.Stack))
            return $"{textStyle} {time} {r.Condition}\n<color=#DDDDDD>{r.Stack}</color>";
        else
            return $"{textStyle} {time} {r.Condition}";
    }

    private void ShowToastSafe(LogRecord rec)
    {
        if (!toastGroup || !toastText) return;
        var msg = $"{rec.Type}: {rec.Condition}";
        // 메인 스레드에서만 UI
        UnityMainThread(Action);

        void Action()
        {
            if (_toastCo != null) StopCoroutine(_toastCo);
            toastText.text = msg;
            _toastCo = StartCoroutine(FadeToast());
        }
    }

    private System.Collections.IEnumerator FadeToast()
    {
        toastGroup.alpha = 1f;
        toastGroup.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(toastSeconds);
        toastGroup.alpha = 0f;
        toastGroup.gameObject.SetActive(false);
    }

    private void UnityMainThread(Action a)
    {
        // Update에서만 호출하므로 현재는 메인 스레드.
        // 다른 스레드에서 들어올 수 있으니 큐잉하고 Update에서 처리하는 구조로 단순화하는 편이 더 안전합니다.
        a();
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void ToggleVisible()
    {
        var on = !(toastGroup ? toastGroup.gameObject.activeSelf : gameObject.activeSelf);
        if (toastGroup) toastGroup.gameObject.SetActive(on);
        on = !(scrollRect ? scrollRect.gameObject.activeSelf : gameObject.activeSelf);
        if (scrollRect) scrollRect.gameObject.SetActive(on);
    }
#endif
}
