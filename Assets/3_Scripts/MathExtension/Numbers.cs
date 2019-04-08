using System;

namespace MathExt
{
	public static class Numbers
	{
		#region Methods

		public static int NextPowerOfTwo(int x)
		{
			int i;
			for(i = 1; i < 31; ++i)
			{
				if (x >> i == 0) return Pow2(i);
			}
			return 0x7FFFFFFF;
		}

		public static int Pow2(int power)
		{
			return 0x00000001 << power;
		}

		#endregion
	}
}
