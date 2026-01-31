using UnityEngine;
using UnityEngine.UI;

namespace DesignPatterns.StatePattern
{
    /// <summary>
    /// 내부 상태 변화에 반응하는 사용자 인터페이스
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerStateView : MonoBehaviour
    {
        [SerializeField] private Text m_LabelText;

        private PlayerController         m_Player;
        private SimplePlayerStateMachine m_PlayerStateMachine;

        // 색상을 변경할 메시
        private MeshRenderer m_MeshRenderer;

        private void Awake()
        {
            m_Player       = GetComponent<PlayerController>();
            m_MeshRenderer = GetComponent<MeshRenderer>();

            // 타이핑을 줄이기 위해 캐싱
            m_PlayerStateMachine = m_Player.PlayerStateMachine;

            // 모든 상태 변경을 수신
            m_PlayerStateMachine.stateChanged += OnStateChanged;
        }

        void OnDestroy()
        {
            // 오브젝트를 파괴할 때 구독 해제
            m_PlayerStateMachine.stateChanged -= OnStateChanged;
        }

        // 상태가 변경될 때 UI.Text를 변경
        private void OnStateChanged(IState state)
        {
            if (m_LabelText != null)
            {
                m_LabelText.text  = state.GetType().Name;
                m_LabelText.color = state.MeshColor;
            }

            ChangeMeshColor(state);
        }

        // 메시 머티리얼을 현재 상태의 연관된 색상으로 설정
        private void ChangeMeshColor(IState state)
        {
            if (m_MeshRenderer == null)
            {
                return;
            }

            // meshRenderer.sharedMaterial.color = state.MeshColor;
            m_MeshRenderer.material.SetColor("_BaseColor", state.MeshColor);
        }
    }
}
