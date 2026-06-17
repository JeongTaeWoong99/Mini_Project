using UnityEngine;

// 전투 동작(Battle Actions) 데모용 컨트롤러
// Standard 와 유사하나 공격 1/2/3 트리거 입력을 함께 처리한다.
[RequireComponent(typeof(Animator), typeof(SpriteRenderer), typeof(Rigidbody2D))]
public class BattleActionsSceneSpriteController : BaseSpriteController
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

		// 점프 입력 시 수직 속도만 위로 부여 (수평 속도는 유지)
		if (inputReader.ReadJump())
		{
			m_rigidbody2D.linearVelocity = new Vector2(m_rigidbody2D.linearVelocity.x, 5);
		}

		// 발밑으로 레이캐스트를 쏴 지면까지의 거리를 측정 (착지 애니메이션 판별용)
		Vector2 startPos       = new Vector2(transform.position.x, transform.position.y);
		float   groundDistance = Physics2DUtility.Calculate2DDistanceToLayer(startPos, Vector2.down, 1, groundMask);

		// 애니메이터 파라미터 갱신
		m_animator.SetBool (PixelArtPackConstants.IS_CROUCH_PARAM,       isDown);
		m_animator.SetFloat(PixelArtPackConstants.GROUND_DISTANCE_PARAM, groundDistance);
		m_animator.SetFloat(PixelArtPackConstants.FALL_SPEED_PARAM,      m_rigidbody2D.linearVelocity.y);
		m_animator.SetFloat(PixelArtPackConstants.SPEED_PARAM,           Mathf.Abs(axis));

		// 공격 입력별로 해당 애니메이터 트리거 발동
		if (inputReader.ReadAttack(0))
		{
			m_animator.SetTrigger(PixelArtPackConstants.ATTACK1_PARAM);
		}
		if (inputReader.ReadAttack(1))
		{
			m_animator.SetTrigger(PixelArtPackConstants.ATTACK2_PARAM);
		}
		if (inputReader.ReadAttack(2))
		{
			m_animator.SetTrigger(PixelArtPackConstants.ATTACK3_PARAM);
		}

		// 진행 방향에 맞춰 스프라이트 좌우 반전
		if (axis != 0)
		{
			m_spriteRenderer.flipX = axis < 0;
		}
	}

	private void OnValidate()
	{
		// 필요한 컴포넌트를 같은 오브젝트에서 자동으로 연결한다.
		m_animator       = GetComponent<Animator>();
		m_spriteRenderer = GetComponent<SpriteRenderer>();
		m_rigidbody2D    = GetComponent<Rigidbody2D>();
	}
}
