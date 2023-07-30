using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Program
{
    private static readonly int A = 16; // Define A as required
    
    public static void Main()
    {
        var data = Encoding.UTF8.GetBytes(String.Concat(Enumerable.Repeat("ABCD", 1000)));

        Console.WriteLine("Length of original: " + data.Length);
        
        var compressed = Compress(data);
        Console.WriteLine("Length of compressed: " + compressed.Length);
        
        var combinationBitCount = MaxCombBitFind(data);



        
        // Calculate compression ratio and percentage
        double compressionRatio = (double)data.Length / compressed.Length;
        double compressionPercentage = (1 - (double)compressed.Length / data.Length) * 100;
        
        Console.WriteLine("Compression ratio: " + compressionRatio);
        Console.WriteLine("Compression percentage: " + compressionPercentage + "%");
    }

    public static int MaxCombBitFind(byte[] data)
    {
        var compressedData = new List<byte>();
        int maxComb = 1;
        // Split data into chunks of size 'A' and compress each chunk
        for (int i = 0; i < data.Length; i += A)
        {
            var chunkSize = Math.Min(A, data.Length - i);
            var chunk = new byte[chunkSize];
            Array.Copy(data, i, chunk, 0, chunkSize);
            int ca = CalculateMaxCombinations(chunk);
            if (maxComb < ca)
            {
                maxComb = ca;
            }
        }
        
        var combinationBitCount = CalculateCombinationBitCount(maxComb);

        return combinationBitCount;
    }
    
    public static byte[] Compress(byte[] data)
    {
        var compressedData = new List<byte>();
        int maxComb = 1;
        // Split data into chunks of size 'A' and compress each chunk
        for (int i = 0; i < data.Length; i += A)
        {
            var chunkSize = Math.Min(A, data.Length - i);
            var chunk = new byte[chunkSize];
            Array.Copy(data, i, chunk, 0, chunkSize);
            int ca = CalculateMaxCombinations(chunk);
            if (maxComb < ca)
            {
                maxComb = ca;
            }
        }

        for (int i = 0; i < data.Length; i += A)
        {
            var chunkSize = Math.Min(A, data.Length - i);
            var chunk = new byte[chunkSize];
            Array.Copy(data, i, chunk, 0, chunkSize);

            compressedData.AddRange(ToProByte(chunk, maxComb));
        }

        return compressedData.ToArray();
    }
    
    public static byte[] Uncompress(byte[] compressedData, int originalDataLength, int combinationBitCount)
    {
        var decompressedData = new List<byte>();

        // Calculate the sumBitCount and combinationBitCount from original data length
        var sumBitCount = CalculateBitCount(originalDataLength * (originalDataLength + 1) / 2);

        // Compute the length of each compressed chunk
        var compressedChunkLength = ((8 * A) * sumBitCount) / 8; 

        // Split compressedData into chunks and decompress each chunk
        for (int i = 0; i < compressedData.Length; i += compressedChunkLength)
        {
            var chunkSize = Math.Min(compressedChunkLength, compressedData.Length - i);
            var chunk = new byte[chunkSize];
            Array.Copy(compressedData, i, chunk, 0, chunkSize);
            decompressedData.AddRange(FromProByte(chunk, sumBitCount, combinationBitCount, A));
        }

        // Return the decompressed data, truncating to the original data length in case of padding
        return decompressedData.Take(originalDataLength).ToArray();
    }
    
    private static Random random = new Random();

    public static byte[] GenerateRandomByteArray(int length)
    {
        byte[] byteArray = new byte[length];
        random.NextBytes(byteArray);
        return byteArray;
    }

    private static int CalculateBitCount(int number)
    {
        return (int)Math.Floor(Math.Log(number, 2)) + 1;
    }

    public static List<List<int>> GetCombinations(int target, int max)
    {
        var memo = new Dictionary<string, List<List<int>>>();
        var dpTable = new List<List<int>>[target + 1];

        for (int i = 0; i <= target; i++)
        {
            dpTable[i] = new List<List<int>>();
        }

        dpTable[0].Add(new List<int>());

        for (int i = 1; i <= max; i++)
        {
            for (int j = target; j >= i; j--)
            {
                foreach (var combination in dpTable[j - i])
                {
                    var newCombination = new List<int>(combination) { i };
                    dpTable[j].Add(newCombination);
                }
            }
        }

        return dpTable[target];
    }


    private static List<List<int>> ExploreCombinations(int target, int max, int start, List<int> currentCombination, Dictionary<string, List<List<int>>> memo)
    {
        var key = target + ":" + string.Join(',', currentCombination);

        if (memo.ContainsKey(key))
        {
            return memo[key];
        }

        if (target == 0)
        {
            return new List<List<int>> { new List<int>(currentCombination) };
        }

        var result = new List<List<int>>();
        for (var i = start; i <= max; i++)
        {
            if (target - i < 0) break;
            currentCombination.Add(i);
            var combinations = ExploreCombinations(target - i, max, i + 1, currentCombination, memo);
            result.AddRange(combinations.Select(combination => new List<int>(combination)));
            currentCombination.RemoveAt(currentCombination.Count - 1);
        }
        memo[key] = result;
        return result;
    }

    private static byte[] ToProByte(byte[] bytes, int maxCombinations)
    {
        var proBits = new List<bool>();

        var sumBitCount = CalculateBitCount(bytes.Length * (bytes.Length + 1) / 2);

        var combinationBitCount = CalculateCombinationBitCount(maxCombinations);

        for (var bitPos = 0; bitPos < 8; bitPos++)
        {
            var sum = CalculateProbitSum(bytes, bitPos);
            proBits.AddRange(ConvertSumToBits(sum, sumBitCount));

            if (sum > 1)
            {
                var combinations = GetCombinations(sum, bytes.Length);
                if (combinations.Count > 1)
                {
                    var actualCombination = GetActualCombination(bytes, bitPos);
                    var combinationIndex = combinations.FindIndex(c => c.SequenceEqual(actualCombination));

                    proBits.AddRange(ConvertSumToBits(combinationIndex, combinationBitCount));
                }
            }
        }

        return ConvertBitArrayToByteArray(new BitArray(proBits.ToArray()));
    }

    private static byte[] FromProByte(byte[] proBytes, int sumBitCount, int combinationBitCount, int resultBytesCount)
    {
        var proBits = new BitArray(proBytes);
        var resultBytes = new byte[resultBytesCount];
        var bitPos = 0;

        for (var i = 0; i < 8 * resultBytes.Length; i++)
        {
            var sumBits = proBits.OfType<bool>().Skip(bitPos).Take(sumBitCount).ToList();
            var sum = ConvertBitsToSum(sumBits);
            bitPos += sumBitCount;

            if (sum > 1)
            {
                var combinations = GetCombinations(sum, resultBytes.Length);
                if (combinations.Count > 1)
                {
                    var combinationIndexBits = proBits.OfType<bool>().Skip(bitPos).Take(combinationBitCount).ToList();
                    var combinationIndex = ConvertBitsToSum(combinationIndexBits);
                    bitPos += combinationBitCount;

                    SetResultBytes(resultBytes, combinations[combinationIndex], i);
                }
                else
                {
                    SetResultBytes(resultBytes, combinations[0], i);
                }
            }
        }

        return resultBytes;
    }

    private static int CalculateMaxCombinations(byte[] bytes)
    {
        var maxCombinations = 0;
        for (var bitPos = 0; bitPos < 8; bitPos++)
        {
            var sum = CalculateProbitSum(bytes, bitPos);
            if (sum > 1)
            {
                var combinations = GetCombinations(sum, bytes.Length).Count;
                if (combinations > maxCombinations)
                {
                    maxCombinations = combinations;
                }
            }
        }
        return maxCombinations;
    }

    private static int ConvertBitsToSum(List<bool> bits)
    {
        return bits.Select((bit, i) => bit ? 1 << (bits.Count - 1 - i) : 0).Sum();
    }

    private static int CalculateCombinationBitCount(int combinationCount)
    {
        return combinationCount <= 1 ? 0 : CalculateBitCount(combinationCount - 1);
    }

    private static IEnumerable<bool> ConvertSumToBits(int sum, int bitCount)
    {
        return Enumerable.Range(0, bitCount).Select(i => (sum & (1 << i)) != 0).Reverse();
    }

    private static int CalculateProbitSum(byte[] bytes, int bitPosition)
    {
        return Enumerable.Range(0, bytes.Length).Where(i => ((bytes[i] >> (7 - bitPosition)) & 1) == 1).Sum(i => i + 1);
    }

    private static List<int> GetActualCombination(byte[] bytes, int bitPos)
    {
        return Enumerable.Range(0, bytes.Length).Where(i => ((bytes[i] >> (7 - bitPos)) & 1) == 1).Select(i => i + 1).ToList();
    }

    private static void SetResultBytes(byte[] resultBytes, List<int> actualCombination, int i)
    {
        foreach (var index in actualCombination)
        {
            resultBytes[index - 1] |= (byte)(1 << (7 - (i % 8)));
        }
    }

    private static byte[] ConvertBitArrayToByteArray(BitArray bitArray)
    {
        var byteArray = new byte[(bitArray.Length - 1) / 8 + 1];
        bitArray.CopyTo(byteArray, 0);
        return byteArray;
    }
}
