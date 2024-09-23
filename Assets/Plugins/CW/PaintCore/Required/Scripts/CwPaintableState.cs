using UnityEngine;
using System.Collections.Generic;

namespace PaintCore
{
	/// <summary>This class stores information about a particular paintable texture state. Either a full texture copy, or a list of commands used to draw it.</summary>
	public class CwPaintableState
	{
		public RenderTexture Texture;

		public List<CwCommand> Commands = new List<CwCommand>();

		private static Stack<CwPaintableState> pool = new Stack<CwPaintableState>();

		public static CwPaintableState Pop()
		{
			return pool.Count > 0 ? pool.Pop() : new CwPaintableState();
		}

		public void Write(RenderTexture current)
		{
			Clear();

			Texture = CwCommon.GetRenderTexture(current.descriptor, current);

			CwCommon.Blit(Texture, current);
		}

		public void Write(List<CwCommand> commands)
		{
			Clear();

			Commands.AddRange(commands);
		}

		public void Write(RenderTexture current, List<CwCommand> commands)
		{
			Clear();

			Texture = CwCommon.GetRenderTexture(current.descriptor, current);

			CwCommon.Blit(Texture, current);

			Commands.AddRange(commands);
		}

		private void Clear()
		{
			if (Texture != null)
			{
				CwCommon.ReleaseRenderTexture(Texture);

				Texture = null;
			}

			for (var i = Commands.Count - 1; i >= 0; i--)
			{
				Commands[i].Pool();
			}

			Commands.Clear();
		}

		public void Pool()
		{
			Clear();

			pool.Push(this);
		}
	}
}