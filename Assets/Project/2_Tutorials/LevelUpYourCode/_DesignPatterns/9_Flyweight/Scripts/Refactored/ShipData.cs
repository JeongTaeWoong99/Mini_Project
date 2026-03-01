using UnityEngine;

namespace DesignPatterns.Flyweight
{
    /// <summary>
    /// 플라이웨이트 패턴 : 모든 함선 인스턴스가 공유하는 내재적(intrinsic) 데이터 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "ShipData", menuName = "Flyweight/ShipData", order = 1)]
    public class ShipData : ScriptableObject
    {
        [Header("공유 데이터")]
        [Tooltip("모든 함선 인스턴스에서 공유되는 문자열 데이터")]
        public string UnitName;

        [TextArea(5, 10)]
        public string Description;

        [Tooltip("모든 함선 인스턴스에서 변경되지 않는 내재적(intrinsic) 속도 데이터")]
        public float Speed;

        [Tooltip("모든 함선 인스턴스에서 변경되지 않는 내재적(intrinsic) 공격력 데이터")]
        public int AttackPower;

        [Tooltip("모든 함선 인스턴스에서 변경되지 않는 내재적(intrinsic) 방어력 데이터")]
        public int Defense;

        public Texture2D IconA;
        public Texture2D IconB;
        public Texture2D IconC;
    }
}
