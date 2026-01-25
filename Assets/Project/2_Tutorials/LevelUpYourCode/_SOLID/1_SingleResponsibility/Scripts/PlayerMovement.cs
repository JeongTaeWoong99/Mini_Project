using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace DesignPatterns.SRP
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("수평 이동 속도")]
        [SerializeField] private float m_MoveSpeed       = 5f;

        [Tooltip("이동 속도 변화율")]
        [SerializeField] private float m_Acceleration    = 10f;

        [Tooltip("입력이 없을 때의 감속율")]
        [SerializeField] private float m_Deceleration    = 5f;

        private float               m_CurrentSpeed      = 0f;
        private CharacterController m_CharController;
        private float               m_InitialYPosition;
        private float               m_SpeedMultiplier   = 1f;

        public CharacterController CharController => m_CharController;

        public float SpeedMultiplier
        {
            get => m_SpeedMultiplier;
            set => m_SpeedMultiplier = value;
        }

        private void Awake()
        {
            m_CharController = GetComponent<CharacterController>();
        }

        void Start()
        {
            m_InitialYPosition = transform.position.y;
        }

        public void Move(Vector3 inputVector)
        {
            if (inputVector == Vector3.zero)
            {
                // 입력이 없을 때 감속 적용
                if (m_CurrentSpeed > 0)
                {
                    m_CurrentSpeed -= m_Deceleration * Time.deltaTime;
                    m_CurrentSpeed  = Mathf.Max(m_CurrentSpeed, 0); // 속도가 음수가 되지 않도록 보장
                }
            }
            else
            {
                // 입력이 있을 때 목표 속도로 부드럽게 전환
                m_CurrentSpeed = Mathf.Lerp(m_CurrentSpeed, m_MoveSpeed, Time.deltaTime * m_Acceleration);
            }

            Vector3 movement    = m_CurrentSpeed * m_SpeedMultiplier * Time.deltaTime * inputVector.normalized;
            m_CharController.Move(movement);

            // Y 위치를 일정하게 고정
            transform.position = new Vector3(transform.position.x, m_InitialYPosition, transform.position.z);
        }

    }
}