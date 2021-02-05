@echo off
start .\..\ManagementCenter\ManagementCenter\bin\Debug\ManagementCenter.exe
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\Cloud\Cloud\bin\Debug\Cloud.exe
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\ControlCenter\ControlCenter\bin\Debug\ControlCenter.exe
ping 192.0.2.2 -n 1 -w 200 > nul

start .\..\ClientNode\ClientNode\bin\Debug\ClientNode.exe ".\..\sharedResources\tsst_config.xml" "1"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\ClientNode\ClientNode\bin\Debug\ClientNode.exe ".\..\sharedResources\tsst_config.xml" "2"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\ClientNode\ClientNode\bin\Debug\ClientNode.exe ".\..\sharedResources\tsst_config.xml" "3"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\ClientNode\ClientNode\bin\Debug\ClientNode.exe ".\..\sharedResources\tsst_config.xml" "4"
ping 192.0.2.2 -n 1 -w 200 > nul

start .\..\NetworkNode\NetworkNode\bin\Debug\NetworkNode.exe ".\..\sharedResources\tsst_config.xml" "1"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\NetworkNode\NetworkNode\bin\Debug\NetworkNode.exe ".\..\sharedResources\tsst_config.xml" "2"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\NetworkNode\NetworkNode\bin\Debug\NetworkNode.exe ".\..\sharedResources\tsst_config.xml" "3"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\NetworkNode\NetworkNode\bin\Debug\NetworkNode.exe ".\..\sharedResources\tsst_config.xml" "4"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\NetworkNode\NetworkNode\bin\Debug\NetworkNode.exe ".\..\sharedResources\tsst_config.xml" "5"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\NetworkNode\NetworkNode\bin\Debug\NetworkNode.exe ".\..\sharedResources\tsst_config.xml" "6"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\NetworkNode\NetworkNode\bin\Debug\NetworkNode.exe ".\..\sharedResources\tsst_config.xml" "7"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\NetworkNode\NetworkNode\bin\Debug\NetworkNode.exe ".\..\sharedResources\tsst_config.xml" "8"
ping 192.0.2.2 -n 1 -w 200 > nul
start .\..\NetworkNode\NetworkNode\bin\Debug\NetworkNode.exe ".\..\sharedResources\tsst_config.xml" "9"

timeout /t 1

setlocal enableDelayedExpansion

set /a shift_x=382 Rem do modyfikacji stosownie do rozmiarów monitora
set /a shift_y=530 Rem do modyfikacji stosownie do rozmiarów monitora

for /l %%A in (1, 1, 4) do (
  set /a i=%%A-1
  set /a x=i*shift_x
  start ./cmdow.exe Host%%A /mov !x! 0
)

for /l %%A in (1, 1, 9) do (
  set /a i=%%A-1
  set /a x=i*shift_x
  start ./cmdow.exe Router%%A /mov !x! !shift_y!
)

set /a cloud_pos_x=4*shift_x
set /a mc_pos_x=5*shift_x
set /a cc_pos_x=6*shift_x

start ./cmdow.exe Cloud /mov !cloud_pos_x! 0
start ./cmdow.exe ManagementCenter /mov !mc_pos_x! 0
start ./cmdow.exe ControlCenter /mov !cc_pos_x! 0