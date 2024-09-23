namespace PaintCore
{
	/// <summary>This component allows you to manage undo/redo states on all CwPaintableTextures in your scene.</summary>
	public static class CwStateManager
	{
		public static bool CanUndo
		{
			get
			{
				foreach (var paintableTexture in CwPaintableTexture.Instances)
				{
					if (paintableTexture.CanUndo == true)
					{
						return true;
					}
				}

				return false;
			}
		}

		public static bool CanRedo
		{
			get
			{
				foreach (var paintableTexture in CwPaintableTexture.Instances)
				{
					if (paintableTexture.CanRedo == true)
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>This method will call StoreState on all active and enabled CwPaintableTextures.</summary>
		public static void StoreAllStates()
		{
			foreach (var paintableTexture in CwPaintableTexture.Instances)
			{
				paintableTexture.StoreState();
			}
		}

		/// <summary>This method will call StoreState on all active and enabled CwPaintableTextures.</summary>
		public static void ClearAllStates()
		{
			foreach (var paintableTexture in CwPaintableTexture.Instances)
			{
				paintableTexture.ClearStates();
			}
		}

		/// <summary>This method will call Undo on all active and enabled CwPaintableTextures.</summary>
		public static void UndoAll()
		{
			foreach (var paintableTexture in CwPaintableTexture.Instances)
			{
				paintableTexture.Undo();
			}
		}

		/// <summary>This method will call Redo on all active and enabled CwPaintableTextures.</summary>
		public static void RedoAll()
		{
			foreach (var paintableTexture in CwPaintableTexture.Instances)
			{
				paintableTexture.Redo();
			}
		}
	}
}