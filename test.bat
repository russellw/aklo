if "%VCINSTALLDIR%"=="" call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat"

csc -debug -out:aklo.exe *.cs
if %errorlevel% neq 0 goto :eof

aklo.exe %*
if %errorlevel% neq 0 goto :eof
