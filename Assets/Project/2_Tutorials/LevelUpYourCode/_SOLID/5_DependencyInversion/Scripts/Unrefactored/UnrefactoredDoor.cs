using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.DIP
{

    public class UnrefactoredDoor : MonoBehaviour
    {
        public void Open()
        {
            Debug.Log("도어가 열렸습니다.");
        }

        public void Close()
        {
            Debug.Log("도어가 닫혔습니다.");
        }
    }
}
