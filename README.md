# Backup

# Usage

## Source of backup
dotnet Backup.dll OR ./Backup.exe source [ip address] [port] [buffer size (in bytes)] [directories and files (list)]
###Examples
- dotnet Backup.dll source 127.0.0.1 8000 10485760 ./source1 /source2
- ./Backup.exe source 127.0.0.1 8000 10485760 ./source1 /source2
 
## Target of backup
dotnet Backup.dll OR ./Backup.exe target [ip address (0.0.0.0 for any)] [port]  [buffer size (in bytes)] [target directory]
###Examples
- dotnet Backup.dll  target 0.0.0.0 8000 10485760 ./target
- ./Backup.exe  target 0.0.0.0 8000 10485760 ./target