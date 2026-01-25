using UnityEngine;

public class TimeManagerTest : MonoBehaviour
{
    public float timeScale = 1f;

    void Start()
    {
        Time.timeScale = timeScale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}