using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralAnim
{
	public interface IPATarget
	{
		#region Methods

		bool RequestSetTargetUser(PAEndpoint userEndpoint);
		bool RequestUnsetTargetUser(PAEndpoint currentUserEndpoint);

		bool GetTargetContact(PAEndpoint endpoint, ref Vector3 outPosition, ref Quaternion outRotation);

		#endregion
		#region Properties

		PAEndpoint TargetUser { get; }
		Transform TargetTransform { get; }

		#endregion
	}
}
