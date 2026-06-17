using UnityEngine.Assertions;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// 입력을 추상화하는 리더 클래스
// 새 Input System 사용 시 PlayerInput 의 액션을, 그렇지 않으면 레거시 Input 매니저를 사용한다.
// 컨트롤러는 이 클래스를 통해서만 입력을 읽으므로, 입력 방식이 바뀌어도 컨트롤러 코드는 영향받지 않는다.
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class PixelArtPackInputReader : MonoBehaviour
{
#if ENABLE_INPUT_SYSTEM
	private          InputAction   m_moveAction;                                                       // 이동 액션
	private          InputAction   m_jumpAction;                                                       // 점프 액션
	private readonly InputAction[] m_attackActions = new InputAction[PixelArtPackConstants.NUM_ATTACKS]; // 공격 액션 배열

	[SerializeField] private PlayerInput m_playerInput;                                                // 입력 컴포넌트 (OnValidate 에서 자동 연결)
#endif

//----------------------------------------------------------------------------------------------------------------------

	private void Awake()
	{
#if ENABLE_INPUT_SYSTEM
		// 현재 활성화된 액션 맵에서 사용할 액션들을 미리 찾아 캐싱한다.
		InputActionMap currentMap = m_playerInput.currentActionMap;
		m_moveAction = currentMap.FindAction("Move", throwIfNotFound : false);
		m_jumpAction = currentMap.FindAction("Jump", throwIfNotFound : false);

		// 공격 액션은 "Attack0" ~ "Attack(N-1)" 형태로 인덱스 순회하며 캐싱한다.
		for (int i = 0; i < PixelArtPackConstants.NUM_ATTACKS; i++)
		{
			m_attackActions[i] = currentMap.FindAction("Attack" + (i), throwIfNotFound : false);
		}
#endif
	}

	private void OnValidate()
	{
#if ENABLE_INPUT_SYSTEM
		// 인스펙터 값 변경 시 같은 오브젝트의 PlayerInput 을 자동으로 연결해 둔다.
		m_playerInput = GetComponent<PlayerInput>();
#endif
	}

//----------------------------------------------------------------------------------------------------------------------

	// 이동 입력(좌우/상하)을 Vector2 로 반환한다.
	public Vector2 ReadMove()
	{
#if ENABLE_INPUT_SYSTEM
		return m_moveAction.ReadValue<Vector2>();
#else
		float x = Input.GetAxis("Horizontal");
		float y = Input.GetAxis("Vertical");
		return new Vector2(x, y);
#endif
	}

	// 이번 프레임에 점프 입력이 눌렸는지 반환한다.
	public bool ReadJump()
	{
#if ENABLE_INPUT_SYSTEM
		return m_jumpAction.WasPressedThisFrame();
#else
		return Input.GetButtonDown("Jump");
#endif
	}

	// 이번 프레임에 i 번째 공격 입력이 눌렸는지 반환한다.
	// 레거시 입력에서는 Z / X / C 키에 각각 매핑된다.
	public bool ReadAttack(int i)
	{
		Assert.IsTrue(i < PixelArtPackConstants.NUM_ATTACKS);
#if ENABLE_INPUT_SYSTEM
		return m_attackActions[i].WasPressedThisFrame();
#else
		switch (i)
		{
			case 0  : return Input.GetKeyDown(KeyCode.Z);
			case 1  : return Input.GetKeyDown(KeyCode.X);
			case 2  : return Input.GetKeyDown(KeyCode.C);
			default : return false;
		}
#endif
	}
}
