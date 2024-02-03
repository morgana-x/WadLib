using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace WadLib
{
    public class Wad
    {
        Stream WadStream { get; set; }
        private static byte[] WadIdentifier = { 0x41, 0x47, 0x41, 0x52 };
        int NumberOfFiles { get; set; } = 0;
        int NumberOfDirectories { get; set; } = 0;
        long DataSectionOffset { get; set; } = 0; // Where all the filedata is stored from

        List<WadFileEntry> FileEntries = new List<WadFileEntry>();
        List<WadDirectoryEntry> DirectoryEntries = new List<WadDirectoryEntry>();
        private void ReadHeader()
        {
            if (!IsWad(WadStream))
            {
                return;
            }
            WadStream.Position = 12;
            byte[] tempIntBuffer = new byte[4];

            int headerSize = 0;
            WadStream.Read(tempIntBuffer);
            headerSize = BitConverter.ToInt32(tempIntBuffer);
            WadStream.Position += headerSize; // Skip the header according to https://wiki.spiralframework.info/view/WAD

            WadStream.Read(tempIntBuffer);
            NumberOfFiles = BitConverter.ToInt32(tempIntBuffer);
            //Console.WriteLine("Number of files: " + NumberOfFiles);
            FileEntries.Clear();
            DirectoryEntries.Clear();
            for (int i =0; i < NumberOfFiles; i++)
            {
                WadFileEntry entry = new WadFileEntry();
                entry.ReadData(WadStream);
                FileEntries.Add(entry);
            }

            WadStream.Read(tempIntBuffer);
            NumberOfDirectories = BitConverter.ToInt32(tempIntBuffer);
            //Console.WriteLine("Number of Directories: " + NumberOfDirectories);
            for (int i = 0; i < NumberOfDirectories; i++)
            {
                WadDirectoryEntry entry = new WadDirectoryEntry();
                entry.ReadData(WadStream);
                DirectoryEntries.Add(entry);
            }

            DataSectionOffset = WadStream.Position;

            
        }
        public static bool IsWad(Stream stream)
        {
            stream.Position = 0;
            byte[] ident = new byte[WadIdentifier.Length];
            stream.Read(ident);
            return ident.SequenceEqual(WadIdentifier);
        }
        public void Patch(WadFileEntry entry, byte[] fileData) // For Virtual Wad Files, so reloadedII things can happen!
        {
            int index = FileEntries.IndexOf(entry);
            long oldFileSize = entry.FileSize;
            entry.FileSize = fileData.LongLength;
            FileEntries[index] = entry;
            if (index == fileData.Length -1)
            {
                return;
            }
            long sizeOfBytesToBeShifted = FileEntries[index + 1].FileOffset - WadStream.Length; // Oh lord...
            WadStream.Position = FileEntries[index + 1].FileOffset;
            long offset = 0;
            byte[] buffer = new byte[80000];
            while(offset < sizeOfBytesToBeShifted)
            {
                int bytesRead = WadStream.Read(buffer);
                WadStream.Position = FileEntries[index].FileOffset + fileData.LongLength + offset;
                WadStream.Write(buffer);
                offset += bytesRead;
            }
            //byte[] shiftBuffer = new byte[sizeOfBytesToBeShifted]; // This will crash my computer!
            //WadStream.Read(shiftBuffer);
            //WadStream.Position = FileEntries[index].FileOffset + fileData.LongLength;
            //WadStream.Write(shiftBuffer);
            WadStream.Position = FileEntries[index].FileOffset;
            WadStream.Write(fileData);
            for (int i = index + 1; i < fileData.Length; i ++)
            {
                FileEntries[i].FileOffset += (entry.FileSize - oldFileSize);
            }
        }
        public void Patch(string path, byte[] fileData) // Path relative to wad, eg: Dr1/data/us/script/something.lin etc
        {
            foreach(WadFileEntry entry in FileEntries)
            {
                if (entry.FileName ==  path)
                {
                    Patch(entry, fileData);
                    break;
                }
            }
        }

        public static void Repack(string inPath, string outPath = null)
        {
            if (outPath == null)
            {
                outPath = inPath + ".wad";
            }
       
            EnumerationOptions options = new EnumerationOptions();
            options.RecurseSubdirectories = true;
            options.MaxRecursionDepth = 200; // sure hope it doesn't get that far!

            List<string> filesToBePacked = Directory.GetFiles(inPath, "*", options).ToList();
            List<string> directorysToBePacked = Directory.GetDirectories(inPath, "*", options).ToList();


            FileStream fileStream = new FileStream(outPath, FileMode.Create);

            fileStream.Write(WadIdentifier);

            fileStream.Write(BitConverter.GetBytes((int)1)); // Major Version
            fileStream.Write(BitConverter.GetBytes((int)0)); // Minor Version

            fileStream.Write(BitConverter.GetBytes((int)0)); // Header Size
            //Skip Header data since danganronpa doesnt use that

            fileStream.Write(BitConverter.GetBytes(filesToBePacked.Count)); // Number of files

            long fileOffset = 0;

            foreach(string file in filesToBePacked) // Write all file entries
            {
                string newFileName = file.Replace(inPath + "\\", "").Replace("\\", "/"); ;//file.Replace(inPath, "");
                //newFileName = newFileName.Replace("\\", "/");
                int fileNameLength = newFileName.Length;

                fileStream.Write(BitConverter.GetBytes(fileNameLength));
                fileStream.Write(System.Text.Encoding.Default.GetBytes(newFileName));

                byte[] fileData = File.ReadAllBytes(file);

                long size = fileData.Length;
                fileData = null;
                fileStream.Write(BitConverter.GetBytes(size));

                fileStream.Write(BitConverter.GetBytes(fileOffset));

                fileOffset += size;
            }
            //directorysToBePacked.Insert(0, directorysToBePacked[0]); // why?
            directorysToBePacked.Insert(0, inPath + "\\");
            fileStream.Write(BitConverter.GetBytes((long)directorysToBePacked.Count)); // Number of Directories
            
            foreach(string dir in directorysToBePacked) // I hate abstractiongames  i hate abstractiongames i hate as
            { // Just kidding they are an amazing studio but please make better file formats :(

                string dirName = dir.Replace(inPath + "\\", "").Replace("\\", "/");
                //Console.WriteLine(dirName);
                if (dirName.Length > 0)
                {
                    fileStream.Write(BitConverter.GetBytes(dirName.Length));
                }
                
                fileStream.Write(Encoding.Default.GetBytes(dirName));

                string[] subDirectories = Directory.GetDirectories(dir);

                string[] subFiles = Directory.GetFiles(dir);

                int numberOfSubFiles = subDirectories.Length + subFiles.Length;

                fileStream.Write(BitConverter.GetBytes(numberOfSubFiles));
                // dear lord...
                foreach (string file in subFiles)
                {
                    string fileName = file.Replace(inPath + "\\", "").Replace("\\", "/").Replace(dirName + "/", "");
                    //Console.WriteLine(fileName);
                    fileStream.Write(BitConverter.GetBytes(fileName.Length));
                    fileStream.Write(Encoding.Default.GetBytes(fileName));
                    fileStream.WriteByte(0);
                }
                foreach (string file in subDirectories)
                {
                    string fileName = file.Replace(inPath + "\\", "").Replace("\\", "/");
                    if (fileName.Contains("/"))
                    {
                        fileName = fileName.Substring(fileName.LastIndexOf("/")+1);
                    }
                    //Console.WriteLine(fileName);
                    fileStream.Write(BitConverter.GetBytes(fileName.Length));
                    fileStream.Write(Encoding.Default.GetBytes(fileName));
                    fileStream.WriteByte(1);
                }

            }

            // Write all file data
            foreach (string file in filesToBePacked)
            {
                fileStream.Write(File.ReadAllBytes(file));
            }
            fileStream.Dispose();
            fileStream.Close();
        }
        public void ExtractFile(WadFileEntry entry, string outFolder)
        {
            byte[] data = new byte[entry.FileSize];
            WadStream.Position = DataSectionOffset + entry.FileOffset;
            WadStream.Read(data);
            //FileStream outStream = new FileStream(outFolder + entry.FileName, FileMode.Create);
            //outStream.Write(data);
            //outStream.Dispose();
            //outStream.Close();
            Directory.CreateDirectory( Directory.GetParent(outFolder + "\\" + entry.FileName).FullName);
            File.WriteAllBytes(outFolder + "\\" + entry.FileName, data);
            data = null;
        }
        public void ExtractFile(int id, string outFolder)
        {
            ExtractFile(FileEntries[id], outFolder);
        }
        public void ExtractFile(string file, string outFolder)
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
            {
                ExtractFile(entry, outFolder);
            }
        }
        public void Dispose()
        {
            WadStream.Dispose();
            WadStream.Close();
            FileEntries.Clear();
            DirectoryEntries.Clear();
        }
        public Wad CreateVirtualWad()
        {
            Stream virtualStream = new MemoryStream();
            WadStream.CopyTo(virtualStream);
            return new Wad(virtualStream);
        }
        public Wad(Stream stream)
        { 
            WadStream = stream;
            ReadHeader();
        }
        public Wad(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }
            this.WadStream = new FileStream(filePath, FileMode.Open);
            ReadHeader();
        }
    }
}
