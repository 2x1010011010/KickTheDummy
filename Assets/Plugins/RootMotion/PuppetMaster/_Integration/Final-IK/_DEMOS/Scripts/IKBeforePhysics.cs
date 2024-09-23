using UnityEngine;
using System.Collections;
using RootMotion.FinalIK;
using RootMotion.Dynamics;

namespace RootMotion.Demos {
	
	// Solving IK before the PuppetMaster reads the current pose of the character
	// NB! IK will solve before physics by default so you onle need to use this script when you need to update your IK components in another order 
	// or when you don't have those IK components in the animated character (Target) hierarchy.
	[HelpURL("http://root-motion.com/puppetmasterdox/html/page7.html")]
	public class IKBeforePhysics : MonoBehaviour {

		public PuppetMaster puppetMaster;
		public IK[] IKComponents;

		void Start() {
			// Register to get some calls from PuppetMaster
			puppetMaster.OnRead += OnPuppetMasterRead;
			puppetMaster.OnFixTransforms += OnPuppetMasterFixTransforms;

			// Take control of updating IK solvers
			foreach (IK ik in IKComponents) ik.enabled = false;
		}

		// Called by the PuppetMaster before it starts reading the pose of the animated character for following.
		void OnPuppetMasterRead() {
			if (!enabled) return;

			// Solve IK before PuppetMaster reads the pose of the character
			foreach (IK ik in IKComponents) ik.GetIKSolver().Update();
		}

		void OnPuppetMasterFixTransforms() {
			if (!enabled) return;

			foreach (IK ik in IKComponents) {
				if (ik.fixTransforms) ik.GetIKSolver().FixTransforms();
			}
		}

		// Cleaning up the delegates
		void OnDestroy() {
			if (puppetMaster != null) {
				puppetMaster.OnRead -= OnPuppetMasterRead;
				puppetMaster.OnFixTransforms -= OnPuppetMasterFixTransforms;
			}
		}
	}
}
