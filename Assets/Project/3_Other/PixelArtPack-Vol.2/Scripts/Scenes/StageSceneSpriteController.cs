using UnityEngine;

// 스테이지(Stage) 데모용 컨트롤러
// 다른 컨트롤러와 달리 입력으로 실제 수평 이동 속도까지 직접 제어하며,
// 트리거 충돌 시 피격(Damage) 애니메이션을 재생한다.
[RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class StageSceneSpriteController : BaseSpriteController
{
	[SerializeField] LayerMask groundMask; // 지면 판정에 사용할 레이어 마스크

	[SerializeField, HideInInspector] Animator       m_animator;       // 애니메이터 (자동 연결)
	[SerializeField, HideInInspector] SpriteRenderer m_spriteRenderer; // 스프라이트 렌더러 (자동 연결)
	[SerializeField, HideInInspector] Rigidbody2D    m_rigidbody2D;    // 2D 리지드바디 (자동 연결)

//----------------------------------------------------------------------------------------------------------------------

	protected override void UpdateInternalV()
	{
		PixelArtPackInputReader inputReader = GetInputReader();

		Vector2 move   = inputReader.ReadMove();
		float   axis   = move.x;     // 좌우 입력 (-1 ~ 1)
		bool    isDown = move.y < 0; // 아래 입력 여부 (웅크리기)

		// 현재 속도를 가져와 점프/이동 입력을 반영한 뒤 다시 적용한다.
		Vector2 velocity = m_rigidbody2D.linearVelocity;
		if (inputReader.ReadJump())
		{
			velocity.y = 5; // 점프 : 수직 속도 부여
		}
		if (axis != 0)
		{
			m_spriteRenderer.flipX = axis < 0; // 진행 방향에 맞춰 스프라이트 반전
			velocity.x             = axis * 2; // 좌우 이동 속도 적용
		}
		m_rigidbody2D.linearVelocity = velocity;

		// 발밑으로 레이캐스트를 쏴 지면까지의 거리를 측정 (착지 애니메이션 판별용)
		Vector2 startPos       = new Vector2(transform.position.x, transform.position.y);
		float   groundDistance = Physics2DUtility.Calculate2DDistanceToLayer(startPos, Vector2.down, 1, groundMask);

		// 애니메이터 파라미터 갱신
		m_animator.SetBool (PixelArtPackConstants.IS_CROUCH_PARAM,       isDown);
		m_animator.SetFloat(PixelArtPackConstants.GROUND_DISTANCE_PARAM, groundDistance);
		m_animator.SetFloat(PixelArtPackConstants.FALL_SPEED_PARAM,      m_rigidbody2D.linearVelocity.y);
		m_animator.SetFloat(PixelArtPackConstants.SPEED_PARAM,           Mathf.Abs(axis));
	}

	// [생명주기] 다른 콜라이더(트리거)와 닿으면 피격 애니메이션을 재생한다.
	void OnTriggerEnter2D(Collider2D other)
	{
		m_animator.SetTrigger(PixelArtPackConstants.DAMAGE_PARAM);
	}

	private void OnValidate()
	{
		// 필요한 컴포넌트를 같은 오브젝트에서 자동으로 연결한다.
		m_animator       = GetComponent<Animator>();
		m_spriteRenderer = GetComponent<SpriteRenderer>();
		m_rigidbody2D    = GetComponent<Rigidbody2D>();
	}
}
