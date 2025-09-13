using System.Text;

namespace WadLib
{
    public class WadFile
    {
        public string FileName { get; set; } = "NULL";
        public long FileSize { get; set; } = 0; // int 64, 8 bytes
        public long FileOffset { get; set; } = 0; // int 64, 8 bytes The offset is from the beginning of the file data section in the parent WAD

        public WadFile() { }
        public WadFile(BinaryReader br)
        {
            FileName = Encoding.Default.GetString(br.ReadBytes(br.ReadInt32()));
            FileSize = br.ReadInt64();
            FileOffset = br.ReadInt64();
        }

        public static void WriteEntry(Stream stream, string name, long size, long offset)
        {
            stream.Write(BitConverter.GetBytes(name.Length));
            stream.Write(Encoding.Default.GetBytes(name));
            stream.Write(BitConverter.GetBytes(size));
            stream.Write(BitConverter.GetBytes(offset));
        }
    }
}
