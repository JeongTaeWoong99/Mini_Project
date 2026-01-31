using UnityEngine;

namespace DesignPatterns.StatePattern
{
    // 예제 프로젝트를 위해 추가된 인터페이스,
    // 플레이어의 색상을 변경하는 데 사용

    public interface IColorable
    {
        public Color MeshColor { get; set; }
    }
}
