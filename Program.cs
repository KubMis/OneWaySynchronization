using System.Diagnostics;

namespace OneWaySynchronization;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        if (args.Length != 4)
        {
            Console.WriteLine("Usage: MyProgram.exe <sourcePath> <destinationPath> <logPath> <syncInterval>");
            return;
        }

        var sourcePath = args[0];
        var destinationPath = args[1];
        var logPath = args[2];


        if (!int.TryParse(args[3], out var syncIntervalInSeconds) || syncIntervalInSeconds <= 0)
        {
            Console.WriteLine("Invalid synchronization interval. Must be a positive integer.");
            return;
        }

        var syncInterval = int.Parse(args[3]);

        var timer = new PeriodicTimer(TimeSpan.FromSeconds(syncInterval));
        while (await timer.WaitForNextTickAsync())
        {
            var stopwatch = Stopwatch.StartNew();
            await SynchronizeFolders(sourcePath, destinationPath, logPath);
            stopwatch.Stop();
            LogInformation($"Synchronizing folders completed took {stopwatch.Elapsed.TotalSeconds} seconds. \n", logPath);
            stopwatch.Reset();
        }
    }

    private static void LogInformation(string logMessage, string logFilePath)
    {
        Console.WriteLine(logMessage);
        using var writer = new StreamWriter(Path.Combine(logFilePath, "logs.txt"), true);
        writer.WriteLine(logMessage + " at " + DateTime.UtcNow);
    }

    private static async Task<bool> AreFilesEqual(string sourcePath, string destinationPath, string file)
    {
        var file1Path = Path.Combine(sourcePath, Path.GetFileName(file));
        var file2Path = Path.Combine(destinationPath, Path.GetFileName(file));

        var file1Bytes = await File.ReadAllBytesAsync(file1Path);
        var file2Bytes = await File.ReadAllBytesAsync(file2Path);
        if (file1Bytes.Length != file2Bytes.Length) return false;

        for (var i = 0; i < file1Bytes.Length; i++)
        {
            if (file1Bytes[i] != file2Bytes[i])
                return false;
        }

        return true;
    }

    private static async Task SynchronizeFolders(string source, string destination, string logFilePath)
    {
        if (!Directory.Exists(logFilePath))
        {
            Directory.CreateDirectory(logFilePath);
            LogInformation($"Log directory does not exist, creating folder at {logFilePath}", logFilePath);
        }

        if (!Directory.Exists(source))
        {
            LogInformation("Source directory does not exist!", logFilePath);
            return;
        }

        if (!Directory.Exists(destination))
        {
            Directory.CreateDirectory(destination);
            LogInformation($"Destination directory does not exist, creating folder at {destination}", logFilePath);
        }

        var sourceFiles = Directory.GetFiles(source, "*", SearchOption.AllDirectories).ToList();
        var destinationFiles = Directory.GetFiles(destination, "*", SearchOption.AllDirectories).ToList();

        var possibleFilesToRemove = destinationFiles
            .Where(x => sourceFiles.All(y => Path.GetFileName(y) != Path.GetFileName(x))).ToList();

        var possibleSameFiles = sourceFiles
            .SelectMany(x => destinationFiles.Where(y => Path.GetFileName(x) == Path.GetFileName(y))).ToList();

        if (possibleSameFiles.Count > 0)
            foreach (var file in possibleSameFiles)
                if (await AreFilesEqual(source, destination, file))
                {
                    LogInformation($"File {Path.GetFileName(file)} already exist in both directories!", logFilePath);
                    sourceFiles.Remove(Path.Combine(source, Path.GetFileName(file)));
                }
                else
                {
                    LogInformation(
                        $"File {Path.GetFileName(file)} exist in both directories, but are not equal. File in destination directory will be overriden",
                        logFilePath);
                }

        if (possibleFilesToRemove.Count > 0)
            foreach (var file in possibleFilesToRemove)
            {
                File.Delete(file);
                LogInformation(
                    $"File {Path.GetFileName(file)} removed from destination as it does not exist in source.",
                    logFilePath);
            }

        foreach (var file in sourceFiles)
        {
            File.Copy(file, Path.Combine(destination, Path.GetFileName(file)), true);
            LogInformation($"File {Path.Combine(destination, Path.GetFileName(file))} copied", logFilePath);
        }

        LogInformation("All files synchronized!", logFilePath);
    }
}