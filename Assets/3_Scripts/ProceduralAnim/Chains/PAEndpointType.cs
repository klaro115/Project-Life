using System;

namespace ProceduralAnim
{
	public enum PAEndpointType
	{
		Contact,	// The endpoint will directly interact with the environment, utmost priority for position and rotation.
		Pointer,	// The endpoint is mostly directional, position is secondary at best.
		Root		// Some central point of the character, position adjusted to match center of mass.
	}
}
