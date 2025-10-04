using System.Collections.Generic;
using UnityEngine;

public enum HumanType
{
    Cost1, Cost2, Cost3, Cost4
}

public class HumanFactory : Factory<HumanType>
{
    [SerializeField] private GameObject cost1Prefab;
    [SerializeField] private GameObject cost2Prefab;
    [SerializeField] private GameObject cost3Prefab;
    [SerializeField] private GameObject cost4Prefab;
    
    private Dictionary<HumanType, GameObject> prefabDictionary;
    
    private void Awake()
    {
        InitializePrefabDictionary();
    }
    
    private void InitializePrefabDictionary()
    {
        prefabDictionary = new Dictionary<HumanType, GameObject>
        {
            { HumanType.Cost1, cost1Prefab },
            { HumanType.Cost2, cost2Prefab },
            { HumanType.Cost3, cost3Prefab },
            { HumanType.Cost4, cost4Prefab }
        };
    }
    
    protected override GameObject Create(HumanType type)
    {
        if (prefabDictionary == null)
            InitializePrefabDictionary();
        
        if (prefabDictionary.TryGetValue(type, out GameObject prefab) && prefab != null)
        {
            return Instantiate(prefab);
        }
        
        Debug.LogError($"Prefab not found for HumanType: {type}");
        return null;
    }
}
