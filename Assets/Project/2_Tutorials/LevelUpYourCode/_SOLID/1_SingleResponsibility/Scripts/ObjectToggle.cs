using UnityEngine;

namespace DesignPatterns.SRP
{
    /// <summary>
    /// Player와 리팩토링되지 않은 로직 간 전환을 위한 데모 스크립트입니다.
    /// </summary>
    public class ObjectToggle : MonoBehaviour
    {
        [SerializeField] private GameObject m_FirstObject;
        [SerializeField] private GameObject m_SecondObject;
        [SerializeField] private KeyCode    toggleKey = KeyCode.T; // 오브젝트 전환에 사용되는 키. 기본값은 'T'

        private void Start()
        {
            SyncObjectPositions();
        }

        private void Update()
        {
            // 토글 키가 눌렸는지 확인
            if (Input.GetKeyDown(toggleKey))
            {
                SyncObjectPositions();
                ToggleObjects();
            }
        }

        /// <summary>
        /// Player 오브젝트의 활성 상태를 전환합니다.
        /// </summary>
        public void ToggleObjects()
        {
            // null 참조 에러를 방지하기 위해 두 오브젝트가 할당되었는지 확인
            if (m_FirstObject == null || m_SecondObject == null)
            {
                Debug.LogWarning("[ObjectToggle] ToggleObjects : 하나 또는 두 오브젝트가 할당되지 않았습니다.");
                return;
            }

            m_FirstObject.SetActive(!m_FirstObject.activeSelf);
            m_SecondObject.SetActive(!m_SecondObject.activeSelf);
        }

        /// <summary>
        /// 비활성 오브젝트를 활성 오브젝트와 동기화합니다.
        /// </summary>
        public void SyncObjectPositions()
        {
            if (m_FirstObject == null || m_SecondObject == null)
            {
                Debug.LogWarning("[ObjectToggle] SyncObjectPositions : 하나 또는 두 오브젝트가 할당되지 않았습니다.");
                return;
            }

            if (m_FirstObject.activeInHierarchy && !m_SecondObject.activeInHierarchy)
            {
                m_SecondObject.transform.position = m_FirstObject.transform.position;
            }
            else if (!m_FirstObject.activeInHierarchy && m_SecondObject.activeInHierarchy)
            {
                m_FirstObject.transform.position = m_SecondObject.transform.position;
            }
        }
    }

}
