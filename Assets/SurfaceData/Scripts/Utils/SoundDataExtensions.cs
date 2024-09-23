using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SurfaceDataSystem
{
    public static class SoundDataExtensions
    {
        public static AudioClip GetRandom( this AudioClip[] clips, out int index, int previousIndex = -1 )
        {
            List<int> c = new();

            for( int i = 0; i < clips.Length; i++ )
                c.Add( i );

            if( previousIndex != -1 && previousIndex < c.Count && c.Count > 1 )
                c.RemoveAt( previousIndex );

            index = c[ Random.Range( 0, c.Count ) ];

            return clips[ index ];
        }
    }
}