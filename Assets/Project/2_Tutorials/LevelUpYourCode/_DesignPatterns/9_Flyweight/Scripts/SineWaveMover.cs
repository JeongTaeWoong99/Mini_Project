using UnityEngine;
using UnityEngine.Serialization;

namespace DesignPatterns.Flyweight
{
    /// <summary>
    /// 진폭과 주파수를 조절할 수 있는 사인파 운동 컴포넌트.
    /// 트랜스폼의 초기 위치를 기반으로 위상 오프셋을 추가한다.
    /// </summary>
    public class SineWaveMover : MonoBehaviour
    {
        [Tooltip("파동의 최대 높이")]
        [SerializeField] private float m_Amplitude = 1.0f; // 사인파의 높이
        [Tooltip("파동의 진동 속도")]
        [SerializeField] private float m_Frequency = 1.0f; // 파동이 진동하는 속도

        private Vector3 m_StartPosition;
        private float   m_PhaseOffset;

        // 프로퍼티
        public float Amplitude { get => m_Amplitude; set => m_Amplitude = value; }
        public float Frequency { get => m_Frequency; set => m_Frequency = value; }

        void Start()
        {
            // 트랜스폼의 초기 위치 저장
            m_StartPosition = transform.position;

            // 파동 효과를 다양화하기 위해 초기 위치 기반 위상 오프셋 생성
            m_PhaseOffset = m_StartPosition.x + m_StartPosition.z;
        }

        void Update()
        {
            // 사인파 기반 새 위치 계산
            float sineValue = Mathf.Sin((Time.time + m_PhaseOffset) * m_Frequency) * m_Amplitude;

            // y축에 사인파 효과 적용
            transform.position = m_StartPosition + Vector3.up * sineValue;
        }
    }
}
