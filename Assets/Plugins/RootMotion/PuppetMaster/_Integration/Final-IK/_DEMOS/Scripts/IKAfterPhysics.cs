using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using RootMotion.Dynamics;

namespace RootMotion.Demos {

	// Solving IK after the physics simulation to make cosmetic adjustments to the final pose of the character for that frame.
	[HelpURL("http://root-motion.com/puppetmasterdox/html/page7.html")]
	public class IKAfterPhysics : MonoBehaviour {

		public PuppetMaster puppetMaster;
		public IK[] IKComponents;
		
		void Start() {
			// Register to get some calls from PuppetMaster
			puppetMaster.OnWrite += OnPuppetMasterWrite;

			// Take control of updating IK solvers
			foreach (IK ik in IKComponents) ik.enabled = false;
		}

		// PuppetMaster calls this when it is done mapping the animated character to the ragdoll so if we can apply our kinematic adjustments to it now
		void OnPuppetMasterWrite() {
			if (!enabled) return;

			// Solve IK after PuppetMaster writes the solved pose on the animated character.
			foreach (IK ik in IKComponents) ik.GetIKSolver().Update();
		}

		// Cleaning up the delegates
		void OnDestroy() {
			if (puppetMaster != null) {
				puppetMaster.OnWrite -= OnPuppetMasterWrite;
			}
		}
	}
}
