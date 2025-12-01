
namespace DesignPatterns.ISP
{
    /// <summary>
    /// 폭발할 수 있는 객체에 대한 계약을 정의합니다.
    /// </summary>
    public interface IExplodable
    {
        /// <summary>
        /// 폭발을 트리거합니다 (예 : 파티클 또는 다른 GameObject 이펙트)
        /// </summary>
        void Explode();
    }

}
