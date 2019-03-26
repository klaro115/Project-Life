using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralAnim
{
	public class PAAnimator : MonoBehaviour
	{
		#region Fields

		public Transform root = null;
		public PAChain[] chains = null;

		private Dictionary<PABodyMapping, PAChain> mappings = null;
		private PAJoint[] joints = null;

		private float stance = 1.0f;

		#endregion
		#region Properties

		public float Stance { get => stance; set => SetStance(value); }

		#endregion
		#region Methods

		private void OnDrawGizmosSelected()
		{
			if(chains != null)
			{
				foreach (PAChain chain in chains)
					chain.DrawGizmos();
			}
		}

		[ContextMenu("Generate chain")]
		public void GenerateChain()
		{
			if (root == null) root = transform;
			if (chains == null) return;

			if(mappings == null) mappings = new Dictionary<PABodyMapping, PAChain>();
			mappings.Clear();

			foreach (PAChain chain in chains)
			{
				if (!chain.GenerateChain())
				{
					Debug.LogError($"[PAAnimator] Error! Failed to create chain '{chain}'");
					continue;
				}

				PABodyMapping mapping = chain.end.bodyMapping;
				if (mapping != PABodyMapping.None)
					mappings.Add(mapping, chain);
			}

			joints = GetComponentsInChildren<PAJoint>(true);
		}

		public bool GetMapping(PABodyMapping mapping, out PAChain outResult)
		{
			if (mappings == null || mapping == PABodyMapping.None)
			{
				outResult = null;
				return false;
			}
			return mappings.TryGetValue(mapping, out outResult);
		}

		public void SetTarget(PABodyMapping mapping, Vector3 targetPoint, Quaternion targetRotation)
		{
			if(GetMapping(mapping, out PAChain chain))
				chain.SetTarget(targetPoint, targetRotation);
		}
		public void SetTarget(PABodyMapping mapping, Transform target)
		{
			if (GetMapping(mapping, out PAChain chain))
				chain.SetTarget(target);
		}
		public void SetTarget(PABodyMapping mapping, IPATarget target)
		{
			if (GetMapping(mapping, out PAChain chain))
				chain.SetTarget(target);
		}

		private void Update()
		{
			// TODO

			// Update actual joint movement:
			if(joints != null)
			{
				foreach (PAJoint joint in joints)
					joint.UpdateJoint();
			}
		}

		//TEMP
		private void SetStance(float newStance)
		{
			stance = Mathf.Clamp01(newStance);

			// TODO 1: Determine center of mass position (in world space).
			// TODO 2: ...

			//TEMP
			Vector3 centerOfMass = transform.position + Vector3.up * (stance - 1);
			Vector3 headTargetPos = centerOfMass + Vector3.up * stance;
			if (GetMapping(PABodyMapping.Head, out PAChain headChain) && headChain != null)
				headChain.SetTarget(headTargetPos, Quaternion.identity);

			foreach(PAChain chain in chains)
			{
				// TODO
			}
		}

		#endregion
	}
}
