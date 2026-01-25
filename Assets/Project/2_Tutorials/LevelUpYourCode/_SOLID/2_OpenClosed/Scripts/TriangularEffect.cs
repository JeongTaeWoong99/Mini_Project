using UnityEngine;

namespace DesignPatterns.OCP
{
    /// <summary>
    /// 정삼각형 효과를 표시하는 클래스.
    ///
    /// 각 효과 영역은 고유한 면적 계산 공식을 구현할 수 있습니다.
    /// 새로운 AreaOfEffect를 생성해도 기존 클래스에 영향을 주지 않으며,
    /// 이는 개방-폐쇄 원칙(Open Closed Principle)을 따릅니다.
    /// </summary>
    public class TriangularEffect : AreaOfEffect
    {
        [Header("Shape")]
        [Tooltip("삼각형의 한 변 길이")]
        [SerializeField] private float m_SideLength;

        public override float CalculateArea()
        {
            return (Mathf.Sqrt(3) / 4) * m_SideLength * m_SideLength;
        }
    }
}