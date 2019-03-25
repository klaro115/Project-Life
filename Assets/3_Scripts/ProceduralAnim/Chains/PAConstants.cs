using System;
namespace ProceduralAnim
{
	public static class PAConstants
	{
		#region Fields Constants

		public const float endpointAlternatingWeight = 0.1f;	// Endpoint distance weighting for alternating mode.
		public const float endpointPreferentialWeight = 0.2f;   // Endpoint distance weighting when prefering chain end.

		public const float jointDefaultUrgency = 0.3f;

		#endregion
	}
}
