using UnityEngine;

namespace DesignPatterns.OCP
{
    public class UnrefactoredAreaCalculator
    {
        // Non-SOLID 구현 : 개방-폐쇄 원칙을 사용하지 않습니다.
        // 이 접근 방식은 적은 수의 효과에서는 잘 작동하지만,
        // 프로젝트가 커지면 확장성이 떨어지고 다루기 어려워집니다.

        public float GetRectangleArea(Rectangle rectangle)
        {
            return rectangle.Width * rectangle.Height;
        }

        public float GetCircleArea(Circle circle)
        {
            return circle.Radius * circle.Radius * Mathf.PI;
        }

        // 추가 도형에 대한 추가 메서드
        // 예 : GetPentagonArea, GetHexagonArea 등
    }

    public class Rectangle
    {
        public float Height;
        public float Width;
        
    }

    public class Circle
    {
        public float Radius;
    }
}  
