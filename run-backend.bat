@echo off
title Task Management API (.NET 10)
echo ===================================================
echo Starting Task Management System API (.NET 10)...
echo ===================================================
cd /d "%~dp0TaskManagement.Server"
dotnet run --launch-profile http
pause
