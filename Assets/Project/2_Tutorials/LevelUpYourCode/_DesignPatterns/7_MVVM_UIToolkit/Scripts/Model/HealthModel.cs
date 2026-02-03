using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DesignPatterns.MVVM
{
    /// <summary>
    /// MVVM 디자인 패턴을 따르는 체력 데이터 컨테이너.
    /// </summary>
    [CreateAssetMenu(fileName = "TargetHealth", menuName = "DesignPatterns/MVVM/TargetHealth")]
    public class HealthModel : ScriptableObject
    {
        // ViewModel과 통신하기 위한 이벤트
        // public event Action HealthChanged;

        private const int k_MinHealth = 0;
        private const int k_MaxHealth = 100;

        [Tooltip("체력 오브젝트의 ID")]
        public string LabelName;

        [Space]
        [Tooltip("현재 체력 값, 데이터 바인딩을 위해 public")]
        public int CurrentHealth;

        public int MinHealth => k_MinHealth;
        public int MaxHealth => k_MaxHealth;

        // 런타임 인스턴스를 반환 (각 오브젝트가 독립적인 데이터로 동작)
        public static HealthModel CreateInstance(HealthModel original)
        {
            var newInstance = CreateInstance<HealthModel>();

            // 필요한 필드 복사
            newInstance.CurrentHealth = original.CurrentHealth;
            newInstance.LabelName     = original.LabelName;
            return newInstance;
        }

        /// <summary>
        /// 데이터 바인딩용 컨버터 초기화. 원본 체력 값(정수)을 UI 프로퍼티에 호환되는
        /// 형식(예 : 배경 스타일용 색상, 텍스트 프로퍼티용 문자열)으로 변환한다.
        /// 필요에 따라 추가 컨버터를 등록할 수 있다.
        /// </summary>
        [InitializeOnLoadMethod]
        public static void RegisterConverters()
        {
            // 정수 값을 0~1 사이의 float으로 변환하는 람다
            float HealthRatio(int health) => health / (float)k_MaxHealth;

            // 쉽게 접근할 수 있도록 ConverterGroup 이름 지정
            var converter = new ConverterGroup("Int to HealthBar");

            // 체력 값을 색상으로 변환
            converter.AddConverter((ref int value) =>
                new StyleColor(Color.Lerp(Color.red, Color.green, HealthRatio(value))));

            // 체력 값을 텍스트로 변환
            converter.AddConverter((ref int value) =>
            {
                float healthRatio = HealthRatio(value);
                return healthRatio switch
                {
                    >= 0 and < 1.0f / 3.0f           => "Danger",  // 위험
                    >= 1.0f / 3.0f and < 2.0f / 3.0f => "Neutral", // 보통
                    _                                 => "Good"    // 양호
                };
            });

            // InitializeOnLoadMethod에서 컨버터 그룹을 등록하여 UI Builder에서 접근 가능하게 함
            ConverterGroups.RegisterConverterGroup(converter);
        }

        // Inspector에서 입력 시 유효 범위로 클램핑
        private void OnValidate()
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth, k_MinHealth, k_MaxHealth);
        }

        private void OnEnable()
        {
            // 씬 재시작 시 오브젝트가 활성화되면 체력을 최대값으로 초기화
            Restore();
        }

        // 체력 증가
        public void Increment(int amount)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, k_MinHealth, k_MaxHealth);
        }

        // 체력 감소
        public void Decrement(int amount)
        {
            CurrentHealth = Mathf.Clamp(CurrentHealth - amount, k_MinHealth, k_MaxHealth);
        }

        // 체력을 최대값으로 복원
        public void Restore()
        {
            CurrentHealth = k_MaxHealth;
        }
    }
}
