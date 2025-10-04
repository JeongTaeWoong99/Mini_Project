using DesingPatten.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace DesingPatten.Command
{
    public class SpawnCost1Command : ISpawnCommand
    {
        private HumanFactory humanFactory;
        private Transform    spawnTrans;
        private Button       spawnButton;

        public SpawnCost1Command(HumanFactory humanFactory, Transform spawnTrans, Button spawnButton)
        {
            this.humanFactory = humanFactory;
            this.spawnTrans   = spawnTrans;
            this.spawnButton  = spawnButton;
        }

        public void Execute()
        {
            if (GameManager.Instance.HasCoin(1))
            {
                GameManager.Instance.UseCoin(1);
                humanFactory.Spawn((HumanType.Cost1), spawnTrans);
            }
        }

        public void StateUpdate()
        {
            if (GameManager.Instance.HasCoin(1))
                spawnButton.interactable = true;
            else
                spawnButton.interactable = false;
        }
    }
}