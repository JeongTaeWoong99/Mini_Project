using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace DesignPatterns.SRP
{
    /// <summary>
    /// Unity에서 단일 책임 원칙(SRP)을 따르지 않는 플레이어 기능의 예시입니다.
    ///
    /// 이 스크립트는 이동 제어, 입력 처리, 오디오 관리, 파티클 효과 등
    /// 여러 책임을 하나의 클래스에 병합합니다.
    ///
    /// 현재는 작은 크기로 인해 관리 가능하지만, 이러한 접근 방식은 코드의 확장,
    /// 유지보수, 확장에 어려움을 초래할 수 있습니다.
    /// </summary>
    public class UnrefactoredPlayer : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("수평 이동 속도")]
        [SerializeField] private float moveSpeed    = 5f;
        [Tooltip("이동 속도 변화율")]
        [SerializeField] private float acceleration = 10f;
        [Tooltip("입력이 없을 때의 감속율")]
        [SerializeField] private float deceleration = 5f;

        [Header("Controls")]
        [Tooltip("WASD 키를 사용하여 이동")]
        [SerializeField] private KeyCode forwardKey  = KeyCode.W;
        [SerializeField] private KeyCode backwardKey = KeyCode.S;
        [SerializeField] private KeyCode leftKey     = KeyCode.A;
        [SerializeField] private KeyCode rightKey    = KeyCode.D;

        [Header("Collision")]
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Audio")]
        [SerializeField] private AudioClip[] bounceClips;
        [SerializeField] private float       audioCooldownTime = 2f;
        private float lastAudioPlayedTime;

        [Header("Effects")]
        [SerializeField] private ParticleSystem m_ParticleSystem;
        private const float effectCooldown   = 1f;
        private float       timeToNextEffect = -1f;

        private Vector3             inputVector;
        private float               currentSpeed = 0f;
        private CharacterController charController;
        private float               initialYPosition;
        private AudioSource         audioSource;

        private void Awake()
        {
            charController   = GetComponent<CharacterController>();
            initialYPosition = transform.position.y;
            audioSource      = GetComponent<AudioSource>();
        }

        private void Start()
        {
            lastAudioPlayedTime = -audioCooldownTime;
        }

        private void Update()
        {
            HandleInput();
            Move(inputVector);
        }

        private void HandleInput()
        {
            // 입력 벡터 초기화
            float xInput = 0;
            float zInput = 0;

            if (Input.GetKey(forwardKey))
                zInput++;
            if (Input.GetKey(backwardKey))
                zInput--;
            if (Input.GetKey(leftKey))
                xInput--;
            if (Input.GetKey(rightKey))
                xInput++;

            inputVector = new Vector3(xInput, 0, zInput);
        }

        private void Move(Vector3 inputVector)
        {
            if (inputVector == Vector3.zero)
            {
                if (currentSpeed > 0)
                {
                    currentSpeed -= deceleration * Time.deltaTime;
                    currentSpeed  = Mathf.Max(currentSpeed, 0);
                }
            }
            else
            {
                currentSpeed = Mathf.Lerp(currentSpeed, moveSpeed, Time.deltaTime * acceleration);
            }

            Vector3 movement    = inputVector.normalized * currentSpeed * Time.deltaTime;
            charController.Move(movement);
            transform.position = new Vector3(transform.position.x, initialYPosition, transform.position.z);
        }

        public void PlayRandomAudioClip()
        {
            // 다음 클립을 재생할 시간이 지났고 사용 가능한 클립이 있으면 무작위 클립을 재생합니다.
            if (Time.time > (audioCooldownTime + lastAudioPlayedTime))
            {
                lastAudioPlayedTime = Time.time;
                audioSource.clip    = bounceClips[Random.Range(0, bounceClips.Length)];
                audioSource.Play();
            }
        }

        public void PlayEffect()
        {
            if (Time.time < timeToNextEffect)
                return;

            if (m_ParticleSystem != null)
            {
                m_ParticleSystem.Stop();
                m_ParticleSystem.Play();
                timeToNextEffect = Time.time + effectCooldown;
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // 충돌한 오브젝트의 레이어가 obstacleLayer LayerMask에 포함되어 있는지 확인
            if ((obstacleLayer.value & (1 << hit.gameObject.layer)) > 0)
            {
                PlayRandomAudioClip();
                PlayEffect();
            }
        }

    }
}
