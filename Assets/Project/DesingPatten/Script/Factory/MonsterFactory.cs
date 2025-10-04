using System.Collections.Generic;
using UnityEngine;

public enum MonsterType
{
    Crab, Ghost, Mummy, Spider
}

public class MonsterFactory : Factory<MonsterType>
{
    [SerializeField] private GameObject crabPrefab;
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private GameObject mummyPrefab;
    [SerializeField] private GameObject spiderPrefab;
    
    private Dictionary<MonsterType, GameObject> prefabDictionary;
    
    private void Awake()
    {
        InitializePrefabDictionary();
    }
    
    private void InitializePrefabDictionary()
    {
        prefabDictionary = new Dictionary<MonsterType, GameObject>
        {
            { MonsterType.Crab, crabPrefab },
            { MonsterType.Ghost, ghostPrefab },
            { MonsterType.Mummy, mummyPrefab },
            { MonsterType.Spider, spiderPrefab }
        };
    }
    
    protected override GameObject Create(MonsterType type)
    {
        if (prefabDictionary == null)
            InitializePrefabDictionary();
            
        if (prefabDictionary.TryGetValue(type, out GameObject prefab) && prefab != null)
        {
            return Instantiate(prefab);
        }
        
        Debug.LogError($"Prefab not found for MonsterType: {type}");
        return null;
    }
}
