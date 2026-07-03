using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// SceneGame
/// - 사용자가 입력한 Addressables 키(라벨/주소)로 Sprite를 로드해
///   데모용 프리팹(SpriteRenderer)에 적용하는 간단한 예제입니다.
///
/// 중요 포인트:
/// - Addressables.LoadAssetAsync는 '필요 번들을 자동 다운로드'할 수 있습니다.
///   (캐시에 없으면 네트워크 발생) → 부팅 단계에서 GetDownloadSize / 선행 다운로드 권장.
/// - 로드 결과(핸들)는 참조 카운팅 기반입니다. 더 이상 사용하지 않으면 Release로 해제하세요.
///   본 예제는 마지막 로드 핸들을 보관했다가, 새 로드를 시작할 때 이전 핸들을 해제합니다.
/// </summary>
public class SceneGame : MonoBehaviour
{
    [Header("Demo")]
    [Tooltip("사용자가 Addressables 키(라벨/주소)를 입력하는 필드입니다.")]
    public TMP_InputField inputField;

    [Tooltip("로드한 Sprite를 표시할 데모 프리팹(예: SpriteRenderer 포함) 입니다.")]
    public GameObject prefab;

    /// <summary>
    /// 마지막으로 성공적으로 로드한 Sprite 핸들(참조 카운팅 관리용).
    /// - 새 로드를 시작하기 전, 기존 핸들을 Release하여 메모리 누수를 방지합니다.
    /// - 실제 게임에선 '오브젝트가 살아있는 동안' 핸들을 유지하거나,
    ///   일괄 언로드 타이밍에 정리하는 등 일관된 정책을 사용하세요.
    /// </summary>
    private AsyncOperationHandle<Sprite>? _lastLoadHandle;

    // -------- Load demo --------

    /// <summary>
    /// [버튼] 입력된 키로 Sprite를 로드해서 데모 프리팹에 적용합니다.
    /// - 키가 비어 있으면 에러 로그 후 종료
    /// - 로드 핸들 유효성 체크
    /// - 완료 콜백에서 성공/실패 처리 및 프리팹 생성
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
        // 1) 키 확보
        var key = "";
        if (inputField && !string.IsNullOrEmpty(inputField.text))
        {
            key = inputField.text.Trim();
        }

        if (string.IsNullOrEmpty(key))
        {
            Debug.LogError("[Addr] inputField가 비어있습니다. inputField에 Key를 입력하세요.");
            return;
        }

        // 2) 이전 로드 핸들 정리(메모리/참조 카운트 누수 방지)
        //    - 실제 프로젝트에서는 오브젝트 소멸 타이밍에 맞춰 해제하는 등 정책화 권장
        if (_lastLoadHandle is { } prev && prev.IsValid())
        {
            Addressables.Release(prev);
            _lastLoadHandle = null;
        }

        // 3) 로드 요청 (주의: 캐시에 없으면 네트워크 다운로드가 발생할 수 있음)
        var handle = Addressables.LoadAssetAsync<Sprite>(key);
        if (!handle.IsValid())
        {
            Debug.LogError("[Addr] LoadAssetAsync<Sprite>: 핸들 무효");
            return;
        }

        // 마지막 핸들 기록
        _lastLoadHandle = handle;

        // 4) 완료 콜백
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
                // 실패 시 핸들 해제
                if (op.IsValid()) Addressables.Release(op);
                _lastLoadHandle = null;
                return;
            }

            // 5) 결과 적용 대상 확인
            if (prefab == null)
            {
                Debug.LogError("[Addr] prefab 참조가 비어있습니다.");
                // 더 이상 사용할 계획이 없으므로 핸들 해제
                if (op.IsValid()) Addressables.Release(op);
                _lastLoadHandle = null;
                return;
            }

            // 6) 데모 오브젝트 생성 후 Sprite 적용
            var obj = Instantiate(prefab, new Vector3(0, 1, 0), Quaternion.identity);
            var sr = obj.GetComponent<SpriteRenderer>();
            if (sr != null) sr.sprite = op.Result;

            // 데모: 1초 뒤 파괴(실제 게임에선 오브젝트 수명과 함께 핸들 관리 정책 필요)
            Destroy(obj, 1f);

            // 참고:
            // - 여기서 곧바로 Addressables.Release(op)를 호출하면, 번들이 언로드될 수 있어
            //   sr.sprite 참조가 깨질 수 있습니다. (상황/설정에 따라 다름)
            // - 안전하게 운영하려면, 오브젝트가 파괴될 때(또는 교체될 때) Release 하거나,
            //   '화면에 표시되는 동안' 핸들을 유지하는 정책을 사용하세요.
            // - 본 예제에서는 마지막 핸들을 필드에 보관(_lastLoadHandle)하고,
            //   다음 로드 시 이전 핸들을 해제하는 전략을 사용합니다.
        };
    }

    private void OnDestroy()
    {
        // 씬 종료/오브젝트 파괴 시 마지막 핸들 정리(누수 방지)
        if (_lastLoadHandle is { } last && last.IsValid())
        {
            Addressables.Release(last);
            _lastLoadHandle = null;
        }
    }
}
