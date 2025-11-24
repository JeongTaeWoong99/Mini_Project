using UnityEngine;

public class Human : Character  
{
    private void Update()
    {
        if (!isAttack)
        {
            // 나중에 velocity 이동으로 변경
            transform.Translate(Vector2.right * (characterData.speed * Time.deltaTime));
        }
    }
}
