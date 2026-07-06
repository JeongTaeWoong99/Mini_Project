using UnityEditor;
using UnityEngine;

/// <summary>
/// AddressablesCacheTool
/// - 다운로드된 원격 에셋 번들 "클라이언트 캐시"를 비우는 에디터 유틸.
/// - 캐시가 비면 다음 Play 때 GetDownloadSize 가 0 이 아니게 되어 다운로드 과정을 다시 확인할 수 있다.
/// - 메뉴 : Tools > Addressables > Clear Bundle Cache
///
/// [무엇이 지워지나]
///   ○ 지워짐    :
///     - Unity 의 모든 AssetBundle 캐시 = 서버에서 내려받아 저장해 둔 "원격 번들 파일의 사본".
///     - (Addressables 원격 그룹이 다운로드해 캐시에 보관 중인 번들)
///     - 실제 위치(Windows 에디터, 이 프로젝트 예시) :
///         C:\Users\&lt;사용자&gt;\AppData\LocalLow\Unity\DefaultCompany_UNITY6 URP 2D\
///       └ 이 아래에 "번들 해시명 폴더 / __data" 형태로 저장됨.
///         (persistentDataPath = C:\Users\&lt;사용자&gt;\AppData\LocalLow\&lt;Company&gt;_&lt;Product&gt; 인근)
///
///   ○ 안 지워짐 :
///     - 카탈로그 캐시(catalog_*.hash 비교 상태) — persistentDataPath 의 별도 위치라 남음.
///     - 로컬 그룹 에셋(앱 내장) · 프로젝트 원본 에셋.
///     - 빌드 산출물(ServerData\StandaloneWindows64\packedassetsremote_*.bundle 등)
///       · 서버 파일(XAMPP htdocs\…) — 이건 캐시가 아니라 "파일 원천"이라 삭제 대상이 아님.
/// </summary>
public static class AddressablesCacheTool
{
    [MenuItem("Tools/Addressables/Clear Bundle Cache")]
    private static void ClearBundleCache()
    {
        // Caching.ClearCache : 앱의 모든 AssetBundle 캐시(내려받은 번들 사본)를 삭제.
        // - 이 메서드는 UnityEngine 엔진 API(UnityEngine.Caching) 이다. (Addressables 라이브러리 아님)
        //   Addressables 가 원격 번들을 엔진의 Caching 시스템에 저장하므로, 그 캐시를 이 엔진 API 로 비운다.
        // - Play 중이라 번들이 사용 중이면 false 를 반환(먼저 Play 를 멈춰야 함).
        bool cleared = Caching.ClearCache();

        if (cleared)
        {
            Debug.Log("[Addr] Bundle 캐시를 비웠습니다. 다음 Play 때 원격 번들을 다시 다운로드합니다.");
            EditorUtility.DisplayDialog
            (
                "Addressables Cache",
                "원격 번들 캐시를 비웠습니다.\n다음 Play 때 다운로드 과정이 다시 재현됩니다.\n\n(카탈로그 · 로컬 그룹 · 서버 파일은 그대로입니다.)",
                "확인"
            );
        }
        else
        {
            Debug.LogWarning("[Addr] 캐시를 비우지 못했습니다. 사용 중인 번들이 있을 수 있습니다(Play 를 멈춘 뒤 재시도).");
            EditorUtility.DisplayDialog
            (
                "Addressables Cache",
                "캐시를 비우지 못했습니다.\nPlay 를 멈춘 뒤 다시 시도하세요.",
                "확인"
            );
        }
    }
}
