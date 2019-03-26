using System;

namespace ProceduralAnim.IK
{
	public enum PASolverState
	{
		Solved,			// IK chain to target could be successfully resolved.
		Reaching,		// Target is out of reach, trying to get as close to target as possible.
		OutOfReach,		// Target is out of reach, failed to resolve IK chain.

		Failed			// Unable to solve IK.
	}
}
