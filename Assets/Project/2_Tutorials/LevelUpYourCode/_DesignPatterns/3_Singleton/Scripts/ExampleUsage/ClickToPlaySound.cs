using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Singleton
{
    // 정적 싱글톤 인스턴스에 접근하는 방법을 보여주는 예제
    public class ClickToPlaySound : MonoBehaviour
    {
        [SerializeField] private AudioClip m_Clip;
        [SerializeField] private LayerMask m_LayerToClick;

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // 레이캐스트로 콜라이더를 클릭했는지 확인
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, Mathf.Infinity, m_LayerToClick))
                {
                    PlaySoundFromAudioManager();
                }
            }
        }

        // 전역 싱글톤 인스턴스에서 오디오 클립 재생
        private void PlaySoundFromAudioManager()
        {
            if (m_Clip != null)
            {
                AudioManager.Instance.PlaySoundEffect(m_Clip);
            }
        }
    }
}
