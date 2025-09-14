using System.Text;

namespace WadLib
{
    public class WadDir
    {
        public string DirectoryName;
        public List<WadSubEntry> SubDirectories { get; set; } = new List<WadSubEntry>();

        public WadDir() { }
        public WadDir(BinaryReader br) {  ReadData(br); }

        public void ReadData(BinaryReader br)
        {
            DirectoryName = Encoding.Default.GetString(br.ReadBytes(br.ReadInt32()));

            int NumberOfFiles = br.ReadInt32();
            SubDirectories.Clear();
            for (int i = 0; i < NumberOfFiles; i++)
                SubDirectories.Add(new WadSubEntry(br));
        }

        public static void WriteEntry(Stream stream, string name, int numfiles)
        {
            if (name.Length > 0)
            {
                stream.Write(BitConverter.GetBytes(name.Length));
                stream.Write(Encoding.Default.GetBytes(name));
            }
            stream.Write(BitConverter.GetBytes(numfiles));
        }
    }
}
