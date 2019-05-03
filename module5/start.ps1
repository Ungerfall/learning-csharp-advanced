Start-Process "D:\development\csharp\l2-epam\leonid-petrov1\module5\DocumentsHub.WindowsService\bin\Debug\DocumentsHub.WindowsService.exe" -PassThru
Start-Sleep -Seconds 3
Start-Process "D:\development\csharp\l2-epam\leonid-petrov1\module5\DocumentsHub.Monitoring.ConsoleApp\bin\Debug\DocumentsHub.Monitoring.ConsoleApp.exe" -PassThru
Start-Sleep -Seconds 3
Start-Process "D:\development\csharp\l2-epam\leonid-petrov1\module5\DocumentsJoiner.WindowsService\bin\Debug\DocumentsJoiner.WindowsService.exe" -PassThru
Start-Sleep -Seconds 3
Start-Process "D:\development\csharp\l2-epam\leonid-petrov1\module5\DocumentsJoiner.WindowsService\bin\joiner0\DocumentsJoiner.WindowsService.exe" -PassThru
