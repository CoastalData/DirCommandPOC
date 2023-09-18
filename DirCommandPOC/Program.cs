using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management;
using System.Text;

namespace DirCommandPOC
{
    class Program
    {
        private static PerformanceCounter cpuCounter;
        private static PerformanceCounter diskCounter;
        private static float maxCpuUsage = 0;
        private static float avgCpuUsage = 0;
        private static float maxDiskUsage = 0;
        private static float avgDiskUsage = 0;
        private static int sampleCount = 0;

        static void Main(string[] args)
        {
            try
            {
                // Initialize the performance counters
                InitializePerformanceCounters();

                string pathToScan;

                // Check for path argument
                if (args.Length == 0)
                {
                    Console.WriteLine("Please enter the source directory to scan:");
                    pathToScan = Console.ReadLine();

                    // Handle null or invalid inputs
                    while (string.IsNullOrWhiteSpace(pathToScan) || !System.IO.Directory.Exists(pathToScan))
                    {
                        Console.WriteLine("Invalid directory. Please enter a valid source directory:");
                        pathToScan = Console.ReadLine();
                    }
                }
                else
                {
                    pathToScan = args[0];
                }

                // Clear the console and set background and foreground colors
                Console.Clear();
                Console.BackgroundColor = ConsoleColor.Blue;
                Console.ForegroundColor = ConsoleColor.White;

                // Set console title
                Console.Title = "DirCommandPOC - Fast File Enumerator";

                // Print the title
                Console.WriteLine("===========================");
                Console.WriteLine("  DirCommandPOC Scanner    ");
                Console.WriteLine("===========================");
                int startingTop = Console.CursorTop; // Save our starting point

                // Return console colors to normal
                Console.ResetColor();

                Console.WriteLine($"Starting the DIR command POC for path: {pathToScan}...");

                // Define the command and its arguments
                string cmd = "cmd.exe";
                string cmdArgs = $"/c dir \"{pathToScan}\" /s/b";

                // Create a process to execute the command
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = cmd,
                        Arguments = cmdArgs,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };

                // Start the process and begin reading its output
                DateTime startTime = DateTime.Now;
                process.Start();
                StreamReader reader = process.StandardOutput;

                int fileCount = 0;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    fileCount++;

                    if (fileCount % 10000 == 0)
                    {
                        var currentCpuUsage = GetCpuUsage();
                        var currentDiskUsage = GetDiskUsage();
                        UpdateStatistics(currentCpuUsage, currentDiskUsage);

                        TimeSpan currentRunTime = DateTime.Now - startTime;
                        double filesPerSecond = fileCount / currentRunTime.TotalSeconds;

                        Console.SetCursorPosition(0, startingTop);
                        Console.Write($"Processed: {fileCount.ToString("N0")} files | " +
                                      $"Files/Sec: {filesPerSecond.ToString("N2")}        ");
                        Console.SetCursorPosition(0, startingTop + 1);
                        Console.Write($"CPU: {currentCpuUsage:F2}% | " +
                                      $"Disk: {currentDiskUsage:F2}%      ");
                        Console.SetCursorPosition(0, startingTop + 2);
                        Console.Write($"Run Time: {currentRunTime.ToString(@"hh\:mm\:ss")}          ");
                    }
                }

                Console.SetCursorPosition(0, startingTop + 3); // Move cursor down to start the final stats

                process.WaitForExit();

                DateTime endTime = DateTime.Now;
                TimeSpan runtime = endTime - startTime;

                // Display the final statistics
                Console.WriteLine("\n\nFinal Statistics:");
                Console.WriteLine($"Start Time: {startTime.ToLongTimeString()}");
                Console.WriteLine($"End Time: {endTime.ToLongTimeString()}");
                Console.WriteLine($"Total Run Time: {runtime.ToString(@"hh\:mm\:ss")}");
                Console.WriteLine($"Total Files Listed: {fileCount.ToString("N0")}");
                Console.WriteLine($"Files per Second: {(fileCount / runtime.TotalSeconds).ToString("N2")}");
                Console.WriteLine($"Files per Minute: {(fileCount / runtime.TotalMinutes).ToString("N2")}");
                Console.WriteLine($"Max CPU Usage: {maxCpuUsage:F2}%");
                Console.WriteLine($"Average CPU Usage: {avgCpuUsage / sampleCount:F2}%");
                Console.WriteLine($"Max Disk Usage: {maxDiskUsage:F2}%");
                Console.WriteLine($"Average Disk Usage: {avgDiskUsage / sampleCount:F2}%");

                string SanitizeForFilename(string input)
                {
                    var invalidChars = Path.GetInvalidFileNameChars();
                    foreach (var c in invalidChars)
                    {
                        input = input.Replace(c.ToString(), "_");
                    }
                    return input;
                }


                try
                {

                    // Construct the filename
                    string appName = AppDomain.CurrentDomain.FriendlyName.Split('.')[0];
                    string rootDirName = new DirectoryInfo(pathToScan).Name;
                    if (String.IsNullOrWhiteSpace(rootDirName) || rootDirName.Length < 3) // it's probably a drive root
                    {
                        rootDirName = new DirectoryInfo(pathToScan).Root.Name.TrimEnd('\\'); // just get drive letter
                    }
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH");
                    //string logFilename = $"{appName}-{rootDirName}-{timestamp}.log.txt";
                    string logFilename = SanitizeForFilename($"{appName}-{rootDirName}-{timestamp}.log.txt");


                    // Determine save location
                    string savePath = (args.Length > 1) ? args[1] : AppDomain.CurrentDomain.BaseDirectory;
                    string fullLogPath = Path.Combine(savePath, logFilename);

                    // Save the log
                    StringBuilder logContent = new StringBuilder();
                    logContent.AppendLine("===========================");
                    logContent.AppendLine("  DirCommandPOC Scanner Log");
                    logContent.AppendLine("===========================");
                    logContent.AppendLine($"Scan Path: {pathToScan}");
                    logContent.AppendLine($"Start Time: {startTime.ToLongTimeString()}");
                    logContent.AppendLine($"End Time: {endTime.ToLongTimeString()}");
                    logContent.AppendLine($"Total Run Time: {runtime.ToString(@"hh\:mm\:ss")}");
                    logContent.AppendLine($"Total Files Listed: {fileCount.ToString("N0")}");
                    logContent.AppendLine($"Files per Second: {(fileCount / runtime.TotalSeconds).ToString("N2")}");
                    logContent.AppendLine($"Files per Minute: {(fileCount / runtime.TotalMinutes).ToString("N2")}");
                    logContent.AppendLine($"Max CPU Usage: {maxCpuUsage:F2}%");
                    logContent.AppendLine($"Average CPU Usage: {avgCpuUsage / sampleCount:F2}%");
                    logContent.AppendLine($"Max Disk Usage: {maxDiskUsage:F2}%");
                    logContent.AppendLine($"Average Disk Usage: {avgDiskUsage / sampleCount:F2}%");

                    File.WriteAllText(fullLogPath, logContent.ToString());

                    Console.WriteLine($"Log saved to: {fullLogPath}");

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}\n{ex.StackTrace}");
                    Console.ReadLine();  // Wait for the user to press Enter
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}\n{ex.StackTrace}");
                Console.ReadLine();  // Wait for the user to press Enter
            }
        }


        static void InitializePerformanceCounters()
        {
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total", true);
            diskCounter = new PerformanceCounter("LogicalDisk", "% Disk Time", "_Total", true);
        }

        static float GetCpuUsage()
        {
            return cpuCounter.NextValue();
        }

        static float GetDiskUsage()
        {
            return diskCounter.NextValue();
        }

        static void UpdateStatistics(float currentCpuUsage, float currentDiskUsage)
        {
            maxCpuUsage = Math.Max(maxCpuUsage, currentCpuUsage);
            avgCpuUsage += currentCpuUsage;

            maxDiskUsage = Math.Max(maxDiskUsage, currentDiskUsage);
            avgDiskUsage += currentDiskUsage;

            sampleCount++;
        }
    }
}
