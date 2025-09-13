using System.Text;

namespace WadLib
{
    public class WadFileEntry
    {
        public string FileName { get; set; } = "NOFILENAME";
        public long FileSize { get; set; } = 0; // int 64, 8 bytes
        public long FileOffset { get; set; } = 0; // int 64, 8 bytes The offset is from the beginning of the file data section in the parent WAD

        public long EntryOffset { get; set; } = 0; // location of where this file entry is at so it can be edited

        public WadFileEntry() { }
        public WadFileEntry(BinaryReader br) { ReadData(br); }
        public void ReadData(BinaryReader br)
        {
            EntryOffset = br.BaseStream.Position;
            FileName = Encoding.Default.GetString(br.ReadBytes(br.ReadInt32()));
            FileSize = br.ReadInt64();
            FileOffset = br.ReadInt64();
        }
        public void WriteData(Stream stream, bool flexible = false)
        {
            if (!flexible)
            {
                stream.Position = EntryOffset;
            }
            stream.Write(BitConverter.GetBytes(FileName.Length));
            stream.Write(Encoding.Default.GetBytes(FileName));
            stream.Write(BitConverter.GetBytes(FileSize));
            stream.Write(BitConverter.GetBytes(FileOffset));
        }
    }
}
