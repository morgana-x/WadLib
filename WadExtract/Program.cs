// See https://aka.ms/new-console-template for more information
namespace WadLib
{
    public partial class Program
    {
        private static void Extract(string wadFilePath)
        {
            FileStream stream = new FileStream(wadFilePath, FileMode.Open);
            if (!Wad.IsWad(stream))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Not a wad file!");
                Console.ForegroundColor = ConsoleColor.Gray;
                return;
            }
            
            string fullPath = Directory.GetParent(wadFilePath).FullName;
            string outFolder = fullPath + "\\" + wadFilePath.Replace(fullPath, "").Replace(".wad", "") + "_extracted";

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Extracting files...");
            Console.ForegroundColor = ConsoleColor.Gray;

            Wad wadFile = new Wad(stream);
            wadFile.ExtractAllFiles(outFolder);
            wadFile.Dispose();
            wadFile = null;

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Finished!");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Extracted to \n" + outFolder);
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        private static void Repack(string wadFilePath)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Repacking files...");
            Console.ForegroundColor = ConsoleColor.Gray;

            Wad.Repack(wadFilePath);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Finished!");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Repacked to \n" + wadFilePath + ".wad");
            Console.ForegroundColor = ConsoleColor.Gray;
        }
        private static void Loop(string wadFilePath) 
        {
            Console.Clear();
            Console.Title = "WAD Extract Repack";
            Console.ForegroundColor = ConsoleColor.Gray;
            if (wadFilePath == null)
            {
                Console.WriteLine("Drag and drop:\nWAD file to extract\nOR\nFolder to repack");
                wadFilePath = Console.ReadLine();
                wadFilePath = wadFilePath.Replace("\"", "");
            }
            if (File.Exists(wadFilePath))
            {
                Extract(wadFilePath);
            }
            else if (Directory.Exists(wadFilePath))
            {
                Repack(wadFilePath);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No file or folder found!");
                Console.ForegroundColor = ConsoleColor.Gray;
            }
           
        }
        public static void Main(string[] args)
        {
            // Test of extracting singular files
            /*string path = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Danganronpa Trigger Happy Havoc\\dr1_data_us.wad";
            Wad wad = new Wad(path);
            string folder = Directory.GetParent(path).FullName;
            wad.ExtractFile("Dr1/data/us/script/e00_000_000.lin", folder);
            wad.Dispose();
            wad = null;
            return;*/
            string arg = args.Length > 0 ? args[0] : null;
            while (true)
            {
                Loop(arg);
                if (arg != null)
                {
                    break;
                }
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Press any key to close...");
            Console.ReadKey();
        }
    }
}