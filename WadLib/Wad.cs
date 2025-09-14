namespace WadLib
{
    public class Wad
    {
        private static byte[] WadIdentifier = { 0x41, 0x47, 0x41, 0x52 }; // Identifier of wad files
        public  Stream WadStream { get; set; }
        private BinaryReader br;


        long DataSectionOffset = 0; // Where all the filedata is stored from
        public List<WadFile> FileEntries       = new List<WadFile>();
        public List<WadDir>  DirectoryEntries  = new List<WadDir>();

        public Wad(string filePath) : this(new FileStream(filePath, FileMode.Open)) { }
        public Wad(Stream stream)
        {
            WadStream = stream;
            if (!IsWad(WadStream)) { throw (new Exception("Trying to read an invalid WAD file!!!")); }
            br = new BinaryReader(stream);

            // Skip versions, identifier, etc
            br.BaseStream.Position = 12;
            // Skip the header according to https://wiki.spiralframework.info/view/WAD
            var headersize = br.ReadInt32();
            br.BaseStream.Position += headersize;

            // Read all the file entries
            int NumberOfFiles = br.ReadInt32();
            for (int i = 0; i < NumberOfFiles; i++)
                FileEntries.Add(new WadFile(br));

            // Read all the directory entries
            int NumberOfDirectories = br.ReadInt32();
            for (int i = 0; i < NumberOfDirectories; i++)
                DirectoryEntries.Add(new WadDir(br));

            DataSectionOffset = WadStream.Position;
        }

        public static bool IsWad(Stream stream) // Check if first 4 bytes equals the identifier (AGAR) for WAD files
        {
            byte[] identifierBuffer = new byte[WadIdentifier.Length];
            stream.Position = 0;
            stream.Read(identifierBuffer);
            return identifierBuffer.SequenceEqual(WadIdentifier);
        }

        public static bool IsWad(string filepath)
        {
            using (var s = new FileStream(filepath, FileMode.Open))
                return IsWad(s);
        }

        private static string GetRelativeFilePath(string filePath, string sourcePath)
        {
            string newFileName = filePath.Replace(sourcePath, "").Replace("\\", "/");
            if (newFileName[0] == '/')
                newFileName = newFileName.Substring(1);
            return newFileName;
        }

        public static void Repack(string inFolder, string? outPath = null)
        {
            if (outPath == null)
                outPath = inFolder + ".wad";
       
            EnumerationOptions options = new EnumerationOptions() { RecurseSubdirectories = true, MaxRecursionDepth = 220 };

            List<string> filesToBePacked = Directory.GetFiles(inFolder, "*", options).ToList();
            List<string> directorysToBePacked = Directory.GetDirectories(inFolder, "*", options).ToList();


            FileStream fileStream = new FileStream(outPath, FileMode.Create);

            fileStream.Write(WadIdentifier);

            fileStream.Write(BitConverter.GetBytes((int)1)); // Major Version
            fileStream.Write(BitConverter.GetBytes((int)1)); // Minor Version, Needs to be 1 or Danganronpa 1 crashes?
            fileStream.Write(BitConverter.GetBytes((int)0)); // Header Size, set to zero to tell it to skip

            fileStream.Write(BitConverter.GetBytes(filesToBePacked.Count)); // Number of files
            long fileOffset = 0;
            foreach(string file in filesToBePacked) // Write all file entries
            {
                string internalFilePath = GetRelativeFilePath(file, inFolder);
                long fileSize = new FileInfo(file).Length;

                WadFile.WriteEntry(fileStream, internalFilePath, new FileInfo(file).Length, fileOffset);

                fileOffset += fileSize;
            }

            directorysToBePacked.Insert(0, inFolder + "/");
            fileStream.Write(BitConverter.GetBytes((long)directorysToBePacked.Count)); // Number of Directories
            
            foreach(string dir in directorysToBePacked) // I hate abstractiongames  i hate abstractiongames
            { // Just kidding they are an amazing studio but please make simpler file formats :(

                string dirName = GetRelativeFilePath(dir, inFolder);
          
                string[] subDirectories = Directory.GetDirectories(dir);
                string[] subFiles       = Directory.GetFiles(dir);

                // Write directory metadata with total number of sub file / directory entries
                WadDir.WriteEntry(fileStream, dirName, subDirectories.Length + subFiles.Length);


                foreach (string file in subFiles)
                {
                    string fileName = GetRelativeFilePath(file, inFolder).Replace(dirName + "/", "");
                    WadSubEntry.WriteEntry(fileStream, fileName, false);
                }

                foreach (string file in subDirectories)
                {
                    string fileName = GetRelativeFilePath(file, inFolder);
                    if (fileName.Contains("/"))
                        fileName = fileName.Substring(fileName.LastIndexOf("/")+1);

                    WadSubEntry.WriteEntry(fileStream, fileName, true);
                }

            }

            // Write all file data
            foreach (string file in filesToBePacked)
                fileStream.Write(File.ReadAllBytes(file));

            fileStream.Dispose();
            fileStream.Close();
        }

        public void ExtractAllFiles(string outFolder)
        { 
            foreach(var entry in FileEntries)
                ExtractFile(entry, outFolder);
        }

        public void ExtractFile(string file, string outFolder)
        { 
            ExtractFile(FindFileEntry(file), outFolder);
        }

        public void ExtractFile(WadFile? entry, string outFolder)
        {
            if (entry == null) return;

            Directory.CreateDirectory( Directory.GetParent( Path.Combine(outFolder,entry.FileName)).FullName);
            File.WriteAllBytes(outFolder + "/" + entry.FileName, GetFileData(entry));
        }

        public byte[] GetFileData(WadFile entry)
        {
            br.BaseStream.Position = DataSectionOffset + entry.FileOffset;
            return br.ReadBytes((int)entry.FileSize);
        }

        public WadFile? FindFileEntry(string path)
        {
            return FileEntries.Find((x) => x.FileName.ToLower() == path.ToLower());
        }


        public void Dispose() // Cleanup everything
        {
            WadStream.Dispose();
            WadStream.Close();
            FileEntries.Clear();
            DirectoryEntries.Clear();
        }
        
    }
}
