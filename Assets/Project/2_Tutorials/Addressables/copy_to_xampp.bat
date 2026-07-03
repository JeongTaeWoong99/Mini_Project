@echo off
setlocal

REM ============================================================
REM  copy_to_xampp.bat
REM  Copy Addressables build output (ServerData) to the XAMPP web root.
REM
REM  Usage : After an Addressables build (New / Update / Clean),
REM          just double-click this file.
REM  Action: robocopy /MIR mirrors SRC into DST exactly
REM          (extra files in DST are removed -> server always = latest build).
REM
REM  NOTE  : ASCII-only + goto-style (no parenthesis blocks) on purpose.
REM          Korean text breaks .bat parsing on Korean Windows (cp949), and
REM          an if ( ... ) block breaks if a formatter moves "(" to a new line.
REM          Edit only the SRC / DST lines below.
REM ============================================================

REM --- SRC : project ServerData (4 levels up from this .bat = project root) ---
set "SRC=%~dp0..\..\..\..\ServerData\StandaloneWindows64"

REM --- DST : remote server folder under the XAMPP web root ---
set "DST=C:\xampp\htdocs\Addressables\StandaloneWindows64"

echo ============================================================
echo  Addressables ServerData -^> XAMPP mirror copy
echo ------------------------------------------------------------
echo   SRC : %SRC%
echo   DST : %DST%
echo ============================================================
echo.

if not exist "%SRC%" goto :no_src

REM /MIR : mirror (recursive + delete extras in DST)
REM /NFL /NDL /NJH /NP : quieter log
robocopy "%SRC%" "%DST%" /MIR /NFL /NDL /NJH /NP
set "RC=%ERRORLEVEL%"

echo.
REM robocopy exit code : 0-7 = success, 8+ = failure
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
