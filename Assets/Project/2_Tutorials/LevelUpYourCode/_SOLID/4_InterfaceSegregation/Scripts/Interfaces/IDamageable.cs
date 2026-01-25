namespace DesignPatterns.ISP
{
    /// <summary>
    /// 데미지를 받을 수 있는 객체에 대한 계약을 정의합니다.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// 지정된 양만큼 데미지를 받습니다.
        /// </summary>
        /// <param name="amount">받을 데미지 양</param>
        void TakeDamage(float amount);
    }
}
