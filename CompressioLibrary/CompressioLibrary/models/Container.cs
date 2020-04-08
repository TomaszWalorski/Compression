using Compression.enumDataTypes;
using Compression.interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Compression.models
{
    public class Container
    {
        public List<CompressedFile> compressedFiles { get; set; }
        public List<StringBuilder> resultMessages { get; set; }
        ICompressorJPG compressorJPG;
        ICompressorDAT compressorDAT;
        ICompressorTXT compressorTXT;
        public Container(ICompressorJPG compressorJPG, ICompressorDAT compressorDAT, ICompressorTXT compressorTXT)
        {
            compressedFiles = new List<CompressedFile>();
            resultMessages = new List<StringBuilder>();
            this.compressorJPG = compressorJPG;
            this.compressorDAT = compressorDAT;
            this.compressorTXT = compressorTXT;
        }
        public ICompressor adjustingCompressor(EnumDataType dataType)
        {
            if (dataType == EnumDataType.image)
            {
                return compressorJPG;
            }
            else if(dataType == EnumDataType.numerical)
            {
                return compressorDAT;
            }
            else if (dataType == EnumDataType.text)
            {
                return compressorTXT;
            }
            return null;
        }
        public void setFile(string path, EnumDataType dataType)
        {
            var coder = adjustingCompressor(dataType);
            var output = coder.compress(path);
            compressedFiles.Add(output.compressedFile);
            resultMessages.Add(output.resultMessage);
        }
        public void getFile(int index)
        {
            var file = compressedFiles.ElementAt(index);
            var coder = adjustingCompressor(file.dataType);
            resultMessages.Add(coder.decompress(file));
        }
        public void getFile(int index, string path)
        {
            var file = compressedFiles.ElementAt(index);
            var coder = adjustingCompressor(file.dataType);
            resultMessages.Add(coder.decompress(path, file));
        }
        public void open(string path)
        {
            if (checkFileFormat(path))
            {
                byte[] compressedFiles = new byte[0];
                try
                {
                    compressedFiles = File.ReadAllBytes(path);
                }
                catch (IOException e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }
                int counter = 0;
                while (true)
                {
                    try
                    {
                        bool isCompressed = BitConverter.ToBoolean(compressedFiles, counter);
                        EnumDataType enumData;
                        int value = compressedFiles[counter + 1];
                        switch (value)
                        {
                            case 1:
                                enumData = EnumDataType.image;
                                break;
                            case 2:
                                enumData = EnumDataType.text;
                                break;
                            case 3:
                                enumData = EnumDataType.numerical;
                                break;
                            default:
                                enumData = EnumDataType.unknown;
                                break;
                        }
                        string fileName = enumData.ToString() + counter;
                        UInt32 lenght = BitConverter.ToUInt32(compressedFiles, counter + 2);
                        byte[] compressedFile = new byte[lenght];
                        for (int i = counter + 10; i < counter + 10 + lenght; i++)
                        {
                            compressedFile[i - (counter + 10)] = compressedFiles.ElementAt(counter);
                        }
                        this.compressedFiles.Add(new CompressedFile(enumData, isCompressed, fileName, compressedFile));
                        counter = counter + 10 + (int)lenght;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
            else
            {
                Console.WriteLine("Wrong entered data format.");
            }
        }
        public void save(string path)
        {
            if (checkFileFormat(path))
            {
                List<byte> outputStream = new List<byte>();
                foreach (CompressedFile file in compressedFiles)
                {
                    byte inCompressed = file.isCompressed ? byte.MaxValue : byte.MinValue;
                    byte dataType = (byte)file.dataType;
                    byte[] leght = BitConverter.GetBytes((UInt32)file.compressedFile.Length);
                    outputStream.Add(inCompressed);
                    outputStream.Add(dataType);
                    outputStream.AddRange(file.compressedFile);
                }
                var outputBytes = outputStream.ToArray();
                FileStream fs = new FileStream(path, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(outputBytes);
                bw.Close();
            }
            else
            {
                Console.WriteLine("Wrong entered data format.");
            }
        }
        public void save()
        {
            List<byte> outputStream = new List<byte>();
            foreach (CompressedFile file in compressedFiles)
            {
                byte inCompressed = file.isCompressed ? byte.MinValue : byte.MaxValue;
                byte dataType = (byte)file.dataType;
                byte[] leght = BitConverter.GetBytes((UInt32)file.compressedFile.Length);
                outputStream.Add(inCompressed);
                outputStream.Add(dataType);
                outputStream.AddRange(leght);
                outputStream.AddRange(file.compressedFile);
            }
            var outputBytes = outputStream.ToArray();
            FileStream fs = new FileStream("compressedFiles.LCFS", FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(outputBytes);
            bw.Close();
        }
        private bool checkFileFormat(string fileName)
        {
            bool result = false;
            string fileFormat = "";
            for (int i = fileName.Length - 5; i < fileName.Length; i++)
            {
                fileFormat += fileName[i];
            }
            result = (fileFormat == ".LCFS");
            return result;
        }
    }
}
