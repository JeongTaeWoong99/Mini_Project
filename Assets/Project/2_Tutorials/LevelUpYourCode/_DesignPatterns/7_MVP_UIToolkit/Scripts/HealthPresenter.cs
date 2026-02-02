using UnityEngine;
using DesignPatterns.Utilities;
using Unity.Properties;
using UnityEngine.UIElements;

namespace DesignPatterns.MVP_UIToolkit
{
    /// <summary>
    /// Model(데이터)과 View(UI) 사이의 중재자 역할.
    /// HealthModel의 변경사항을 감지하고 리스너에게 알린다.
    /// UI에서 발생하는 사용자 상호작용을 처리하여 HealthModel에 반영한다.
    /// </summary>
    public class HealthPresenter : MonoBehaviour
    {
        [Tooltip("View(UI) 참조")]
        [SerializeField] private UIDocument  m_Document;

        [Tooltip("Model 데이터(ScriptableObject 에셋) 참조")]
        [SerializeField] private HealthModel m_HealthModelAsset;

        // UI 루트 요소
        private VisualElement m_Root;

        // UI 요소
        private ProgressBar m_HealthBar;
        private Label       m_StatusLabel;
        private Label       m_ValueLabel;

        private void OnEnable()
        {
            // 유틸리티 헬퍼 클래스로 필수 필드 검증
            NullRefChecker.Validate(this);

            // 루트 요소 캐싱
            m_Root = m_Document.rootVisualElement;

            // UI 요소 검색 및 할당
            m_HealthBar   = m_Root.Q<ProgressBar>("health-bar");
            m_StatusLabel = m_Root.Q<Label>("health-bar__status-label");
            m_ValueLabel  = m_Root.Q<Label>("health-bar__value-label");

            // UI 상호작용 및 버튼 설정
            RegisterElements();

            if (m_HealthModelAsset != null)
            {
                m_HealthBar.title = m_HealthModelAsset.LabelName;
                m_HealthModelAsset.HealthChanged += OnHealthChanged;
            }

            UpdateUI();
        }

        // 체력 변경 이벤트 핸들러
        private void OnHealthChanged()
        {
            UpdateUI();
        }

        private void OnDisable()
        {
            if (m_HealthModelAsset != null)
            {
                m_HealthModelAsset.HealthChanged -= OnHealthChanged;
            }
        }

        // View와 상호작용하기 위한 메서드
        private void RegisterElements()
        {
            // UXML에서 버튼 검색
            var resetButton = m_Root.Q<Button>("reset-button");

            // 리셋 버튼의 clicked 이벤트에 RestoreHealth 메서드 구독
            if (resetButton != null)
            {
                resetButton.clicked += RestoreHealth;
            }
        }

        // UI 업데이트
        private void UpdateUI()
        {
            // 체력 비율 계산
            float healthRatio = (float)m_HealthModelAsset.CurrentHealth / m_HealthModelAsset.MaxHealth;

            // 색상 보간 (빨강 → 초록)
            Color healthColor = Color.Lerp(Color.red, Color.green, healthRatio);

            // 체력바 값 업데이트
            m_HealthBar.value = healthRatio * 100f;

            // 체력바 색상 업데이트
            var healthBarProgress = m_HealthBar?.Q<VisualElement>(className: "unity-progress-bar__progress");
            if (healthBarProgress != null)
            {
                healthBarProgress.style.backgroundColor = new StyleColor(healthColor);
            }

            // 체력 비율에 따른 상태 라벨 업데이트
            m_StatusLabel.text = healthRatio switch
            {
                >= 0 and < 1.0f / 3.0f           => "Danger",  // 위험
                >= 1.0f / 3.0f and < 2.0f / 3.0f => "Neutral", // 보통
                _                                 => "Good"     // 양호
            };

            // 상태 라벨 색상을 보간된 색상으로 변경
            m_StatusLabel.style.color = new StyleColor(healthColor);

            // 수치 라벨 업데이트
            m_ValueLabel.text = m_HealthModelAsset.CurrentHealth.ToString();
        }

        // Model과 상호작용 : 체력 복원
        public void RestoreHealth()
        {
            m_HealthModelAsset.Restore();
        }

        // Model과 상호작용 : 데미지 적용
        public void ApplyDamage(int damage)
        {
            m_HealthModelAsset.Decrement(damage);
        }
    }
}
