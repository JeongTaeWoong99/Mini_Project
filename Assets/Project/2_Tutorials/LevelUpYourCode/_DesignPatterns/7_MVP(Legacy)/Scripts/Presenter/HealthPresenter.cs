using DesignPatterns.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace DesignPatterns.MVP
{
    // Presenter 클래스. View(UI)의 변경사항을 감지하고 Model(Health)을 조작한다.
    // Model이 변경되면 View를 업데이트한다.
    public class HealthPresenter : MonoBehaviour
    {
        [Header("Model")]
        [Tooltip("체력 데이터를 포함하는 오브젝트")]
        [SerializeField] Health m_Health;

        [Header("View")]
        [Tooltip("체력바를 나타내는 UI 슬라이더")]
        [SerializeField] Slider m_HealthSlider;
        [Optional]
        [SerializeField] Text m_HealthLabel;

        private void Awake()
        {
            NullRefChecker.Validate(this);
        }

        private void Start()
        {
            m_Health.HealthChanged += Health_HealthChanged;
            InitializeSlider();

            Reset();
            UpdateView();
        }

        private void OnDestroy()
        {
            m_Health.HealthChanged -= Health_HealthChanged;
        }

        // 슬라이더 최대값을 Model의 최대 체력으로 초기화
        private void InitializeSlider()
        {
            m_HealthSlider.maxValue = m_Health.MaxHealth;
        }

        // Model에 데미지 전달
        public void Damage(int amount)
        {
            m_Health.Decrement(amount);
        }

        // Model에 회복 전달
        public void Heal(int amount)
        {
            m_Health.Increment(amount);
        }

        // Model 체력을 최대값으로 리셋
        public void Reset()
        {
            m_Health.Restore();
        }

        // View 업데이트 : Model 데이터를 UI에 반영
        public void UpdateView()
        {
            if (m_Health == null)
                return;

            // 데이터를 View 형식에 맞게 변환
            if (m_Health.MaxHealth != 0)
            {
                m_HealthSlider.value = ((float)m_Health.CurrentHealth / (float)m_Health.MaxHealth) * 100f;
            }

            if (m_HealthLabel != null)
            {
                m_HealthLabel.text = m_Health.CurrentHealth.ToString();
            }
        }

        // 이벤트 핸들러 : Model 변경을 감지하여 View 업데이트
        public void Health_HealthChanged()
        {
            UpdateView();
        }
    }
}
