set WORKSPACE=..
set LUBAN_DLL=%WORKSPACE%\Tools\Luban\Luban.dll
set CONF_ROOT=.

dotnet %LUBAN_DLL% ^
    -t client ^
    -c cs-bin ^
    -d json ^
    -d bin ^
    --conf %CONF_ROOT%\luban.conf ^
    -x cs-bin.outputCodeDir=../../Assets/Scripts/Config/Gen ^
    -x json.outputDataDir=..\..\Assets/Res/Config/GenerateDatas/Json ^
    -x bin.outputDataDir=..\..\Assets/Res/Config/GenerateDatas/Byte
pause