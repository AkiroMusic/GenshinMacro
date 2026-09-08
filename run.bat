@echo off
cd /d "%~dp0"

echo Building project...
dotnet build -c Release --nologo -v q
if %errorlevel% neq 0 (
    echo Build failed!
    pause
    exit /b 1
)

echo Starting application...
start "" "%~dp0bin\Release\net10.0-windows\AkiMacro.exe"
