using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.SRP
{
    public class PlayerInput : MonoBehaviour
    {
        [Header("Controls")]
        [Tooltip("WASD 키를 사용하여 이동")]
        [SerializeField] private KeyCode m_ForwardKey  = KeyCode.W;
        [SerializeField] private KeyCode m_BackwardKey = KeyCode.S;
        [SerializeField] private KeyCode m_LeftKey     = KeyCode.A;
        [SerializeField] private KeyCode m_RightKey    = KeyCode.D;

        private Vector3 m_InputVector;
        private float   m_XInput;
        private float   m_ZInput;
        private float   m_YInput;

        public Vector3 InputVector => m_InputVector;

        private void Update()
        {
            HandleInput();
        }

        public void HandleInput()
        {
            // 각 프레임 시작 시 입력 값을 0으로 초기화
            m_XInput = 0;
            m_ZInput = 0;

            if (Input.GetKey(m_ForwardKey))
            {
                m_ZInput++;
            }

            if (Input.GetKey(m_BackwardKey))
            {
                m_ZInput--;
            }

            if (Input.GetKey(m_LeftKey))
            {
                m_XInput--;
            }

            if (Input.GetKey(m_RightKey))
            {
                m_XInput++;
            }

            m_InputVector = new Vector3(m_XInput, m_YInput, m_ZInput);
        }
    }
}
