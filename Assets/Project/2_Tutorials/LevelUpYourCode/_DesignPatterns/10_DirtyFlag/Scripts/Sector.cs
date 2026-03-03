using System;
using DesignPatterns.Utilities;
using UnityEngine;
using UnityEngine.Serialization;


namespace DesignPatterns.DirtyFlag
{
    /// <summary>
    /// 각 섹터는 플레이어와의 근접도에 따라 레벨의 특정 구역에 대한 콘텐츠 로드/언로드를 관리합니다.
    ///
    /// GameSectors 스크립트와 연동하여 더티 플래그를 설정/해제함으로써 불필요한 업데이트를 최소화합니다.
    /// </summary>
    public class Sector : MonoBehaviour
    {
        [Header("씬 에셋")]
        [SerializeField] SceneLoader m_SceneLoader;
        [SerializeField] string      m_ScenePath;

        [Tooltip("트랜스폼 위치에 대한 오프셋")]
        public Vector3 m_CenterOffset;

        [Tooltip("로드를 시작할 최소 거리")]
        public float m_LoadRadius;

        [Header("시각화")]
        [Tooltip("섹터 콘텐츠가 로드되었을 때 사용할 머티리얼")]
        [SerializeField] Material m_ActiveMaterial;

        [Tooltip("섹터 콘텐츠가 언로드되었을 때 사용할 머티리얼")]
        [SerializeField] Material m_InactiveMaterial;

        // 시각화를 위한 MeshRenderer 참조
        MeshRenderer m_MeshRenderer;

        // 프로퍼티
        public bool IsLoaded { get; private set; } = false;
        public bool IsDirty  { get; private set; } = false;

        void Awake()
        {
            m_MeshRenderer = GetComponent<MeshRenderer>();
            m_SceneLoader  = FindFirstObjectByType<SceneLoader>();

            if (m_SceneLoader == null)
            {
                Debug.LogError("[Sector] : 씬에서 SceneLoader를 찾을 수 없습니다.");
            }

            // 시작 시 더티 플래그 초기화
            Clean();

            IsLoaded = false;
        }

        // 섹터를 업데이트가 필요한 상태(더티)로 표시
        public void MarkDirty()
        {
            IsDirty = true;

            Debug.Log("섹터 " + gameObject.name + " 가 더티로 표시되었습니다.");
        }

        // 섹터 콘텐츠 로드
        public void LoadContent()
        {
            // 콘텐츠 로드 로직 구현
            IsLoaded = true;

            if (m_MeshRenderer != null)
                m_MeshRenderer.material = m_ActiveMaterial;

            Debug.Log($"{gameObject.name} 섹터 콘텐츠를 로드하는 중...");

            // [SceneLoader.LoadSceneAdditivelyByPath 내부 동작]
            // 1. SceneManager.GetSceneByPath()로 해당 씬이 이미 로드되어 있는지 확인 (중복 방지)
            // 2. 처음 로드 시 m_AdditiveScenes 리스트에 씬을 추가하여 추적
            // 3. LoadSceneAsync(path, LoadSceneMode.Additive) 코루틴으로 비동기 로드
            //    → 기존 씬을 유지하면서 새 씬을 위에 추가로 올리는 방식 (Additive)
            // 4. 로드 완료 후 새로 추가된 씬을 ActiveScene으로 설정
            if (!string.IsNullOrEmpty(m_ScenePath))
                m_SceneLoader.LoadSceneAdditivelyByPath(m_ScenePath);
        }

        // 섹터 콘텐츠 언로드
        public void UnloadContent()
        {
            // 콘텐츠 언로드 로직
            IsLoaded = false;

            if (m_MeshRenderer != null)
                m_MeshRenderer.material = m_InactiveMaterial;

            // [SceneLoader.UnloadSceneByPath 내부 동작]
            // 1. SceneManager.GetSceneByPath()로 씬 참조를 가져옴
            // 2. 씬이 유효한 경우 UnloadScene 코루틴을 시작
            // 3. 내부적으로 UnloadSceneAsync(scene)로 비동기 언로드 처리
            //    → GameObject는 즉시 제거되지만, 에셋(Texture, Mesh 등)은 메모리에 잔류
            //    → 에셋까지 완전히 해제하려면 Resources.UnloadUnusedAssets() 추가 호출 필요
            // 4. 씬이 1개만 남은 경우 언로드를 중단 (최소 1개 씬 유지 보장)
            m_SceneLoader.UnloadSceneByPath(m_ScenePath);
            Debug.Log("섹터 콘텐츠를 언로드하는 중...");
        }

        // 플레이어가 이 섹터를 로드할 만큼 충분히 가까운지 확인
        public bool IsPlayerClose(Vector3 playerPosition)
        {
            return Vector3.Distance(playerPosition, transform.position + m_CenterOffset) <= m_LoadRadius;
        }

        // 업데이트 후 더티 플래그 초기화
        public void Clean()
        {
            IsDirty = false;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position + m_CenterOffset, m_LoadRadius);
        }

        void OnDestroy()
        {
            // [SceneLoader.UnloadSceneImmediately 내부 동작]
            // 1. SceneManager.GetSceneByPath()로 씬 참조를 가져옴
            // 2. UnloadSceneAsync와 달리 SceneManager.UnloadScene()으로 즉시(동기) 언로드
            //    → 코루틴 없이 그 자리에서 바로 처리하므로 OnDestroy 타이밍에 적합
            // 3. 언로드 성공 시 m_AdditiveScenes 리스트에서 해당 씬을 제거
            m_SceneLoader.UnloadSceneImmediately(m_ScenePath);
        }
    }
}
