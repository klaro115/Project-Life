using System;

namespace ProceduralAnim
{
	/// <summary>
	/// The method to use for chosing which endpoint of a chain a target is best assigned to.
	/// </summary>
	public enum PAChainTargeting
	{
		End,				// Always assign targeting to chain's end endpoint. Use if chain has only a functional end.
		Start,				// Always assign targeting to chain's start endpoint. Use if chain has only a functional start.
		Closest,			// Assign targeting to closest endpoint on chain. Use this for hands.
		Alternating,		// Strictly alternate between chain's start and end points.
		AlternatingClosest,	// Try alternating between start and end, but factor in distance as well. Use this for feet.
		Preferential,		// Try prefering one of the chain's endpoints over the other, based on distance.
	}
}
