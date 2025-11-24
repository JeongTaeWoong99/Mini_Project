using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DesingPatten.Command
{
    [System.Serializable]
    public class SpawnButtonConfig
    {
        public Button button;
        public HumanType humanType;
        public int cost;
    }

    public class SpawnHuman : MonoBehaviour
    {
        [SerializeField] private HumanFactory humanFactory;
        [SerializeField] private Transform spawnTransform;
        [SerializeField] private List<SpawnButtonConfig> spawnConfigs;

        private List<ISpawnCommand> spawnCommands = new List<ISpawnCommand>();

        private void Awake()
        {
            InitializeCommands();
        }

        private void InitializeCommands()
        {
            if (spawnConfigs == null) return;

            foreach (var config in spawnConfigs)
            {
                if (config.button != null)
                {
                    var command = new SpawnUnitCommand(humanFactory, spawnTransform, config.button, config.humanType, config.cost);
                    spawnCommands.Add(command);
                    
                    config.button.onClick.AddListener(() => command.Execute());
                }
            }
        }

        private void Update()
        {
            foreach (var command in spawnCommands)
            {
                command.StateUpdate();
            }
        }
    }
}