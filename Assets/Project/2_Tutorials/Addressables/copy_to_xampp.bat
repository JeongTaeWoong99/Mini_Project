@echo off
setlocal

REM ============================================================
REM  copy_to_xampp.bat
REM  Addressables 빌드 산출물(ServerData)을 XAMPP 웹 루트로 복사한다.
REM
REM  사용법 : Addressables 빌드(New / Update / Clean) 이후,
REM           이 파일을 더블클릭하기만 하면 된다.
REM
REM  동작   : robocopy /MIR 로 SRC 를 DST 에 그대로 미러링한다.
REM           (DST 의 남는 파일은 삭제됨 -> 서버는 항상 = 최신 빌드)
REM
REM  참고   : REM 주석만 한글이며, echo / set / 명령 / 라벨 라인은 ASCII 로 둔다.
REM           한글 Windows(cp949) 콘솔에서 한글을 echo 하면 깨질 수 있고,
REM           if ( ... ) 블록은 포매터가 "(" 를 다음 줄로 옮기면 깨질 수 있어
REM           echo 는 ASCII, 분기는 goto 방식으로 유지한다.
REM           수정은 아래 SRC / DST 라인만 건드릴 것.
REM ============================================================

REM --- SRC : 프로젝트 ServerData (이 .bat 기준 4단계 위 = 프로젝트 루트) ---
set "SRC=%~dp0..\..\..\..\ServerData\StandaloneWindows64"

REM --- DST : XAMPP 웹 루트 아래의 원격 서버 폴더 ---
set "DST=C:\xampp\htdocs\Addressables\StandaloneWindows64"

echo ============================================================
echo  Addressables ServerData -^> XAMPP mirror copy
echo ------------------------------------------------------------
echo   SRC : %SRC%
echo   DST : %DST%
echo ============================================================
echo.

if not exist "%SRC%" goto :no_src

REM /MIR : 미러링(재귀 복사 + DST 의 남는 파일 삭제)
REM /NFL /NDL /NJH /NP : 로그를 간결하게
robocopy "%SRC%" "%DST%" /MIR /NFL /NDL /NJH /NP
set "RC=%ERRORLEVEL%"

echo.
REM robocopy 종료 코드 : 0-7 = 성공, 8 이상 = 실패
if %RC% GEQ 8 goto :failed

echo [DONE] Server synced with the latest build. (exit code %RC%)
echo        Verify : http://localhost/Addressables/StandaloneWindows64/
goto :end

:no_src
echo [ERROR] SRC folder not found. Build Addressables first.
echo         %SRC%
goto :end

:failed
echo [FAILED] robocopy exit code %RC% - copy error.
echo          Check DST write permission / path / locked files.
goto :end

:end
echo.
pause
endlocal
