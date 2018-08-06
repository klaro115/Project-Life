
namespace UFaceAnim
{
	public enum FABlendCurve
	{
		Linear,				// Linear scaling between states, for a continuous blending gradient.		(y=x)
		SmoothStep,			// Steeper slope between 2 plateaus, slightly harder gradient.				(y=3x^2-2x^3)
		Square,				// Values stay low but then skyrocket towards the maximum value range.		(y=x^2)
		OneMinusSquare,		// Values raise quickly, then reach a plateau. Hard slope, strong extremes.	(y=1-(1-x)^2)
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

	/// <summary>
	/// Designates a set of blend states that will be overwritten or ignored.
	/// Ex.1: Temporarily overwrite a character's expression, but keep the speech
	/// based blending going. Ex.2: It's a telepathic scene, so overwrite the
	/// character's speech blending.
	/// </summary>
	public enum FABlendOverwrite
	{
		Emotion,	// Overwrite/Ignore only emotion based blend states. [default]
		Speech,		// Overwrite/Ignore only speech based blend states.

		All,		// Overwrite all blendstates processed by the controller.
		None		// Don't overwrite any blend states, aka do nothing.
	}
}
