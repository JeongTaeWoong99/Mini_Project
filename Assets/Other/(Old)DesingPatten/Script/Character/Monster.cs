using UnityEngine;

public class Monster : Character
{
    private void Update()
    {
        if (!isAttack)
        {
            // 나중에 velocity 이동으로 변경
            transform.Translate(Vector2.left * (characterData.speed * Time.deltaTime));
        }
    }
}
