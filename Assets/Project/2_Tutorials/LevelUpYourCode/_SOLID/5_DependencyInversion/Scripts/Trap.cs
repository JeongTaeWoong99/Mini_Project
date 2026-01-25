using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.DIP
{
    /// <summary>
    /// Trap 클래스는 ISwitchable을 구현하는 물리 기반 트랩도어를 나타냅니다.
    /// </summary>
    public class Trap : MonoBehaviour, ISwitchable
    {
        // 물리 상호작용을 위한 Rigidbody 컴포넌트
        private Rigidbody m_Rigidbody;

        // 트랩의 원래 위치, 위치 리셋에 사용됩니다.
        private Vector3 m_OriginalPosition;

        // 트랩의 원래 회전, 회전 리셋에 사용됩니다.
        private Quaternion m_OriginalRotation;

        // ISwitchable 활성 상태
        private bool m_IsActive;
        public bool IsActive => m_IsActive; // 읽기 전용
        
        private void Start()
        {
            // 물리 컴포넌트 캐싱
            m_Rigidbody = GetComponent<Rigidbody>();

            // 물리 기반 이동을 비활성화하지만 충돌 감지와 수동 이동은 허용합니다.
            m_Rigidbody.isKinematic = true;

            // 원래 트랜스폼 값 캐싱
            m_OriginalPosition = transform.position;
            m_OriginalRotation = transform.rotation;
        }

        // 물리를 활성화하고 활성 상태로 표시합니다.
        public void Activate()
        {
            m_IsActive = true;
            Debug.Log("트랩이 활성화되었습니다.");

            m_Rigidbody.isKinematic = false;
        }

        // 트랩을 비활성화하고 비활성 상태로 표시합니다.
        public void Deactivate()
        {
            // Rigidbody를 kinematic으로 리셋하여 물리 기반 이동을 비활성화합니다.
            m_Rigidbody.isKinematic = true;
            m_IsActive = false;

            // 트랩의 위치와 회전을 원래 값으로 리셋합니다.
            transform.position = m_OriginalPosition;
            transform.rotation = m_OriginalRotation;

            Debug.Log("트랩이 리셋되었습니다.");
        }
    }
}
