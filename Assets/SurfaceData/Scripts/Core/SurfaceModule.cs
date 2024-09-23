using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem
{
    public abstract class SurfaceModule : ScriptableObject
    {
        protected Surface Surface { get; private set; }


        public void Init( Surface surface )
        {
			Surface = surface;

			OnInit();
        }


        protected virtual void OnInit(){ }
    }
}