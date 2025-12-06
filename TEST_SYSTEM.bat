@echo off
echo ========================================
echo Sparkle E-Commerce - System Test
echo ========================================
echo.

echo [1/6] Checking .NET SDK...
dotnet --version
if errorlevel 1 (
    echo [ERROR] .NET SDK not found!
    echo Please install .NET 8.0 SDK from: https://dotnet.microsoft.com/download/dotnet/8.0
    pause
    exit /b 1
)
echo [OK] .NET SDK found
echo.

echo [2/6] Checking SQL Server...
sc query MSSQL$SQLEXPRESS | find "RUNNING" &gt;nul
if errorlevel 1 (
    echo [WARNING] SQL Server Express not running
    echo Attempting to start...
    net start MSSQL$SQLEXPRESS
    if errorlevel 1 (
        echo [ERROR] Could not start SQL Server
        echo Please install SQL Server Express from: https://www.microsoft.com/sql-server/sql-server-downloads
        pause
        exit /b 1
    )
)
echo [OK] SQL Server is running
echo.

echo [3/6] Checking project structure...
if not exist "Sparkle.Api\Sparkle.Api.csproj" (
    echo [ERROR] Sparkle.Api project not found!
    pause
    exit /b 1
)
if not exist "Sparkle.Domain\Sparkle.Domain.csproj" (
    echo [ERROR] Sparkle.Domain project not found!
    pause
    exit /b 1
)
if not exist "Sparkle.Infrastructure\Sparkle.Infrastructure.csproj" (
    echo [ERROR] Sparkle.Infrastructure project not found!
    pause
    exit /b 1
)
echo [OK] All project files found
echo.

echo [4/6] Restoring NuGet packages...
dotnet restore
if errorlevel 1 (
    echo [ERROR] Package restore failed!
    pause
    exit /b 1
)
echo [OK] Packages restored
echo.

echo [5/6] Building project (Release configuration)...
dotnet build --configuration Release --no-restore
if errorlevel 1 (
    echo [ERROR] Build failed!
    pause
    exit /b 1
)
echo [OK] Build successful
echo.

echo [6/6] Checking documentation...
if not exist "README.md" echo [WARNING] README.md not found
if not exist "HOW_TO_RUN.md" echo [WARNING] HOW_TO_RUN.md not found
if not exist "DATABASE_SETUP.md" echo [WARNING] DATABASE_SETUP.md not found
if not exist "PROJECT_STATUS.md" echo [WARNING] PROJECT_STATUS.md not found
echo [OK] Documentation check complete
echo.

echo ========================================
echo System Test Results
echo ========================================
echo.
echo [SUCCESS] All system checks passed!
echo.
echo Your Sparkle E-Commerce system is ready to run.
echo.
echo To start the application:
echo   1. Double-click START.bat
echo   2. Or run: cd Sparkle.Api ^&^& dotnet run
echo.
echo Default credentials:
echo   Admin:  admin@sparkle.local / Admin@123
echo   Vendor: techzone@sparkle.local / Vendor@123
echo   User:   user@sparkle.local / User@123
echo.
echo Application will be available at: http://localhost:5000
echo.
pause
