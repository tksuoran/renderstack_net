::
:: call $(ProjectDir)quick_publish.bat $(ProjectName) $(TargetDir) $(TargetFileName) $(ProjectDir) $(SolutionDir)
::

@echo off
Setlocal EnableDelayedExpansion

set NAME=%1
set SOURCE=%2
set EXE_FILENAME=%3
set PROJECT_DIR=%4
set SOLUTION_DIR=%5
set DEST=%SOLUTION_DIR%quick_publish\%NAME%
set SOURCE_EXE=%2%3
set DEST_EXE=%SOLUTION_DIR%quick_publish\%NAME%\%3
set ARCHIVE=%SOLUTION_DIR%quick_publish\test.zip
set ZIP="C:\Program Files\7-Zip\7z.exe"
set WINSCP="C:\Program Files (x86)\WinSCP\WinSCP.exe"
set NETSHRINK="C:\Program Files (x86)\.netshrink\netshrink.exe"
set OBFUSCAR=%SOLUTION_DIR%technologies\Obfuscar\Obfuscar.exe


::
::  Info
::
echo Quick publish %NAME% from %SOURCE%

echo ZIP          = %ZIP%
echo ARCHIVE      = %ARCHIVE%
echo DEST         = %DEST%
echo PROJECT_DIR  = %PROJECT_DIR%
echo NETSHRINK    = %NETSHRINK%
echo OBFUSCAR     = %OBFUSCAR%
echo EXE_FILENAME = %EXE_FILENAME% 
echo SOURCE_EXE   = %SOURCE_EXE%
echo DEST_EXE     = %DEST_EXE%
echo USERPROFILE  = %USERPROFILE%
:: echo copy "%SOURCE%%EXE_FILENAME%" "%DEST%"

::
::  Clean
::
echo Cleaning old publish...
if exist "%DEST%" rd /s/q "%DEST%"
if exist "%ARCHIVE%" del /q "%ARCHIVE%"


::
::  Build publish directory
::
echo Building publish directory...
md "%DEST%"
md "%DEST%\res"

echo.
echo Copying Exe and DLLs
copy %SOURCE_EXE% %DEST%
copy %SOURCE_EXE%.config %DEST%
xcopy "%SOURCE%*.dll" "%DEST%" /s/i /EXCLUDE:%PROJECT_DIR%exclude.txt
copy %SOURCE%*.dll.config %DEST%
xcopy "%PROJECT_DIR%res" "%DEST%\res" /s/i /EXCLUDE:%PROJECT_DIR%exclude.txt
copy "%PROJECT_DIR%ReadMe.txt" "%DEST%"

::
::  Extra clean
::
del /q "%DEST%\res\images\*.jpg"
del /q "%DEST%\res\fonts\large.*"

::
::  Obfuscate
::
cd "%DEST%"
%OBFUSCAR% %PROJECT_DIR%obfuscar.xml
if exist Mapping.txt del /q Mapping.txt

::
::  Compress
::
echo Compressing...
cd "%DEST%\.."
chcp 1252
%ZIP% a "%ARCHIVE%" "%DEST%"


::
::  Upload
::
echo Uploading...
%WINSCP% "/script=%PROJECT_DIR%scp.txt" /parameter "%ARCHIVE%"

echo Done
exit