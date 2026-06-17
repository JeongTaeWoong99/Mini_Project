using UnityEngine;

// 애니메이터 파라미터 해시 값과 공용 상수를 모아둔 정적 클래스
// 매 프레임 문자열로 파라미터를 찾으면 비용이 크므로, 미리 해시(int)로 변환해 캐싱한다.
public static class PixelArtPackConstants
{
	public static readonly int SPEED_PARAM            = Animator.StringToHash("Speed");          // 좌우 이동 속도 (걷기/달리기 블렌드)
	public static readonly int FALL_SPEED_PARAM       = Animator.StringToHash("FallSpeed");      // 수직 속도 (점프/낙하 판별)
	public static readonly int GROUND_DISTANCE_PARAM  = Animator.StringToHash("GroundDistance"); // 지면까지의 거리 (착지 판별)
	public static readonly int IS_CROUCH_PARAM        = Animator.StringToHash("IsCrouch");       // 웅크리기 상태 여부
	public static readonly int DAMAGE_PARAM           = Animator.StringToHash("Damage");         // 피격 트리거

	// InputSystem 의 "Attack" 액션은 인덱스 0 부터 시작한다.
	public static readonly int ATTACK1_PARAM          = Animator.StringToHash("Attack1");        // 공격 1 트리거
	public static readonly int ATTACK2_PARAM          = Animator.StringToHash("Attack2");        // 공격 2 트리거
	public static readonly int ATTACK3_PARAM          = Animator.StringToHash("Attack3");        // 공격 3 트리거

	public static readonly int EMOTION_POSITIVE_PARAM    = Animator.StringToHash("Emotion.Positive");  // 긍정 감정 표현 상태
	public static readonly int EMOTION_NEGATIVE_PARAM    = Animator.StringToHash("Emotion.Negative");  // 부정 감정 표현 상태
	public static readonly int EMOTION_WARMING_UP_PARAM  = Animator.StringToHash("Emotion.Warmingup"); // 워밍업 감정 표현 상태

	public const int NUM_ATTACKS     = 3;  // 공격 액션 개수
	public const int NO_HIT_DISTANCE = 99; // 레이캐스트가 아무것도 맞히지 못했을 때 사용할 기본 거리
}
