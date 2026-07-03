using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Slider = UnityEngine.UI.Slider;

/// <summary>
/// CatalogUpdateExample
/// - Addressables 카탈로그 업데이트 → 다운로드 용량 확인 → 실제 다운로드 → 데모 로드
/// - 실무 흐름(권장): Initialize(외부) → CheckForCatalogUpdates → (동의 시) UpdateCatalogs → GetDownloadSize/DownloadDependencies
/// - 본 예제는 '카탈로그 최신화 이후'를 가정하고, 카탈로그 확인/업데이트와 다운로드 과정을 UI 버튼으로 분리함.
/// </summary>
public class CatalogUpdateExample : MonoBehaviour
{
    [Header("Demo")]
    [Tooltip("사용자가 키(라벨/주소)를 직접 입력")]
    public TMP_InputField inputField; 
    [Tooltip("로드한 스프라이트를 붙여서 잠시 보여줄 프리팹(예: SpriteRenderer 포함)")]
    public GameObject prefab;

    [Header("Progress UI (선택)")]
    [Tooltip("다운로드/업데이트 진행률 바")]
    [SerializeField] private Slider progressBar;
    [Tooltip("진행 상태 텍스트")]
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Download Scope (선택)")]
    [Tooltip(
        "비워두면 모든 Addressables 키를 대상으로 합니다.\n" +
        "라벨이나 주소를 넣으면 해당 키들이 참조하는 번들을 다운로드합니다.\n" +
        "주의: 특정 리소스만이 아니라, 그 리소스가 포함된 번들 전체가 내려옵니다.")]
    [SerializeField] private List<string> downloadScopes = new(); // 다운로드 범위를 제한할 키(라벨/주소). 비우면 전체.

    // ---- 내부 상태 ----
    private List<string> _listUpdateCatalog = new(); // CheckForCatalogUpdates 결과로 받은 '업데이트할 카탈로그 id 목록'
    private Coroutine _progressRoutine;              // 진행률 UI 코루틴 핸들

    private void Start()
    {
        // 앱 시작 시점에서 카탈로그 변경이 있는지 확인
        // 실제 서비스에서는 '사용자 동의 팝업' 이후 UpdateCatalogs를 호출하도록 분리하는 것을 권장
        CheckUpdate();
    }

    // ======================================================================
    // 1) Catalog update (callbacks)
    // ======================================================================

    /// <summary>
    /// 원격 카탈로그에 변경 사항이 있는지 확인
    /// - CheckForCatalogUpdates는 '현재 로딩된 카탈로그'와 '원격 해시'를 비교하여 업데이트 목록을 반환
    /// - 이 함수 자체는 다운로드를 발생시키지 않음(메타데이터 비교)
    /// </summary>
    private void CheckUpdate()
    {
        var check = Addressables.CheckForCatalogUpdates(); // 비동기 핸들 취득
        if (!check.IsValid())
        {
            Debug.LogError("[Addr] CheckForCatalogUpdates: 핸들 무효");
            return;
        }
        Debug.Log("[Addr] CheckForCatalogUpdates: 대기 중…");

        // Completed 콜백: Task/await 대신 콜백 방식 사용
        // Addressables 구현 버전에 따라 자동 해제되는 경우도 있으나, 일반적으로 직접 Release 권장
        check.Completed += CheckUpdateComplete; 
    }

    /// <summary>
    /// 카탈로그 변경 확인 완료 콜백
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

        var list = check.Result; // 업데이트 필요한 카탈로그 id 모음(없으면 비어있음)
        if (list is { Count: > 0 })
        {
            Debug.Log($"[Addr] {list.Count}개의 카탈로그 업데이트 필요");
            foreach (var cat in list) Debug.Log($"   - {cat}");
            _listUpdateCatalog = list;

            // UI: '업데이트 준비됨' 상태
            UpdateProgressUI(0f, "UpdateCatalogs Ready");
        }
        else
        {
            Debug.Log("[Addr] 변경된 카탈로그 없음");
            UpdateProgressUI(0f, "Catalog Latest");
        }

        // (선택) check 핸들 해제: Completed 후에도 유효 시 Addressables.Release(check) 호출 가능
        // Addressables 구현 버전에 따라 자동 해제되는 경우도 있으나, 일반적으로 직접 Release 권장
        // Addressables.Release(check);
    }

    /// <summary>
    /// '카탈로그 업데이트' 버튼 콜백
    /// - 사용자 동의 후에 호출하는 것이 정석(되돌리기 공식 API 없음)
    /// - UpdateCatalogs 동안 다른 Addressables 요청은 블록될 수 있으므로 초반에 호출 권장
    /// </summary>
    public void OnClickCatalogUpdate()
    {
        if (_listUpdateCatalog == null || _listUpdateCatalog.Count == 0)
        {
            Debug.LogError("[Addr] 업데이트 대상 카탈로그가 없습니다. 먼저 CheckForCatalogUpdates.");
            return;
        }

        var update = Addressables.UpdateCatalogs(_listUpdateCatalog); // 실제 카탈로그 갱신(메모리/디스크 반영)
        if (!update.IsValid())
        {
            Debug.LogError("[Addr] UpdateCatalogs: 핸들 무효");
            return;
        }

        Debug.Log("[Addr] UpdateCatalogs: 진행 중…");
        BeginProgress(update, "Catalog Update");
        update.Completed += UpdateCatalogComplete; // 완료 콜백
    }

    /// <summary>
    /// 카탈로그 업데이트 완료 콜백
    /// - 결과로 IResourceLocator 목록이 반환될 수 있음(새로 적용된 로케이터)
    /// - 여기서부터 GetDownloadSize/DownloadDependencies를 하면 '최신 카탈로그 기준'으로 정확한 결과를 얻음
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
        }
        else
        {
            Debug.Log($"카탈로그 갱신 실패 - {update.Status}");
        }

        // Addressables 구현 버전에 따라 자동 해제되는 경우도 있으나, 일반적으로 직접 Release 권장
        // (선택) update 핸들 해제: Addressables.Release(update);
    }

    // ======================================================================
    // 2) Download size & download (버전 호환 방식)
    //    - 핵심: 키(라벨/주소) → ResourceLocations(리소스 단위 위치정보) → 실제 다운로드는 '번들 단위'
    // ======================================================================

    /// <summary>
    /// [버튼] 다운로드 필요 용량 계산
    /// - 키 집합 → ResourceLocations(중복 제거 Union) → 해당 로케이션들이 필요로 하는 번들 총 합계 용량
    /// - GetDownloadSizeAsync(locations)는 '중복 번들'을 내부에서 제거하여 실제 네트워크 필요량만 반환
    /// </summary>
    public void OnClickCheckDownloadSize()
    {
        var keys = BuildDownloadKeys(); // 입력된 downloadScopes가 있으면 제한, 없으면 전체 키
        if (keys.Count == 0)
        {
            Debug.LogError("[Addr] 다운로드 대상 키가 없습니다.");
            UpdateProgressUI(0f, "No Targets");
            return;
        }

        // 키 집합을 '로케이션'으로 해석(Union: 합집합)
        var locHandle = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union);
        if (!locHandle.IsValid())
        {
            Debug.LogError("[Addr] LoadResourceLocationsAsync: 핸들 무효");
            return;
        }

        // 로케이션 해석 완료 시 용량 계산
        locHandle.Completed += locationsOp =>
        {
            if (!locationsOp.IsValid() || locationsOp.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("[Addr] LoadResourceLocations 실패");
                return;
            }

            IList<IResourceLocation> locations = locationsOp.Result; // 리소스(키) 단위 위치 목록
            // 주의: '번들 파일 목록'이 아님. 여러 로케이션이 같은 번들을 공유할 수 있음.

            // 실제 다운로드 필요량(중복 번들 제거) 계산
            var sizeHandle = Addressables.GetDownloadSizeAsync(locations);
            if (!sizeHandle.IsValid())
            {
                Debug.LogError("[Addr] GetDownloadSizeAsync(locations): 핸들 무효");
                // locHandle 은 auto-release가 아님 → 해제
                Addressables.Release(locationsOp);
                return;
            }

            // 용량 계산 완료
            sizeHandle.Completed += sizeOp =>
            {
                if (sizeOp.IsValid())
                {
                    long bytes = sizeOp.Result;
                    var mb = GetSize(bytes);
                    Debug.Log($"[Addr] 필요 다운로드 용량: {mb} ({bytes} bytes)");
                    UpdateProgressUI(0f, $"Need {mb}");
                    Addressables.Release(sizeOp); // size 핸들 해제
                }
                else
                {
                    Debug.Log("[Addr] GetDownloadSizeAsync Completed: 핸들 무효");
                }

                // locations 핸들 해제(중요: LoadResourceLocationsAsync 결과는 자동 해제 아님)
                if (locationsOp.IsValid()) Addressables.Release(locationsOp);
            };
        };
    }

    /// <summary>
    /// 사람이 읽기 쉬운 용량 문자열 변환(간단 버전)
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
    /// [버튼] 실제 다운로드 시작 (동일 범위)
    /// - 키 집합 → 로케이션 → DownloadDependenciesAsync(locations)
    /// - 주의: 다운로드는 '번들 단위'로 일어나며, 재다운로드 허용(true) 시 캐시 무시(force) 아님. 이미 캐시된 번들은 다시 받지 않음.
    /// </summary>
    public void OnClickDownloadStart()
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
                    if (op.IsValid()) Addressables.Release(op);
                };
            },
            onError: msg => Debug.LogError("[Addr] " + msg)
        );
    }

    // ======================================================================
    // 3) Progress utils (비제네릭 핸들 트래킹)
    // ======================================================================

    /// <summary>
    /// 진행률 코루틴 시작
    /// - AsyncOperationHandle.PercentComplete를 주기적으로 폴링하여 UI 갱신
    /// </summary>
    private void BeginProgress(AsyncOperationHandle handle, string label)
    {
        if (_progressRoutine != null) StopCoroutine(_progressRoutine);
        _progressRoutine = StartCoroutine(CoTrackProgress(handle, label));
    }

    /// <summary>
    /// 진행률 코루틴 종료 + UI 완료 표시
    /// </summary>
    private void EndProgress(string labelDone = "Complete")
    {
        if (_progressRoutine != null) { StopCoroutine(_progressRoutine); _progressRoutine = null; }
        UpdateProgressUI(1f, labelDone);
    }

    /// <summary>
    /// 진행률 트래킹 코루틴
    /// - 너무 잦은 로그 스팸을 막기 위해 2% 단위로 로그 출력
    /// </summary>
    private System.Collections.IEnumerator CoTrackProgress(AsyncOperationHandle handle, string label)
    {
        float lastLogged = -1f;
        while (!handle.IsDone)
        {
            float p = handle.PercentComplete; // 0~1
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

    /// <summary>
    /// 진행률 UI 갱신 유틸
    /// </summary>
    private void UpdateProgressUI(float normalized, string label)
    {
        if (progressBar) progressBar.value = Mathf.Clamp01(normalized);
        if (progressText) progressText.text = $"{label} - {(int)(Mathf.Clamp01(normalized) * 100f)}%";
    }

    // ======================================================================
    // 4) Helpers
    // ======================================================================

    /// <summary>
    /// 다운로드 대상 키 빌드
    /// - downloadScopes가 비어 있으면 '모든 키'를 대상으로(주의: 실제 서비스에서는 너무 광범위할 수 있음)
    /// - 주소/라벨을 섞어서 넣을 수 있음(LoadResourceLocationsAsync가 내부에서 해석)
    /// </summary>
    private List<object> BuildDownloadKeys()
    {
        if (downloadScopes != null && downloadScopes.Count > 0)
            return downloadScopes.Cast<object>().ToList(); // 사용자가 지정한 스코프(주소/라벨)

        // 모든 키 수집: 현재 로딩된 리소스 로케이터들의 키를 합집합으로 모음
        var set = new HashSet<object>();
        foreach (var locator in Addressables.ResourceLocators)
        {
            if (locator is IResourceLocator rl)
                foreach (var k in rl.Keys) set.Add(k);
        }
        return set.ToList();
    }

    // ======================================================================
    // 5) Load demo
    // ======================================================================

    /// <summary>
    /// 데모: 사용자가 입력한 key(주소/라벨)로 Sprite를 로드해서 프리팹에 적용, 1초 후 파괴
    /// - 실제 운영에서는 '로드 전에 GetDownloadSizeAsync(key)==0'을 확인하여 네트워크 유발 여부를 판단/가드하는 패턴 권장
    /// </summary>
    public void OnClickCreateItem()
    {
        // 데모용 안전성 체크
        if (prefab == null)
        {
            Debug.LogError("[Addr] prefab 참조가 비어있습니다.");
            return;
        }
        if (inputField == null)
        {
            Debug.LogError("[Addr] inputField 참조가 비어있습니다.");
            return;
        }

        var key = inputField.text; // 사용자가 입력한 Addressables 키(주소/라벨)
        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[Addr] inputField가 비어있습니다. inputField에 Key를 입력하세요.");
            return;
        }

        // (바로 로드) LoadAssetAsync: 캐시에 번들이 없으면 즉시 네트워크 다운로드가 발생할 수 있음
        var handle = Addressables.LoadAssetAsync<Sprite>(key);
        if (!handle.IsValid())
        {
            Debug.LogError($"[Addr] LoadAssetAsync<Sprite>: 핸들 무효. key: {key}");
            return;
        }

        // 완료 콜백
        handle.Completed += op =>
        {
            if (!op.IsValid())
            {
                Debug.LogError("[Addr] LoadAssetAsync<Sprite]: Completed 시점 핸들 무효");
                return;
            }

            if (op.Status != AsyncOperationStatus.Succeeded || op.Result == null)
            {
                Debug.LogError($"[Addr] Sprite 로드 실패: {key} ({op.Status})");
                if (op.IsValid()) Addressables.Release(op); // 실패 시도라도 핸들 해제
                return;
            }

            // 로드 성공: 간단한 데모 오브젝트 생성 후 스프라이트 적용
            var obj = Instantiate(prefab, new Vector3(0, 1, 0), Quaternion.identity);
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = op.Result;

            // 데모: 1초 후 파괴
            Destroy(obj, 1f);

            // 주의: LoadAssetAsync 핸들은 Result를 더 이상 사용하지 않을 때 Release 필요
            // 여기서는 임시 데모이므로 즉시 Release하지 않았음. 실제 코드에선 참조 정책에 맞춰 Release를 호출하세요.
            // Addressables.Release(op);
        };
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