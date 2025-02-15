@echo off
setlocal

:: Check if file path is provided
if "%~1"=="" (
    echo Usage: upload_log.bat "C:\path\to\Current.log"
    exit /b 1
)

:: Store the file path
set FILE_PATH=%~1

:: Check if file exists
if not exist "%FILE_PATH%" (
    echo Error: File not found - %FILE_PATH%
    exit /b 1
)

:: API endpoint
set API_URL=https://jiayi-api.vercel.app/api/v1/webhook/error-from-file

:: Perform the file upload using curl
curl -X POST "%API_URL%" -F "file=@%FILE_PATH%" -H "Content-Type: multipart/form-data"

:: Check if the command was successful
if %ERRORLEVEL% NEQ 0 (
    echo Upload failed.
    exit /b %ERRORLEVEL%
)

echo Upload successful!
exit /b 0
