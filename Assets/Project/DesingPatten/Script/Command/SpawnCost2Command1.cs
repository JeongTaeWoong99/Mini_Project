using DesingPatten.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace DesingPatten.Command
{
    public class SpawnCost2Command : ISpawnCommand
    {
        private HumanFactory humanFactory;
        private Transform    spawnTrans;
        private Button       spawnButton;

        public SpawnCost2Command(HumanFactory humanFactory, Transform spawnTrans, Button spawnButton)
        {
            this.humanFactory = humanFactory;
            this.spawnTrans   = spawnTrans;
            this.spawnButton  = spawnButton;
        }

        public void Execute()
        {
            if (GameManager.Instance.HasCoin(2))
            {
                GameManager.Instance.UseCoin(2);
                humanFactory.Spawn((HumanType.Cost2), spawnTrans);
            }
        }
        
        public void StateUpdate()
        {
            if (GameManager.Instance.HasCoin(2))
                spawnButton.interactable = true;
            else
                spawnButton.interactable = false;
        }
    }
}

