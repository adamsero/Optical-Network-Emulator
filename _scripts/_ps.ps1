get-process | where-object {$_.MainWindowTitle -eq "ControlCenter1"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "ControlCenter2"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "ControlCenter3"} | stop-process

get-process | where-object {$_.MainWindowTitle -eq "Cloud"} | stop-process

get-process | where-object {$_.MainWindowTitle -eq "ManagementCenter"} | stop-process

get-process | where-object {$_.MainWindowTitle -eq "Host1"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Host2"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Host3"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Host4"} | stop-process

get-process | where-object {$_.MainWindowTitle -eq "Router1"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Router2"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Router3"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Router4"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Router5"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Router6"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Router7"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Router8"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Router9"} | stop-process
get-process | where-object {$_.MainWindowTitle -eq "Router10"} | stop-process
