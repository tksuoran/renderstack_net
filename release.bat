::
::
::

@echo off
Setlocal EnableDelayedExpansion

set ROOT=%CD%
set /p VERSION= < VERSION
set NAME=renderstack-%VERSION%
set DEST=..\release\%NAME%
set ARCHIVE=..\release\%NAME%.zip
set ZIP="C:\Program Files\7-Zip\7z.exe"
set WINSCP="C:\Program Files (x86)\WinSCP\WinSCP.exe"
set DOXYGEN="C:\Program Files (x86)\doxygen\bin\doxygen.exe"
set DOXYPATH=..\release\%NAME%\documentation\RenderStack


::
::  Info
::
echo Release %NAME%

echo SOURCE       = %SOURCE%
echo ZIP          = %ZIP%
echo ARCHIVE      = %ARCHIVE%
echo DEST         = %DEST%
echo USERPROFILE  = %USERPROFILE%
echo DOXYPATH     = %DOXYPATH%


::
::  Clean
::
echo.
echo Cleaning old publish...
if exist "%DEST%" rd /s/q "%DEST%"
if exist "%ARCHIVE%" del /q "%ARCHIVE%"
call clean.bat


::
::  Build publish directory
::
echo.
echo Building publish directory...
md "%DEST%"

echo Copying files to publish directory...
echo robocopy . "%DEST%" /e /v /xf clean.bat build.bat release.bat renderstack_vs2008.sln doc.zip VERSION scp_rel.txt scp_doc.txt /xd *Collada *Sandbox *collada *fmod *Jitter *Obfuscar *WildMagic *RenderStack.Parameters
robocopy . "%DEST%" /e /v /xf clean.bat build.bat release.bat renderstack_vs2008.sln doc.zip VERSION scp_rel.txt scp_doc.txt /xd *Collada *Sandbox *example.Game *collada *fmod *Jitter *Obfuscar *WildMagic *RenderStack.Parameters *NetSerializer


::
::  Compress
::
echo.
echo Compressing release archive...
cd "%DEST%\.."
chcp 1252
%ZIP% a "%ARCHIVE%" "%DEST%"


::
::  Upload release archive
::
echo.
echo Uploading release archive...
%WINSCP% "/script=%ROOT%\scp_rel.txt" /parameter "%ARCHIVE%"


::
::  Generate documentation
::
echo.
echo Generating documentation...
echo cd %DOXYPATH%
cd %DOXYPATH%
%DOXYGEN% Doxyfile
echo Fixing style sheets...
copy css\*.css renderstack\ /y
echo Compressing documentation...
cd renderstack
%ZIP% a doc.zip *.*
echo Uploading documentation...
%WINSCP% "/script=%ROOT%\scp_doc.txt" /parameter doc.zip
cd %ROOT%


::
::
::
echo.
echo Done
goto :eof
