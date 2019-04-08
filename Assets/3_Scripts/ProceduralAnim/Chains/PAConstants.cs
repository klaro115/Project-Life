using System;
namespace ProceduralAnim
{
	public static class PAConstants
	{
		#region Fields Constants

		public const float endpointAlternatingWeight = 0.1f;	// Endpoint distance weighting for alternating mode.
		public const float endpointPreferentialWeight = 0.2f;   // Endpoint distance weighting when prefering chain end.

		public const float jointDefaultUrgency = 0.3f;

		public const int maxChainOrder = 10;                    // Maximum IK chain recursion depth and number of interconnected chains a target may be forwarded to.
		public const int maxHierarchySearchDepth = 30;          // Maximum depth of a character's hierarchy to explore. (used to prevent infinite loops)

		public const int maxSolverJointListLength = 32;			// Maximum number of ball joints in hierarchical order to remember while solving IK. (used for static buffer size)

		#endregion
	}
}
