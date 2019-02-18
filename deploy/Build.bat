@echo off
setlocal
FOR %%a IN (.) DO SET currentfolder=%%~nxa
CALL "%~dp0Sync.%currentfolder%.bat"

@echo Deleting all BIN and OBJ folders...
for /d /r "%~dp0" %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"

SET PARAM_VERSION="-version %1"
if "%1"=="" (SET PARAM_VERSION=)
SET PARAM_ADDTIONAL_LABEL="-additionalLabel %2"
if "%2"=="" (SET PARAM_ADDTIONAL_LABEL=)
@powershell -NoProfile -ExecutionPolicy unrestricted -Command "%~dp0build.ps1" %PARAM_VERSION% %PARAM_ADDTIONAL_LABEL%
