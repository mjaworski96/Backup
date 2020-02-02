# Backup
Program can be run in two modes:
- Source - source of copy
- Target - destination of copy
# Usage
```
dotnet Backup.dll <arguments>
./Backup.exe <arguments>
 ```
# Armuments
| Argument | Source | Target | Default value | Mandatory |
|----------|--------|--------|---------------|-----------|
|-m|If "source" run source mode|If "target" run target mode|(None)|Yes|
|-a|IP addres of target|IP address to listen|127.0.0.1|Yes|
|-p|TCP Port of target|TCP port to liten|7000|Yes|
|-f|Files and directories to copy (list)|Destination of copy (directory)|(None)|Yes|
|-bs|Maximum buffer size (to read files)|Maximum buffer size (to read files)|10M|Yes|
|-i|Regex of files that will be ignored|Regex of files that will be ignored|(None)|No|

If argument is mandatory but not specified program will ask about value. Same if only parameter name is specified.

# Buffer size
Buffer size can be specifed with postfix (without spaces).

|Prefix|Multiplier|Example|Real value|
|------|----------|-------|----------|
|(None)|1|2024|2KiB| 
|k or K|1024|4k or 4K|4KiB|
|m or m|1024 * 1024 = 1048576|8m or 8M|8MiB|
|g or G|1024 * 1024 * 1024 = 1073741824|1g or 1G|1GiB|

# Aliases
Files can be renamed while copying to target. It can be done by specifing alias (in source): <br/>
```
old_path*alias
```
It allow copying files with the same name from other directories. For example:
```
<path1/name*first_file>
<path2/name*second_file>
```
