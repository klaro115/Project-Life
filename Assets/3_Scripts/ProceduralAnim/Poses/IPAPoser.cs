using System;

namespace ProceduralAnim.Poses
{
	public interface IPAPoser
	{
		#region Methods

		bool SetPose(PAPose newPose);
		PAPose GetCurrentPose();

		#endregion
	}
}
