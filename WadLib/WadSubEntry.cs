using System.Text;

namespace WadLib
{
    public class WadSubEntry
    {
        public string SubFileName;
        public bool IsDirectory;

        public WadSubEntry(BinaryReader br) {
            SubFileName = Encoding.Default.GetString(br.ReadBytes(br.ReadInt32()));
            IsDirectory = br.ReadByte() != 0;
        }

        public WadSubEntry(string subFileName, bool isDirectory)
        {
            SubFileName = subFileName;
            IsDirectory = isDirectory;
        }
        public void WriteEntry(Stream stream) { WriteEntry(stream, SubFileName, IsDirectory); }
        public static void WriteEntry(Stream stream, string name, bool dir)
        {
            stream.Write(BitConverter.GetBytes(name.Length));
            stream.Write(Encoding.Default.GetBytes(name));
            stream.WriteByte(dir ? (byte)1 : (byte)0);
        }
    }
}
