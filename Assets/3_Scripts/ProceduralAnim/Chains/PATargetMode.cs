using System;

namespace ProceduralAnim
{
	public enum PATargetMode
	{
		Coordinate,	// Use position-rotation coordinate set, no automation, must be updated continuously.
		Transform,	// Use a regular transform type object, for simple automated targeting.
		Target		// Use the dedicated target interface, for very precise automated targeting.
	}
}
