using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Makes a ragdoll aim at a target.
	public class RagdollAiming : MonoBehaviour {

		public PuppetMaster puppetMaster;
		public AimIK aimIKBeforePhysics;
		public Transform target;

		[Header("Cosmetics")]
		public bool fixAiming = true;
		public bool fixLeftHand = true;
		public AimIK aimIKAfterPhysics;
		public LimbIK leftHandIK;
		public Transform leftHandTarget;

		void Start() {
			// Register to get some calls from PuppetMaster
			puppetMaster.OnWrite += OnPuppetMasterWrite;

			// Make the other AimIK and left hand IK update after PuppetMaster writes for cosmetic non-physical fixes
			aimIKAfterPhysics.enabled = false;
			leftHandIK.enabled = false;
		}

		// Solve the first pass of IK before PuppetMaster reads, to edit the pose it will follow
		void LateUpdate() {
			aimIKBeforePhysics.solver.IKPosition = puppetMaster.muscles[0].target.TransformPoint(puppetMaster.muscles[0].transform.InverseTransformPoint(target.position));
		}

		// PuppetMaster calls this when it is done mapping the animated character to the ragdoll so if we can apply our kinematic adjustments to it now
		void OnPuppetMasterWrite() {
			if (fixAiming) {
				aimIKAfterPhysics.solver.IKPosition = target.position;
				aimIKAfterPhysics.solver.Update();
			}

			if (fixLeftHand) {
				leftHandIK.solver.IKPosition = leftHandTarget.position;
				leftHandIK.solver.IKRotation = leftHandTarget.rotation;
				leftHandIK.solver.Update();
			}
		}

		// Cleaning up the delegates
		void OnDestroy() {
			if (puppetMaster != null) {
				puppetMaster.OnWrite -= OnPuppetMasterWrite;
			}
		}

		void FixedUpdate() {
			foreach (Muscle m in puppetMaster.muscles) if (m.rigidbody.IsSleeping()) m.rigidbody.WakeUp();
		}
	}
}
