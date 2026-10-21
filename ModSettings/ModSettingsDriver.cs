using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MagicMike.ModSettings;

// Lives on BepInEx's manager object, so it survives scene changes; the panel itself is rebuilt with each scene's GUI
internal sealed class ModSettingsDriver : MonoBehaviour
{
	private SettingsPanel m_panel;

	private void Start()
	{
		// Every plugin has loaded by now, so the election is settled
		if (!ModSettings.IsHost)
		{
			Destroy(this);
			return;
		}
		ModSettings.StartHost();
	}

	private void Update()
	{
		// While a key is being bound, the toggle key is captured like any other instead of closing the panel
		if (ModSettings.ToggleKey == null || !ModSettings.ToggleKey.Value.IsDown() || IsTyping() || (m_panel != null && m_panel.IsCapturingKey))
		{
			return;
		}
		if (m_panel == null)
		{
			if (GUIManager.IsHeadless() || GUIManager.CustomGUIFront == null)
			{
				return;
			}
			m_panel = SettingsPanel.Create();
		}
		m_panel.Toggle();
	}

	// The hotkey is ignored while the player is typing into the game's own text boxes, or a text box a mod added
	private static bool IsTyping()
	{
		if (SettingsPanel.IsOpen)
		{
			return false;
		}
		return global::Console.IsVisible()
			|| TextInput.IsVisible()
			|| (Chat.instance != null && Chat.instance.HasFocus())
			|| TextBoxFocused();
	}

	private static bool TextBoxFocused()
	{
		GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
		if (selected == null)
		{
			return false;
		}
		TMP_InputField tmp = selected.GetComponent<TMP_InputField>();
		if (tmp != null && tmp.isFocused)
		{
			return true;
		}
		InputField legacy = selected.GetComponent<InputField>();
		return legacy != null && legacy.isFocused;
	}
}
