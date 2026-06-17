using UnityEngine;

// 감정 표현(Emotion Actions) 데모용 컨트롤러
// 이동 표현(Speed)과 함께, 공격 입력을 감정 표현 상태(긍정/부정/워밍업) 재생으로 사용한다.
// 물리 이동 없이 Animator.Play 로 해당 상태를 직접 재생한다.
[RequireComponent(typeof(Animator))]
public class EmotionActionsSceneSpriteController : BaseSpriteController
{
	[SerializeField] Animator m_animator; // 애니메이터 (자동 연결)

//----------------------------------------------------------------------------------------------------------------------

	protected override void UpdateInternalV()
	{
		PixelArtPackInputReader inputReader = GetInputReader();

		Vector2 move = inputReader.ReadMove();

		// 좌우 입력 값을 그대로 Speed 파라미터로 전달 (이동 표현)
		m_animator.SetFloat(PixelArtPackConstants.SPEED_PARAM, move.x);

		// 공격 입력별로 해당 감정 표현 상태를 재생
		if (inputReader.ReadAttack(0))
		{
			m_animator.Play(PixelArtPackConstants.EMOTION_POSITIVE_PARAM); // 긍정
		}
		if (inputReader.ReadAttack(1))
		{
			m_animator.Play(PixelArtPackConstants.EMOTION_NEGATIVE_PARAM); // 부정
		}
		if (inputReader.ReadAttack(2))
		{
			m_animator.Play(PixelArtPackConstants.EMOTION_WARMING_UP_PARAM); // 워밍업
		}
	}

	private void OnValidate()
	{
		// 같은 오브젝트의 애니메이터를 자동으로 연결한다.
		m_animator = GetComponent<Animator>();
	}
}
