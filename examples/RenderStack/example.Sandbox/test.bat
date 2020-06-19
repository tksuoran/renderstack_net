
@echo off
Setlocal 

:: EnableDelayedExpansion

set ZIP="%PROGRAMFILES%\7-Zip\7z.exe"
if not exist "%ZIP%" echo 3
