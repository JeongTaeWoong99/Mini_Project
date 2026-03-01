using UnityEngine;

namespace DesignPatterns.Flyweight
{
    /// <summary>
    /// 플라이웨이트 패턴 : 공유 데이터(ShipData)를 재사용하여 함선을 생성하는 팩토리
    /// </summary>
    public class ShipFactory : MonoBehaviour
    {
        [SerializeField] private Ship     m_ShipPrefab;
        [SerializeField] private ShipData m_SharedData;

        [Header("레이아웃")]
        [Tooltip("함선 간의 간격")]
        [SerializeField] private float m_Spacing   = 1.0f;
        [Tooltip("파동 운동의 최대 높이")]
        [SerializeField] private float m_Amplitude = 0.075f;
        [Tooltip("파동 운동의 진동 속도")]
        [SerializeField] private float m_Frequency = 0.3f;

        void Start()
        {
            GenerateShips(10, 10);
        }

        public void GenerateShips(int rows, int columns)
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    // 위치 계산
                    Vector3 position = new Vector3(i * m_Spacing, 0, j * m_Spacing);

                    // 함선 인스턴스 생성 및 초기화
                    Ship newShip = Instantiate(m_ShipPrefab, position, Quaternion.identity, transform);

                    // 시작 체력 100으로 설정
                    newShip.Initialize(m_SharedData, 100);

                    // 사인파 이동 추가 (선택)
                    SineWaveMover oscillation = newShip.gameObject.AddComponent<SineWaveMover>();
                    oscillation.Amplitude = m_Amplitude;
                    oscillation.Frequency = m_Frequency;

                    // 함선 이름 설정 (선택)
                    newShip.name = $"Ship_{i * columns + j}";
                }
            }
        }
    }
}
