using System.Diagnostics;

namespace ModeRemover
{
    internal sealed class Program
    {
        static void Main()
        {
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



            long total = 0;
            var sw = new StreamWriter(@".\IoModeRemover.log", true);
            foreach (var file in Directory.EnumerateFiles(@"zone", "*", SearchOption.AllDirectories).Where(f => f.Contains(modePrefix)))
            {
                var fs = File.OpenRead(file);
                total += fs.Length;
                fs.Dispose();
                File.Delete(file);
                Debug.WriteLine($"Removed {file}");
                sw.WriteLine($"Removed {file}");
            }

            foreach (var file in Directory.EnumerateFiles(@"video", "*", SearchOption.AllDirectories).Where(f => f.Contains(modePrefix)))
            {
                var fs = File.OpenRead(file);
                total += fs.Length;
                fs.Dispose();
                File.Delete(file);
                Debug.WriteLine($"Removed {file}");
                sw.WriteLine($"Removed {file}");
            }

            if (total == 0)
            {
                DisplayMessage("Nothing to remove", ConsoleColor.Green);
            }
            else
            {
                double roundTotal = Double.Round(ConvertToMegabytes(total), 2);
                sw.WriteLine($"\nTotal removed {roundTotal}MB");
                DisplayMessage($"\nRemoved {roundTotal}MB. Logged at IoModeRemover.log", ConsoleColor.Magenta);
            }
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
            long totalsize = 0;


            //Get all the files in zone
            var zone = new DirectoryInfo(@"zone");
            sw.WriteLine("\n ZONE \n");
            foreach (var file in zone.EnumerateFiles())
            {
                sw.WriteLine($"{file.Name} {ConvertToMegabytes(file.Length)}MB"); // Get name and size in Megabytes
                totalsize += file.Length;
            }

            //Get sub dirs of zone
            foreach (var dir in zone.EnumerateDirectories("*", SearchOption.AllDirectories))
            {
                sw.WriteLine($"\n{dir.Name:X}\n");

                foreach (var file in dir.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    sw.WriteLine($"{file.Name} {ConvertToMegabytes(file.Length)}MB");
                    totalsize += file.Length;
                }
            }

            //Get all the files in video
            sw.WriteLine("\n VIDEO \n");
            var video = new DirectoryInfo(@"video");
            foreach (var file in video.EnumerateFiles())
            {
                sw.WriteLine($"{file.Name} {ConvertToMegabytes(file.Length)}MB");
                totalsize += file.Length;
            }

            sw.WriteLine($"\nTotal size {ConvertToMegabytes(totalsize)}MB");

            sw.Dispose();
            DisplayMessage($"Logged at {logFile}", ConsoleColor.Cyan);
            Console.WriteLine("Press any key to continue");
            Console.ReadKey(true);
        }
    }
}