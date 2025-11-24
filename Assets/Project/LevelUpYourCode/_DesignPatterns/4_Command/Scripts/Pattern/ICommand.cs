using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DesignPatterns.Command
{
    // 명령 패턴의 핵심 인터페이스
    // 모든 명령(Command)은 이 인터페이스를 구현해야 함
    // Execute() : 명령 실행
    // Undo() : 명령 되돌리기
    public interface ICommand
    {
        public void Execute();  // 명령 실행
        public void Undo();     // 명령 취소 (되돌리기)
    }
}