# .NET Bit Position Lookup Performance

This repo contains some code that tests the performance of two different approaches to finding the indices of set bits in a number:

1. Loop through all the bits and store the indices of the set bits.
2. Generate a lookup table that precomputes the indices for 8-bit and 16-bit values. For 32-bit and 64-bit integers, split the number up into 16-bit chunks and combine the results.

The general trend shows that there is little difference between the approaches when compiling for .NET Framework 4.8, but on .NET Core 3.1 there is a marked performance benefit to the lookup method.

Performance results can be found in the text files. Time values in the columns are in milliseconds. The benchmarks were calculated on a single core of an Intel Xeon Platinum 8276L processor running at approx 2.6 - 2.8GHz boost clock.

If you want to try it yourself, open the solution in VS2019 and go to Build - Batch Build, select Check All, deselect the Debug builds, then click Rebuild. You can then run `benchmark.bat` from the project directory to run all the benchmarks.

