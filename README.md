# ProbitCompress
<img width="1400" alt="image" src="https://github.com/danikishin/ProbitCompress/assets/68384137/40484e01-e07c-48a3-9f49-2274c5436a78">


## Overview 

ProbitCompress is a data compression algorithm that operates on the binary representation of data. It manipulates the bits in a sequence of data, potentially reducing its size. While this algorithm can operate on data groups of various sizes, it is optimized for 16 bytes. However, the compression process can sometimes yield a larger size than the original data, depending on the input data's characteristics.

## Methodology 

The ProbitCompress algorithm operates in several stages involving mathematical and bitwise operations. Here's a detailed look at its operation:

### Stage 1: Data Input
ProbitCompress accepts input data of any type and size.

### Stage 2: Data Group Conversion
In this stage, which is the core of the compression process, each group of data is potentially compressed. The data groups can vary in size, but through testing, it's been found that the optimal performance is achieved when using 16-byte groups. This number was arrived at empirically, and it is assumed that it provides a balance between computational efficiency and compression effectiveness.

Here's a more in-depth look at the compression process:

1. **Bit Count Calculation**: The algorithm calculates the bit count of the sum of the lengths of the bytes, based on the formula `n(n+1)/2` where `n` is the length of the byte array. This gives us the total number of bits that the combined bytes can represent.

2. **Combination Bit Count**: The algorithm determines the number of bits needed to represent the maximum number of combinations of the bytes.

3. **Summation and Combination Evaluation**: For every bit position from 0 to 7, the algorithm performs the following steps:
   - It calculates the sum of the bits in the current bit position across all bytes.
   - The sum is then converted to bits and added to the `proBits` collection.
   - If the sum of the bits is greater than 1, the algorithm calculates the combinations of the sum and byte array length.
   - If there is more than one combination, the actual combination is determined from the bytes in the current bit position, and the index of the actual combination is found from the combinations list.
   - The index of the combination is then converted to bits and added to the `proBits` collection.

4. **Conversion to Byte Array**: Finally, the `proBits` collection is converted to a byte array which represents the compressed data.

### Stage 3: Output
The algorithm outputs the compressed data in the form of a byte array.

The ability of this algorithm to reduce data size lies in its usage of combinations and sum of bits rather than the original bytes themselves. By representing data in this way, there is a potential for data size reduction. However, it's important to understand that this process can sometimes result in a larger output, especially when the input data doesn't have a favorable pattern for this kind of compression.

## Setup 

1. Install the latest .NET SDK from Microsoft's official website.
2. Clone this repository.
3. Run the project using your preferred C# compiler or IDE (like Visual Studio or Rider).

## Usage 

To use ProbitCompress, call the `Compress` method with your data. To retrieve your original data, call the `Uncompress` method with the compressed data.

## Contribution

Contributions to ProbitCompress are welcome.
