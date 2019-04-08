﻿using System;
using UnityEngine;

namespace ProceduralAnim.IK
{
	class PASolverReachResult
	{
		#region Fields

		public bool outOfReach;
		public int maxIndex;
		public Vector3 offset;
		public Quaternion rotation;

		#endregion
		#region Properties

		public static PASolverReachResult None => new PASolverReachResult() { outOfReach = true, maxIndex = -1 };

		#endregion
	}
}