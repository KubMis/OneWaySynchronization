# OneWaySynchronization

A simple C# application for one-way folder synchronization. The program ensures that files in the source directory are synchronized with the destination directory at regular intervals.

## Features

- Synchronizes files from a source directory to a destination directory.
- Removes files from the destination directory if they do not exist in the source directory.
- Logs synchronization details, including file operations and execution time.
- Configurable synchronization interval.

## Usage

```bash
MyProgram.exe <sourcePath> <destinationPath> <logPath> <syncInterval>
```
`<sourcePath>` : Path to the source directory.
`<destinationPath>`: Path to the destination directory.
`<logPath>`: Path to the directory where logs will be stored.
`<syncInterval>`: Synchronization interval in seconds (must be a positive integer).

`MyProgram.exe "C:\Source" "C:\Destination" "C:\Logs" 10`

This will synchronize the `C:\Source` directory with the `C:\Destination` directory every 10 seconds and store logs in the `C:\Logs` directory.

## Requirements
- .NET 8.0 or later

## Logs
Synchronization details are logged in a file named `logs.txt` located in the specified log directory. Logs include:
- Files copied, removed, or skipped.
- Execution time for each synchronization cycle.

## License
This project is licensed under the MIT License.