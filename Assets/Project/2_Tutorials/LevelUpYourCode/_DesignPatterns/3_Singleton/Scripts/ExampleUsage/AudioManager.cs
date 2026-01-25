using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Singleton
{
    /// <summary>
    /// 싱글톤으로 구현된 간단한 오디오/사운드 매니저의 예제입니다.
    /// 어떤 GameObject에서든 AudioManager.Instance에 접근하여 AudioClip을 재생하세요.
    /// </summary>
    public class AudioManager : Singleton<AudioManager>
    {
        public AudioSource audioSource;

        public Vector2 volume = new Vector2(0.5f, 0.9f);
        public Vector2 pitch  = new Vector2(0.8f, 1.2f);

        // 지정된 AudioSource에서 클립 재생
        public void PlaySoundEffect(AudioClip clip)
        {
            if (audioSource == null)
                return;

            // 볼륨과 피치 무작위화
            audioSource.volume = Random.Range(volume.x, volume.y);
            audioSource.pitch  = Random.Range(pitch.x, pitch.y);

            // 클립 업데이트
            audioSource.clip = clip;
            // 다시 재생하기 전에 오디오 소스가 중지되었는지 확인
            audioSource.Stop();
            audioSource.Play();
        }
    }
}  
