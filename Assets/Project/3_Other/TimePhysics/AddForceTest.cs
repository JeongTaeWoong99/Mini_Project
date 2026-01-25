using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class AddForceTest : MonoBehaviour
{
    public  bool        isNotEffectTimeScale;
    public  float       forceMagnitude = 10f; 
    private Rigidbody2D rb;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 탸임스케일 영향 O -> Normal
        if(!isNotEffectTimeScale)
            Invoke("EffectTimeScaleApplyForce", 1.0f);
        // 탸임스케일 영향 X -> Test
        else
            Invoke("NotEffectTimeScaleApplyForceApplyForce", 1.0f);
    }

    // 시간의 영향을 받는 파편들은 AddForce를 사용!
    void EffectTimeScaleApplyForce()
    {
        Vector2 direction = new Vector2(1, 1).normalized;
        
        rb.AddForce(direction * forceMagnitude, ForceMode2D.Impulse);
    }
    
    // 시간의 영향을 받지 않는 플레이어는
    // rb.velocity += direction * forceMagnitude * Time.unscaledDeltaTime; 를 이용해서 이동하고
    // 타임 스케일이 변경되어, 중력이 가벼워 지거나 더 무거워 지는 것을 방지하기 위해서(+ 조작감이 달라질 수 있기 때문에)
    // rb.gravityScale = rb.gravityScale * 1 / Time.timeScale;를 이용해서, 중력 조정을 해준다.
    void NotEffectTimeScaleApplyForceApplyForce()
    {
        StartCoroutine(ApplyForceOverTime());
    }
    
    private IEnumerator ApplyForceOverTime()
    {
        Vector2 direction = new Vector2(1, 1).normalized;
        rb.gravityScale = rb.gravityScale * 1 / Time.timeScale;

        while (true)
        {
            rb.linearVelocity += direction * forceMagnitude * Time.unscaledDeltaTime;
            
            yield return new FixedUpdate();
        }
    }
}
