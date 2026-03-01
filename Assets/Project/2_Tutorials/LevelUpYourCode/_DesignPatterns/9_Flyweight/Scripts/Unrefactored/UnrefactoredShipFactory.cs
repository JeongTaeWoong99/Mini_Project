using UnityEngine.Serialization;

namespace DesignPatterns.Flyweight
{
    using UnityEngine;

    public class UnrefactoredShipFactory : MonoBehaviour
    {
        [SerializeField] private UnrefactoredShip m_ShipPrefab;

        [Header("레이아웃")]
        [SerializeField] private float m_Spacing = 1.0f; // 함선 간격

        [Tooltip("파동 운동의 최대 높이")]
        [SerializeField] private float m_Amplitude = 0.075f;

        [Tooltip("파동 운동의 진동 속도")]
        [SerializeField] private float m_Frequency = 0.3f;

        [Header("함선 설정 (복사됨)")]
        [SerializeField] private string m_UnitName = "SpaceFighter";

        [TextArea(5, 10)]
        [SerializeField] private string m_Description = ".";

        [Space]
        [SerializeField] private float m_Speed       = 5f;
        [SerializeField] private int   m_AttackPower = 100;
        [SerializeField] private int   m_Defense     = 50;

        [Space]
        [SerializeField] private Texture2D m_IconA;
        [SerializeField] private Texture2D m_IconB;
        [SerializeField] private Texture2D m_IconC;

        [Space]
        [Tooltip("시작 체력")]
        [SerializeField] private float m_MaxHealth = 100f;

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

                    // 함선 인스턴스 생성
                    UnrefactoredShip newShip = Instantiate(m_ShipPrefab, position, Quaternion.identity, transform);

                    // 공유 데이터 복사
                    newShip.UnitName    = m_UnitName;
                    newShip.Description = m_Description;
                    newShip.Speed       = m_Speed;
                    newShip.AttackPower = m_AttackPower;
                    newShip.Defense     = m_Defense;
                    newShip.IconA       = m_IconA;
                    newShip.IconB       = m_IconB;
                    newShip.IconC       = m_IconC;
                    newShip.Health      = m_MaxHealth;

                    // 사인파 이동 추가 (선택)
                    SineWaveMover oscillation = newShip.gameObject.AddComponent<SineWaveMover>();
                    oscillation.Amplitude = m_Amplitude;
                    oscillation.Frequency = m_Frequency;

                    // 식별을 위한 이름 설정 (선택)
                    newShip.name = $"Ship_{i * columns + j}";
                }
            }
        }
    }
}
