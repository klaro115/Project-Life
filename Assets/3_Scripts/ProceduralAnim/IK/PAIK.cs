using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralAnim
{
	public abstract class PAIKGroup : MonoBehaviour
	{
		#region Fields

		//...

		#endregion
		#region Methods

		public abstract void initialize();

		public abstract void update(float deltaTime);
		
		#endregion
	}
}
