using UnityEngine;

namespace UImGui
{
	[System.Serializable]
	internal struct FontDefinition
	{
		[Tooltip("Path relative to Application.streamingAssetsPath")]
		public string Path;
		public FontConfig Config;

		[SerializeField]
		private Object _fontAsset;
	}
}
