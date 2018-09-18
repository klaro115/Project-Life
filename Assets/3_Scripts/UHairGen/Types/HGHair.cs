using System;
using UnityEngine;

namespace UHairGen
{
	/// <summary>
	/// Descriptor and serialization class for hair assets. This is read from file and then passed to the generator.
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
			new HGRegion(0.25f, 0.05f, new HGRange(0.2f, 0.075f))
		};

		public float lengthRandom = 0.0f;
		public float minLengthThreshold = 0.005f;
		public float posXRandom = 0.0f;
		public float posYRandom = 0.02f;
		public float segmentLength = 0.02f;
		public float segmentWidth = 0.05f;
		public float ringSpacing = 0.025f;

		#endregion
	}
}
