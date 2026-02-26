using UnityEngine;

namespace DesignPatterns.Strategy
{
    [CreateAssetMenu(fileName = "FirstAidAbility", menuName = "Abilities/FirstAid")]
    public class FirstAidAbility : Ability
    {
        // 각 전략은 고유하고 다양한 커스텀 로직을 사용할 수 있다.
        // 오버라이드된 Use 메서드에서 원하는 게임플레이 로직을 구현한다
        public override void Use(GameObject gameObject)
        {
            base.Use(gameObject);

            // 응급처치 로직을 여기에 구현한다
        }
    }
}
