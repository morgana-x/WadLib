using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WadLib
{
    //internal class WadDirectorySubFileEntry;
    internal class WadDirectoryEntry
    {
        public string DirectoryName { get; set; }
        int NumberOfFiles = 0;
        long EntryOffset = 0;
        public List<WadSubDirectoryEntry> SubDirectories { get; set; } = new List<WadSubDirectoryEntry>();
        public void ReadData(Stream stream)
        {
            EntryOffset = stream.Position;
            byte[] int32tempbuffer = new byte[4];

            stream.Read(int32tempbuffer);

            int dirNameLength = BitConverter.ToInt32(int32tempbuffer);
            byte[] tempDirNameBuffer = new byte[dirNameLength];

            stream.Read(tempDirNameBuffer);

            DirectoryName = System.Text.Encoding.Default.GetString(tempDirNameBuffer);

            stream.Read(int32tempbuffer);
            NumberOfFiles = BitConverter.ToInt32(int32tempbuffer);
            SubDirectories.Clear();
            for (int i = 0; i < NumberOfFiles; i++) // Screw this :) I ain't keeping track of this junk :D
            {
                WadSubDirectoryEntry subEntry = new WadSubDirectoryEntry();
                subEntry.ReadData(stream);
                SubDirectories.Add(subEntry);
                /*
                stream.Read(int32tempbuffer);
                int subdirNameLength = BitConverter.ToInt32(int32tempbuffer);
                stream.Position += subdirNameLength;
                stream.ReadByte();*/
            }
        }

        public void WriteData(Stream stream, bool flexible = false)
        {
            if (!flexible)
            {
                stream.Position = EntryOffset;
            }
            stream.Write(BitConverter.GetBytes(DirectoryName.Length));
            stream.Write(Encoding.Default.GetBytes(DirectoryName));
            stream.Write(BitConverter.GetBytes(SubDirectories.Count));
            foreach(var subDirEntry in SubDirectories) 
            {
                subDirEntry.WriteData(stream);
            }
        }
    }
}
