using System;
using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP
using System.Runtime.Intrinsics.X86;
#endif

public static class SetBitPositionLookup
{
	private static Lazy<byte[]> BitPositionLookup8;
	private static Lazy<byte[]> BitPositionLookup16;

	static SetBitPositionLookup()
	{
		BitPositionLookup8 = new Lazy<byte[]>(() => { return GenerateLookupTable(8); });
		BitPositionLookup16 = new Lazy<byte[]>(() => { return GenerateLookupTable(16); });
	}

	private static byte[] GenerateLookupTable(int valueSize)
	{
		// this function generates a set bit position lookup table for n-bit chunks of an integer.
		// each group of n elements in the array represents contains the indices of set bits for an input value.
		// for example, the number 6 (00000110) would be encoded as {1,2,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF} as the 1st and 2nd bits are set.

		// only allow 8-bit and 16-bit lookups (LUT sizes of 2KB / 1MB)
		// you can technically do 32-bit lookups but the LUT would be 128GB in size (!!!) and would take a long time to generate.
		if (valueSize != 8 && valueSize != 16)
		{
			throw new ArgumentOutOfRangeException(nameof(valueSize), "Value size must be 8 or 16.");
		}

		// calculate the number of values in an n-bit integer
		int valueCount = 1 << valueSize;
		// calculate the size of the table necessary to hold all the values
		int tableSize = valueCount * valueSize;

		byte[] lut = new byte[tableSize];

		// initially set all LUT elements to a padding value (0xFF)
		for (int i = 0; i < lut.Length; i++)
		{
			lut[i] = 0xFF;
		}

		// loop through all possible input values (0 .. 2^n-1)
		for (int i = 0; i < valueCount; i++)
		{
			// calculate table pointer for this input value (i * n)
			int tablePtr = i * valueSize;
			// loop through all the bit positions (0 .. n-1)
			for (int b = 0; b < valueSize; b++)
			{
				// if the b'th bit is set, add this bit position to the table
				if ((i & (1 << b)) != 0)
				{
					lut[tablePtr++] = (byte)b;
				}
			}
		}
		return lut;
	}

	public static int[] Lookup(byte value)
	{
		if (value == 0)
			return new int[] { };

		// get the 8-bit LUT
		var lut = BitPositionLookup8.Value;

		// calcualte the offset of this value in the LUT
		int offset = value * 8;

		// find out how many set bits we have
		int count = 0;
		for (int n = 0; n < 8; n++)
		{
			if (lut[offset + n] != 0xFF)
				count++;
		}

		// extract the bit positions
		int[] positions = new int[count];
		for (int i = 0; i < count; i++)
		{
			positions[i] = (int)lut[offset + i];
		}
		return positions;
	}

	public static int[] Lookup(UInt16 value)
	{
		if (value == 0)
			return new int[] { };

		// get the 16-bit LUT
		var lut = BitPositionLookup16.Value;

		// calcualte the offset of this value in the LUT
		int offset = value * 16;

		// find out how many set bits we have
		int count = 0;
		for (int n = 0; n < 16; n++)
		{
			if (lut[offset + n] != 0xFF)
				count++;
		}

		// extract the bit positions
		int[] positions = new int[count];
		for (int i = 0; i < count; i++)
		{
			positions[i] = (int)lut[offset + i];
		}
		return positions;
	}

	public static uint GetSetBitCount(uint i)
	{

#if NETCOREAPP
		// use the popcnt intrinsic to find the number of set bits for the input value
		return Popcnt.PopCount(i);
#else
		i = i - ((i >> 1) & 0x55555555U);
		i = (i & 0x33333333U) + ((i >> 2) & 0x33333333U);
		return (uint)(unchecked(((i + (i >> 4)) & 0x0F0F0F0FU) * 0x01010101U) >> 24);
#endif
	}

	public static ulong GetSetBitCount(ulong i)
	{
#if NETCOREAPP
		// use the popcnt intrinsic to find the number of set bits for the input value
#if WIN64
		return Popcnt.X64.PopCount(i);
#else
		return Popcnt.PopCount(unchecked((uint)(i & 0xFFFFFFFFUL))) +
			Popcnt.PopCount(unchecked((uint)((i >> 32) & 0xFFFFFFFFUL)));
#endif
#else
		i = i - ((i >> 1) & 0x5555555555555555UL);
		i = (i & 0x3333333333333333UL) + ((i >> 2) & 0x3333333333333333UL);
		return (ulong)(unchecked(((i + (i >> 4)) & 0xF0F0F0F0F0F0F0FUL) * 0x101010101010101UL) >> 56);
#endif
	}

	public static IEnumerable<int> Lookup(UInt32 value)
	{
		if (value == 0)
			return new int[] { };

		// find the number of set bits for the input value
		uint setBitCount = GetSetBitCount(value);
		// we now know how many results we will have, so we can use a fixed array size
		int[] results = new int[setBitCount];

		UInt16 a = unchecked((UInt16)(value & 0xFFFFU));
		UInt16 b = unchecked((UInt16)((value >> 16) & 0xFFFFU));

		var lowPositions = Lookup(a); // get positions for lower 16-bit chunk
		Array.Copy(lowPositions, 0, results, 0, lowPositions.Length);

		if (b != 0)
		{
			var highPositions = Lookup(b); // get positions for upper 16-bit chunk
			int offset = lowPositions.Length;
			for (int i = 0; i < highPositions.Length; i++)
			{
				results[offset + i] = highPositions[i] + 16; // offset indices by 16
			}
		}

		return results;
	}

	public static IEnumerable<int> Lookup(UInt64 value)
	{
		if (value == 0)
			return new int[] { };

		// use the popcnt intrinsic to find the number of set bits for the input value
		ulong setBitCount = GetSetBitCount(value);
		// we now know how many results we will have, so we can use a fixed array size
		int[] results = new int[setBitCount];

		UInt16 a = unchecked((UInt16)(value & 0xFFFFU));
		UInt16 b = unchecked((UInt16)((value >> 16) & 0xFFFFU));
		UInt16 c = unchecked((UInt16)((value >> 32) & 0xFFFFU));
		UInt16 d = unchecked((UInt16)((value >> 48) & 0xFFFFU));

		var lowPositions = Lookup(a); // get positions for lower 16-bit chunk
		Array.Copy(lowPositions, 0, results, 0, lowPositions.Length);

		int offset = lowPositions.Length;

		if (b != 0)
		{
			var bitPositions = Lookup(b); // get positions for mid-low 16-bit chunk
			for (int i = 0; i < bitPositions.Length; i++)
			{
				results[offset + i] = bitPositions[i] + 16; // offset indices by 16
			}
			offset += bitPositions.Length;
		}

		if (c != 0)
		{
			var bitPositions = Lookup(c); // get positions for mid-high 16-bit chunk
			for (int i = 0; i < bitPositions.Length; i++)
			{
				results[offset + i] = bitPositions[i] + 32; // offset indices by 32
			}
			offset += bitPositions.Length;
		}

		if (d != 0)
		{
			var bitPositions = Lookup(d); // get positions for high 16-bit chunk
			for (int i = 0; i < bitPositions.Length; i++)
			{
				results[offset + i] = bitPositions[i] + 48; // offset indices by 48
			}
		}

		return results;
	}

	public static IEnumerable<int> Lookup(sbyte value) => Lookup(unchecked((byte)value));

	public static IEnumerable<int> Lookup(Int16 value) => Lookup(unchecked((UInt16)value));

	public static IEnumerable<int> Lookup(Int32 value) => Lookup(unchecked((UInt32)value));

	public static IEnumerable<int> Lookup(Int64 value) => Lookup(unchecked((UInt64)value));

	public static IEnumerable<int> Calculate(byte value)
	{
		const int TypeSize = 8;
		var result = new List<int>(TypeSize);
		for (int n = 0; n < TypeSize; n++)
		{
			if ((value & (1U << n)) != 0)
			{
				result.Add(n);
			}
		}
		return result;
	}

	public static IEnumerable<int> Calculate(UInt16 value)
	{
		const int TypeSize = 16;
		var result = new List<int>(TypeSize);
		for (int n = 0; n < TypeSize; n++)
		{
			if ((value & (1U << n)) != 0)
			{
				result.Add(n);
			}
		}
		return result;
	}

	public static IEnumerable<int> Calculate(UInt32 value)
	{
		const int TypeSize = 32;
		var result = new List<int>(TypeSize);
		for (int n = 0; n < TypeSize; n++)
		{
			if ((value & (1U << n)) != 0)
			{
				result.Add(n);
			}
		}
		return result;
	}

	public static IEnumerable<int> Calculate(UInt64 value)
	{
		const int TypeSize = 64;
		var result = new List<int>(TypeSize);
		for (int n = 0; n < TypeSize; n++)
		{
			if ((value & (1UL << n)) != 0)
			{
				result.Add(n);
			}
		}
		return result;
	}

	public static IEnumerable<int> Calculate(sbyte value) => Calculate(unchecked((byte)value));

	public static IEnumerable<int> Calculate(Int16 value) => Calculate(unchecked((UInt16)value));

	public static IEnumerable<int> Calculate(Int32 value) => Calculate(unchecked((UInt32)value));

	public static IEnumerable<int> Calculate(Int64 value) => Calculate(unchecked((UInt64)value));
}