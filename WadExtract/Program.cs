namespace WadLib
{
    public partial class Program
    {
        static void Execute(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                Console.WriteLine($"Packing {filePath}...");
                Wad.Repack(filePath);
                Console.WriteLine("Packed!");
                return;
            }

            if (!File.Exists(filePath))
            {
                Console.WriteLine($"File {filePath} doesn't exist!");
                return;
            }

            if (!Wad.IsWad(filePath))
            {
                Console.WriteLine("Invalid or corrupted WAD File!");
                return;
            }

            Console.WriteLine($"Extracting {filePath}...");

            var wad = new Wad(filePath);
            wad.ExtractAllFiles(filePath.Replace(".wad", ""));
            wad.Dispose();

            Console.WriteLine("Extracted!");
        }
        public static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                Execute(args[0]);
                return;
            }

            while (true)
            {
                Console.WriteLine("Drag and drop file to extract!");
                Console.WriteLine("OR Drag and drop folder to repack!");
                string? filePath = Console.ReadLine().Replace("\"", "");
                if (filePath == null || filePath.Trim() == "") break;
                Execute(filePath);
            }
        }
    }
}