::
::
::

@echo off
Setlocal EnableDelayedExpansion

set ROOT=%CD%

set NAME=%1
set TARGET_DIR=%2
set TARGET_PATH=%3
set PROJECT_DIR=%4

echo ROOT        : %ROOT%
echo NAME        : %NAME%
echo TARGET_DIR  : %TARGET_DIR%
echo TARGET_PATH : %TARGET_PATH%
echo PROJECT_DIR : %PROJECT_DIR%

echo.
echo Copying DLLs...
cd %PROJECT_DIR%
xcopy *.dll %TARGET_DIR% /D /I /Y

echo.
echo Linking data...
cd "%TARGET_DIR%"
if not exist "res" mkdir "res"
cd "%PROJECT_DIR%"
cd res
set PROJECT_RES=%CD%
cd "%TARGET_DIR%"
cd res
set TARGET_RES=%CD%
echo %PROJECT_RES% > source.txt

echo PROJECT_RES : %PROJECT_RES%
echo TARGET_RES  : %TARGET_RES%

echo.
echo Copying res...
cd "%PROJECT_DIR%"
xcopy "%PROJECT_RES%" "%TARGET_RES%" /S /I /D /Y /EXCLUDE:exclude.txt

echo Copying done.
echo.
