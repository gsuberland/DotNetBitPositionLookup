# .NET Bit Position Lookup Performance

This repo contains some code that tests the performance of two different approaches to finding the indices of set bits in a number:

1. Loop through all the bits and store the indices of the set bits.
2. Generate a lookup table that precomputes the indices for 8-bit and 16-bit values. For 32-bit and 64-bit integers, split the number up into 16-bit chunks and combine the results.



Performance results can be found in the text files. Time values in the columns are in milliseconds. The benchmarks were calculated on a single core of an Intel Xeon Platinum 8276L processor running at approx 2.6 - 2.8GHz boost clock.

The general trend shows that there is a small speedup by using 16-bit lookup tables on 32-bit and 64-bit values.