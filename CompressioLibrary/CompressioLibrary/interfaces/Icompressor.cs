using Compression.models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compression.interfaces
{
    public interface ICompressor
    {
        CompressionOutput compress(string path);
        StringBuilder decompress(string path, CompressedFile compressedFile);
        StringBuilder decompress(CompressedFile compressedFile);
        bool checkFileFormat(string fileName);
    }
}
