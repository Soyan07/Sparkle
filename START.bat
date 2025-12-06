@echo off
echo ========================================
echo Starting Sparkle E-commerce Application
echo ========================================
echo.

REM Kill any existing processes
echo Stopping existing processes...
taskkill /F /IM Sparkle.Api.exe 2&gt;nul
taskkill /F /IM dotnet.exe 2&gt;nul
timeout /t 2 /nobreak &gt;nul

REM Navigate to project directory
cd "%~dp0Sparkle.Api"

echo.
echo Starting application...
echo Application will be available at: http://localhost:5000
echo.
echo Press Ctrl+C to stop the server
echo.

REM Run the application with hot reload
dotnet watch run

pause
