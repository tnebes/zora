@echo off

@REM Set default coverage report directory or use the first argument if provided
set COVERAGE_DIR=coverage-report
if not "%~1"=="" set COVERAGE_DIR=%~1

dotnet tool list -g | findstr /C:"dotnet-reportgenerator-globaltool" > nul
if errorlevel 1 (
    echo Installing dotnet-reportgenerator-globaltool...
    dotnet tool install -g dotnet-reportgenerator-globaltool
)

@REM run the tests and collect coverage
dotnet test --collect:"XPlat Code Coverage"
echo "Coverage report generated at %COVERAGE_DIR%/index.html"

@REM generate the coverage report
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"%COVERAGE_DIR%" -reporttypes:Html

@REM open the coverage report
start %COVERAGE_DIR%/index.html
