using UnityEngine;

public abstract class Factory<T> : MonoBehaviour where T : System.Enum
{
    public GameObject Spawn(T type, Transform spawnPoint)
    {
        var entity = Create(type);
        if (entity == null)
        {
            Debug.LogError($"Failed to create entity of type {type}");
            return null;
        }
        
        if (spawnPoint != null)
        {
            entity.transform.position = spawnPoint.position;
            entity.transform.rotation = spawnPoint.rotation;
        }
        
        return entity;
    }
    
    protected abstract GameObject Create(T type);
}