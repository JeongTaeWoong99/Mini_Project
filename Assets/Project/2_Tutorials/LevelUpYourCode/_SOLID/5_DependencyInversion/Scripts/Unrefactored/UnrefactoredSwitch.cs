using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.DIP
{
    /// <summary>
    /// 리팩토링되지 않은 형태의 스위치 메커니즘을 나타내며, 도어나 트랩을 직접 제어합니다.
    /// 구체적인 클래스(UnrefactoredDoor, UnrefactoredTrap)에 직접 의존하므로
    /// 유연성이 떨어지고 제어하는 메커니즘의 특정 구현에 강하게 결합되어 있습니다.
    /// </summary>
    public class UnrefactoredSwitch : MonoBehaviour
    {
        public UnrefactoredTrap Trap;
        public UnrefactoredDoor Door;
        public bool IsActivated;
        
        public void Activate()
        {
            if (IsActivated)
            {
                IsActivated = false;
                Door.Close();
                Trap.Disable();
            }
            else
            {
                IsActivated = true;
                Door.Open();
                Trap.Enable();
            }
        }

    }
}
