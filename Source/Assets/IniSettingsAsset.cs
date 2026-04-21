using UnityEngine;

namespace UImGui.Assets
{
	/// <summary>
	/// Used to store ImGui Ini settings in an asset instead of the default imgui.ini file.
	/// Optionally persists to PlayerPrefs when a key is provided.
	/// </summary>
	[CreateAssetMenu(menuName = "Dear ImGui/Ini Settings")]
	internal sealed class IniSettingsAsset : ScriptableObject
	{
		[TextArea(3, 20)]
		[SerializeField]
		private string _data;

		[Tooltip("When set, settings are saved/loaded from PlayerPrefs using this key instead of the asset field.")]
		[SerializeField]
		private string _playerPrefsKey;

		public void Save(string data)
		{
			_data = data;
			if (!string.IsNullOrEmpty(_playerPrefsKey))
			{
				PlayerPrefs.SetString(_playerPrefsKey, data);
				PlayerPrefs.Save();
			}
		}

		public string Load()
		{
			if (!string.IsNullOrEmpty(_playerPrefsKey) && PlayerPrefs.HasKey(_playerPrefsKey))
			{
				return PlayerPrefs.GetString(_playerPrefsKey);
			}

			return _data;
		}
	}
}
