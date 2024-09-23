using UnityEngine;


namespace SurfaceDataSystem
{
    public class Poolable : MonoBehaviour
    {
        public System.Action Dispose;


        public virtual void BindDispose( System.Action dispose )
        {
            Dispose = dispose;
        }


        public virtual void Reset()
        {
            gameObject.SetActive( false );
        }


		protected virtual void Update() { }
    }
}