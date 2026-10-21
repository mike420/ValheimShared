using System;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MagicMike.ModSettings;

// One setting in the list: its name, a control that suits its type, and a button to put it back to its default
internal sealed class SettingRow
{
	public const float RowHeight = 40f;

	private const float ResetSize = 28f;

	private const float NumberWidth = 76f;

	public readonly SettingInfo Setting;

	public readonly GameObject Root;

	private readonly SettingsPanel m_panel;

	private readonly Image m_background;

	private readonly TMP_Text m_label;

	private readonly Button m_reset;

	private Toggle m_toggle;

	private Slider m_slider;

	private TMP_InputField m_input;

	private TMP_Dropdown m_dropdown;

	private Button m_keyButton;

	private TMP_Text m_keyLabel;

	private object[] m_choices;

	private Color m_stripe = Theme.RowEven;

	private bool m_hovered;

	// Set while the row writes to its own controls, so their change events do not write the value back
	private bool m_refreshing;

	private ConfigEntryBase Entry => Setting.Entry;

	public SettingRow(SettingsPanel panel, Transform parent, SettingInfo setting)
	{
		m_panel = panel;
		Setting = setting;
		Root = Widgets.Row(parent, RowHeight);

		// The background doubles as the hover target across the whole row
		m_background = Root.AddComponent<Image>();
		m_background.color = m_stripe;
		HoverRelay hover = Root.AddComponent<HoverRelay>();
		hover.OnEnter = () => SetHovered(true);
		hover.OnExit = () => SetHovered(false);

		m_label = Widgets.Label(Root.transform, "", 20f, Theme.Text);
		m_label.enableAutoSizing = true;
		m_label.fontSizeMin = 14f;
		m_label.fontSizeMax = 20f;
		Widgets.Place(m_label.gameObject, new Vector2(0f, 0f), new Vector2(0.42f, 1f), new Vector2(12f, 2f), new Vector2(-8f, -2f));

		GameObject control = Widgets.Child(Root.transform, "Control");
		Widgets.Place(control, new Vector2(0.42f, 0f), new Vector2(1f, 1f), new Vector2(0f, 6f), new Vector2(-(ResetSize + 18f), -6f));
		BuildControl(control.transform);

		m_reset = Widgets.IconButton<ResetIcon>(Root.transform, 3f);
		Widgets.Place(m_reset.gameObject, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-(ResetSize + 8f), -ResetSize / 2f), new Vector2(-8f, ResetSize / 2f));
		m_reset.onClick.AddListener(() => SetValue(Entry.DefaultValue));

		Refresh();
	}

	public bool Matches(string search)
	{
		return string.IsNullOrEmpty(search)
			|| Contains(Setting.Name, search)
			|| Contains(Setting.Section, search)
			|| Contains(Setting.Description, search);
	}

	public bool IsDefault => Equals(Entry.BoxedValue, Entry.DefaultValue);

	// Alternate rows are shaded differently; set again after a search hides some of them
	public void SetStripe(bool odd)
	{
		m_stripe = odd ? Theme.RowOdd : Theme.RowEven;
		m_background.color = m_hovered ? Theme.RowHover : m_stripe;
	}

	private void SetHovered(bool hovered)
	{
		m_hovered = hovered;
		m_background.color = hovered ? Theme.RowHover : m_stripe;
		m_panel.ShowDescription(hovered ? this : null);
	}

	// Reads the setting's current value and lock state back into the controls
	public void Refresh()
	{
		bool locked = Setting.IsReadOnly;
		string tag = Setting.Synced ? "  <size=70%><color=" + Theme.Tag(Theme.Muted) + ">" + (locked ? "server, locked" : "server") + "</color></size>" : "";
		m_label.text = Setting.Name + tag;
		m_label.color = locked ? Theme.Muted : Theme.Text;
		m_reset.interactable = !locked && !IsDefault;
		m_reset.gameObject.SetActive(!IsDefault);

		m_refreshing = true;
		try
		{
			object value = Entry.BoxedValue;
			if (m_toggle != null)
			{
				m_toggle.SetIsOnWithoutNotify(value is bool on && on);
				m_toggle.interactable = !locked;
			}
			if (m_slider != null)
			{
				m_slider.SetValueWithoutNotify(Convert.ToSingle(value, CultureInfo.InvariantCulture));
				m_slider.interactable = !locked;
			}
			if (m_dropdown != null)
			{
				m_dropdown.SetValueWithoutNotify(Math.Max(0, Array.FindIndex(m_choices, c => Equals(c, value))));
				m_dropdown.interactable = !locked;
			}
			if (m_input != null && !m_input.isFocused)
			{
				m_input.SetTextWithoutNotify(Format(value));
				m_input.interactable = !locked;
			}
			if (m_keyButton != null)
			{
				m_keyLabel.text = m_panel.IsCapturing(this) ? "<color=" + Theme.Tag(Theme.AccentBright) + ">Press a key...</color>" : Format(value);
				m_keyButton.interactable = !locked;
			}
		}
		finally
		{
			m_refreshing = false;
		}
	}

	private void BuildControl(Transform parent)
	{
		Type type = Entry.SettingType;
		AcceptableValueBase acceptable = Entry.Description?.AcceptableValues;

		if (type == typeof(bool))
		{
			m_toggle = Widgets.Toggle(parent);
			Widgets.Place(m_toggle.gameObject, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(0f, -12f), new Vector2(24f, 12f));
			m_toggle.onValueChanged.AddListener(on => SetValue(on));
			return;
		}

		if (type == typeof(KeyCode) || type == typeof(KeyboardShortcut))
		{
			m_keyButton = Widgets.Button(parent, "", 15f);
			Widgets.Place(m_keyButton.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
			m_keyLabel = m_keyButton.GetComponentInChildren<TMP_Text>(includeInactive: true);
			m_keyButton.onClick.AddListener(() => m_panel.StartCapture(this));
			return;
		}

		// A fixed list of choices (an enum, or an AcceptableValueList) becomes a dropdown
		object[] choices = GetChoices(type, acceptable);
		if (choices != null)
		{
			m_choices = choices;
			m_dropdown = Widgets.Dropdown(parent, 15f);
			Widgets.Place(m_dropdown.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
			m_dropdown.options = choices.Select(c => new TMP_Dropdown.OptionData(Format(c))).ToList();
			m_dropdown.onValueChanged.AddListener(i => SetValue(m_choices[i]));
			return;
		}

		// A number with a range becomes a slider, with a box beside it for typing an exact value
		if (IsNumber(type) && TryGetRange(acceptable, out float min, out float max))
		{
			m_slider = Widgets.Slider(parent);
			Widgets.Place(m_slider.gameObject, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(2f, 2f), new Vector2(-(NumberWidth + 14f), -2f));
			m_slider.minValue = min;
			m_slider.maxValue = max;
			m_slider.wholeNumbers = IsWholeNumber(type);
			m_slider.onValueChanged.AddListener(v => SetValue(v));
		}

		m_input = Widgets.Input(parent, 15f);
		if (m_slider != null)
		{
			Widgets.Place(m_input.gameObject, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-NumberWidth, 0f), Vector2.zero);
		}
		else
		{
			Widgets.Place(m_input.gameObject, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
		}
		m_input.contentType = IsNumber(type) ? (IsWholeNumber(type) ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.DecimalNumber) : TMP_InputField.ContentType.Standard;
		m_input.onEndEdit.AddListener(OnInputSubmitted);
	}

	private void OnInputSubmitted(string text)
	{
		if (m_refreshing)
		{
			return;
		}
		try
		{
			SetValue(TomlTypeConverter.ConvertToValue(text.Replace(',', '.'), Entry.SettingType));
		}
		catch (Exception)
		{
			// Not a valid value for this setting, so put back what it was
			Refresh();
		}
	}

	public void SetValue(object value)
	{
		if (m_refreshing || Setting.IsReadOnly)
		{
			return;
		}
		try
		{
			if (value != null && value.GetType() != Entry.SettingType && IsNumber(Entry.SettingType))
			{
				value = Convert.ChangeType(value, Entry.SettingType, CultureInfo.InvariantCulture);
			}
			Entry.BoxedValue = value;
		}
		catch (Exception ex)
		{
			Jotunn.Logger.LogWarning("[ModSettings] Could not set " + Setting.Name + ": " + ex.Message);
		}
		Refresh();
	}

	private static string Format(object value)
	{
		switch (value)
		{
			case null:
				return "";
			case float f:
				return f.ToString("0.###", CultureInfo.InvariantCulture);
			case double d:
				return d.ToString("0.###", CultureInfo.InvariantCulture);
			case KeyboardShortcut shortcut:
				return shortcut.MainKey == KeyCode.None ? "None" : shortcut.ToString();
			default:
				return Convert.ToString(value, CultureInfo.InvariantCulture);
		}
	}

	private static object[] GetChoices(Type type, AcceptableValueBase acceptable)
	{
		if (acceptable != null && acceptable.GetType().IsGenericType && acceptable.GetType().GetGenericTypeDefinition() == typeof(AcceptableValueList<>))
		{
			Array values = acceptable.GetType().GetProperty("AcceptableValues")?.GetValue(acceptable) as Array;
			if (values != null && values.Length > 0)
			{
				return values.Cast<object>().ToArray();
			}
		}
		// Flags enums can hold combinations a dropdown cannot show, so they fall back to a text box
		if (type.IsEnum && !type.IsDefined(typeof(FlagsAttribute), inherit: false))
		{
			return Enum.GetValues(type).Cast<object>().Distinct().ToArray();
		}
		return null;
	}

	private static bool TryGetRange(AcceptableValueBase acceptable, out float min, out float max)
	{
		min = max = 0f;
		if (acceptable == null || !acceptable.GetType().IsGenericType || acceptable.GetType().GetGenericTypeDefinition() != typeof(AcceptableValueRange<>))
		{
			return false;
		}
		min = Convert.ToSingle(acceptable.GetType().GetProperty("MinValue").GetValue(acceptable), CultureInfo.InvariantCulture);
		max = Convert.ToSingle(acceptable.GetType().GetProperty("MaxValue").GetValue(acceptable), CultureInfo.InvariantCulture);
		return max > min;
	}

	private static bool IsNumber(Type type)
	{
		return IsWholeNumber(type) || type == typeof(float) || type == typeof(double) || type == typeof(decimal);
	}

	private static bool IsWholeNumber(Type type)
	{
		return type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) || type == typeof(uint) || type == typeof(ulong) || type == typeof(ushort) || type == typeof(sbyte);
	}

	private static bool Contains(string text, string search)
	{
		return text != null && text.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
	}
}
