# Backup

# Usage

## Source of backup
dotnet BackupNetCore.dll OR ./BackupNetFramework.exe source [ip address] [port] [buffer size (in bytes)] [directories and files (list)]
###Examples
- dotnet BackupNetCore.dll source 127.0.0.1 8000 10485760 ./source1 /source2
- ./BackupNetFramework.exe source 127.0.0.1 8000 10485760 ./source1 /source2
 
## Target of backup
dotnet BackupNetCore.dll OR ./BackupNetFramework.exe target [ip address (0.0.0.0 for any)] [port]  [buffer size (in bytes)] [target directory]
###Examples
- dotnet BackupNetCore.dll  target 0.0.0.0 8000 10485760 ./target
- ./BackupNetFramework.exe  target 0.0.0.0 8000 10485760 ./target