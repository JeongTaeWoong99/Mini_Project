using UnityEngine;
using UnityEngine.Serialization;

namespace DesignPatterns.Flyweight
{

    /// <summary>
    /// 플라이웨이트 패턴 : 공유 데이터(ShipData)와 고유 데이터(Health)를 분리한 함선 컴포넌트
    /// </summary>
    public class Ship : MonoBehaviour
    {
        [Header("공유 데이터")]
        [Tooltip("공유 ShipData ScriptableObject 참조")]
        [SerializeField] private ShipData m_SharedData;

        [Header("고유 데이터")]
        [Tooltip("이 외재적(extrinsic) 체력 데이터는 다른 함선 인스턴스와 공유되지 않음")]
        [SerializeField] private float m_Health;

        // 공유 데이터로 함선 초기화 또는 업데이트
        public void Initialize(ShipData data, float health)
        {
            m_SharedData = data;
            
            m_Health     = health;
        }

        // 사용 예시 메서드
        public void DisplayShipInfo()
        {
            Debug.Log($"이름 : {m_SharedData.UnitName}, 체력 : {m_Health}");
        }
    }
}
