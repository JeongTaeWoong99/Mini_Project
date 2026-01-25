using UnityEngine;

namespace DesignPatterns.ISP
{
 public interface ITarget
    {
        void TakeDamage(int amount);
        void Explode();
        void TriggerEffect();
    }
    
 /// <summary>
 /// 이 클래스는 ITarget 인터페이스를 구현하며, 데미지 받기, 폭발, 이펙트 트리거 메서드를 포함합니다.
 ///
 /// 단순한 타겟이 데미지만 받으면 되는 경우에도, ITarget 인터페이스에 정의된 모든 메서드를 구현해야 합니다.
 /// 이는 빈 메서드 구현을 야기합니다.
 /// </summary>
    public class UnrefactoredTarget : MonoBehaviour, ITarget
    {
        // 이 타겟이 데미지만 받으면 되는 경우에도, 모든 메서드를 구현해야 합니다.
        public void TakeDamage(int amount)
        {
            // 데미지 로직 구현
        }

        public void Explode()
        {
            // 이 타겟이 폭발할 필요가 없더라도, 이 메서드를 구현해야 합니다.
        }

        public void TriggerEffect()
        {
            // 마찬가지로, 불필요하더라도 구현이 필요합니다.
        }
    }
}

