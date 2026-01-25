using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.DIP
{
    public class UnrefactoredTrap : MonoBehaviour
    {
        private bool m_IsActive;
        public bool IsActive => m_IsActive;

        public void Enable()
        {
            m_IsActive = true;
            Debug.Log("트랩이 활성화되었습니다.");
        }

        public void Disable()
        {
            m_IsActive = false;
            Debug.Log("트랩이 비활성화되었습니다.");
        }
    }
}
