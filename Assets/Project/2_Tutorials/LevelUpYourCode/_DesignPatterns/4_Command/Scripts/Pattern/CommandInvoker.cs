using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Command
{
    // 명령(Command) 관리자 클래스
    // 모든 명령의 실행, Undo, Redo를 관리합니다
    // Static으로 구현되어 전역에서 접근 가능합니다
    public class CommandInvoker
    {
        // Undo 스택 : 실행된 명령들을 저장 (되돌리기용)
        private static Stack<ICommand> s_UndoStack = new Stack<ICommand>();

        // Redo 스택 : 취소된 명령들을 저장 (다시 실행용)
        private static Stack<ICommand> s_RedoStack = new Stack<ICommand>();

        // 명령을 실행하고 Undo 스택에 저장
        // 새로운 명령 실행 시 Redo 스택은 초기화됨
        public static void ExecuteCommand(ICommand command)
        {
            command.Execute();              // 명령 즉시 실행
            s_UndoStack.Push(command);      // Undo 스택에 저장

            // 새로운 명령을 실행하면 Redo 히스토리는 삭제됨
            // (새로운 분기가 생성되므로 이전 Redo는 무효화)
            s_RedoStack.Clear();
        }

        // 마지막 명령을 되돌리기 (Undo)
        public static void UndoCommand()
        {
            // Undo할 명령이 있는지 확인
            if (s_UndoStack.Count > 0)
            {
                ICommand activeCommand = s_UndoStack.Pop();   // Undo 스택에서 꺼냄
                s_RedoStack.Push(activeCommand);              // Redo 스택에 저장
                activeCommand.Undo();                         // 명령 되돌리기 실행
            }
        }

        // 취소된 명령을 다시 실행 (Redo)
        public static void RedoCommand()
        {
            // Redo할 명령이 있는지 확인
            if (s_RedoStack.Count > 0)
            {
                ICommand activeCommand = s_RedoStack.Pop();   // Redo 스택에서 꺼냄
                s_UndoStack.Push(activeCommand);              // Undo 스택에 다시 저장
                activeCommand.Execute();                      // 명령 다시 실행
            }
        }
    }
}