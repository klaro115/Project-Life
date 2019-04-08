using System;
using UnityEngine;

namespace MathExt
{
	public static class ArrayMath
	{
		// NOTE: These are simply helper methods for repetitive and common array operations.
		// If you need high performance processing across very large arrays, it is recommended
		// to fall back to OpenCL or some comparable hardware accelerated computation technique.

		#region Methods

		private static bool VerifyArrays(float[] values)
		{
			return values != null && values.Length != 0;
		}
		private static bool VerifyArrays(float[] valuesA, float[] valuesB)
		{
			return valuesA != null && valuesB != null && valuesA.Length == valuesB.Length;
		}
		private static bool VerifyArrays(float[] valuesA, float[] valuesB, float[] result)
		{
			return valuesA != null && valuesB != null && result != null && valuesA.Length == valuesB.Length && valuesA.Length == result.Length;
		}

		public static float Max(float[] values)
		{
			if (!VerifyArrays(values)) return 0.0f;

			float max = 0.0f;
			for (int i = 0; i < values.Length; ++i)
				max = Math.Max(values[i], max);
			return max;
		}
		public static bool Max(float[] valuesA, float[] valuesB, float[] results)
		{
			if (!VerifyArrays(valuesA, valuesB, results)) return false;

			for (int i = 0; i < valuesA.Length; ++i)
				results[i] = Math.Max(valuesA[i], valuesB[i]);
			return true;
		}

		public static float Min(float[] values)
		{
			if (!VerifyArrays(values)) return 0.0f;

			float min = 0.0f;
			for (int i = 0; i < values.Length; ++i)
				min = Math.Max(values[i], min);
			return min;
		}
		public static bool Min(float[] valuesA, float[] valuesB, float[] results)
		{
			if (!VerifyArrays(valuesA, valuesB, results)) return false;

			for (int i = 0; i < valuesA.Length; ++i)
				results[i] = Math.Min(valuesA[i], valuesB[i]);
			return true;
		}

		public static bool MinMax(float[] values, out float min, out float max)
		{
			min = 0.0f;
			max = 0.0f;
			if (!VerifyArrays(values)) return false;

			for(int i = 0; i < values.Length; ++i)
			{
				float v = values[i];
				min = Math.Min(v, min);
				max = Math.Max(v, max);
			}
			return true;
		}

		public static float Sum(float[] values)
		{
			if (!VerifyArrays(values)) return 0.0f;

			float sum = 0.0f;
			for (int i = 0; i < values.Length; ++i)
				sum += values[i];
			return sum;
		}

		public static float Average(float[] values)
		{
			if (!VerifyArrays(values)) return 0.0f;

			return Sum(values) / values.Length;
		}

		public static bool Add(float[] valuesA, float[] valuesB, float[] results)
		{
			if (!VerifyArrays(valuesA, valuesB, results)) return false;

			for (int i = 0; i < valuesA.Length; ++i)
				results[i] = valuesA[i] + valuesB[i];
			return true;
		}
		public static bool AddTo(float[] valuesA, float[] valuesB)
		{
			if (!VerifyArrays(valuesA, valuesB)) return false;

			for (int i = 0; i < valuesA.Length; ++i)
				valuesA[i] += valuesB[i];
			return true;
		}
		public static bool AddTo(float[] valuesA, float x)
		{
			if (!VerifyArrays(valuesA)) return false;

			for (int i = 0; i < valuesA.Length; ++i)
				valuesA[i] += x;
			return true;
		}

		public static bool Multiply(float[] valuesA, float[] valuesB, float[] results)
		{
			if (!VerifyArrays(valuesA, valuesB, results)) return false;

			for (int i = 0; i < valuesA.Length; ++i)
				results[i] = valuesA[i] * valuesB[i];
			return true;
		}
		public static bool MultiplyTo(float[] valuesA, float[] valuesB)
		{
			if (!VerifyArrays(valuesA, valuesB)) return false;

			for (int i = 0; i < valuesA.Length; ++i)
				valuesA[i] *= valuesB[i];
			return true;
		}
		public static bool MultiplyTo(float[] valuesA, float x)
		{
			if (!VerifyArrays(valuesA)) return false;

			for (int i = 0; i < valuesA.Length; ++i)
				valuesA[i] *= x;
			return true;
		}

		public static bool Pow(float[] values, float[] results, float power)
		{
			if (!VerifyArrays(values, results)) return false;
			
			for (int i = 0; i < values.Length; ++i)
				results[i] = Mathf.Pow(values[i], power);
			return true;
		}
		public static float[] Pow(float[] values, float power)
		{
			if (!VerifyArrays(values)) return null;

			float[] results = new float[values.Length];
			Pow(values, results, power);

			return results;
		}

		public static bool Lerp(float[] valuesA, float[] valuesB, float[] results, float k)
		{
			if (!VerifyArrays(valuesA, valuesB, results)) return false;

			float invK = 1.0f - k;
			for (int i = 0; i < valuesA.Length; ++i)
			{
				results[i] = invK * valuesA[i] + k * valuesB[i];
			}
			return true;
		}

		public static bool Normalize(float[] values)
		{
			if (!VerifyArrays(values)) return false;

			MinMax(values, out float min, out float max);
			float magnitude = Math.Max(Math.Abs(min), Math.Abs(max));
			return MultiplyTo(values, 1.0f / magnitude);
		}

		public static float Correlation(float[] valuesA, float[] valuesB)
		{
			if (valuesA == null || valuesB == null) return 0;
			if (valuesA == valuesB) return Sum(Pow(valuesA, 2.0f));

			int valueCount = Math.Min(valuesA.Length, valuesB.Length);
			float corr = 0.0f;

			for (int i = 0; i < valueCount; ++i)
				corr += valuesA[i] * valuesB[i];
			return corr / valueCount;
		}

		#endregion
	}
}
