using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralAnim
{
	public abstract class PAIKGroup : MonoBehaviour
	{
		#region Fields

		protected Vector3 targetPos = Vector3.zero;
		protected Vector3 targetDir = Vector3.forward;

		#endregion
		#region Methods

		public abstract void initialize();

		public abstract void update(float deltaTime);

		public virtual void setTargetPos(Vector3 newTargetPos)
		{
			targetPos = newTargetPos;
		}
		public virtual void setTargetDir(Vector3 newTargetDir)
		{
			targetDir = newTargetDir;
		}
		public virtual void setHandlePos(Vector3 newHandlePos, int handleIndex = 0)
		{
			//...
		}

		#endregion
	}
}
