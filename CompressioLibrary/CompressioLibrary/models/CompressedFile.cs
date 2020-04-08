using Compression.enumDataTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compression.models
{
    public class CompressedFile
    {
        public EnumDataType dataType { get; set; }
        public bool isCompressed { get; set; }
        public string fileName { get; set; }
        public byte[] compressedFile { get; set; }
        public CompressedFile(EnumDataType dataType, bool isCompressed, string fileName, byte[] compressedFile)
        {
            this.dataType = dataType;
            this.isCompressed = isCompressed;
            this.fileName = fileName;
            this.compressedFile = compressedFile;
        }
    }
}
