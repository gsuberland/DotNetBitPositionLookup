// uncomment this if you want to verify that the set bit counts match up across lookup/calculate.
//#define VERIFY_COUNTS
// uncomment this if you want to verify that the first bit indices match up across lookup/calculate.
//#define VERIFY_FIRST
// ^ setting either of these can skew the benchmarks a bit since you're adding a .Count() or .FirstOrDefault() call into each check.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Program
{
	// specify seed for reproducibility
	static Random rng = new Random(0x5E7B175);

	static byte[] bytePool = null;
	static sbyte[] sbytePool = null;
	static ushort[] ushortPool = null;
	static short[] shortPool = null;
	static uint[] uintPool = null;
	static int[] intPool = null;
	static ulong[] ulongPool = null;
	static long[] longPool = null;

	static void Main()
	{
		Console.WriteLine("Forcing table generation before benchmark...");
		var sw = new Stopwatch();

		sw.Start();
		var result8 = SetBitPositionLookup.Lookup((byte)53);
		sw.Stop();
		Console.WriteLine("(byte)53  = [ " + string.Join(", ", result8) + " ]");
		Console.WriteLine("8-bit LUT generation took {0:0.0000}ms", sw.Elapsed.TotalMilliseconds);
		sw.Reset();

		sw.Start();
		var result16 = SetBitPositionLookup.Lookup((UInt16)53);
		sw.Stop();
		Console.WriteLine("(ushort)53 = [ " + string.Join(", ", result16) + " ]");
		Console.WriteLine("16-bit LUT generation took {0:0.0000}ms", sw.Elapsed.TotalMilliseconds);
		sw.Reset();

		Console.WriteLine();
		Console.WriteLine("Beginning benchmarks.");

		int[] testSizes = { 1024, 8 * 1024, 64 * 1024, 256 * 1024, 512 * 1024, 1024 * 1024, 4 * 1024 * 1024 };

		foreach (int testSize in testSizes)
		{
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine("Testing {0}K elements:", testSize / 1024);

			InitPools(testSize);
			FillPools(testSize);

			// there are 16 tests (8 types, lookup and calc each)
			int[] results = new int[16];
			int testNum = 0;

			Console.WriteLine("+--------+------------+------------+");
			Console.WriteLine("| Type   | Lookup     | Calculate  |");
			Console.WriteLine("+--------+------------+------------+");

			// benchmark byte
			(results[testNum++], results[testNum++]) = Benchmark(
				"byte",
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Lookup(bytePool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				},
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Calculate(bytePool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				});

			// benchmark sbyte
			(results[testNum++], results[testNum++]) = Benchmark(
				"sbyte",
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Lookup(sbytePool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				},
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Calculate(sbytePool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				});

			// benchmark uint16
			(results[testNum++], results[testNum++]) = Benchmark(
				"ushort",
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Lookup(ushortPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				},
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Calculate(ushortPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				});

			// benchmark int16
			(results[testNum++], results[testNum++]) = Benchmark(
				"short",
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Lookup(shortPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				},
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Calculate(shortPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				});

			// benchmark int32
			(results[testNum++], results[testNum++]) = Benchmark(
				"int",
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Lookup(intPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				},
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Calculate(intPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				});

			// benchmark uint32
			(results[testNum++], results[testNum++]) = Benchmark(
				"uint",
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Lookup(uintPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				},
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Calculate(uintPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				});

			// benchmark int64
			(results[testNum++], results[testNum++]) = Benchmark(
				"long",
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Lookup(longPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				},
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Calculate(longPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				});

			// benchmark uint64
			(results[testNum++], results[testNum++]) = Benchmark(
				"ulong",
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Lookup(ulongPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				},
				() =>
				{
					int r = 0;
					for (int n = 0; n < testSize; n++)
					{
						var luts = SetBitPositionLookup.Calculate(ulongPool[n]);
#if VERIFY_COUNTS
					r += luts.Count();
#elif VERIFY_FIRST
					r += luts.FirstOrDefault();
#endif
					}
					return r;
				});

			Console.WriteLine("+--------+------------+------------+");
			Console.WriteLine();

#if VERIFY_COUNTS || VERIFY_FIRST
		for (int i = 0; i < 8; i++)
		{
			int lookupResult = results[i * 2];
			int calcResult = results[i * 2 + 1];
			Console.WriteLine("Test {0}: {1}", i + 1, (lookupResult == calcResult) ? "PASS" : string.Format("FAIL ({0} != {1})", lookupResult, calcResult));
		}
#endif
		}
	}

	static (int, int) Benchmark(string typeName, Func<int> lookupFunc, Func<int> calcFunc)
	{
		var sw = new Stopwatch();
		int lookupResult = 0;
		int calcResult = 0;
		Console.Write("| " + typeName.PadRight(6) + " | ");
		sw.Start();
		lookupResult = lookupFunc();
		sw.Stop();
		Console.Write(string.Format("{0:0.000}", sw.Elapsed.TotalMilliseconds).PadRight(10) + " | ");
		sw.Reset();
		sw.Start();
		calcResult = calcFunc();
		sw.Stop();
		Console.WriteLine(string.Format("{0:0.000}", sw.Elapsed.TotalMilliseconds).PadRight(10) + " |");
		sw.Reset();
		return (lookupResult, calcResult);
	}

	static void InitPools(int testSize)
	{
		bytePool = new byte[testSize];
		sbytePool = new sbyte[testSize];
		ushortPool = new ushort[testSize];
		shortPool = new short[testSize];
		uintPool = new uint[testSize];
		intPool = new int[testSize];
		ulongPool = new ulong[testSize];
		longPool = new long[testSize];
	}

	static void FillPools(int testSize)
	{
		Console.Write("Filling pools.");

		// fill byte
		rng.NextBytes(bytePool);
		Console.Write(".");
		// fill sbyte
		for (int i = 0; i < testSize; i++)
		{
			sbytePool[i] = checked((sbyte)rng.Next(sbyte.MinValue, (int)sbyte.MaxValue + 1));
		}
		Console.Write(".");
		// fill ushort
		for (int i = 0; i < testSize; i++)
		{
			ushortPool[i] = checked((ushort)rng.Next(ushort.MinValue, (int)ushort.MaxValue + 1));
		}
		Console.Write(".");
		// fill short
		for (int i = 0; i < testSize; i++)
		{
			shortPool[i] = checked((short)rng.Next(short.MinValue, (int)short.MaxValue + 1));
		}
		Console.Write(".");
		// fill uint
		byte[] intBuf = new byte[4];
		for (int i = 0; i < testSize; i++)
		{
			rng.NextBytes(intBuf);
			uintPool[i] = BitConverter.ToUInt32(intBuf, 0);
		}
		Console.Write(".");
		// fill int
		for (int i = 0; i < testSize; i++)
		{
			rng.NextBytes(intBuf);
			intPool[i] = BitConverter.ToInt32(intBuf, 0);
		}
		Console.Write(".");
		// fill ulong
		byte[] longBuf = new byte[8];
		for (int i = 0; i < testSize; i++)
		{
			rng.NextBytes(longBuf);
			ulongPool[i] = BitConverter.ToUInt64(longBuf, 0);
		}
		Console.Write(".");
		// fill long
		for (int i = 0; i < testSize; i++)
		{
			rng.NextBytes(intBuf);
			longPool[i] = BitConverter.ToInt64(longBuf, 0);
		}
		Console.Write(".");
		Console.WriteLine();
	}
}