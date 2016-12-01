@ECHO OFF

PATH
WHERE NODE
WHERE NPM
node --version

CD /D "%~dp0\.."
if %errorlevel% neq 0 exit /b %errorlevel%

node --version
node build/prebuild.js
if %errorlevel% neq 0 exit /b %errorlevel%

SET BUILD_CONFIG=%1
node --version
npm run cleanbuild
if %errorlevel% neq 0 exit /b %errorlevel%
