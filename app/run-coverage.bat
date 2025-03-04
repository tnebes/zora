@echo off

@REM Set default coverage report directory or use the first argument if provided
set COVERAGE_DIR=coverage-report
if not "%~1"=="" set COVERAGE_DIR=%~1

@REM if dotnet-reportgenerator-globaltool is not installed, install it
if not exist "%USERPROFILE%\.dotnet\tools\dotnet-reportgenerator-globaltool" (
    echo Installing dotnet-reportgenerator-globaltool...
    dotnet tool install -g dotnet-reportgenerator-globaltool
    echo Setting up coverage report directory...
    reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"%COVERAGE_DIR%" -reporttypes:Html
    echo Coverage report generated at %COVERAGE_DIR%/index.html
)

@REM run the tests and collect coverage
dotnet test --collect:"XPlat Code Coverage"
echo "Coverage report generated at %COVERAGE_DIR%/index.html"

@REM open the coverage report
start %COVERAGE_DIR%/index.html
