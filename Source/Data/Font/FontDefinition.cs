using UnityEngine;

namespace UImGui
{
	[System.Serializable]
	internal struct FontDefinition
	{
		[SerializeField]
		private Object _fontAsset;

		[Tooltip("Path relative to Application.streamingAssetsPath.")]
		public string Path;
		public FontConfig Config;

	}
}
