using UnityEngine;
using DesignPatterns.Utilities;

namespace DesignPatterns.OCP
{
    /// <summary>
    /// 각 효과 영역은 고유한 면적 계산 공식을 구현할 수 있습니다.
    /// 새로운 AreaOfEffect를 생성해도 기존 클래스에 영향을 주지 않으며,
    /// 이는 개방-폐쇄 원칙(Open Closed Principle)을 따릅니다.
    /// </summary>
    public class CircleEffect : AreaOfEffect
    {
        [Header("Shape")]
        [Tooltip("원의 반지름")]
        [SerializeField] float m_Radius;

        public float Radius { get => m_Radius; set => m_Radius = value; }

        public override float CalculateArea()
        {
            return Radius * Radius * Mathf.PI;
        }
    }
}

