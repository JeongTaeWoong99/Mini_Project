using UnityEngine;

// 모든 씬별 스프라이트 컨트롤러의 공통 베이스 클래스
// 입력 리더 보유 및 null 체크를 담당하고, 실제 동작은 파생 클래스가 UpdateInternalV() 에서 구현한다.
public abstract class BaseSpriteController : MonoBehaviour
{
	[SerializeField] private PixelArtPackInputReader m_inputReader; // 입력을 읽어오는 리더 (인스펙터에서 연결)

//----------------------------------------------------------------------------------------------------------------------

	// [생명주기] 매 프레임 입력 리더 유효성을 확인한 뒤 파생 클래스 로직을 호출한다.
	void Update()
	{
		// 입력 리더가 인스펙터에 연결되지 않았다면 경고만 출력하고 처리를 건너뛴다.
		if (null == m_inputReader)
		{
			Debug.LogWarning($"Input Reader is not assigned to {gameObject.name}");
			return;
		}

		UpdateInternalV();
	}

	// 파생 클래스가 매 프레임 수행할 실제 로직을 구현하는 추상 메서드
	protected abstract void UpdateInternalV();

	// 파생 클래스가 입력 리더에 접근하기 위한 접근자
	protected PixelArtPackInputReader GetInputReader() => m_inputReader;
}
