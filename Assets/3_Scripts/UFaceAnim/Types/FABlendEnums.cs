using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFaceAnim
{
	public enum FABlendCurve
	{
		Linear,
		SmoothStep,
		Square,
	}

	public enum FABlendTarget : int
	{
		None,

		MouthOpenClose,
		MouthShowTeeth,
		MouthInOut,
		MouthCornerL,
		MouthCornerR,

		BrowsInOut,
		BrowsL,
		BrowsR,
		BrowsSharpFlat,

		EyesCloseL,
		EyesCloseR,
		EyesWander,
	}
}
