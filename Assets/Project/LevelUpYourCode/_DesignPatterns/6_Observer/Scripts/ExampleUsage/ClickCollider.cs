using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ★ 서브젝트와 분리된 클릭 감지만 담당하는 컴포넌트 

namespace DesignPatterns.Observer
{
    [RequireComponent(typeof(Collider), typeof(ButtonSubject))]
    public class ClickCollider : MonoBehaviour
    {
        private ButtonSubject m_ButtonSubject;
        
        private Collider      m_Collider;

        void Start()
        {
            m_ButtonSubject = GetComponent<ButtonSubject>();
            m_Collider      = GetComponent<Collider>();
        }

        void Update()
        {
            CheckCollider();
        }

        private void CheckCollider()
        {
            // 마우스 왼쪽 버튼이 콜라이더 위에서 눌렸는지 확인
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;

                if (Physics.Raycast(ray, out hitInfo, 100f))
                {
                    if (hitInfo.collider == this.m_Collider)
                    {
                        m_ButtonSubject.ClickButton();
                    }
                }
            }
        }
    }
}
