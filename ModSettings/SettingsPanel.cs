using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MagicMike.ModSettings;

// Sits on a full-screen dimmed overlay; the panel itself is centred on it
internal sealed class SettingsPanel : MonoBehaviour
{
	private const float MaxWidth = 1040f;

	private const float MaxHeight = 760f;

	private const float Margin = 32f;

	private const float RefreshInterval = 1f;

	// The mod's name and version sit left of the search box; the section buttons go in rows under them
	private const float ModTitleWidth = 300f;

	private const float SectionTop = 162f;

	private const float SectionRowHeight = 32f;

	private const float SectionGap = 5f;

	private const int MaxSectionsPerRow = 7;

	// The mod tabs go in two rows when there are more than this, and everything under them moves down a row
	private const int MaxTabsPerRow = 5;

	private const float TabTop = 74f;

	private const float TabRowHeight = 34f;

	private const float TabGap = 5f;

	private static string DescriptionHint => "<color=" + Theme.Tag(Theme.Muted) + ">Hover over a setting to see what it does.</color>";

	private static readonly KeyCode[] s_modifiers =
	{
		KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftAlt, KeyCode.RightAlt
	};

	// Every key that can be bound, without the controller buttons that would otherwise be picked up by accident
	private static readonly KeyCode[] s_bindableKeys = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>()
		.Where(k => k != KeyCode.None && k < KeyCode.JoystickButton0)
		.Distinct()
		.ToArray();

	private static string s_lastMod;

	public static bool IsOpen { get; private set; }

	private readonly List<SettingRow> m_rows = new List<SettingRow>();

	private readonly List<(GameObject header, List<SettingRow> rows)> m_sections = new List<(GameObject, List<SettingRow>)>();

	private readonly List<string> m_sectionNames = new List<string>();

	// One button per section of the selected mod, jumping the list to it
	private readonly List<(Button button, TMP_Text label, GameObject header)> m_sectionButtons = new List<(Button, TMP_Text, GameObject)>();

	private RectTransform m_sectionBar;

	private GameObject m_shownSection;

	// The section last jumped to, and where the list stood after the jump: it stays lit while the list stays there (a section near
	// the end cannot be scrolled to the very top, so the one at the top could be another)
	private GameObject m_jumpedTo;

	private float m_jumpedOffset;

	private readonly Dictionary<ConfigFile, bool> m_savedSaveOnSet = new Dictionary<ConfigFile, bool>();

	private readonly HashSet<ConfigFile> m_changedFiles = new HashSet<ConfigFile>();

	private readonly List<(Button button, TMP_Text label)> m_tabs = new List<(Button, TMP_Text)>();

	private List<ModInfo> m_mods = new List<ModInfo>();

	private ModInfo m_selected;

	private RectTransform m_tabBar;

	// How far the second row of tabs, if there is one, moves the title, search, sections and list down
	private float m_tabShift;

	private ScrollRect m_settingsList;

	private TMP_Text m_modTitle;

	private TMP_Text m_description;

	private TMP_Text m_hint;

	private TMP_Text m_empty;

	private TMP_InputField m_search;

	private RectTransform m_panel;

	private DragMove m_drag;

	private SettingRow m_capturing;

	private int m_captureFrame;

	private float m_nextRefresh;

	private bool m_inputFocusedLastFrame;

	private bool m_destroying;

	public bool IsCapturingKey => m_capturing != null;

	public static SettingsPanel Create()
	{
		Transform canvas = GUIManager.CustomGUIFront.transform;
		Image overlay = Widgets.Box(canvas, "MagicMikeModSettings", Theme.Dim);
		Widgets.Fill(overlay.gameObject);
		SettingsPanel settings = overlay.gameObject.AddComponent<SettingsPanel>();
		Vector2 canvasSize = ((RectTransform)canvas).rect.size;
		settings.Build(Mathf.Min(MaxWidth, canvasSize.x * 0.9f), Mathf.Min(MaxHeight, canvasSize.y * 0.9f));
		overlay.gameObject.SetActive(false);
		return settings;
	}

	private void Build(float width, float height)
	{
		Image panelImage = Widgets.Box(transform, "Panel", Theme.Panel, Theme.PanelOutline, 2f);
		RectTransform panelRect = (RectTransform)panelImage.transform;
		panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.sizeDelta = new Vector2(width, height);
		Transform panel = panelImage.transform;
		m_panel = panelRect;
		m_drag = panelImage.gameObject.AddComponent<DragMove>();
		m_drag.Target = panelRect;
		// Saved with the panel's other settings when it closes, so it opens in the same place next session
		m_drag.OnMoved = position => ModSettings.PanelPosition = position;

		// Title bar, styled like the section headers; the buttons sit inside it at either end
		Image titleBar = Widgets.Box(panel, "TitleBar", Theme.Header);
		Widgets.Place(titleBar.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -66f), new Vector2(-12f, -12f));
		Image titleAccent = Widgets.Box(titleBar.transform, "Accent", Theme.Accent);
		Widgets.Place(titleAccent.gameObject, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(4f, 0f));

		TMP_Text title = Widgets.Label(titleBar.transform, "Magic Mike's Mod Settings", 26f, Theme.TextBright, TextAlignmentOptions.Center, bold: true);
		Widgets.Place(title.gameObject, Vector2.zero, Vector2.one, new Vector2(120f, 0f), new Vector2(-120f, 0f));

		Button close = Widgets.IconButton<CloseIcon>(titleBar.transform, 9f);
		Widgets.Place(close.gameObject, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-46f, -15f), new Vector2(-12f, 15f));
		close.onClick.AddListener(Close);

		// Puts a dragged panel back in the middle of the screen
		Button recentre = Widgets.Button(titleBar.transform, "Re-centre", 14f);
		Widgets.Place(recentre.gameObject, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, -15f), new Vector2(106f, 15f));
		recentre.onClick.AddListener(ResetPosition);

		// One tab per installed mod (laid out, and the title and search placed under them, when the panel opens)
		m_tabBar = (RectTransform)Widgets.Child(panel, "Tabs").transform;

		// The selected mod's name and version
		m_modTitle = Widgets.Label(panel, "", 20f, Theme.Text, bold: true);
		m_modTitle.enableAutoSizing = true;
		m_modTitle.fontSizeMin = 14f;
		m_modTitle.fontSizeMax = 20f;
		m_modTitle.textWrappingMode = TextWrappingModes.NoWrap;
		m_modTitle.overflowMode = TextOverflowModes.Ellipsis;

		// Search beside it, with a button inside it to clear it
		m_search = Widgets.Input(panel, 17f, "Search settings...");
		((RectTransform)m_search.textViewport).offsetMax = new Vector2(-46f, -2f);
		m_search.onValueChanged.AddListener(_ => ApplySearch());
		Button clear = Widgets.IconButton<CloseIcon>(m_search.transform, 9f);
		Widgets.Place(clear.gameObject, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-38f, -15f), new Vector2(-4f, 15f));
		clear.onClick.AddListener(() => m_search.text = "");

		// The sections of the selected mod (laid out, and the list placed under them, once it is chosen)
		m_sectionBar = (RectTransform)Widgets.Child(panel, "Sections").transform;

		m_settingsList = Widgets.ScrollView(panel);
		Widgets.Place(m_settingsList.gameObject, Vector2.zero, Vector2.one, new Vector2(Margin, 166f), new Vector2(-Margin, -(SectionTop + SectionRowHeight + 10f)));
		m_empty = Widgets.Label(m_settingsList.viewport, "No settings match your search.", 17f, Theme.Muted, TextAlignmentOptions.Center);
		Widgets.Place(m_empty.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -60f), new Vector2(0f, -20f));
		m_empty.gameObject.SetActive(false);

		// What the hovered setting does
		Image descriptionBox = Widgets.Box(panel, "Description", Theme.Field, Theme.FieldOutline);
		Widgets.Place(descriptionBox.gameObject, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(Margin, 48f), new Vector2(-Margin, 158f));
		m_description = Widgets.Label(descriptionBox.transform, DescriptionHint, 18f, Theme.Text, TextAlignmentOptions.TopLeft);
		m_description.textWrappingMode = TextWrappingModes.Normal;
		m_description.overflowMode = TextOverflowModes.Truncate;
		Widgets.Fill(m_description.gameObject, 8f);

		m_hint = Widgets.Label(panel, "", 14f, Theme.Muted, TextAlignmentOptions.Center);
		Widgets.Place(m_hint.gameObject, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(Margin, 14f), new Vector2(-Margin, 40f));
	}

	public void Open()
	{
		if (IsOpen)
		{
			return;
		}
		IsOpen = true;
		gameObject.SetActive(true);
		transform.SetAsLastSibling();
		// Clamped again in case the screen or UI scale has changed since it was saved
		m_panel.anchoredPosition = m_drag.Clamp(ModSettings.PanelPosition);
		GUIManager.BlockInput(true);

		m_mods = SettingsDiscovery.FindMods();
		// Changes are applied straight away but written to disk once, on close, rather than on every slider step
		foreach (ModInfo mod in m_mods)
		{
			if (!m_savedSaveOnSet.ContainsKey(mod.Config))
			{
				m_savedSaveOnSet[mod.Config] = mod.Config.SaveOnConfigSet;
				mod.Config.SaveOnConfigSet = false;
				mod.Config.SettingChanged += OnSettingChanged;
			}
		}

		BuildTabs();
		Select(m_mods.FirstOrDefault(m => m.Name == s_lastMod) ?? m_mods.FirstOrDefault());
		m_hint.text = "Press " + ModSettings.ToggleKey.Value + " or Esc to close   |   Changes are saved when the panel closes";
	}

	public void Close()
	{
		if (!IsOpen)
		{
			return;
		}
		IsOpen = false;
		m_capturing = null;
		GUIManager.BlockInput(false);
		if (!m_destroying)
		{
			gameObject.SetActive(false);
		}

		foreach (KeyValuePair<ConfigFile, bool> saved in m_savedSaveOnSet)
		{
			saved.Key.SettingChanged -= OnSettingChanged;
			saved.Key.SaveOnConfigSet = saved.Value;
			if (m_changedFiles.Contains(saved.Key))
			{
				try
				{
					saved.Key.Save();
				}
				catch (Exception ex)
				{
					Jotunn.Logger.LogWarning("[ModSettings] Could not save " + saved.Key.ConfigFilePath + ": " + ex.Message);
				}
			}
		}
		bool changed = m_changedFiles.Count > 0;
		m_savedSaveOnSet.Clear();
		m_changedFiles.Clear();
		if (changed)
		{
			SyncWithServer();
		}
	}

	public void Toggle()
	{
		if (IsOpen)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	private void OnDestroy()
	{
		// The panel goes with the scene; settings changed while it was open are still saved and input is given back
		m_destroying = true;
		Close();
	}

	// (BepInEx sends the setting itself as the sender, not its file: the file is taken from the setting)
	private void OnSettingChanged(object sender, SettingChangedEventArgs args)
	{
		ConfigFile file = args?.ChangedSetting?.ConfigFile ?? sender as ConfigFile;
		if (file != null)
		{
			m_changedFiles.Add(file);
		}
	}

	// Does what Jotunn does when ConfigurationManager's window closes: tells mods listening for it, and sends
	// changed server-synced settings on (to the server from an admin, or to the players from the server)
	private static void SyncWithServer()
	{
		if (ZNet.instance == null)
		{
			return;
		}
		foreach (string method in new[] { "InvokeOnConfigurationWindowClosed", "SynchronizeChangedConfig" })
		{
			try
			{
				AccessTools.Method(typeof(SynchronizationManager), method)?.Invoke(SynchronizationManager.Instance, null);
			}
			catch (Exception ex)
			{
				Jotunn.Logger.LogWarning("[ModSettings] Could not sync settings with the server (" + method + "): " + (ex.InnerException ?? ex).Message);
			}
		}
	}

	private void ResetPosition()
	{
		m_panel.anchoredPosition = Vector2.zero;
		ModSettings.PanelPosition = Vector2.zero;
	}

	private void BuildTabs()
	{
		ClearChildren(m_tabBar);
		m_tabs.Clear();
		int count = m_mods.Count;
		int rows = count <= MaxTabsPerRow ? 1 : 2;
		int perRow = Math.Max(1, Mathf.CeilToInt(count / (float)rows));
		m_tabShift = (rows - 1) * (TabRowHeight + TabGap);
		float height = TabRowHeight + m_tabShift;
		Widgets.Place(m_tabBar.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(Margin, -(TabTop + height)), new Vector2(-Margin, -TabTop));
		Widgets.Place(m_modTitle.gameObject, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(Margin + 4f, -(154f + m_tabShift)), new Vector2(Margin + ModTitleWidth, -(116f + m_tabShift)));
		Widgets.Place(m_search.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(Margin + ModTitleWidth + 12f, -(154f + m_tabShift)), new Vector2(-Margin, -(116f + m_tabShift)));

		for (int i = 0; i < count; i++)
		{
			ModInfo mod = m_mods[i];
			int row = i / perRow;
			int column = i % perRow;
			int inRow = row == rows - 1 ? count - row * perRow : perRow;
			Button tab = Widgets.Button(m_tabBar, mod.Name, 19f);
			Widgets.Place(tab.gameObject,
				new Vector2(column / (float)inRow, 1f - (row + 1) / (float)rows),
				new Vector2((column + 1) / (float)inRow, 1f - row / (float)rows),
				new Vector2(TabGap / 2f, row < rows - 1 ? TabGap / 2f : 0f),
				new Vector2(-TabGap / 2f, row > 0 ? -TabGap / 2f : 0f));
			TMP_Text label = tab.GetComponentInChildren<TMP_Text>(includeInactive: true);
			label.enableAutoSizing = true;
			label.fontSizeMin = 12f;
			label.fontSizeMax = 19f;
			tab.onClick.AddListener(() => Select(mod));
			m_tabs.Add((tab, label));
		}
	}

	private void HighlightSelectedTab()
	{
		for (int i = 0; i < m_tabs.Count && i < m_mods.Count; i++)
		{
			bool selected = m_mods[i] == m_selected;
			(Button button, TMP_Text label) = m_tabs[i];
			button.colors = selected
				? Theme.Colors(Theme.Selected, Theme.Selected, Theme.ButtonPressed, Theme.ButtonDisabled)
				: Theme.Colors(Theme.Button, Theme.ButtonHover, Theme.ButtonPressed, Theme.ButtonDisabled);
			label.color = selected ? Theme.TextBright : Theme.Text;
			label.fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
		}
	}

	private void Select(ModInfo mod)
	{
		m_selected = mod;
		m_capturing = null;
		s_lastMod = mod?.Name;
		HighlightSelectedTab();

		m_modTitle.text = mod == null ? "No settings found" : mod.Name + "  <size=70%><color=" + Theme.Tag(Theme.Muted) + ">v" + mod.Version + "</color></size>";
		ShowDescription(null);
		ClearChildren(m_settingsList.content);
		m_rows.Clear();
		m_sections.Clear();
		m_sectionNames.Clear();
		m_shownSection = m_jumpedTo = null;
		if (mod == null)
		{
			BuildSectionButtons();
			return;
		}

		string section = null;
		List<SettingRow> sectionRows = null;
		foreach (SettingInfo setting in mod.Settings)
		{
			if (setting.Section != section)
			{
				section = setting.Section;
				sectionRows = new List<SettingRow>();
				m_sections.Add((BuildSectionHeader(section), sectionRows));
				m_sectionNames.Add(section);
			}
			SettingRow row = new SettingRow(this, m_settingsList.content, setting);
			m_rows.Add(row);
			sectionRows.Add(row);
		}
		BuildSectionButtons();
		ApplySearch();
		m_settingsList.verticalNormalizedPosition = 1f;
	}

	// A button for each section, in one row (two if there are many), and the list moved up or down to sit just under them
	private void BuildSectionButtons()
	{
		ClearChildren(m_sectionBar);
		m_sectionButtons.Clear();
		int count = m_sections.Count;
		int rows = count == 0 ? 0 : count <= MaxSectionsPerRow ? 1 : 2;
		int perRow = rows == 0 ? 0 : Mathf.CeilToInt(count / (float)rows);
		float height = rows * SectionRowHeight + Math.Max(0, rows - 1) * SectionGap;
		float top = SectionTop + m_tabShift;
		Widgets.Place(m_sectionBar.gameObject, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(Margin, -(top + height)), new Vector2(-Margin, -top));
		RectTransform list = (RectTransform)m_settingsList.transform;
		list.offsetMax = new Vector2(list.offsetMax.x, -(top + height + (rows > 0 ? 10f : 0f)));

		for (int i = 0; i < count; i++)
		{
			int row = i / perRow;
			int column = i % perRow;
			int inRow = row == rows - 1 ? count - row * perRow : perRow;
			GameObject header = m_sections[i].header;
			Button button = Widgets.Button(m_sectionBar, m_sectionNames[i], 16f);
			Widgets.Place(button.gameObject,
				new Vector2(column / (float)inRow, 1f - (row + 1) / (float)rows),
				new Vector2((column + 1) / (float)inRow, 1f - row / (float)rows),
				new Vector2(SectionGap / 2f, row < rows - 1 ? SectionGap / 2f : 0f),
				new Vector2(-SectionGap / 2f, row > 0 ? -SectionGap / 2f : 0f));
			TMP_Text label = button.GetComponentInChildren<TMP_Text>(includeInactive: true);
			label.enableAutoSizing = true;
			label.fontSizeMin = 11f;
			label.fontSizeMax = 16f;
			label.textWrappingMode = TextWrappingModes.NoWrap;
			label.overflowMode = TextOverflowModes.Ellipsis;
			button.onClick.AddListener(() => JumpTo(header));
			m_sectionButtons.Add((button, label, header));
		}
		HighlightSection(null);
	}

	// Scrolls the list so the section's header is at the top (or as near as the list goes). A section the search hides is shown
	// again by clearing the search.
	private void JumpTo(GameObject header)
	{
		if (header == null)
		{
			return;
		}
		if (!header.activeSelf)
		{
			m_search.text = "";
		}
		RectTransform content = m_settingsList.content;
		LayoutRebuilder.ForceRebuildLayoutImmediate(content);
		float max = Mathf.Max(0f, content.rect.height - m_settingsList.viewport.rect.height);
		float offset = Mathf.Clamp(SectionOffset(header) - 2f, 0f, max);
		m_settingsList.StopMovement();
		content.anchoredPosition = new Vector2(content.anchoredPosition.x, offset);
		m_jumpedTo = header;
		m_jumpedOffset = offset;
		HighlightSection(header);
	}

	// How far down the list the header's top is (the layout puts each row's top at minus its anchored position, less its height
	// above the pivot)
	private static float SectionOffset(GameObject header)
	{
		RectTransform rect = (RectTransform)header.transform;
		return -(rect.anchoredPosition.y + rect.rect.height * (1f - rect.pivot.y));
	}

	// The section at the top of the list as it scrolls: the last header at or above the top
	private GameObject SectionShown()
	{
		float offset = m_settingsList.content.anchoredPosition.y;
		if (m_jumpedTo != null && Mathf.Abs(offset - m_jumpedOffset) < 1f)
		{
			return m_jumpedTo;
		}
		m_jumpedTo = null;
		GameObject shown = null;
		foreach ((GameObject header, List<SettingRow> _) in m_sections)
		{
			if (!header.activeSelf)
			{
				continue;
			}
			if (shown == null || SectionOffset(header) <= offset + 4f)
			{
				shown = header;
			}
		}
		return shown;
	}

	private void HighlightSection(GameObject shown)
	{
		m_shownSection = shown;
		foreach ((Button button, TMP_Text label, GameObject header) in m_sectionButtons)
		{
			bool selected = header == shown;
			button.colors = selected
				? Theme.Colors(Theme.Selected, Theme.Selected, Theme.ButtonPressed, Theme.ButtonDisabled)
				: Theme.Colors(Theme.Button, Theme.ButtonHover, Theme.ButtonPressed, Theme.ButtonDisabled);
			label.color = selected ? Theme.TextBright : Theme.Text;
		}
	}

	private GameObject BuildSectionHeader(string section)
	{
		GameObject header = Widgets.Row(m_settingsList.content, 34f);
		header.AddComponent<Image>().color = Theme.Header;
		Image accent = Widgets.Box(header.transform, "Accent", Theme.Accent);
		Widgets.Place(accent.gameObject, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, new Vector2(4f, 0f));
		TMP_Text text = Widgets.Label(header.transform, section, 17f, Theme.TextBright, bold: true);
		Widgets.Place(text.gameObject, Vector2.zero, Vector2.one, new Vector2(16f, 0f), new Vector2(-8f, 0f));
		return header;
	}

	private void ApplySearch()
	{
		string search = m_search.text?.Trim();
		bool anyVisible = false;
		foreach ((GameObject header, List<SettingRow> rows) in m_sections)
		{
			bool any = false;
			bool odd = false;
			foreach (SettingRow row in rows)
			{
				bool match = row.Matches(search);
				row.Root.SetActive(match);
				if (match)
				{
					row.SetStripe(odd);
					odd = !odd;
					any = true;
				}
			}
			header.SetActive(any);
			anyVisible |= any;
		}
		m_empty.gameObject.SetActive(!anyVisible && m_selected != null);
	}

	public void ShowDescription(SettingRow row)
	{
		if (row == null)
		{
			m_description.text = DescriptionHint;
			return;
		}
		SettingInfo setting = row.Setting;
		List<string> details = new List<string> { "Default: " + FormatDefault(setting.Entry) };
		AcceptableValueBase acceptable = setting.Entry.Description?.AcceptableValues;
		if (acceptable != null && acceptable.GetType().IsGenericType && acceptable.GetType().GetGenericTypeDefinition() == typeof(AcceptableValueRange<>))
		{
			details.Add("Range: " + acceptable.GetType().GetProperty("MinValue").GetValue(acceptable) + " to " + acceptable.GetType().GetProperty("MaxValue").GetValue(acceptable));
		}
		if (setting.Synced)
		{
			details.Add(setting.IsReadOnly ? "Set by the server (only an admin can change it)" : "Synced from the server");
		}
		m_description.text = "<b><color=" + Theme.Tag(Theme.TextBright) + ">" + setting.Name + "</color></b>   <size=85%><color=" + Theme.Tag(Theme.Muted) + ">" + string.Join("   |   ", details) + "</color></size>\n" + setting.Description;
	}

	private static string FormatDefault(ConfigEntryBase entry)
	{
		object value = entry.DefaultValue;
		if (value is KeyboardShortcut shortcut)
		{
			return shortcut.MainKey == KeyCode.None ? "None" : shortcut.ToString();
		}
		return TomlTypeConverter.ConvertToString(value, entry.SettingType);
	}

	public bool IsCapturing(SettingRow row)
	{
		return m_capturing == row;
	}

	public void StartCapture(SettingRow row)
	{
		if (row.Setting.IsReadOnly)
		{
			return;
		}
		SettingRow previous = m_capturing;
		m_capturing = row;
		m_captureFrame = Time.frameCount;
		previous?.Refresh();
		row.Refresh();
	}

	private void EndCapture(object value)
	{
		SettingRow row = m_capturing;
		m_capturing = null;
		if (value != null)
		{
			row.SetValue(value);
		}
		row.Refresh();
	}

	private void Update()
	{
		if (!IsOpen)
		{
			return;
		}
		if (m_capturing != null)
		{
			UpdateCapture();
		}
		else if (Input.GetKeyDown(KeyCode.Escape) && !m_inputFocusedLastFrame)
		{
			Close();
			return;
		}

		// Picks up admin status changes, server syncs and edits made elsewhere while the panel is open
		// Set every frame so a change to Scroll Speed in the panel's own tab takes effect straight away
		// (the 2 is the spacing between rows)
		m_settingsList.scrollSensitivity = ModSettings.ScrollSpeed.Value * (SettingRow.RowHeight + 2f);

		// The button of the section at the top of the list stays lit as it scrolls
		GameObject shown = SectionShown();
		if (shown != m_shownSection)
		{
			HighlightSection(shown);
		}

		if (Time.unscaledTime >= m_nextRefresh)
		{
			m_nextRefresh = Time.unscaledTime + RefreshInterval;
			foreach (SettingRow row in m_rows)
			{
				row.Refresh();
			}
		}

		GameObject selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
		TMP_InputField focused = selected != null ? selected.GetComponent<TMP_InputField>() : null;
		m_inputFocusedLastFrame = focused != null && focused.isFocused;
	}

	private void UpdateCapture()
	{
		// Ignore the click that started the capture
		if (Time.frameCount == m_captureFrame)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			EndCapture(null);
			return;
		}

		bool shortcut = m_capturing.Setting.Entry.SettingType == typeof(KeyboardShortcut);
		foreach (KeyCode key in s_bindableKeys)
		{
			if (!Input.GetKeyDown(key))
			{
				continue;
			}
			if (!shortcut)
			{
				EndCapture(key);
				return;
			}
			// A modifier on its own waits: it becomes the main key only if released without another key
			if (Array.IndexOf(s_modifiers, key) >= 0)
			{
				continue;
			}
			EndCapture(new KeyboardShortcut(key, HeldModifiers(except: key)));
			return;
		}
		if (shortcut)
		{
			foreach (KeyCode modifier in s_modifiers)
			{
				if (Input.GetKeyUp(modifier))
				{
					EndCapture(new KeyboardShortcut(modifier, HeldModifiers(except: modifier)));
					return;
				}
			}
		}
	}

	private static KeyCode[] HeldModifiers(KeyCode except)
	{
		return s_modifiers.Where(m => m != except && Input.GetKey(m)).ToArray();
	}

	private static void ClearChildren(Transform parent)
	{
		for (int i = parent.childCount - 1; i >= 0; i--)
		{
			GameObject child = parent.GetChild(i).gameObject;
			// Detached first so the layout ignores it straight away; Destroy only takes effect at the end of the frame
			child.SetActive(false);
			child.transform.SetParent(null, worldPositionStays: false);
			Destroy(child);
		}
	}
}
