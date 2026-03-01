using UnityEngine;

namespace DesignPatterns
{
    /// <summary>
    /// 플라이웨이트 패턴 적용 전/후 GameObject를 토글하는 컴포넌트
    /// </summary>
    public class ToggleFlyweight : MonoBehaviour
    {
        [Tooltip("플라이웨이트 패턴을 적용한 최상위 GameObject")]
        [SerializeField] private GameObject m_Flyweight;
        [Tooltip("플라이웨이트 패턴 적용 전 최상위 GameObject")]
        [SerializeField] private GameObject m_NonFlyweight;

        // 두 GameObject 활성화 상태를 토글
        public void Toggle()
        {
            // m_Flyweight의 현재 활성화 상태 확인
            bool isFlyweightActive = m_Flyweight.activeSelf;

            // 두 GameObject의 활성화 상태를 반전
            SetActive(!isFlyweightActive);
        }

        public void SetActive(bool state)
        {
            if (m_NonFlyweight != null)
                m_NonFlyweight.SetActive(!state);

            if (m_Flyweight != null)
                m_Flyweight.SetActive(state);
        }
    }
}
