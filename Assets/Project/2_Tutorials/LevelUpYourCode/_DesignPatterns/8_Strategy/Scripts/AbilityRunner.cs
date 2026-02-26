using UnityEngine;
using UnityEngine.UI;

namespace DesignPatterns.Strategy
{
    public class AbilityRunner : MonoBehaviour
    {
        [Tooltip("현재 실행할 특수 능력")] [SerializeField]
        private Ability m_CurrentAbility;

        [Tooltip("현재 능력을 실행할 UI 버튼")] [SerializeField]
        private Button m_Button;

        // 프로퍼티
        public Ability CurrentAbility
        {
            get => m_CurrentAbility;
            set
            {
                m_CurrentAbility = value;
                UpdateButtonState();
            }
        }

        private void OnEnable()
        {
            if (m_Button == null)
            {
                m_Button.onClick.AddListener(OnAbilityButtonClicked);
            }
        }

        private void OnDisable()
        {
            if (m_Button != null)
            {
                m_Button.onClick.RemoveAllListeners();
            }
        }

        private void Start()
        {
            UpdateButtonState();
        }

        public void OnAbilityButtonClicked()
        {
            if (m_CurrentAbility != null)
            {
                m_CurrentAbility.Use(this.gameObject);
            }
        }

        // 버튼을 토글하고 해당 아이콘/스프라이트를 업데이트한다
        private void UpdateButtonState()
        {
            bool hasAbility = m_CurrentAbility != null;

            // CurrentAbility가 설정되지 않은 경우 버튼을 비활성화한다
            m_Button.gameObject.SetActive(hasAbility);

            // 능력이 설정된 경우 버튼 아이콘을 업데이트하고, 그렇지 않으면 초기화한다
            m_Button.image.sprite = hasAbility ? m_CurrentAbility.ButtonIcon : null;
        }
    }
}
