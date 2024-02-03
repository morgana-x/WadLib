using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
namespace WadLib
{
    public class WadFileEntry
    {
        public string FileName { get; set; } = "NOFILENAME";
        public long FileSize { get; set; } = 0; // int 64, 8 bytes
        public long FileOffset { get; set; } = 0; // int 64, 8 bytes The offset is from the beginning of the file data section in the parent WAD
        public void ReadData(Stream stream)
        {
            byte[] temp32intBuffer = new byte[4];
            byte[] temp64intBuffer = new byte[8];

            stream.Read(temp32intBuffer);
            int fileNameLength = BitConverter.ToInt32(temp32intBuffer);
            byte[] tempfilenamebuffer = new byte[fileNameLength];
            stream.Read(tempfilenamebuffer);
            FileName = System.Text.Encoding.Default.GetString(tempfilenamebuffer);

            stream.Read(temp64intBuffer);
            FileSize = BitConverter.ToInt64(temp64intBuffer);

            stream.Read(temp64intBuffer);
            FileOffset = BitConverter.ToInt64(temp64intBuffer);
        }
    }
}
