using System;
using UnityEngine;

public class Character : MonoBehaviour, IDamageable
{
    public  CharacterData characterData;
    private float         lastAttackTime;
    [HideInInspector]
    public  bool          isAttack;

    public float Health
    {
        get
        {
            return characterData.hp;
        }
        set
        {
            characterData.hp = Mathf.Max(value, 0); // Mathf.Max(value, min, max); 문법 순서로, 해석하자면, value의 최소값은 0 이하가 될 수 없고, characterData.hp의 값을 value가 받아서 판단하고, 다시 넣어준다.
            if(characterData.hp <= 0)
                Die();
        }
    }
    
    private void Die()
    {
        Destroy(gameObject);
    }

    private bool CanAttack()
    {
        return Time.time - lastAttackTime >= characterData.attackCoolDown;
    }

    public void Attack(IDamageable target)
    {
        if (CanAttack())
        {
            target.TakeDamage((characterData.damage));
            lastAttackTime = Time.time;
            isAttack = false;
        }
    }

    public void TakeDamage(float damage)
    {
        Health -= damage;   // Health의 Set 실행
    }

    // 캐릭터용
    private void OnCollisionEnter2D(Collision2D col)
    {
        IDamageable target = col.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            isAttack = true;
            Attack(target);
        }
    }
    
    // // 캐슬용
    // private void OnTriggerEnter2D(Collider other)
    // {
    //     IDamageable target = other.gameObject.GetComponent<IDamageable>();
    //     if (target != null)
    //     {
    //         isAttack = true;
    //         Attack(target);
    //     }
    // }
}

