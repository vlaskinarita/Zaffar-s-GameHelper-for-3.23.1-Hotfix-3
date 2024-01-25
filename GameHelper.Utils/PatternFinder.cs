using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameOffsets;

namespace GameHelper.Utils;

internal static class PatternFinder
{
	private const int MaxBytesObject = 84000;

	private static Pattern[] Patterns => StaticOffsetsPatterns.Patterns;

	internal static Dictionary<string, int> Find(SafeMemoryHandle handle, nint baseAddress, int processSize)
	{
		BiggestPatternLength();
		int totalReadOperations = CalculateTotalReadOperations(processSize);
		Dictionary<string, int> result = new Dictionary<string, int>();
		int totalPatterns = Patterns.Length;
		bool[] isPatternFound = new bool[totalPatterns];
		int[] patternOffsets = new int[totalPatterns];
		int totalPatternsFound = 0;
		ParallelOptions pOptions = new ParallelOptions
		{
			MaxDegreeOfParallelism = 4
		};
		Parallel.For(0, totalReadOperations, pOptions, delegate(int i, ParallelLoopState state1)
		{
			ParallelLoopState state3 = state1;
			int currentOffset = i * 84000;
			int nsize = ((i == totalReadOperations - 1) ? (processSize - currentOffset) : 84000);
			if (!state3.ShouldExitCurrentIteration)
			{
				byte[] processData = handle.ReadMemoryArray<byte>(baseAddress + currentOffset, nsize);
				int processDataLength = processData.Length;
				Parallel.For(0, processDataLength, pOptions, delegate(int j, ParallelLoopState state2)
				{
					if (!state2.ShouldExitCurrentIteration)
					{
						for (int m = 0; m < totalPatterns; m++)
						{
							if (!isPatternFound[m])
							{
								Pattern pattern = Patterns[m];
								int num = pattern.Data.Length;
								bool flag = num % 2 != 0;
								int num2 = num / 2 + 1;
								bool flag2 = false;
								if (processDataLength - j >= num && (!flag || pattern.Mask[num2] || processData[j + num2] != pattern.Data[num2]))
								{
									for (int n = 0; n < num / 2; n++)
									{
										if (flag2)
										{
											break;
										}
										if (pattern.Mask[n] && processData[j + n] != pattern.Data[n])
										{
											flag2 = true;
										}
										int num3 = num - (n + 1);
										if (pattern.Mask[num3] && processData[j + num3] != pattern.Data[num3])
										{
											flag2 = true;
										}
									}
									if (!flag2)
									{
										Interlocked.Increment(ref totalPatternsFound);
										isPatternFound[m] = true;
										patternOffsets[m] = currentOffset + j;
									}
								}
							}
						}
						if (totalPatternsFound >= totalPatterns)
						{
							state2.Break();
							state3.Break();
							if (!isPatternFound.All((bool k) => k))
							{
								throw new Exception("There is a non-unique pattern. Kindly fix the patterns.");
							}
						}
					}
				});
			}
		});
		if (totalPatternsFound < totalPatterns)
		{
			throw new Exception("Couldn't find some patterns. kindly fix the patterns.");
		}
		for (int l = 0; l < totalPatterns; l++)
		{
			result.Add(Patterns[l].Name, patternOffsets[l] + Patterns[l].BytesToSkip);
		}
		return result;
	}

	private static int BiggestPatternLength()
	{
		int maxLength = 0;
		Pattern[] patterns = Patterns;
		for (int i = 0; i < patterns.Length; i++)
		{
			int currentPatternLength = patterns[i].Data.Length;
			if (currentPatternLength > maxLength)
			{
				maxLength = currentPatternLength;
			}
		}
		return maxLength;
	}

	private static int CalculateTotalReadOperations(int processSize)
	{
		int ret = processSize / 84000;
		if (processSize % 84000 != 0)
		{
			return ret + 1;
		}
		return ret;
	}
}
