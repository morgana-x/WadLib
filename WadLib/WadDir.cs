using System.Text;

namespace WadLib
{
    public class WadDir
    {
        public string DirectoryName = string.Empty;
        public List<WadSubEntry> SubDirectories = new List<WadSubEntry>();

        public WadDir() { }
        public WadDir(BinaryReader br)
        {
            DirectoryName = Encoding.Default.GetString(br.ReadBytes(br.ReadInt32()));

            int NumberOfFiles = br.ReadInt32();
            for (int i = 0; i < NumberOfFiles; i++)
                SubDirectories.Add(new WadSubEntry(br));
        }

        public static void WriteEntry(Stream stream, string name, int numfiles)
        {
            if (name.Length > 0) // Root folder doesn't have name, but must write number of files anyway
            {
                stream.Write(BitConverter.GetBytes(name.Length));
                stream.Write(Encoding.Default.GetBytes(name));
            }
            stream.Write(BitConverter.GetBytes(numfiles));
        }
    }
}
