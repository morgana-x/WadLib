using System.Text;

namespace WadLib
{
    internal class WadDirectoryEntry
    {
        public string DirectoryName;
        public long EntryOffset = 0;
        public List<WadSubDirectoryEntry> SubDirectories { get; set; } = new List<WadSubDirectoryEntry>();

        public WadDirectoryEntry() { }
        public WadDirectoryEntry(BinaryReader br) {  ReadData(br); }

        public void ReadData(BinaryReader br)
        {
            EntryOffset = br.BaseStream.Position;
            DirectoryName = Encoding.Default.GetString(br.ReadBytes(br.ReadInt32()));


            int NumberOfFiles = br.ReadInt32();
            SubDirectories.Clear();
            for (int i = 0; i < NumberOfFiles; i++)
                SubDirectories.Add(new WadSubDirectoryEntry(br));
        }

        public void WriteData(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(DirectoryName.Length));
            stream.Write(Encoding.Default.GetBytes(DirectoryName));
            stream.Write(BitConverter.GetBytes(SubDirectories.Count));

            foreach(var subDirEntry in SubDirectories) 
                subDirEntry.WriteData(stream);
        }
    }
}
