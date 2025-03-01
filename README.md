# Backup
Program can be run in two modes:
- Source - source of copy
- Destination - destination of copy
# Usage
```
dotnet Backup.dll <arguments>
 ```
Or
 ```
./Backup.exe <arguments>
 ```
# Arguments
| Argument | Source | Destination | Default value | Mandatory |
|----------|--------|--------|---------------|-----------|
|-m|Type "source" to run source mode|Type "destination" to run destination mode|(None)|Yes|
|-a|IP addres of destination|IP address to listen (0.0.0.0 for any)|127.0.0.1|Yes|
|-p|TCP port of destination|TCP port to listen|7000|Yes|
|-f|Files and directories to copy (list)|Destination of copy (directory)|(None)|Yes|
|-bs|Maximum buffer size (to read files)|Maximum buffer size (to read files)|10M|Yes|
|-i|Regex of files that will be ignored|Regex of files that will be ignored|(None)|No|
|-c|(Not used)|Compare larger files by size|0 (ignored)|No|

If argument is mandatory but not specified (by user or from default values) program will ask for this value. Same if only parameter name is specified.

# Buffer size and compare larger files by size
Buffer size and compare larger files by size can be specifed with postfix.

|Postfix|Multiplier|Example|Real value|
|------|----------|-------|----------|
|(None)|1|2024|2KiB| 
|k or K|1024|4k or 4K|4KiB|
|m or M|1024 * 1024 = 1048576|8m or 8M|8MiB|
|g or G|1024 * 1024 * 1024 = 1073741824|1g or 1G|1GiB|

# Aliases
Files can be renamed while copying to destination. It can be done by specifing alias (in source): <br/>
```
old_path*alias
```
It allow copying files with the same name from other directories. For example:
```
<path1/name*first_file>
<path2/name*second_file>
```
# Compare larger files by size
If file (on source and destination) have the same size and is larger than given value then assume that it didn't change (0 will disable this option)

# Guard file (reserved filename)
File "backup_directory_guard" will be created in target directory (on root level). For this reason "backup_directory_guard" filename can not be used on root level. This filename can be used in nested directories.