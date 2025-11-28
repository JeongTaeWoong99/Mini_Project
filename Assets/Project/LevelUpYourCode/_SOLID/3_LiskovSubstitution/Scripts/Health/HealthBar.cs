using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace DesignPatterns.LSP
{
    /// <summary>
    /// Health 컴포넌트의 체력 변화를 UI 슬라이더로 표시하는 클래스
    /// 체력 변화 시 부드러운 애니메이션을 제공합니다.
    /// </summary>
    [RequireComponent(typeof(Slider))]
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Health m_Health;
        [SerializeField] private Slider m_HealthSlider;
        [SerializeField] private Text   m_HealthLabel;

        [Tooltip("선형 보간이 진행되는 시간")]
        [SerializeField] private float m_LerpDuration = 0.5f;

        private Coroutine m_LerpCoroutine;

        private void OnEnable()
        {
            m_Health.HealthChanged.AddListener(UpdateHealthBar);

            if (m_HealthSlider == null)
                m_HealthSlider = GetComponent<Slider>();
        }

        private void OnDisable()
        {
            m_Health.HealthChanged.RemoveListener(UpdateHealthBar);
        }

        private void Start()
        {
            UpdateHealthBar(m_Health.CurrentHealth / m_Health.MaxHealth);
        }

        /// <summary>
        /// 체력바를 업데이트합니다.
        /// </summary>
        /// <param name="healthValue">현재 체력 비율 (0~1)</param>
        private void UpdateHealthBar(float healthValue)
        {
            // 기존 보간 코루틴이 실행 중이면 중지
            if (m_LerpCoroutine != null)
            {
                StopCoroutine(m_LerpCoroutine);
            }
            m_LerpCoroutine = StartCoroutine(LerpHealthBar(healthValue));

            m_HealthSlider.value = healthValue;

            // 체력 퍼센트 텍스트 업데이트
            if (m_HealthLabel != null)
            {
                m_HealthLabel.text = Mathf.RoundToInt(healthValue * 100f).ToString();
            }
        }

        /// <summary>
        /// 간단한 애니메이션을 위한 선형 보간을 추가합니다.
        /// </summary>
        private IEnumerator LerpHealthBar(float targetValue)
        {
            float elapsedTime = 0;
            float startValue  = m_HealthSlider.value;

            while (elapsedTime < m_LerpDuration)
            {
                m_HealthSlider.value = Mathf.Lerp(startValue, targetValue, elapsedTime / m_LerpDuration);
                elapsedTime         += Time.deltaTime;
                yield return null;
            }

            // 완료되면 목표 값으로 설정
            m_HealthSlider.value = targetValue;
        }
    }
}
