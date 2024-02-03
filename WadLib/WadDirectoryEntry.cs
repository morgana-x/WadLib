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
        string DirectoryName;
        int NumberOfFiles = 0;
        public void ReadData(Stream stream)
        {
            byte[] int32tempbuffer = new byte[4];

            stream.Read(int32tempbuffer);

            int dirNameLength = BitConverter.ToInt32(int32tempbuffer);
            byte[] tempDirNameBuffer = new byte[dirNameLength];

            stream.Read(tempDirNameBuffer);

            DirectoryName = System.Text.Encoding.Default.GetString(tempDirNameBuffer);

            stream.Read(int32tempbuffer);
            NumberOfFiles = BitConverter.ToInt32(int32tempbuffer);

            for (int i =0; i< NumberOfFiles; i++) // Screw this :) I ain't keeping track of this junk :D
            {
                stream.Read(int32tempbuffer);
                int subdirNameLength = BitConverter.ToInt32(int32tempbuffer);
                stream.Position += subdirNameLength;
                stream.ReadByte();
            }
        }
    }
}
