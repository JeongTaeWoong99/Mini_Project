using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;
using Slider = UnityEngine.UI.Slider;

/// <summary>
/// SceneIntro
/// - 앱 기동 시 실행되는 인트로 씬에서 Addressables 패치(카탈로그/리소스)를 관리하는 예제입니다.
/// - 플로우:
///   1) CheckForCatalogUpdates: 카탈로그 변경 유무 확인 (네트워크 경량)
///   2) (변경 있음) 사용자 동의 후 UpdateCatalogs: 카탈로그 최신화 (리소스 다운로드는 아님)
///   3) GetDownloadSizeAsync: 실제 필요한 다운로드 용량 계산 (번들 단위 합산)
///   4) 팝업 안내 → 동의 시 DownloadDependenciesAsync(리소스 다운로드)
///   5) 완료/건너뛰기 시 Game 씬 진입
/// - 주의:
///   * Addressables는 '리소스 단위'가 아닌 '번들 단위'로 다운로드합니다.
///   * UpdateCatalogs는 메타데이터 교체이므로, 그 자체로 대용량 다운로드가 발생하지 않습니다.
///   * UpdateCatalogs는 되돌릴 수 있는 공식 API가 없으므로, 실서비스에선 '동의 후 적용'이 정석입니다.
/// </summary>
public class SceneIntro : MonoBehaviour
{
    // ===============================
    // 인스펙터 노출 필드 (UI/옵션)
    // ===============================

    [Header("진행률 UI (선택 표시)")]
    [Tooltip("다운로드 진행 상황을 보여줄 UI 전체 컨테이너입니다. 다운로드 중에만 활성화됩니다.")]
    [SerializeField] private GameObject containerProgress;

    [Tooltip("카탈로그 업데이트/다운로드 진행률(0~1)을 표시하는 슬라이더입니다.")]
    [SerializeField] private Slider progressBar;

    [Tooltip("진행 상태를 텍스트로 표시합니다. 예: \"Download - 37%\"")]
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("다운로드 범위 (선택)")]
    [Tooltip(
        "Addressables에서 패치(다운로드) 대상 범위를 지정합니다.\n" +
        "비워두면 '현재 카탈로그가 알고 있는 모든 키'를 대상으로 계산/다운로드합니다(주의: 과도할 수 있음).\n" +
        "특정 라벨이나 주소를 넣으면 '그 키들이 참조하는 번들 전체'가 다운로드됩니다.\n" +
        "※ Addressables는 '리소스 단위'가 아닌 '번들 단위'로 내려받습니다."
    )]
    [SerializeField] private List<string> downloadScopes = new();

    [Header("안내 팝업")]
    [Tooltip("패치 용량 안내와 [확인/취소] 버튼을 담은 팝업입니다.")]
    public GameObject panelPopup;

    [Tooltip("팝업에 표시되는 예상 다운로드 용량(예: \"12.3 MB\") 텍스트입니다.")]
    public TextMeshProUGUI textDownloadSize;

    // ===============================
    // 내부 상태/상수
    // ===============================

    /// <summary>CheckForCatalogUpdates 결과로 받은 '업데이트할 카탈로그 ID' 목록</summary>
    private List<string> _listUpdateCatalog = new();

    /// <summary>진행률 UI 갱신 코루틴 핸들</summary>
    private Coroutine _progressRoutine;

    /// <summary>패치 완료 후 이동할 씬 이름(예: 실제 게임 씬)</summary>
    private const string NameSceneGame = "Game";

    // ===============================
    // 생명주기
    // ===============================

    private void Awake()
    {
        // 초기 UI 상태 정리
        if (containerProgress) containerProgress.SetActive(false);
        if (panelPopup) panelPopup.SetActive(false);
    }

    private void Start()
    {
        // 1) 앱 기동 후, 먼저 카탈로그 변경 유무만 확인 (동의/정책에 따라 UpdateCatalogs는 뒤로 미루는 게 실무 정석)
        CheckUpdate();
    }

    // ===============================
    // Catalog Update (확인/적용)
    // ===============================

    /// <summary>
    /// 카탈로그 변경 유무 확인
    /// - 네트워크 경량: 원격 hash와 현재 카탈로그(로컬/캐시) 비교
    /// - 결과가 0개면 최신, 1개 이상이면 업데이트 필요
    /// </summary>
    private void CheckUpdate()
    {
        var check = Addressables.CheckForCatalogUpdates();
        if (!check.IsValid())
        {
            Debug.LogError("[Addr] CheckForCatalogUpdates: 핸들 무효");
            return;
        }

        Debug.Log("[Addr] CheckForCatalogUpdates: 대기 중…");
        check.Completed += CheckUpdateComplete; // 완료 콜백 등록
    }

    /// <summary>
    /// CheckForCatalogUpdates 완료 콜백
    /// - 결과에 따라 다음 단계(카탈로그 갱신 or 용량계산)로 분기
    /// </summary>
    private void CheckUpdateComplete(AsyncOperationHandle<List<string>> check)
    {
        if (!check.IsValid())
        {
            Debug.LogError("[Addr] CheckForCatalogUpdates: Completed 시점 핸들 무효");
            return;
        }

        Debug.Log($"[Addr] CheckForCatalogUpdates 완료: {check.Status}");
        if (check.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError("[Addr] 카탈로그 변경 확인 실패");
            return;
        }

        var list = check.Result; // 업데이트 필요 카탈로그 ID들
        if (list is { Count: > 0 })
        {
            Debug.Log($"[Addr] {list.Count}개의 카탈로그 업데이트 필요");
            foreach (var cat in list) Debug.Log($"   - {cat}");
            _listUpdateCatalog = list;

            UpdateProgressUI(0f, "UpdateCatalogs Ready");

            // (데모 편의를 위해) 곧바로 카탈로그 업데이트 실행
            // 실서비스에서는 '사용자 동의 후'에 UpdateCatalogs 호출하는 것을 권장합니다.
            OnClickCatalogUpdate();
        }
        else
        {
            Debug.Log("[Addr] 변경된 카탈로그 없음");
            UpdateProgressUI(0f, "Catalog Latest");

            // 카탈로그가 최신이면 곧바로 용량 계산으로 진행
            OnClickCheckDownloadSize();
        }

        // (선택) check 핸들 해제: Addressables.Release(check);
    }

    /// <summary>
    /// [버튼] 카탈로그 업데이트 실행
    /// - 동의 이후에 호출하는 게 정석 (UpdateCatalogs는 되돌리기 공식 API가 없음)
    /// - 실행 중에는 다른 Addressables 요청이 블록될 수 있어 초기 단계에서 처리 권장
    /// </summary>
    private void OnClickCatalogUpdate()
    {
        if (_listUpdateCatalog == null || _listUpdateCatalog.Count == 0)
        {
            Debug.LogError("[Addr] 업데이트 대상 카탈로그가 없습니다. 먼저 CheckForCatalogUpdates.");
            return;
        }

        var update = Addressables.UpdateCatalogs(_listUpdateCatalog);
        if (!update.IsValid())
        {
            Debug.LogError("[Addr] UpdateCatalogs: 핸들 무효");
            return;
        }

        Debug.Log("[Addr] UpdateCatalogs: 진행 중…");
        BeginProgress(update, "Catalog Update");
        update.Completed += UpdateCatalogComplete;
    }

    /// <summary>
    /// UpdateCatalogs 완료 콜백
    /// - 최신 카탈로그가 메모리/디스크에 반영된 이후이므로,
    ///   이 시점부터의 GetDownloadSize/DownloadDependencies는 최신 상태 기준으로 정확해집니다.
    /// </summary>
    private void UpdateCatalogComplete(AsyncOperationHandle<List<IResourceLocator>> update)
    {
        EndProgress("Catalog Update Complete");

        if (!update.IsValid())
        {
            Debug.LogError("[Addr] UpdateCatalogs: Completed 시점 핸들 무효");
            return;
        }

        if (update.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("카탈로그 갱신 완료");
            if (update.Result != null)
                foreach (var locator in update.Result)
                    Debug.Log($"   - Locator: {locator?.LocatorId}");

            // 카탈로그 최신화 이후 → 용량 계산
            OnClickCheckDownloadSize();
        }
        else
        {
            Debug.LogError($"카탈로그 갱신 실패 - {update.Status}");
        }

        // (선택) Addressables.Release(update);
    }

    // ===============================
    // Download Size & Download
    // ===============================

    /// <summary>
    /// [팝업] 패치 건너뛰기 (사용자 취소)
    /// - 실무 정책에 따라: 강제 패치가 아니라면 게임 씬으로 바로 진입
    /// </summary>
    public void OnClickPopupCancel()
    {
#if UNITY_EDITOR
        // 에디터에서 실행 중이면 Play Mode 중단
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    /// <summary>
    /// [팝업] 패치 진행 (사용자 동의)
    /// </summary>
    public void OnClickPopupConfirm()
    {
        OnClickDownloadStart();
    }

    /// <summary>
    /// 다운로드 필요 용량 계산
    /// - 키 집합 → ResourceLocations(Union) → 해당 로케이션들이 필요로 하는 '번들 총합 용량'
    /// - 주의: IResourceLocation 리스트는 '리소스 단위 좌표'이며, 실제 다운로드는 '번들 단위'
    /// </summary>
    private void OnClickCheckDownloadSize()
    {
        var keys = BuildDownloadKeys();
        if (keys.Count == 0)
        {
            Debug.LogError("[Addr] 다운로드 대상 키가 없습니다.");
            UpdateProgressUI(0f, "No Targets");
            return;
        }

        // 키들을 '리소스 위치'로 해석 (중복 제거: Union)
        var locHandle = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union);
        if (!locHandle.IsValid())
        {
            Debug.LogError("[Addr] LoadResourceLocationsAsync: 핸들 무효");
            return;
        }

        locHandle.Completed += locationsOp =>
        {
            if (!locationsOp.IsValid() || locationsOp.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[Addr] LoadResourceLocations 실패");
                return;
            }

            IList<IResourceLocation> locations = locationsOp.Result;
            // 실제 네트워크 필요량(중복 번들 제거)을 계산
            var sizeHandle = Addressables.GetDownloadSizeAsync(locations);
            if (!sizeHandle.IsValid())
            {
                Debug.LogError("[Addr] GetDownloadSizeAsync(locations): 핸들 무효");
                if (locationsOp.IsValid()) Addressables.Release(locationsOp); // 중요: 로케이션 해제
                return;
            }

            sizeHandle.Completed += sizeOp =>
            {
                if (sizeOp.IsValid())
                {
                    long bytes = sizeOp.Result;
                    var mb = GetSize(bytes);

                    // [상황 분기]
                    // 1) bytes <= 0: 리소스가 모두 캐시에 있으므로 다운로드 불필요 → 바로 게임 진입
                    // 2) bytes > 0: 사용자에게 용량 안내 팝업 → 확인 시 다운로드
                    if (bytes > 0)
                    {
                        if (panelPopup)
                        {
                            panelPopup.SetActive(true);
                            if (textDownloadSize) textDownloadSize.text = mb;
                        }
                    }
                    else
                    {
                        SceneManager.LoadScene(NameSceneGame);
                    }

                    Debug.Log($"[Addr] 필요 다운로드 용량: {mb} ({bytes} bytes)");
                    Addressables.Release(sizeOp); // 해제
                }
                else
                {
                    Debug.Log("[Addr] GetDownloadSizeAsync Completed: 핸들 무효");
                }

                // 로케이션 핸들 해제(LoadResourceLocationsAsync 결과는 자동 해제 아님)
                if (locationsOp.IsValid()) Addressables.Release(locationsOp);
            };
        };
    }

    /// <summary>
    /// 용량 숫자를 사람이 읽기 쉬운 문자열로 변환
    /// - 단순 버전(GB 처리는 생략)
    /// </summary>
    private string GetSize(long bytes)
    {
        var mb = bytes / (1024f * 1024f);
        if (mb >= 0.1) return $"{mb:F1} MB";
        mb = mb / 1024f;
        if (mb >= 0.1) return $"{mb:F1} KB";
        return $"{bytes} bytes";
    }

    /// <summary>
    /// 실제 다운로드 시작
    /// - 키 집합 → 로케이션(Union) → DownloadDependenciesAsync(locations)
    /// - true: 동일 의존성 중복 허용(이미 캐시에 있으면 내려받지 않음)
    /// - 완료 후 게임 씬으로 전환
    /// </summary>
    private void OnClickDownloadStart()
    {
        
        var keys = BuildDownloadKeys();
        if (keys.Count == 0) { UpdateProgressUI(0f, "No Targets"); return; }

        ResolveRemoteBundleLocations(
            keys,
            onSuccess: remoteBundles =>
            {
                if (remoteBundles.Count == 0)
                {
                    UpdateProgressUI(1f, "Nothing to download");
                    return;
                }

                var dlHandle = Addressables.DownloadDependenciesAsync(remoteBundles, true);
                if (!dlHandle.IsValid())
                {
                    Debug.LogError("[Addr] DownloadDependenciesAsync(remoteBundles): invalid handle");
                    return;
                }

                BeginProgress(dlHandle, "Download");
                dlHandle.Completed += op =>
                {
                    EndProgress("Download Complete");
                    Debug.Log("다운로드 완료");
                    SceneManager.LoadScene(NameSceneGame);
                    if (op.IsValid()) Addressables.Release(op);
                };
            },
            onError: msg => Debug.LogError("[Addr] " + msg)
        );
    }

    // ===============================
    // 진행률 UI 유틸
    // ===============================

    /// <summary>진행률 코루틴 시작(AsyncOperationHandle.PercentComplete 폴링)</summary>
    private void BeginProgress(AsyncOperationHandle handle, string label)
    {
        if (_progressRoutine != null) StopCoroutine(_progressRoutine);
        _progressRoutine = StartCoroutine(CoTrackProgress(handle, label));
    }

    /// <summary>진행률 코루틴 종료 및 완료 라벨 표시</summary>
    private void EndProgress(string labelDone = "Complete")
    {
        if (_progressRoutine != null) { StopCoroutine(_progressRoutine); _progressRoutine = null; }
        UpdateProgressUI(1f, labelDone);
    }

    /// <summary>
    /// 진행률 트래킹
    /// - 과다 로그를 방지하기 위해 2% 이상 변화 시에만 로그
    /// </summary>
    private System.Collections.IEnumerator CoTrackProgress(AsyncOperationHandle handle, string label)
    {
        float lastLogged = -1f;
        while (!handle.IsDone)
        {
            float p = handle.PercentComplete;
            UpdateProgressUI(p, label);

            if (p - lastLogged >= 0.02f)
            {
                Debug.Log($"[Addr] {label}: {(int)(p * 100f)}%");
                lastLogged = p;
            }
            yield return null;
        }
        UpdateProgressUI(1f, label + " Complete");
        _progressRoutine = null;
    }

    /// <summary>진행률 바/텍스트 갱신</summary>
    private void UpdateProgressUI(float normalized, string label)
    {
        if (progressBar) progressBar.value = Mathf.Clamp01(normalized);
        if (progressText) progressText.text = $"{label} - {(int)(Mathf.Clamp01(normalized) * 100f)}%";
    }

    // ===============================
    // 헬퍼: 다운로드 대상 키 구성
    // ===============================

    /// <summary>
    /// 다운로드 대상 키 목록 구성
    /// - downloadScopes에 값이 있으면 그대로 사용(라벨/주소 혼용 가능)
    /// - 비어 있으면 현재 로딩된 모든 리소스 로케이터에서 모든 키를 모음(실무에선 과도할 수 있음에 주의)
    /// </summary>
    private List<object> BuildDownloadKeys()
    {
        if (downloadScopes is { Count: > 0 })
            return downloadScopes.Cast<object>().ToList();

        var set = new HashSet<object>();
        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator is IResourceLocator rl)
                foreach (var k in rl.Keys) set.Add(k);
        }
        return set.ToList();
    }
    // ===[1] 원격 번들 판별 유틸 ===
    private static bool IsBundle(IResourceLocation loc)
    {
        return loc != null &&
               loc.ResourceType == typeof(UnityEngine.ResourceManagement.ResourceProviders.IAssetBundleResource);
    }

    private static bool IsRemote(IResourceLocation loc)
    {
        if (loc == null) return false;
        var id = loc.InternalId ?? string.Empty;
        // 가장 안전한 1차 판별: URL 스킴
        if (id.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            id.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return true;

        // (선택) RemoteLoadPath 접두 검증이 필요하면 아래처럼 보강
        // var expandedRemote = Addressables.ResolveInternalId("{{YourRemoteLoadPath}}");
        // if (!string.IsNullOrEmpty(expandedRemote) && id.StartsWith(expandedRemote, StringComparison.OrdinalIgnoreCase))
        //     return true;

        return false;
    }

    // ===[2] 그래프(의존성 포함)에서 "원격 번들"만 추려내기 ===
    private static List<IResourceLocation> CollectRemoteBundlesDeep(IEnumerable<IResourceLocation> roots)
    {
        var result = new HashSet<IResourceLocation>();
        var visited = new HashSet<IResourceLocation>();
        var stack = new Stack<IResourceLocation>(roots.Where(l => l != null));

        while (stack.Count > 0)
        {
            var loc = stack.Pop();
            if (!visited.Add(loc)) continue;

            if (IsBundle(loc) && IsRemote(loc))
                result.Add(loc);

            if (loc.Dependencies != null)
                foreach (var d in loc.Dependencies)
                    if (d != null)
                        stack.Push(d);
        }

        return result.ToList();
    }

    // ===[3] 키 집합 → 원격 번들 로케이션만 해석 ===
    private void ResolveRemoteBundleLocations(
        List<object> keys,
        Action<IList<IResourceLocation>> onSuccess,
        Action<string> onError = null)
    {
        var locHandle = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union);
        if (!locHandle.IsValid())
        {
            onError?.Invoke("LoadResourceLocationsAsync: invalid handle");
            return;
        }

        locHandle.Completed += op =>
        {
            if (!op.IsValid() || op.Status != AsyncOperationStatus.Succeeded)
            {
                onError?.Invoke("LoadResourceLocationsAsync failed");
                return;
            }

            try
            {
                var remoteBundles = CollectRemoteBundlesDeep(op.Result);
                onSuccess?.Invoke(remoteBundles);
            }
            finally
            {
                if (op.IsValid()) Addressables.Release(op);
            }
        };
    }
}
