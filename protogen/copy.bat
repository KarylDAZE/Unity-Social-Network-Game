@echo off
REM 将cs目录中的所有文件复制到 ../Assets/Scripts/Network/Proto 和 ../Server/proto

REM 复制到 ../Scripts/Network/Proto
xcopy /Y /R "cs\*.*" "..\Assets\Scripts\Network\Proto\"
if errorlevel 1 (
    echo 复制到 ..\Scripts\Network\Proto 失败.
) else (
    echo 成功复制到 ..\Scripts\Network\Proto.
)

REM 复制到 ../../Server/proto
xcopy /Y /R "cs\*.*" "..\Server\proto\"
if errorlevel 1 (
    echo 复制到 ..\..\Server\proto 失败.
) else (
    echo 成功复制到 ..\..\Server\proto.
)

pause