using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace SurfaceDataSystem
{
    public static class MeshExtension
    {
		private readonly static Dictionary<Mesh, int[]> _trianglesCache = new Dictionary<Mesh, int[]>();
		private readonly static Dictionary<Mesh, Vector3[]> _verticesCache = new Dictionary<Mesh, Vector3[]>();


		/// <summary>
		/// Available only for read/write meshes
		/// </summary>
		/// <param name="mesh">Mesh to get TriangleData</param>
		/// <param name="transform">Transform with used mesh</param>
		/// <param name="point">Point to search closest</param>
		/// <param name="normal">Normal that allows to now which side of mesh triangle is backside</param>
		/// <param name="threshold">Is distance nearest that threshold - function will break and return closest</param>
		/// <returns>Return closest TriangleData to point from mesh binded to tranform</returns>
		public static bool TryGetClosestTriangle( this Mesh mesh,out TriangleData result, Transform transform, Vector3 point, Vector3 normal, float threshold = 0.05f )
        {
			result = new TriangleData();
			if( !mesh.isReadable )
            {
                string combined = "Combined Mesh";
				if( mesh.name.Length > combined.Length )
                {
					if( mesh.name.StartsWith( combined ) )
                    {
                        //if( EditorUserBuildSettings.development )
                            //Debug.LogWarning( "Renderer materials mapping can't be used with static batching!" );
						return false;
					}
				}
				//if( EditorUserBuildSettings.development )
					//Debug.LogWarning( mesh.name + " isn't readable! To use this function you need to enable Read/Write in mesh settings and disable static to avoid mesh combaning" );
				return false;
			}
			
            point = transform.InverseTransformPoint( point );
            normal = transform.InverseTransformDirection( normal );

            float nearestDistance = Mathf.Infinity;
    
            int[] triangles = GetTriangles( mesh );
            Vector3[] vertices = GetVertices( mesh );
    		for ( int v = 0; v < triangles.Length; v += 3 )
    		{
                TriangleData info = new TriangleData( point, normal, vertices, triangles, v);
                if( nearestDistance > info.Distance )
                {
                    nearestDistance = info.Distance;
                    result = info;
                    if( nearestDistance < threshold )
                        break;
                }
            }
    
    
            int triangleCounter = 0;
            for ( int subMeshIndex = 0; subMeshIndex < mesh.subMeshCount; subMeshIndex++ )
            {
                var indexCount = mesh.GetSubMesh( subMeshIndex ).indexCount;
                triangleCounter += indexCount / 3;
                if ( result.TriangleIndex < triangleCounter)
                {
                    result.SetSubmeshIndex( subMeshIndex );
                    break;
                }
            }
    
            return true;
        }


        private static Vector3[] GetVertices( this Mesh mesh )
        {
            if( _verticesCache.TryGetValue( mesh, out var vertices ) )
                return vertices;
            else
            {
                vertices = mesh.vertices;
                _verticesCache.Add( mesh, vertices );
                return vertices;
            }
        }


		private static int[] GetTriangles( this Mesh mesh )
		{
			if( _trianglesCache.TryGetValue( mesh, out var triangles ) )
				return triangles;
			else
			{
				triangles = mesh.triangles;
				_trianglesCache.Add( mesh, triangles );
				return triangles;
			}
		}


		/// <summary>
		/// Available only for read/write meshes!
		/// </summary>
		public static bool TryGetMaterial( this MeshRenderer renderer, Mesh mesh, RaycastHit hit, out Material material ) =>
            renderer.TryGetMaterial( mesh, hit.point, hit.normal, out material );


        /// <summary>
        /// Available only for read/write meshes!
        /// </summary>
        public static bool TryGetMaterial( this MeshRenderer renderer, Mesh mesh, Vector3 hitPoint, Vector3 hitNormal, out Material material )
        {
            material = null;
            if( !mesh.TryGetClosestTriangle( out TriangleData closestTriangle, renderer.transform, hitPoint, hitNormal ) )
                return false;

            if( closestTriangle.SubmeshIndex == -1 )
                return false;
            else
            {
                material = renderer.sharedMaterials[ closestTriangle.SubmeshIndex ];
                return material != null;
            }
        }
    }
    

    public struct TriangleData
    {
        public float Distance { get; private set; }
        public int TriangleIndex { get; private set; }
        public Vector3 Normal { get; private set; }
        public Vector3 Centre { get; private set; }
        public Vector3 ClosestPoint { get; private set; }
        public int SubmeshIndex { get; private set; }

        public void SetSubmeshIndex( int index ) => SubmeshIndex = index;


        public TriangleData( Vector3 point, Vector3 normal, Vector3[] vertices, int[] triangles, int index )
        {
            TriangleIndex = index / 3;
    		Distance = float.PositiveInfinity;
            SubmeshIndex = 0;
    
    		Vector3 v1 = vertices[ triangles[ index ] ];
            Vector3 v2 = vertices[ triangles[ index + 1 ] ];
            Vector3 v3 = vertices[ triangles[ index + 2 ] ];
    
            Normal = Vector3.Cross( v2 - v1, v3 - v1 );
            Centre = v1 * 0.3333f + v2 * 0.3333f + v3 * 0.3333f;

            float dot = Vector3.Dot( Normal, normal );
            if( dot > 0 )
            {
                ClosestPoint = NearestPointOnTriangle( point, v1, v2, v3 );
                Distance = Vector3.Distance( point, ClosestPoint );
                 
                if( float.IsNaN( Distance ) )
                    Distance = float.PositiveInfinity;
            }
            else
                ClosestPoint = Vector3.zero;
        }


        public static Vector3 NearestPointOnTriangle( Vector3 point, Vector3 a, Vector3 b, Vector3 c )
        {
            // get barycentric coords
            Vector3 v0 = c - a;
            Vector3 v1 = b - a;
            Vector3 v2 = point - a;
    
            float d00 = Vector3.Dot( v0, v0 );
            float d01 = Vector3.Dot( v0, v1 );
            float d11 = Vector3.Dot( v1, v1 );
            float d20 = Vector3.Dot( v2, v0 );
            float d21 = Vector3.Dot( v2, v1 );
    
            float denom = d00 * d11 - d01 * d01;
            float u = (d11 * d20 - d01 * d21) / denom;
            float v = (d00 * d21 - d01 * d20) / denom;
            
            // check is point inside triangle
            if ( u >= 0f && v >= 0f && u + v <= 1f )
                return a + u * v0 + v * v1; // return point on triangle
            else
            {
                Vector3 nearestOnAB = NearestPointOnLineSegment( a, b, point );
                Vector3 nearestOnBC = NearestPointOnLineSegment( b, c, point );
                Vector3 nearestOnCA = NearestPointOnLineSegment( c, a, point );
            
                float distAB = Vector3.Distance( point, nearestOnAB );
                float distBC = Vector3.Distance( point, nearestOnBC );
                float distCA = Vector3.Distance( point, nearestOnCA );
            
                if ( distAB <= distBC && distAB <= distCA )
                    return nearestOnAB;
                else if ( distBC <= distAB && distBC <= distCA )
                    return nearestOnBC;
                else
                    return nearestOnCA;
            }
        }
    
        public static Vector3 NearestPointOnLineSegment( Vector3 a, Vector3 b, Vector3 p )
        {
            Vector3 ab = b - a;
            float t = Vector3.Dot( p - a, ab ) / Vector3.Dot( ab, ab );
            t = Mathf.Clamp01( t );
            return a + t * ab;
        }
    }
}