@echo off
@echo Deleting all BIN and OBJ folders...
for /d /r "%~dp0" %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"