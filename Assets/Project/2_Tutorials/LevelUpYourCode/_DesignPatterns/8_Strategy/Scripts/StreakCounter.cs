using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DesignPatterns.Strategy
{
    [System.Serializable]
    public struct AbilityThreshold
    {
        public Ability ability;
        public int     minimumStreak;
    }

    public class StreakCounter : MonoBehaviour
    {
        [SerializeField] private List<AbilityThreshold> m_AbilityThresholds;
        [SerializeField] private AbilityRunner          m_AbilityRunner;

        [Tooltip("연속 수집 수를 표시하는 UI 텍스트 요소")]
        [SerializeField] private TextMeshProUGUI        m_StreakText;

        private int m_CurrentStreak = 0;

        // 프로퍼티
        public int CurrentStreak
        {
            get => m_CurrentStreak;
            set
            {
                m_CurrentStreak = value;
                UpdateCurrentAbility();
                UpdateStreakText();
            }
        }

        private void OnEnable()
        {
            GameEvents.OnCollectibleCollected += IncrementStreak;
        }

        private void OnDisable()
        {
            GameEvents.OnCollectibleCollected -= IncrementStreak;
        }

        private void Start()
        {
            UpdateCurrentAbility();
            UpdateStreakText();
        }

        // 현재 연속 수집 수에서 사용 가능한 가장 높은 능력을 사용하도록 AbilityRunner를 업데이트한다
        private void UpdateCurrentAbility()
        {
            if (m_AbilityRunner == null)
                return;

            // 현재 연속 수집 수를 초과하지 않는 가장 높은 연속 능력을 찾는다
            var highestAbility = m_AbilityThresholds
                .Where(x => x.minimumStreak <= m_CurrentStreak)
                .OrderByDescending(x => x.minimumStreak)
                .FirstOrDefault().ability;

            if (highestAbility != null)
            {
                m_AbilityRunner.CurrentAbility = highestAbility;
            }
        }
        
        // 현재 연속 수집 수를 표시하도록 텍스트를 업데이트한다
        private void UpdateStreakText()
        {
            if (m_StreakText != null)
            {
                m_StreakText.text = m_CurrentStreak.ToString();
            }
        }

        // 연속 수집 수를 증가시킨다
        public void IncrementStreak()
        {
            CurrentStreak++;
        }
    }
}
