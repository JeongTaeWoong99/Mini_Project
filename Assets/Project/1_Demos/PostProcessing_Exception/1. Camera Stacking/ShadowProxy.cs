using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// NoPostProcessing 레이어 오브젝트에 붙여서 그림자 프록시를 자동 생성한다.
/// 프록시는 Default 레이어에 Shadow-Only 모드로 생성되어 Main Camera의
/// Shadow Caster Pass에 포함되므로 그림자가 정상적으로 렌더링된다.
/// </summary>
public class ShadowProxy : MonoBehaviour
{
    // Private variables
    private GameObject   proxyObject;
    private MeshRenderer proxyRenderer;

    void Start()
    {
        MeshFilter   originFilter   = GetComponent<MeshFilter>();
        MeshRenderer originRenderer = GetComponent<MeshRenderer>();

        if (originFilter == null || originRenderer == null)
        {
            Debug.LogWarning($"[ShadowProxy] {name} : MeshFilter 또는 MeshRenderer가 없습니다.");
            enabled = false;
            return;
        }

        proxyObject = new GameObject($"{name}_ShadowProxy");

        // Default 레이어에 배치하여 Main Camera Shadow Caster Pass에 포함
        proxyObject.layer = 0;

        MeshFilter proxyFilter = proxyObject.AddComponent<MeshFilter>();
        proxyFilter.sharedMesh = originFilter.sharedMesh;

        proxyRenderer                    = proxyObject.AddComponent<MeshRenderer>();
        proxyRenderer.sharedMaterials    = originRenderer.sharedMaterials;
        proxyRenderer.shadowCastingMode  = ShadowCastingMode.ShadowsOnly; // 그림자만 드리움
        proxyRenderer.receiveShadows     = false;
        proxyRenderer.lightProbeUsage    = LightProbeUsage.Off;
        proxyRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;

        SyncProxyTransform();
    }

    void LateUpdate()
    {
        SyncProxyTransform();
    }

    void OnDestroy()
    {
        if (proxyObject != null)
            Destroy(proxyObject);
    }

    private void SyncProxyTransform()
    {
        if (proxyObject == null) return;

        proxyObject.transform.position   = transform.position;
        proxyObject.transform.rotation   = transform.rotation;
        proxyObject.transform.localScale = transform.lossyScale;
    }
}
