@ECHO OFF

CD /D "%~dp0\.."
if %errorlevel% neq 0 exit /b %errorlevel%

node build/prebuild.js
if %errorlevel% neq 0 exit /b %errorlevel%

SET BUILD_CONFIG=%1
npm run cleanbuild
if %errorlevel% neq 0 exit /b %errorlevel%
