using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData",menuName = "Scriptable/CharacterData",order = 1)]
public class CharacterData : ScriptableObject
{
    public float hp;
    public float damage;
    public float attackCoolDown;
    public float speed;
}