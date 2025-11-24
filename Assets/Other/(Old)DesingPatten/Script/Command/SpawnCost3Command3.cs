using DesingPatten.Singleton;
using UnityEngine;
using UnityEngine.UI;

namespace DesingPatten.Command
{
    public class SpawnCost3Command : ISpawnCommand
    {
        private HumanFactory humanFactory;
        private Transform    spawnTrans;
        private Button       spawnButton;

        public SpawnCost3Command(HumanFactory humanFactory, Transform spawnTrans, Button spawnButton)
        {
            this.humanFactory = humanFactory;
            this.spawnTrans   = spawnTrans;
            this.spawnButton  = spawnButton;
        }

        public void Execute()
        {
            if (GameManager.Instance.HasCoin(3))
            {
                GameManager.Instance.UseCoin(3);
                humanFactory.Spawn((HumanType.Cost3), spawnTrans);
            }
        }
        
        public void StateUpdate()
        {
            if (GameManager.Instance.HasCoin(3))
                spawnButton.interactable = true;
            else
                spawnButton.interactable = false;
        }
    }
}

