
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

		BrowsL,
		BrowsR,
		BrowsInOut,
		BrowsSharpFlat,

		EyesCloseL,
		EyesCloseR,
		EyesWander,
		EyesDirX,
		EyesDirY,
	}
}
