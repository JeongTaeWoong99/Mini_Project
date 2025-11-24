using DesingPatten.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace DesingPatten.Command
{
    public class SpawnUnitCommand : ISpawnCommand
    {
        private readonly HumanFactory humanFactory;
        private readonly Transform spawnTransform;
        private readonly Button spawnButton;
        private readonly HumanType humanType;
        private readonly int cost;

        public SpawnUnitCommand(HumanFactory factory, Transform spawnPoint, Button button, HumanType type, int unitCost)
        {
            humanFactory = factory;
            spawnTransform = spawnPoint;
            spawnButton = button;
            humanType = type;
            cost = unitCost;
        }

        public void Execute()
        {
            if (GameManager.Instance.HasCoin(cost))
            {
                GameManager.Instance.UseCoin(cost);
                humanFactory.Spawn(humanType, spawnTransform);
            }
        }

        public void StateUpdate()
        {
            if (spawnButton != null)
            {
                spawnButton.interactable = GameManager.Instance.HasCoin(cost);
            }
        }
    }
}