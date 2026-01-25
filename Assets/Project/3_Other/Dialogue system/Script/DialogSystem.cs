using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogSystem : MonoBehaviour
{
	[SerializeField] 
	private int branch;
	[SerializeField] 
	private DialogDB dialogDB;
	
	[SerializeField]
	private	Speaker[]		speakers;					// 대화를 진행하는 캐릭터들의 UI 배열
	[SerializeField]
	private	DialogData[]	dialogs;					// 현재 읽어야 할 대화 내용 배열
	[SerializeField]
	private	bool			isAutoStart = true;			// 자동 시작 여부
	private	bool			isFirst = true;				// 최초 1회만 호출하기 위한 변수
	private	int				currentDialogIndex = -1;	// 현재 대화 순서
	private	int				currentSpeakerIndex = 0;	// 현재 말을 하는 화자(Speaker)의 speakers 배열 순서
	private	float			typingSpeed = 0.05f;	    // 텍스트 타이핑 효과의 재생 속도
	private	bool			isTypingEffect = false;		// 텍스트 타이핑 효과를 재생중인지
	
	private void Awake()
	{
		int index = 0;
		for (int i = 0; i < dialogDB.Tutorial.Count; i++)
		{
			if (dialogDB.Tutorial[i].branch == branch)
			{
				dialogs[index].name = dialogDB.Tutorial[i].name;
				dialogs[index].dialogue = dialogDB.Tutorial[i].dialog;
				index++;
			}
		}
		Setup();
	}

	private void Setup()
	{
		// 모든 대화 관련 게임오브젝트 비활성화
		for ( int i = 0; i < speakers.Length; ++ i )
		{
			ResetSetting(speakers[i]);
		}
	}

	public bool UpdateDialog()
	{
		// 대화 읽기가 시작된 후 1회만 호출
		if ( isFirst == true )
		{
			// 초기화. 캐릭터 이미지는 활성화하고, 대화 관련 UI는 모두 비활성화
			Setup();

			// 자동 시작(isAutoStart=true)으로 설정되어 있으면 첫 번째 대화 시작
			if ( isAutoStart ) SetNextDialog();

			isFirst = false;
		}

		if ( Input.GetMouseButtonDown(0) )
		{
			// 텍스트 타이핑 효과를 재생중일때 마우스 왼쪽 클릭하면 타이핑 효과 중단
			if ( isTypingEffect == true )
			{
				isTypingEffect = false;
				
				// 타이핑 효과를 중단하고, 현재 대화 전체를 출력한다
				StopCoroutine("OnTypingText");
				speakers[currentSpeakerIndex].textDialogue.text = dialogs[currentDialogIndex].dialogue;
				// 대화가 완료되었을 때 출력되는 커서 활성화
				speakers[currentSpeakerIndex].objectArrow.SetActive(true);

				return false;
			}

			// 대화가 남아있을 때 다음 대화 진행
			if ( dialogs.Length > currentDialogIndex + 1 )
			{
				SetNextDialog();
			}
			// 대화가 더 이상 없을 때 모든 오브젝트를 비활성화하고 true 반환
			else
			{
				// 현재 대화를 출력했던 모든 캐릭터, 대화 관련 UI를 화면에 보이지 않게 비활성화
				for ( int i = 0; i < speakers.Length; ++ i )
				{
					SetActiveObjects(speakers[i], false);
					// SetActiveObjects()는 캐릭터 이미지를 완전히 보이지 않게 하는 부분이 없기 때문에 별도로 호출
					speakers[i].portraitImage.gameObject.SetActive(false);
				}

				return true;
			}
		}

		return false;
	}

	private void SetNextDialog()
	{
		// 현재 화자의 대화 관련 오브젝트 비활성화
		SetActiveObjects(speakers[currentSpeakerIndex], false);

		// 다음 대화로 이동하도록 
		currentDialogIndex ++;

		// 현재 화자 순서 설정
		currentSpeakerIndex = dialogs[currentDialogIndex].speakerIndex;

		// 현재 화자의 대화 관련 오브젝트 활성화
		SetActiveObjects(speakers[currentSpeakerIndex], true);
		// 현재 화자 이름 텍스트 설정
		speakers[currentSpeakerIndex].textName.text = dialogs[currentDialogIndex].name;
		// 현재 화자의 대화 텍스트 설정
		//speakers[currentSpeakerIndex].textDialogue.text = dialogs[currentDialogIndex].dialogue;
		StartCoroutine("OnTypingText");
	}

	private void SetActiveObjects(Speaker speaker, bool visible)
	{
		speaker.imageDialog.gameObject.SetActive(visible);
		speaker.textName.gameObject.SetActive(visible);
		speaker.textDialogue.gameObject.SetActive(visible);

		// 화살표는 대화가 완료되었을 때만 활성화하기 때문에 항상 false
		speaker.objectArrow.SetActive(false);
		
		// 캐릭터 이미지 색상 변경
		Color color = speaker.portraitImage.color;
		color.a = visible == true ? 1 : 0.2f;
		speaker.portraitImage.color = color;
	}

	private void ResetSetting(Speaker speaker)
	{
		speaker.portraitImage.gameObject.SetActive(true);
		speaker.imageDialog.gameObject.SetActive(false);
		speaker.textName.gameObject.SetActive(false);
		speaker.textDialogue.gameObject.SetActive(false);

		// 화살표는 대화가 완료되었을 때만 활성화하기 때문에 항상 false
		speaker.objectArrow.SetActive(false);
		
		// 캐릭터 이미지 색상 변경
		Color color = speaker.portraitImage.color;
		color.a = 0f;
		speaker.portraitImage.color = color;
	}

	private IEnumerator OnTypingText()
	{
		int index = 0;
		
		isTypingEffect = true;

		// 텍스트를 한글자씩 타이핑치듯 재생
		while ( index < dialogs[currentDialogIndex].dialogue.Length )
		{
			speakers[currentSpeakerIndex].textDialogue.text = dialogs[currentDialogIndex].dialogue.Substring(0, index);

			index ++;
		
			yield return new WaitForSeconds(typingSpeed);
		}

		isTypingEffect = false;

		// 대화가 완료되었을 때 출력되는 커서 활성화
		speakers[currentSpeakerIndex].objectArrow.SetActive(true);
	}
}

[System.Serializable]
public struct Speaker
{
	public	Image	        portraitImage;		// 캐릭터 이미지 (청자/화자 상태를 표현)
	public	Image			imageDialog;		// 대화창 Image UI
	public	TextMeshProUGUI	textName;			// 현재 대화하는 캐릭터 이름 출력 Text UI
	public	TextMeshProUGUI	textDialogue;		// 현재 대화 내용 출력 Text UI
	public	GameObject		objectArrow;		// 대화가 완료되었을 때 출력되는 커서 오브젝트
}

[System.Serializable]
public struct DialogData
{
	public	int		speakerIndex;	// 이름과 대화를 출력할 화자 DialogSystem의 speakers 배열 순서
	public	string	name;			// 캐릭터 이름
	[TextArea(3, 5)]
	public	string	dialogue;		// 대화
}