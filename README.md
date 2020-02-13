# .NET Bit Position Lookup Performance

This repo contains some code that tests the performance of two different approaches to finding the indices of set bits in a number:

1. Loop through all the bits and store the indices of the set bits.
2. Generate a lookup table that precomputes the indices for 8-bit and 16-bit values. For 32-bit and 64-bit integers, split the number up into 16-bit chunks and combine the results.

The first approach can be trivially implemented as follows:

```c#
int[] GetSetBitPositions(ulong value)
{
    // shortcut optimisation
    if (value == 0)
        return new int[] { };
    
    // find the positions of the set bits
    var positions = new int[64];
    int posPtr = 0;
    for (int n = 0; n < 64; n++)
    {
        if ((value & (1UL << n)) != 0)
        {
            positions[posPtr++] = n;
        }
    }
    
    // extract and return the results
    int resultCount = posPtr;
    var results = new int[resultCount];
    Array.Copy(positions, 0, results, 0, resultCount);
    return results;
}
```

In practice we can make some optimisations here, such as computing the population count (possibly using an intrinsic) to avoid the expensive double array allocation.

The second approach is a bit too long to show in full detail here, but some pseudocode is as follows:

```c#
int[][] LUT = { // represents set bit positions for all 16-bit values
    { },
    { ... }
};

int[] GetSetBitPositions(ushort value)
{
    return LUT[value];
}

int[] GetSetBitPositions(ulong value)
{
    ushort a = unchecked((ushort)(value & 0xFFFFU));
    ushort b = unchecked((ushort)((value >> 16) & 0xFFFFU));
    ushort c = unchecked((ushort)((value >> 32) & 0xFFFFU));
    ushort d = unchecked((ushort)((value >> 48) & 0xFFFFU));
    
    return GetSetBitPositions(a)
        .Concat(GetSetBitPositions(b))
        .Concat(GetSetBitPositions(c))
        .Concat(GetSetBitPositions(d))
        .ToArray();
}
```

In practice we end up needing to do a bit more than this to make it efficient. Jagged arrays are replaced with a more efficient flat array, and some tricks are used to avoid the allocations that would be performed in `Concat` calls.

## Results

The general performance trend shows that there is some benefit to using the lookup table approach.

Performance results can be found in the text files. Time values in the columns are in milliseconds. The benchmarks were calculated on a single core of an Intel Xeon Platinum 8276L processor running at approx 2.6 - 2.8GHz boost clock.

If you want to try it yourself, open the solution in VS2019 and go to Build - Batch Build, select Check All, deselect the Debug builds, then click Rebuild. You can then run `benchmark.bat` from the project directory to run all the benchmarks.

