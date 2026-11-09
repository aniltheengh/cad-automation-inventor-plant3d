@echo off
rem batch_coreconsole.bat - run a .scr on every DWG in a folder with AutoCAD Core Console (no UI, fast)
rem   batch_coreconsole.bat "D:\Project\Ortho" titleblock_rev.scr
rem accoreconsole.exe lives in the AutoCAD install folder. For Plant 3D objects to load correctly,
rem pass the Plant 3D product switch (/product PLNT3D) - see docs/12 (VERIFY on your install).
setlocal
set ACC="C:\Program Files\Autodesk\AutoCAD 2026\accoreconsole.exe"
set FOLDER=%~1
set SCR=%~f2
if "%FOLDER%"=="" ( echo usage: %~nx0 folder script.scr & exit /b 2 )
for %%F in ("%FOLDER%\*.dwg") do (
  echo === %%~nxF
  %ACC% /i "%%~fF" /s "%SCR%" /l en-US /product PLNT3D >> "%FOLDER%\coreconsole_log.txt" 2>&1
)
echo done - see "%FOLDER%\coreconsole_log.txt"
