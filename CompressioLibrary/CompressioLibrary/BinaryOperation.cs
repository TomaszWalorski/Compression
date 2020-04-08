using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Compression
{
    public class BinaryOperation
    {
        public List<bool> bitStream { get; set; }
        public BinaryOperation()
        {
            bitStream = new List<bool>();
        }
        public void add(int inputValue, int inputSize)
        {
            var bitArray = new BitArray(System.BitConverter.GetBytes(inputValue));
            for (int i = 0; i < inputSize; i++) 
            {
                try
                {
                    bitStream.Add(bitArray[i]);
                }
                catch
                {
                    bitStream.Add(false);
                }
            }
        }
        public byte[] bitsToBytes()
        {
            var bitArr = new BitArray(bitStream.ToArray());
            int missingBits = (bitArr.Count % 8 == 0) ? 0 : (8 - (bitArr.Count % 8));
            int bytesAmount = (bitArr.Count + missingBits) / 8;
            if (missingBits != 0)
            {
                bitArr = addBitArray(bitArr, new BitArray(missingBits, false));
            }
            byte[] bytes = new byte[bytesAmount];
            bitArr.CopyTo(bytes, 0);
            return bytes;
        }
        public BitArray addBitArray(BitArray bitStream1, BitArray bitStream2)
        {
            List<bool> addedStream = new List<bool>();
            bool[] array = new bool[bitStream1.Count + bitStream2.Count];
            for (int n = 0; n < bitStream1.Count; n++)
            {
                array[n] = bitStream1[n];
            }
            for (int n = 0; n < bitStream2.Count; n++)
            {
                array[n + bitStream1.Count] = bitStream2[n];
            }
            return new BitArray(array);
        }
        public BitArray getArray(BitArray bitArray, int sizeOfNewArray, int index)
        {
            var newBitArray = new BitArray(sizeOfNewArray);
            for(int i = 0; i < sizeOfNewArray; i++)
            {
                newBitArray[i] = bitArray[index + i];
            }
            return newBitArray;
        }
        public int getInt(BitArray bitArray, int sizeOfNewArray, Int32 index)
        {
            int returnedInt = 0;
            for (int i = 0; i < sizeOfNewArray; i++)
            {
                returnedInt += bitArray[index + i] ? (int)Math.Pow(2,i) : 0;
            }
            return returnedInt;
        }
    }
}
