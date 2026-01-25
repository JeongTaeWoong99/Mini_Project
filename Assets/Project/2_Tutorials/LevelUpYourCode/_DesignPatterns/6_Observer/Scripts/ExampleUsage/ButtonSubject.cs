using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// ★ 서브젝트 안에 클릭 감지를 같이 넣어놓은 형태

namespace DesignPatterns.Observer
{
    [RequireComponent(typeof(Collider))]
    public class ButtonSubject: MonoBehaviour
    {
        public event Action Clicked;

        private Collider m_Collider;

        void Start()
        {
            m_Collider = GetComponent<Collider>();
        }

        public void ClickButton()
        {
            Clicked?.Invoke();
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
                        ClickButton();
                    }
                }
            }
        }
    }
}

