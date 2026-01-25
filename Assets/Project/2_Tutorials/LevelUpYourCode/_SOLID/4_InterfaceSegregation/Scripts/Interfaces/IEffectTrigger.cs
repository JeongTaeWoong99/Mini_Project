using UnityEngine;

namespace DesignPatterns.ISP
{
    /// <summary>
    /// 특정 위치에서 파티클 시스템이나 사운드 이펙트 같은 효과를 트리거하기 위한 계약을 정의합니다.
    /// </summary>
    public interface IEffectTrigger
    {
        /// <summary>
        /// 지정된 위치에서 이펙트를 트리거합니다.
        /// </summary>
        /// <param name="position">이펙트를 트리거할 위치</param>
        void TriggerEffect(Vector3 position);
    }
}
