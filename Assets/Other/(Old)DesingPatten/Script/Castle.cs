using DesingPatten.Observer;
using DesingPatten.Singleton;
using UnityEngine;

public class Castle : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHP = 100f;
    private float currentHP;

    public float CurrentHP => currentHP;
    public float MaxHP => maxHP;

    private void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);

        if (currentHP <= 0)
        {
            GameManager.Instance.GameOver();
        }   
    }
}
