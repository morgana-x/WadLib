using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WadLib
{
    internal class WadSubDirectoryEntry
    {
        public string SubFileName { get; set; }
        public bool IsDirectory;

        public void ReadData(Stream stream)
        {
            byte[] nameLengthBuffer = new byte[4];
            stream.Read(nameLengthBuffer);
            byte[] nameBuffer = new byte[BitConverter.ToInt32(nameLengthBuffer)];
            stream.Read(nameBuffer);
            SubFileName = Encoding.Default.GetString(nameBuffer);
            IsDirectory = stream.ReadByte() != 0;
        }
        public void WriteData(Stream stream)
        {
            stream.Write(BitConverter.GetBytes(SubFileName.Length));
            stream.Write(Encoding.Default.GetBytes(SubFileName));
            stream.Write(BitConverter.GetBytes(IsDirectory));
        }
    }
}
