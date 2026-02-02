using UnityEngine;
using System;
using DesignPatterns.Utilities;
using Unity.Properties;
using UnityEngine.UIElements;

namespace DesignPatterns.MVVM
{
    /// <summary>
    /// Model(데이터)과 View(UI) 사이의 중재자 역할.
    /// HealthModel의 변경사항을 감지하고 리스너에게 알린다.
    /// UI에서 발생하는 사용자 상호작용을 처리하여 HealthModel에 반영한다.
    /// </summary>
    public class HealthViewModel : MonoBehaviour
    {
        [Tooltip("View(UI) 참조")]
        [SerializeField] private UIDocument  m_Document;

        [Tooltip("Model 데이터(ScriptableObject 에셋) 참조")]
        [SerializeField] private HealthModel m_HealthModelAsset;

        // UI 루트 요소
        private VisualElement m_Root;

        private void OnEnable()
        {
            // 필수 필드 검증
            NullRefChecker.Validate(this);

            // 루트 요소 캐싱
            m_Root = m_Document.rootVisualElement;

            // UI 상호작용 및 버튼 설정
            RegisterElements();

            // HealthModel을 UI에 바인딩
            SetDataBindings();
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

        // HealthModel과 UI 요소 사이의 데이터 바인딩을 설정하여 자동 업데이트 활성화
        private void SetDataBindings()
        {
            // UI 요소 탐색
            var healthBar         = m_Root.Q<ProgressBar>("health-bar");
            var healthBarProgress = healthBar?.Q<VisualElement>(className: "unity-progress-bar__progress");

            if (healthBarProgress != null)
            {
                // 요소에 데이터 소스로 오브젝트 정의 (이 경우 ScriptableObject)
                healthBarProgress.dataSource = m_HealthModelAsset;

                // 체력바 배경 색상을 위한 새로운 데이터 바인딩 생성
                var binding = new DataBinding
                {
                    dataSourcePath = new PropertyPath(nameof(HealthModel.CurrentHealth)),

                    // 데이터 소스에서 UI로 단방향 바인딩
                    bindingMode = BindingMode.ToTarget,
                };

                // int 값을 색상으로 매핑하는 공식 추가
                binding.sourceToUiConverters.AddConverter((ref int value) =>
                    new StyleColor(Color.Lerp(Color.red, Color.green,
                        (float)value / (float)m_HealthModelAsset.MaxHealth)));

                // 체력바 프로그레스 요소에 바인딩 등록
                healthBarProgress.SetBinding("style.backgroundColor", binding);
            }
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
