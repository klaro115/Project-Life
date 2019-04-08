using System.Collections;
using UnityEngine;

// TODO: Make hand pose into a struct? Scriptable object is too large/permament for this purpose; should be a more short-lived type.

namespace ProceduralAnim.Poses
{
	public class PAHand : MonoBehaviour, IPAPoser
	{
		#region Fields

		private bool initialized = false;

		[Header("Rotation Axis")]
		public PAHandAxisSet thumbAxis = PAHandAxisSet.Default;
		public PAHandAxisSet fingerAxis = PAHandAxisSet.Default;

		[Header("Finger Definitions")]
		[SerializeField]
		private PAFinger fingerThumb = new PAFinger();
		[SerializeField]
		private PAFinger fingerIndex = new PAFinger();
		[SerializeField]
		private PAFinger fingerMiddle = new PAFinger();
		[SerializeField]
		private PAFinger fingerRing = new PAFinger();
		[SerializeField]
		private PAFinger fingerPinky = new PAFinger();

		[SerializeField] //temp
		private PAHandPose currentPose = PAHandPose.Default;

		#endregion
		#region Methods

#if UNITY_EDITOR
		private void DrawFingerGizmos(ref PAFinger finger)
		{
			Gizmos.color = Color.cyan;
			Vector3 fcSize = new Vector3(0.002f, 0.002f, 0.002f);
			if (finger.phalanx0 != null)
			{
				Vector3 pos0 = finger.phalanx0.position;
				Gizmos.DrawWireCube(pos0, fcSize);
				if (finger.phalanx1 != null)
				{
					Vector3 pos1 = finger.phalanx1.position;
					Gizmos.DrawLine(pos0, pos1);
					Gizmos.DrawWireCube(pos1, fcSize);
					if (finger.phalanx2 != null)
					{
						Vector3 pos2 = finger.phalanx2.position;
						Gizmos.DrawLine(pos1, pos2);
						Gizmos.DrawWireCube(pos2, fcSize);
					}
				}
			}
		}
		void OnDrawGizmosSelected()
		{
			DrawFingerGizmos(ref fingerThumb);
			DrawFingerGizmos(ref fingerIndex);
			DrawFingerGizmos(ref fingerMiddle);
			DrawFingerGizmos(ref fingerRing);
			DrawFingerGizmos(ref fingerPinky);
		}
#endif

		public void Initialize()
		{
			if (initialized) return;

			// Initialize all axis: (basically just nromalizes the vectors)
			thumbAxis.Initialize();
			fingerAxis.Initialize();

			// Initialize finger poses:
			fingerThumb.Initialize();
			fingerIndex.Initialize();
			fingerMiddle.Initialize();
			fingerRing.Initialize();
			fingerPinky.Initialize();

			// Initialize current/default pose instance:
			currentPose = PAHandPose.Default;
			
			initialized = true;
		}

		//TEST
		void Update()
		{
			if (!initialized) Initialize();
			SetHandPose(ref currentPose);
		}

		public PAPose GetCurrentPose()
		{
			throw new System.NotImplementedException();
		}

		public PAHandPose GetCurrentHandPose()
		{
			return currentPose;
		}

		public bool SetPose(PAPose newPose)
		{
			if (newPose == null) return false;

			// TODO

			return true;
		}

		public bool SetHandPose(ref PAHandPose newPose)
		{
			// Store the new active pose:
			currentPose = newPose;

			// Do some preprocessing for finger poses:
			float spread = newPose.fingerSpread;
			float halfSpread = newPose.fingerSpread * 0.5f;

			// Set pose of each finger individually:
			SetFingerPose(ref fingerThumb, ref thumbAxis, newPose.thumbOpenClose, newPose.thumbSpread, newPose.thumbRoll);
			SetFingerPose(ref fingerIndex, ref fingerAxis, newPose.indexOpenClose, spread);
			SetFingerPose(ref fingerMiddle, ref fingerAxis, newPose.middleOpenClose, halfSpread);
			SetFingerPose(ref fingerRing, ref fingerAxis, newPose.ringOpenClose, -halfSpread);
			SetFingerPose(ref fingerPinky, ref fingerAxis, newPose.pinkyOpenClose, -spread);

			return true;
		}

		private void SetFingerPose(ref PAFinger finger, ref PAHandAxisSet axis, float openClose, float spread = 0.0f, float roll = 0.0f)
		{
			if (finger.phalanx0 == null) return;

			// Calculate local rotation offsets relative to phalanx default rotation:
			float phalangeAngle = openClose * axis.openCloseMaxAngle * 0.3334f;
			Quaternion rollRot = Quaternion.AngleAxis(roll * axis.rollMaxAngle, axis.rollAxis);
			Quaternion spreadRot = Quaternion.AngleAxis(spread * axis.spreadMaxAngle, axis.spreadAxis);
			Quaternion openCloseRot = Quaternion.AngleAxis(phalangeAngle, axis.openCloseAxis);

			// Apply rotations to phalanges of the finger:
			finger.phalanx0.localRotation = finger.defaultRotation0 * rollRot * spreadRot * openCloseRot;
			finger.phalanx1.localRotation = finger.defaultRotation1 * openCloseRot;
			finger.phalanx2.localRotation = finger.defaultRotation2 * openCloseRot;
		}

		#endregion
	}
}
