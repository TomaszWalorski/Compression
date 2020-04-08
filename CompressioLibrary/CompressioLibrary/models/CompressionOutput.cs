using System;
using System.Collections.Generic;
using System.Text;

namespace Compression.models
{
    public class CompressionOutput
    {
        public StringBuilder resultMessage { get; set; }
        public CompressedFile compressedFile { get; set; }
        public CompressionOutput(CompressedFile compressedFile, StringBuilder resultMessage)
        {
            this.compressedFile = compressedFile;
            this.resultMessage = resultMessage;
        }
    }
}
