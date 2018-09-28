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

			int i = regionLowIndex;
			HGRegion curRegion = regions[0];
			HGRegion nextRegion = new HGRegion();
			for(; i < regions.Length + 1; ++i)
			{
				if(i < regions.Length)
				{
					nextRegion = regions[i];
					if (nextRegion.x > x) break;
					curRegion = nextRegion;
				}
				else
				{
					nextRegion = regions[0];
					nextRegion.x += 1.0f;
				}
			}
			float k = (x - curRegion.x) / (nextRegion.x - curRegion.x);

			outLerpFactor = k;
			outRegionLow = curRegion;
			outRegionHigh = nextRegion;
			regionLowIndex = i >= regions.Length ? 0 : i - 1;

			return true;
		}

		public HGRegion lerpRegions(float x, ref int startRegionIndex)
		{
			if (regions == null || regions.Length == 0) return new HGRegion(0, 0, new HGRange(0));
			if (regions.Length < 2) return regions[0];

			float k = 0.0f;
			HGRegion regionLow = regions[0];
			HGRegion regionHigh = regions[1];

			getRegionsAtCood(x, ref k, ref regionLow, ref regionHigh, ref startRegionIndex);

			return HGRegion.lerp(regionLow, regionHigh, k);
		}

		#endregion
	}
}
