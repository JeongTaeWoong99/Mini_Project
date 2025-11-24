using System.Collections.Generic;
using UnityEngine;

public class SpawnUI : MonoBehaviour
{
    public List<Transform> spawnTransList = new List<Transform>();

    public HumanFactory   humanFactory;
    //public MonsterFactory monsterFactory;
    
    public void OnHumanFactory()
    {
        int randomType = Random.Range(0, 4);
        humanFactory.Spawn((HumanType)randomType, spawnTransList[0]);
    }
    
    // public void OnMonsterFactory()
    // {
    //     int randomType = Random.Range(0, 4);
    //     monsterFactory.Spawn((MonsterType)randomType, spawnTransList[1]);
    // }
}
