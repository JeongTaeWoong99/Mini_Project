using UnityEngine;

namespace DesignPatterns.DIP
{
    /// <summary>
    /// (스위치 오브젝트에 붙여, ISwitchable을 상속하는 문과 트랩등을 m_Client로 제어)
    /// ISwitchable 클라이언트의 상태를 전환할 수 있는 스위치 컴포넌트입니다. 이 클래스는
    /// 구체적인 구현이 아닌 추상화(ISwitchable)에 의존함으로써 의존 역전 원칙을 보여줍니다.
    /// </summary>
    public class Switch : MonoBehaviour
    {
        // ★ TIP : Unity의 직렬화 시스템은 인터페이스를 직접 지원하지 않습니다. 이 제한을 우회하기 위해
        // ★ TIP : ISwitchable을 구현하는 MonoBehaviour에 대한 직렬화된 참조를 사용합니다.
        [SerializeField] 
        private MonoBehaviour m_ClientBehaviour;
        private ISwitchable m_Client => m_ClientBehaviour as ISwitchable;

        // 연결된 ISwitchable 클라이언트의 활성 상태를 전환합니다.
        public void Toggle()
        {
            if (m_Client == null) 
                return;

            if (m_Client.IsActive)
            {
                m_Client.Deactivate();
            }
            else
            {
                m_Client.Activate();
            }
        }
    }
}
