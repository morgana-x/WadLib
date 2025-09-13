using System.Text;

namespace WadLib
{
    internal class WadSubDirectoryEntry
    {
        public string SubFileName;
        public bool IsDirectory;

        public WadSubDirectoryEntry() { }
        public WadSubDirectoryEntry(BinaryReader br) { ReadData(br); }

        public void ReadData(BinaryReader br)
        {
            SubFileName = Encoding.Default.GetString(br.ReadBytes(br.ReadInt32()));
            IsDirectory = br.ReadByte() != 0;
        }

        public void WriteData(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(SubFileName.Length));
            stream.Write(Encoding.Default.GetBytes(SubFileName));
            stream.Write(BitConverter.GetBytes(IsDirectory));
        }
    }
}
