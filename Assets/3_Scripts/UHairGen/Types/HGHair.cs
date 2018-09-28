using System;
using UnityEngine;

namespace UHairGen
{
	/// <summary>
	/// Descriptor and serialization class for hairstyle assets. This may also be read from file, to be passed to the generator.
	/// </summary>
	[System.Serializable]
	[CreateAssetMenu(fileName = "new Hairstyle", menuName = "Hair/Style")]
	public class HGHair : ScriptableObject
	{
		#region Fields

		public HGRegion[] regions = new HGRegion[4]
		{
			new HGRegion(0, 0.2f, new HGRange(0.1f, 0.05f)),
			new HGRegion(0.25f, 0.05f, new HGRange(0.2f, 0.075f)),
			new HGRegion(0.5f, 0, new HGRange(0.25f, 0.075f)),
			new HGRegion(0.75f, 0.05f, new HGRange(0.2f, 0.075f))
		};

		public float lengthRandom = 0.0f;			// Overal randomized length multiplier across the entire hairstyle. [%]
		public float minLengthThreshold = 0.005f;
		public float posXRandom = 0.0f;
		public float posYRandom = 0.02f;
		public float segmentLength = 0.02f;
		public float segmentWidth = 0.05f;
		public float ringSpacing = 0.025f;

		#endregion
		#region Methods

		public bool getRegionsAtCood(float x, ref float outLerpFactor, ref HGRegion outRegionLow, ref HGRegion outRegionHigh, ref int regionLowIndex)
		{
			if (regions == null || regions.Length == 0 || x < 0 || x > 1) return false;

			int j = regionLowIndex;
			int curIndex = regions[0].x > 0 ? regions.Length - 1 : 0;
			HGRegion curRegion = regions[curIndex];
			int nextIndex = regions.Length > 1 ? 1 : 0;
			HGRegion nextRegion = regions[nextIndex];

			if (x > nextRegion.x)
			{
				for (; j < regions.Length + 1; ++j)
				{
					int jIndex = j < regions.Length ? j : 0;
					curRegion = nextRegion;
					nextRegion = regions[jIndex];
					if (x <= nextRegion.x)
					{
						if (nextRegion.x < curRegion.x) nextRegion.x = 1.0f - nextRegion.x;
						break;
					}
				}
			}
			float k = Mathf.Abs(x - curRegion.x) / Mathf.Abs(nextRegion.x - curRegion.x); ;

			outLerpFactor = k;
			outRegionLow = curRegion;
			outRegionHigh = nextRegion;
			regionLowIndex = j;

			return true;
		}

		public HGRegion lerpRegions(float x, ref int startRegionIndex)
		{
			if (regions == null || regions.Length == 0) return new HGRegion(0, 0, new HGRange(0));
			if (regions.Length < 2) return regions[0];

			float k = 0.0f;
			HGRegion regionLow = regions[0];
			HGRegion regionHigh = regions[1];

			return getRegionsAtCood(x, ref k, ref regionLow, ref regionHigh, ref startRegionIndex) ? HGRegion.lerp(regionLow, regionHigh, k) : regionLow;
		}

		#endregion
	}
}
