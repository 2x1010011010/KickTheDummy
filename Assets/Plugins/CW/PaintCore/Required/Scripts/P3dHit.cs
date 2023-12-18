using UnityEngine;

namespace PaintIn3D
{
	/// <summary>This stores information about a scene point on a mesh. This is usually generated from a <b>RaycastHit</b>, but it can also be filled manually.</summary>
	public struct P3dHit
	{
		public P3dHit(RaycastHit hit)
		{
			Raw           = hit;
			first         = default(Vector2); // hit.textureCoord; this can cause warnings in newer versions of Unity...
			firstSet      = false;
			second        = default(Vector2); // hit.textureCoord2; this can cause warnings in newer versions of Unity...
			secondSet     = false;
			Position      = hit.point;
			Normal        = hit.normal;
			Transform     = hit.transform;
			TriangleIndex = hit.triangleIndex;
			Distance      = hit.distance;
			Collider      = hit.collider;
		}

		public RaycastHit Raw;

		/// <summary>The first UV coord that was hit.</summary>
		private Vector2 first;

		private bool firstSet;

		public Vector2 First
		{
			set
			{
				first   = value;
				firstSet = true;
			}

			get
			{
				return firstSet == true ? first : Raw.textureCoord;
			}
		}

		/// <summary>The second UV coord that was hit.</summary>
		private Vector2 second;

		private bool secondSet;

		public Vector2 Second
		{
			set
			{
				second   = value;
				secondSet = true;
			}

			get
			{
				return secondSet == true ? second : Raw.textureCoord2;
			}
		}

		/// <summary>The world position that was hit.</summary>
		public Vector3 Position;

		/// <summary>The world normal that was hit.</summary>
		public Vector3 Normal;

		/// <summary>The Transform that was hit.</summary>
		public Transform Transform;

		/// <summary>The triangle index that was hit.</summary>
		public int TriangleIndex;

		/// <summary>The world distance that was hit.</summary>
		public float Distance;

		/// <summary>The Collider that was hit.</summary>
		public Collider Collider;
	}
}