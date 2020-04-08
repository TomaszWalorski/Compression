using Compression.enumDataTypes;
using Compression.interfaces;
using Compression.models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Compression.compressors
{
    public class CompressorJPG : ICompressorJPG
    {
        private readonly int lenghtOfCodedWord = 9;
        private readonly int lenghtOfDictionarydWord = 8;
        private readonly int initialDictionarySize = 256;
        private BinaryOperation binaryOperator = new BinaryOperation();
        private Dictionary<string, int> DictionaryToCompress;
        private Dictionary<int, List<int>> DictionaryToDecompress;
        public byte[] originalFile;

        private byte[] StreamFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
            byte[] ImageInBytes = new byte[fs.Length];
            fs.Read(ImageInBytes, 0, System.Convert.ToInt32(fs.Length));
            fs.Close();
            return ImageInBytes;
        }
        public StringBuilder initialiseComression(string path)
        {
            StringBuilder inputFile = new StringBuilder();
            DictionaryToCompress = new Dictionary<string, int>();
            for (int i = 0; i < initialDictionarySize; i++)
            {
                char sign = (char)i;
                DictionaryToCompress.Add(sign.ToString(), i);
            }
            inputFile = new StringBuilder();
            originalFile = StreamFile(path);
            for (int i = 0; i < originalFile.Length; i++)
            {
                inputFile.Append((char)originalFile[i]);
            }
            return inputFile;
        }
        private byte[] compressionLZW(string path)
        {
            byte[] compressedFile;
            StringBuilder inputFile = initialiseComression(path);
            int dictionaryCounter = initialDictionarySize;
            int maxLenghtOfDictionry = (int)Math.Pow(2, lenghtOfCodedWord);
            string currentWord = inputFile[0].ToString();
            char nextChar;
            string newWord;
            for (int Counter = 0; Counter < inputFile.Length - 1; Counter++)
            {
                nextChar = inputFile[Counter + 1];
                newWord = currentWord + nextChar.ToString();
                if (DictionaryToCompress.ContainsKey(newWord))
                {
                    currentWord += nextChar;
                }
                else
                {
                    binaryOperator.add(DictionaryToCompress[currentWord], lenghtOfCodedWord);
                    currentWord = nextChar.ToString();
                    if (DictionaryToCompress.Count >= maxLenghtOfDictionry) DictionaryToCompress.Remove(DictionaryToCompress.FirstOrDefault(element => element.Value == dictionaryCounter).Key);
                    DictionaryToCompress.Add(newWord, dictionaryCounter++);
                    if (dictionaryCounter >= maxLenghtOfDictionry)
                        dictionaryCounter = initialDictionarySize;
                }
            }
            binaryOperator.add(DictionaryToCompress[currentWord], lenghtOfCodedWord);
            compressedFile = binaryOperator.bitsToBytes();
            DictionaryToCompress.Clear();
            binaryOperator.bitStream.Clear();

            return compressedFile;
        }
        public CompressionOutput compress(string path)
        {
            byte[] compressedBytes;
            bool isCompressed = false;
            StringBuilder message = new StringBuilder();
            if (checkFileFormat(path))
            {
                try
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    compressedBytes = compressionLZW(path);
                    timer.Stop();
                    message.Append("File:" + path + "\nCompression is preformed properly.\nTime of compression:" + timer.Elapsed + "\n");
                    isCompressed = (compressedBytes.Length < originalFile.Length);
                    if (isCompressed)
                    {
                        var value1 = (double)originalFile.Length;
                        var value2 = (double)compressedBytes.Length;
                        double ratio = Math.Truncate(value1) / value2;
                        message.Append("Compression Ratio: " + string.Format("{0:N2}", ratio) + "\n");
                        double percent = 100 - ((Math.Truncate((double)compressedBytes.Length) / originalFile.Length) * 100);
                        message.Append("Percent of saved size: " + string.Format("{0:N2}", percent) + "%\n");
                    }
                    else
                    {
                        message.Append("File is not compressed.\n");
                    }
                }
                catch
                {
                    compressedBytes = new byte[0];
                    message.Append("Compression is not preformed properly.");
                }
            }
            else
            {
                compressedBytes = new byte[0];
                message.Append("Wrong format file.");
            }
            var compressedFile = new CompressedFile(EnumDataType.image, isCompressed, "compressedText", isCompressed ? compressedBytes : originalFile);
            return new CompressionOutput(compressedFile, message);
        }
        public List<int> initialiseDecompression(byte[] compressedFile)
        {
            DictionaryToDecompress = new Dictionary<int, List<int>>();
            for (int i = 0; i < initialDictionarySize; i++)
            {
                var newList = new List<int>();
                newList.Add(i);
                DictionaryToDecompress.Add(i, newList);
            }
            var codedString = new List<int>();
            var inputBit = new BitArray(compressedFile);
            for (Int32 i = 0; i < inputBit.Count; i += lenghtOfCodedWord)
            {
                try
                {
                    var newInt = binaryOperator.getInt(inputBit, lenghtOfCodedWord, i);
                    codedString.Add(newInt);
                }
                catch
                {
                    var newInt = binaryOperator.getInt(inputBit, inputBit.Count - i, i);
                    codedString.Add(newInt);
                }
            }
            return codedString;
        }
        private void decompressionLZW(string path, byte[] compressedFile)
        {
            var codedString = initialiseDecompression(compressedFile);
            int dictionaryCounter = initialDictionarySize, current = 0, next = 0, maxLenghtOfDictionry = (int)Math.Pow(2, lenghtOfCodedWord);
            var decodedString = new List<int>();
            var toDictionary = new List<int>();
            
            for (int Counter = 0; Counter < codedString.Count - 1; Counter++)
            {
                current = codedString.ElementAt(Counter);
                next = codedString.ElementAt(Counter + 1);
                decodedString.AddRange(DictionaryToDecompress[current]);

                toDictionary.Clear();
                toDictionary.AddRange(DictionaryToDecompress[current]);
                if (next == dictionaryCounter)
                {
                    toDictionary.Add(DictionaryToDecompress[current].First());
                }
                else
                {
                    toDictionary.Add(DictionaryToDecompress[next].First());
                }
                if (DictionaryToDecompress.Count >= maxLenghtOfDictionry) DictionaryToDecompress.Remove(dictionaryCounter);
                DictionaryToDecompress.Add(dictionaryCounter++, toDictionary.ToList());
                if (dictionaryCounter >= maxLenghtOfDictionry) dictionaryCounter = initialDictionarySize;
            }
            decodedString.AddRange(DictionaryToDecompress[next]);
            
            var binaryWriter = new BinaryOperation();
            for (int i = 0; i < decodedString.Count; i++)
            {
                binaryWriter.add(decodedString.ElementAt(i), lenghtOfDictionarydWord);
            }
            var bitArr = new BitArray(binaryWriter.bitStream.ToArray());
            var outputStream = binaryWriter.bitsToBytes();
            using (FileStream sw = File.Create(path))
            {
                sw.Write(outputStream, 0, outputStream.Length);
            }
            binaryOperator.bitStream.Clear();
            DictionaryToDecompress.Clear();
        }
        public StringBuilder decompress(CompressedFile compressedFile)
        {
            StringBuilder message = new StringBuilder();
            if (compressedFile.isCompressed)
            {
                try
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    decompressionLZW("decompressedText.txt", compressedFile.compressedFile);
                    timer.Stop();
                    message.Append("\nDecompression is preformed properly.\nTime of decompression:" + timer.Elapsed + "\n");
                }
                catch
                {
                    message.Append("Decompression is not preformed properly.");
                }
            }
            else
            {
                using (FileStream sw = File.Create("decompressedText.txt"))
                {
                    sw.Write(compressedFile.compressedFile, 0, compressedFile.compressedFile.Length);
                    message.Append("File's recovering (without decompression) is preformed properly.");
                }
            }
            return message;
        }
        public StringBuilder decompress(string path, CompressedFile compressedFile)
        {
            StringBuilder message = new StringBuilder();
            if (compressedFile.isCompressed)
            {
                try
                {
                    Stopwatch timer = new Stopwatch();
                    timer.Start();
                    decompressionLZW(path, compressedFile.compressedFile);
                    timer.Stop();
                    message.Append("\nDecompression is preformed properly.\nTime of decompression:" + timer.Elapsed + "\n");
                }
                catch
                {
                    message.Append("Decompression is not preformed properly.");
                }
            }
            else
            {
                using (FileStream sw = File.Create(path))
                {
                    sw.Write(compressedFile.compressedFile, 0, compressedFile.compressedFile.Length);
                    message.Append("File's recovering (without decompression) is preformed properly.");
                }
            }
            return message;
        }
        public bool checkFileFormat(string fileName)
        {
            bool result = false;
            string fileFormat = "";
            for (int i = fileName.Length - 4; i < fileName.Length; i++)
            {
                fileFormat += fileName[i];
            }
            result = (fileFormat == ".png") || (fileFormat == ".jpg");
            return result;
        }
    }
}
