using System.Text;

namespace WadLib
{
    public class Wad
    {
        public Stream WadStream { get; set; }

        private BinaryReader br;

        private static byte[] WadIdentifier = { 0x41, 0x47, 0x41, 0x52 }; // Identifier of wad files
        public int NumberOfFiles { get; set; } = 0;
        int NumberOfDirectories { get; set; } = 0;
        long DataSectionOffset { get; set; } = 0; // Where all the filedata is stored from



        public List<WadFileEntry> FileEntries = new List<WadFileEntry>();

        private List<WadDirectoryEntry> DirectoryEntries = new List<WadDirectoryEntry>(); // 
        private void ReadHeader() // Read File entries etc
        {
            if (!IsWad(WadStream)) { return; }

            // Skip versions, identifier, etc
            br.BaseStream.Position = 12;

            int headerSize = br.ReadInt32();
            br.BaseStream.Position += headerSize; // Skip the header according to https://wiki.spiralframework.info/view/WAD

            
            // Probably don't want any left overs from previous wad files
            FileEntries.Clear();
            DirectoryEntries.Clear();

            // Read all the file entries
            NumberOfFiles = br.ReadInt32();
            for (int i =0; i < NumberOfFiles; i++)
                FileEntries.Add(new WadFileEntry(br));

            // Read all the directory entries
            NumberOfDirectories = br.ReadInt32();
            for (int i = 0; i < NumberOfDirectories; i++)
                DirectoryEntries.Add(new WadDirectoryEntry(br));

            DataSectionOffset = WadStream.Position;      
        }
        public static bool IsWad(Stream stream) // Check if first 4 bytes equals the identifier (AGAR) for WAD files
        {
            stream.Position = 0;
            byte[] ident = new byte[WadIdentifier.Length];
            stream.Read(ident);
            return ident.SequenceEqual(WadIdentifier);
        }

        private static string GetRelativePath(string filePath, string sourcePath)
        {
            string newFileName = filePath.Replace(sourcePath, "").Replace("\\", "/");
            if (newFileName[0] == '/')
                newFileName = newFileName.Substring(1);
            return newFileName;
        }

        public static void Repack(string inPath, string outPath = null)
        {
            if (outPath == null)
                outPath = inPath + ".wad";
       
            EnumerationOptions options = new EnumerationOptions() { RecurseSubdirectories = true, MaxRecursionDepth = 220 };

            List<string> filesToBePacked = Directory.GetFiles(inPath, "*", options).ToList();
            List<string> directorysToBePacked = Directory.GetDirectories(inPath, "*", options).ToList();


            FileStream fileStream = new FileStream(outPath, FileMode.Create);

            fileStream.Write(WadIdentifier);

            fileStream.Write(BitConverter.GetBytes((int)1)); // Major Version
            fileStream.Write(BitConverter.GetBytes((int)1)); // Minor Version, Needs to be 1 or Danganronpa 1 crashes?

            fileStream.Write(BitConverter.GetBytes((int)0)); // Header Size
            //Skip Header data since danganronpa doesnt use that

            fileStream.Write(BitConverter.GetBytes(filesToBePacked.Count)); // Number of files

            long fileOffset = 0;

            foreach(string file in filesToBePacked) // Write all file entries
            {
                string newFileName = GetRelativePath(file, inPath);

                fileStream.Write(BitConverter.GetBytes(newFileName.Length));
                fileStream.Write(Encoding.Default.GetBytes(newFileName));

                long filesize = new FileInfo(file).Length;
                fileStream.Write(BitConverter.GetBytes(filesize));
                fileStream.Write(BitConverter.GetBytes(fileOffset));

                fileOffset += filesize;
            }

            directorysToBePacked.Insert(0, inPath + "/");
            fileStream.Write(BitConverter.GetBytes((long)directorysToBePacked.Count)); // Number of Directories
            
            foreach(string dir in directorysToBePacked) // I hate abstractiongames  i hate abstractiongames i hate as
            { // Just kidding they are an amazing studio but please make better file formats :(

                string dirName = GetRelativePath(dir, inPath);
                if (dirName.Length > 0)
                    fileStream.Write(BitConverter.GetBytes(dirName.Length));
                fileStream.Write(Encoding.Default.GetBytes(dirName));

                string[] subDirectories = Directory.GetDirectories(dir);
                string[] subFiles = Directory.GetFiles(dir);

                int numberOfSubFiles = subDirectories.Length + subFiles.Length;

                fileStream.Write(BitConverter.GetBytes(numberOfSubFiles));
                // dear lord...
                foreach (string file in subFiles)
                {
                    string fileName = GetRelativePath(file, inPath).Replace(dirName + "/", "");

                    fileStream.Write(BitConverter.GetBytes(fileName.Length));
                    fileStream.Write(Encoding.Default.GetBytes(fileName));
                    fileStream.WriteByte(0);
                }
                foreach (string file in subDirectories)
                {
                    string fileName = GetRelativePath(file, inPath);
                    if (fileName.Contains("/"))
                        fileName = fileName.Substring(fileName.LastIndexOf("/")+1);

                    fileStream.Write(BitConverter.GetBytes(fileName.Length));
                    fileStream.Write(Encoding.Default.GetBytes(fileName));
                    fileStream.WriteByte(1);
                }

            }

            // Write all file data
            foreach (string file in filesToBePacked)
                fileStream.Write(File.ReadAllBytes(file));

            fileStream.Dispose();
            fileStream.Close();
        }
        public byte[] GetFileData(WadFileEntry entry)
        {
            byte[] data = new byte[entry.FileSize];
            WadStream.Position = DataSectionOffset + entry.FileOffset;
            WadStream.Read(data);
            return data;
        }
        public void ExtractFile(WadFileEntry entry, string outFolder)
        {
            byte[] data = GetFileData(entry);
            Directory.CreateDirectory( Directory.GetParent( Path.Combine(outFolder,entry.FileName)).FullName);
            File.WriteAllBytes(outFolder + "/" + entry.FileName, data);

            data = null;
        }
        public void ExtractFile(int id, string outFolder)
        {
            ExtractFile(FileEntries[id], outFolder);
        }
        public void ExtractFile(string file, string outFolder) // Path relative to wad, eg: Dr1/data/us/script/something.lin etc
        {
            file = file.Replace("\\", "/");
            foreach(WadFileEntry entry in FileEntries)
            {             
                if (entry.FileName.ToLower() == file.ToLower())
                {
                    ExtractFile(entry, outFolder);
                    break;
                }
            }
        }
        public void ExtractAllFiles(string outFolder)
        {
            foreach (WadFileEntry entry in FileEntries)
                ExtractFile(entry, outFolder);
        }
        public void Dispose() // Cleanup everything
        {
            WadStream.Dispose();
            WadStream.Close();
            FileEntries.Clear();
            DirectoryEntries.Clear();
        }
        public Wad(Stream stream)
        {
            WadStream = stream;
            br = new BinaryReader(stream);
            ReadHeader();
        }
        public Wad(string filePath) : this (new FileStream(filePath, FileMode.Open))
        {
          
        }
    }
}
