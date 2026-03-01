using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace DesignPatterns.Flyweight
{
    public class UnrefactoredShip : MonoBehaviour
    {
        [Tooltip("모든 함선 인스턴스에서 공유되는 문자열 데이터")]
        public string UnitName;

        [Space]
        [TextArea(5, 10)] public string Description;
        public float Speed;
        public int   AttackPower;
        public int   Defense;

        [Space]
        public Texture2D IconA;
        public Texture2D IconB;
        public Texture2D IconC;

        [Space]
        [Tooltip("다른 함선 인스턴스와 공유되지 않는 외재적(extrinsic) 체력 데이터")]
        public float Health;
    }
}
