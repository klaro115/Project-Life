using UnityEngine;

namespace UFaceAnim
{
	public static class FABlendStateTools
	{
		#region Methods
		
		public static float getBlendFactor(float k, FABlendCurve blendCurve)
		{
			switch (blendCurve)
			{
			case FABlendCurve.SmoothStep:
				{
					float t = Mathf.Abs(k);
					float f = 3 * t * t - 2 * t * t * t;
					return f * Mathf.Sign(k);
				}
			case FABlendCurve.Square:
				return k * k * Mathf.Sign(k);
			case FABlendCurve.OneMinusSquare:
				{
					float t = 1.0f - k;
					return (1.0f - t * t) * Mathf.Sign(k);
				}
			default:
				// Default to linear behaviour:
				break;
			}
			return k;
		}
		
		#endregion
	}
}
