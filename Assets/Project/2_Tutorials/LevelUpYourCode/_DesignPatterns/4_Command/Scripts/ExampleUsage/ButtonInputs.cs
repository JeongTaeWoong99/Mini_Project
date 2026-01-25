using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace DesignPatterns.Command
{
    // 사용자 입력을 처리하고 명령을 생성하는 클래스 (Client 역할)
    // Command Pattern에서 명령을 요청하는 주체
    public class ButtonInputs : MonoBehaviour
    {
        [Header("키보드 입력 설정")]
        [SerializeField] private KeyCode m_ForwardKey = KeyCode.W;  // 앞으로 이동
        [SerializeField] private KeyCode m_BackKey    = KeyCode.S;  // 뒤로 이동
        [SerializeField] private KeyCode m_LeftKey    = KeyCode.A;  // 왼쪽 이동
        [SerializeField] private KeyCode m_RightKey   = KeyCode.D;  // 오른쪽 이동
        [SerializeField] private KeyCode m_UndoKey    = KeyCode.U;  // 되돌리기
        [SerializeField] private KeyCode m_RedoKey    = KeyCode.R;  // 다시 실행

        [Header("UI 버튼 설정")]
        [SerializeField] private Button m_ForwardButton;  // 앞으로 버튼
        [SerializeField] private Button m_BackButton;     // 뒤로 버튼
        [SerializeField] private Button m_LeftButton;     // 왼쪽 버튼
        [SerializeField] private Button m_RightButton;    // 오른쪽 버튼
        [SerializeField] private Button m_UndoButton;     // Undo 버튼
        [SerializeField] private Button m_RedoButton;     // Redo 버튼

        [SerializeField] private PlayerMover m_Player;    // 제어할 플레이어

        private void Start()
        {
            // UI 버튼에 이벤트 리스너 등록
            m_ForwardButton.onClick.AddListener(OnForwardInput);
            m_BackButton.onClick.AddListener(OnBackInput);
            m_RightButton.onClick.AddListener(OnRightInput);
            m_LeftButton.onClick.AddListener(OnLeftInput);
            m_UndoButton.onClick.AddListener(OnUndoInput);
            m_RedoButton.onClick.AddListener(OnRedoInput);
        }

        // 플레이어 이동 명령을 생성하고 실행
        // Command Pattern의 핵심 : 명령 객체를 만들어 Invoker에게 전달
        private void RunPlayerCommand(PlayerMover playerMover, Vector3 movement)
        {
            if (playerMover == null)
            {
                return;
            }

            // 이동이 가능한지 검사 (장애물 체크)
            if (playerMover.IsValidMove(movement))
            {
                // 1. 명령 객체 생성 (MoveCommand)
                ICommand command = new MoveCommand(playerMover, movement);

                // 2. Invoker를 통해 명령 실행 및 Undo 스택에 저장
                // 여기서는 즉시 실행하지만, 지연 실행도 가능함
                CommandInvoker.ExecuteCommand(command);
            }
        }

        private void Update()
        {
            // 키보드 입력 감지 및 대응 버튼 이벤트 실행

            // 앞으로 이동 키 감지
            if (Input.GetKeyDown(m_ForwardKey))
            {
                m_ForwardButton.onClick.Invoke();
            }

            // 뒤로 이동 키 감지
            if (Input.GetKeyDown(m_BackKey))
            {
                m_BackButton.onClick.Invoke();
            }

            // 왼쪽 이동 키 감지
            if (Input.GetKeyDown(m_LeftKey))
            {
                m_LeftButton.onClick.Invoke();
            }

            // 오른쪽 이동 키 감지
            if (Input.GetKeyDown(m_RightKey))
            {
                m_RightButton.onClick.Invoke();
            }

            // Undo 키 감지
            if (Input.GetKeyDown(m_UndoKey))
            {
                m_UndoButton.onClick.Invoke();
            }

            // Redo 키 감지
            if (Input.GetKeyDown(m_RedoKey))
            {
                m_RedoButton.onClick.Invoke();
            }
        }

        // === 이동 명령 입력 핸들러들 ===
        // 각 방향별로 RunPlayerCommand를 호출하여 명령 객체 생성 및 실행

        private void OnLeftInput()
        {
            RunPlayerCommand(m_Player, Vector3.left);  // 왼쪽 이동 명령
        }

        private void OnRightInput()
        {
            RunPlayerCommand(m_Player, Vector3.right);  // 오른쪽 이동 명령
        }

        private void OnForwardInput()
        {
            RunPlayerCommand(m_Player, Vector3.forward);  // 앞으로 이동 명령
        }

        private void OnBackInput()
        {
            RunPlayerCommand(m_Player, Vector3.back);  // 뒤로 이동 명령
        }

        // === Undo/Redo 입력 핸들러들 ===
        // CommandInvoker를 통해 명령 되돌리기/재실행

        private void OnUndoInput()
        {
            CommandInvoker.UndoCommand();  // 마지막 명령 되돌리기
        }

        private void OnRedoInput()
        {
            CommandInvoker.RedoCommand();  // 취소된 명령 다시 실행
        }
    }
}