using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.DIP
{

    /// <summary>
    /// 두 개의 슬라이딩 도어를 열고 닫는 Door 컴포넌트입니다.
    /// 이 클래스는 추상 인터페이스 ISwitchable을 통해
    /// 제어될 수 있도록 함으로써 의존 역전 원칙(DIP)을 보여줍니다. 이를 통해 도어를 트리거하는
    /// 스위치로부터 도어를 분리합니다.
    /// </summary>
    public class Door : MonoBehaviour, ISwitchable
    {
        [Tooltip("왼쪽 슬라이딩 도어")]
        [SerializeField] private Transform m_LeftDoor;
        [Tooltip("오른쪽 슬라이딩 도어")]
        [SerializeField] private Transform m_RightDoor;
        [Tooltip("왼쪽 도어를 열 때의 오프셋 위치")]
        [SerializeField] private Vector3 m_LeftDoorOffset;
        [Tooltip("오른쪽 도어를 열 때의 오프셋 위치")]
        [SerializeField] private Vector3 m_RightDoorOffset;
        [Tooltip("도어 열림/닫힘 속도")]
        [SerializeField] private float m_Speed = 5f;
        
        // 도어 위치 캐싱
        private Vector3 m_LeftDoorStartPosition;
        private Vector3 m_RightDoorStartPosition;
        private Vector3 m_LeftDoorEndPosition;
        private Vector3 m_RightDoorEndPosition;

        // 도어가 현재 열린 상태인지 추적합니다.
        private bool m_IsActive;
        public bool IsActive => m_IsActive; // 읽기 전용
        
        private void Start()
        {
            // 도어 트랜스폼이 닫힌 위치에서 시작한다고 가정합니다.
            m_LeftDoorStartPosition  = m_LeftDoor.position;
            m_RightDoorStartPosition = m_RightDoor.position;
            m_LeftDoorEndPosition    = m_LeftDoorStartPosition + m_LeftDoorOffset;
            m_RightDoorEndPosition   = m_RightDoorStartPosition + m_RightDoorOffset;
        }

        /// 도어를 열고, 지정된 열림 위치로 이동시킵니다.
        public void Activate()
        {
            m_IsActive = true;
            Debug.Log("도어가 열렸습니다.");
            StartCoroutine(SlideDoor(m_LeftDoor, m_LeftDoorEndPosition, m_Speed));
            StartCoroutine(SlideDoor(m_RightDoor, m_RightDoorEndPosition, m_Speed));
        }

        /// 도어를 닫고, 시작 위치로 되돌립니다.
        public void Deactivate()
        {
            m_IsActive = false;
            Debug.Log("도어가 닫혔습니다.");
            StartCoroutine(SlideDoor(m_LeftDoor, m_LeftDoorStartPosition, m_Speed));
            StartCoroutine(SlideDoor(m_RightDoor, m_RightDoorStartPosition, m_Speed));
        }

        // 단일 도어를 특정 위치로 보간합니다.
        private IEnumerator SlideDoor(Transform door, Vector3 targetPosition, float speed)
        {
            while (door.position != targetPosition)
            {
                door.position = Vector3.MoveTowards(door.position, targetPosition, speed * Time.deltaTime);
                yield return null;
            }
        }
    }

}
