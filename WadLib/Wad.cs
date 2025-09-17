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

        private static string GetRelativePath(string sourcePath, string filePath)
        {
            string newFileName = filePath.Replace(sourcePath, "").Replace("\\", "/");
            if (newFileName[0] == '/') { newFileName = newFileName.Substring(1); }
            return newFileName;
        }

        private static string GetRelativeFolder(string sourcePath, string filePath)
        {
            var newstr = GetRelativePath(sourcePath, filePath);
            if (newstr.Contains("/")) newstr = newstr.Substring(newstr.LastIndexOf("/") + 1);
            return newstr;
        }

        public static void Repack(string inFolder, string? outPath = null)
        {
            if (outPath == null) outPath = inFolder + ".wad";
    
            FileStream fileStream = new FileStream(outPath, FileMode.Create);

            fileStream.Write(WadIdentifier); // Write WAD Identifier (AGAR)
            fileStream.Write(BitConverter.GetBytes((int)1)); // Major Version
            fileStream.Write(BitConverter.GetBytes((int)1)); // Minor Version, Needs to be 1 or Danganronpa 1 crashes?
            fileStream.Write(BitConverter.GetBytes((int)0)); // Header Size, set to zero to tell it to skip


            // Retrieve files to be processed
            var fileSearchOptions = new EnumerationOptions() { RecurseSubdirectories = true, MaxRecursionDepth = 256 };

            List<string> filesToBePacked = Directory.GetFiles(inFolder, "*", fileSearchOptions).ToList();
            List<string> directorysToBePacked = Directory.GetDirectories(inFolder, "*", fileSearchOptions).ToList();
            directorysToBePacked.Insert(0, inFolder + "/"); // Include root directory of folder
            
            
            // Write Number File Entries
            fileStream.Write(BitConverter.GetBytes(filesToBePacked.Count));
            // Write all file entries
            long fileOffset = 0;
            foreach(string file in filesToBePacked)
            {
                long fileSize = new FileInfo(file).Length;

                WadFile.WriteEntry(fileStream, GetRelativePath(inFolder, file), fileSize, fileOffset);

                fileOffset += fileSize;
            }

            // Write Number of Directory Entries
            fileStream.Write(BitConverter.GetBytes((long)directorysToBePacked.Count)); // Number of Directories
            // Write all Directory entries
            foreach(string dir in directorysToBePacked)
            {
                string dirName = Wad.GetRelativePath(inFolder, dir);

                string[] subFiles        = Directory.GetFiles(dir);
                string[] subDirectories  = Directory.GetDirectories(dir);
  
                // Write dir name & num of sub entries
                WadDir.WriteEntry(fileStream, dirName, subDirectories.Length + subFiles.Length);

                // Write Sub File Entries
                foreach (string subfile in subFiles)
                    WadSubEntry.WriteEntry(fileStream, GetRelativePath(dir, subfile), false);

                // Write Sub Dir Entries
                foreach (string subdir in subDirectories)
                    WadSubEntry.WriteEntry(fileStream, GetRelativeFolder(dir, subdir), true);
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
