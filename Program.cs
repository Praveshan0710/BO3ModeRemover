using System.Diagnostics;

namespace ModeRemover
{
    internal sealed class Program
    {
        private static bool verbose = false;
        static void Main(string[] args)
        {
            verbose = args.Contains("-v");

            Console.Title = "Io Mode Remove";
            Console.CursorVisible = false;
            if (!File.Exists("BlackOps3.exe"))
            {
                DisplayMessage("Please run this in your Black Ops III game folder.", ConsoleColor.Red);
                Console.ReadKey(true);
                return;
            }

            do
            {
                Console.Clear();
                DisplayMessage("[L] Log the files in your game directory to a file on your Desktop", ConsoleColor.Cyan);
                DisplayMessage("[M] Remove Multiplayer Files", ConsoleColor.Red);
                DisplayMessage("[C] Remove Campaign Files", ConsoleColor.Red);
                DisplayMessage("[Z] Remove Zombies Files", ConsoleColor.Red);

                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.L:
                        WriteLog();
                        break;

                    case ConsoleKey.M:
                        RemoveModeFiles("mp_");
                        break;

                    case ConsoleKey.C:
                        RemoveModeFiles("cp_");
                        break;

                    case ConsoleKey.Z:
                        RemoveModeFiles("zm_");
                        break;
                }

            }while (true);
        }
        
        private static void RemoveModeFiles(string modePrefix)
        {
            DisplayMessage("Are you sure that you want to do this? Cannot be undone(y/n)", ConsoleColor.DarkYellow);
            if (Console.ReadKey(true).Key != ConsoleKey.Y)
            {
                DisplayMessage("Aborted", ConsoleColor.Yellow);
                Console.WriteLine("\nPress any key to continue");
                Console.ReadKey(true);
                return;
            }

            Console.Clear();

            long removedSize = 0;
            uint removedFilesCount = 0, failedFilesCount = 0;

            var sw = new StreamWriter(@".\IoModeRemover.log", true);

            ReadOnlySpan<string> dirs = ["zone", "video"];

            foreach (var dir in dirs)
            {
                if (Directory.Exists(dir))
                {
                    foreach (var file in new DirectoryInfo(dir).EnumerateFiles($"*{modePrefix}*", SearchOption.AllDirectories))
                    {
                        try
                        {
                            long fileSize = file.Length;
                            file.Delete();
                            removedSize += fileSize;
                            removedFilesCount++;
                            sw.WriteLine($"Removed {Path.GetRelativePath(@".\", file.FullName)}");
                            if (verbose) Console.WriteLine($"Removed {file}");
                        }
                        catch (IOException e)
                        {
                            failedFilesCount++;
                            DisplayMessage($"Failed to remove {file}, please close any processes that are using it, like Black Ops III then attempt to remove it again.", ConsoleColor.Red);
                            sw.WriteLine($"Failed to remove {file}\t {e.Message}");
                            if (verbose) Console.WriteLine(e.Message);
                        }
                        catch (Exception e)
                        {
                            failedFilesCount++;
                            DisplayMessage($"Error {e.Message} when attempting to remove {file}", ConsoleColor.Red);
                            sw.WriteLine($"Error {e.Message} when attempting to remove {file}");
                            if (verbose) Console.WriteLine(e.Message);
                        }
                    }
                }
            }

            if (removedFilesCount + failedFilesCount == 0)
                Console.WriteLine("No files to remove, the selected mode is not installed");
            else if (removedSize > 0)
            {
                double roundRemovedSize = Double.Round(ConvertToMegabytes(removedSize), 2);
                sw.WriteLine($"\nRemoved {roundRemovedSize}MB\nRemoved {removedFilesCount} Files");
                DisplayMessage($"\nRemoved {roundRemovedSize}MB\nRemoved {removedFilesCount} Files", ConsoleColor.Magenta);
            }

            if (failedFilesCount > 0)
            {
                sw.WriteLine($"Failed to remove {failedFilesCount} files");
                DisplayMessage($"Failed to remove {failedFilesCount} files", ConsoleColor.Red);
            }

            if (failedFilesCount != 0 || removedFilesCount != 0)
                DisplayMessage("Logged at IoModeRemover.log", ConsoleColor.Cyan);

            sw.Dispose();

            Console.WriteLine("Press any key to continue");
            Console.ReadKey(true);
        }

        public static void DisplayMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static double ConvertToMegabytes(long bytes)
        {
            return bytes / (1024 * 1024f);
        }

        public static void WriteLog()
        {
            Console.Clear();
            string logFile = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\BlackOpsIIIFiles.log";

            if (File.Exists(logFile))
            {
                DisplayMessage($"A log file already exists at {logFile}\nWould you like to overwrite it?(y/n)", ConsoleColor.DarkYellow);
                if (Console.ReadKey(true).Key == ConsoleKey.Y)
                    File.Delete(logFile);
                else
                    Console.WriteLine("Appending to existing log");
            }

            var sw = new StreamWriter(logFile, true);
            long gameDirSize = 0;

            foreach (var dir in new DirectoryInfo(@".\").EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                long dirSize = 0;
                string dirPath = Path.GetRelativePath(@".\", dir.FullName);

                sw.WriteLine($"\n{dirPath}\n");

                foreach (var file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    sw.WriteLine($"{Path.GetRelativePath(@".\", file.FullName)} file size {ConvertToMegabytes(file.Length)}MB");
                    dirSize += file.Length;
                    gameDirSize += file.Length;
                }
                sw.WriteLine($"\n{dirPath} Directory size {ConvertToMegabytes(dirSize)}MB");
            }
            sw.WriteLine($"\nTotal Game Directory Size {ConvertToMegabytes(gameDirSize)}MB");

            sw.Dispose();
            DisplayMessage($"Logged at {logFile}", ConsoleColor.Cyan);
            Console.WriteLine("Press any key to continue");
            Console.ReadKey(true);
        }
    }
}