@echo off
CALL %~dp0Sync.DevZest.Data.bat
CALL %~dp0Sync.DevZest.Data.SqlServer.bat
CALL %~dp0Sync.DevZest.Data.MySql.bat
CALL %~dp0Sync.DevZest.Data.WPF.bat
CALL %~dp0Sync.DevZest.Data.AspNetCore.bat
CALL %~dp0Sync.DevZest.Data.DbInit.bat