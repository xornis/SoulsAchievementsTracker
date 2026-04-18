
namespace SoulsAchievementsTracker
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("=== Dark Souls Achievement Tracker ===\n");

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string saveFolderPath = Path.Combine(documentsPath, "NBGI", "DARK SOULS REMASTERED");

            Console.WriteLine($"Searching in: {saveFolderPath}\n");

            if (Directory.Exists(saveFolderPath))
            {
                Console.WriteLine("Save folder found!");

                var steamIdFolders = Directory.GetDirectories(saveFolderPath);
                Console.WriteLine($"\nFound {steamIdFolders.Length} Steam ID folder(s):");

                foreach (var folder in steamIdFolders)
                {
                    Console.WriteLine();
                    string steamId = Path.GetFileName(folder);
                    Console.WriteLine($"Steam ID: {steamId}");
                    Console.WriteLine();

                    var slSaveFiles = Directory.GetFiles(folder, "*.sl2");
                    var coSaveFiles = Directory.GetFiles(folder, "*.co2");

                    Console.WriteLine($".sl2 Save files found: {slSaveFiles.Length}");

                    foreach (var slSaveFile in slSaveFiles)
                    {
                        FileInfo fileInfo = new FileInfo(slSaveFile);
                        Console.WriteLine($"- {Path.GetFileName(slSaveFile)} ({fileInfo.Length / 1024} KB)");
                    }

                    
                    Console.WriteLine($".co2 Save files found: {coSaveFiles.Length}");

                    foreach (var coSaveFile in coSaveFiles)
                    {
                        FileInfo fileInfo = new FileInfo(coSaveFile);
                        Console.WriteLine($"- {Path.GetFileName(coSaveFile)} ({fileInfo.Length / 1024} KB)");
                    }
                }
            }
            else
            {
                Console.WriteLine("Save folder NOT found!");
                Console.WriteLine("Make sure Dark Souls Remastered is installed.");
            }
        }
    }
}

