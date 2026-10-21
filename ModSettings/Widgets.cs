using System;
using System.Linq;
using System.Reflection;
using Jotunn.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MagicMike.ModSettings;

internal static class Theme
{
	public static readonly Color Dim = new Color(0f, 0f, 0f, 0.68f);

	// Behind the Reset All confirmation, over the panel
	public static readonly Color DimConfirm = new Color(0f, 0f, 0f, 0.44f);

	public static readonly Color Panel = new Color(0.2f, 0.135f, 0.105f, 0.99f);

	public static readonly Color PanelOutline = new Color(0.62f, 0.34f, 0.11f, 1f);

	public static readonly Color Header = new Color(0.29f, 0.17f, 0.09f, 1f);

	public static readonly Color Field = new Color(0.09f, 0.07f, 0.06f, 0.92f);

	public static readonly Color FieldHover = new Color(0.15f, 0.11f, 0.08f, 0.95f);

	public static readonly Color FieldOutline = new Color(0.42f, 0.27f, 0.14f, 1f);

	public static readonly Color Button = new Color(0.16f, 0.13f, 0.11f, 0.98f);

	public static readonly Color ButtonHover = new Color(0.3f, 0.2f, 0.12f, 1f);

	public static readonly Color ButtonPressed = new Color(0.42f, 0.27f, 0.12f, 1f);

	public static readonly Color ButtonDisabled = new Color(0.13f, 0.11f, 0.1f, 0.6f);

	public static readonly Color ButtonOutline = new Color(0.55f, 0.32f, 0.12f, 0.9f);

	public static readonly Color Selected = new Color(0.34f, 0.22f, 0.12f, 1f);

	public static readonly Color Track = new Color(0.12f, 0.1f, 0.1f, 0.95f);

	public static readonly Color ScrollHandle = new Color(0.72f, 0.4f, 0.13f, 1f);

	public static readonly Color SliderHandleOutline = new Color(0f, 0f, 0f, 0.6f);

	public static readonly Color ListBackground = new Color(0f, 0f, 0f, 0.1f);

	public static readonly Color Accent = new Color(0.95f, 0.62f, 0.17f, 1f);

	public static readonly Color AccentBright = new Color(1f, 0.72f, 0.28f, 1f);

	public static readonly Color Text = new Color(0.92f, 0.78f, 0.52f, 1f);

	public static readonly Color TextBright = new Color(1f, 0.64f, 0.12f, 1f);

	public static readonly Color Muted = new Color(0.62f, 0.54f, 0.44f, 1f);

	public static readonly Color Caret = new Color(1f, 0.78f, 0.28f, 1f);

	public static readonly Color TextSelection = new Color(0.82f, 0.48f, 0.12f, 0.52f);

	public static readonly Color RowEven = new Color(0f, 0f, 0f, 0.14f);

	public static readonly Color RowOdd = new Color(0f, 0f, 0f, 0.05f);

	public static readonly Color RowHover = new Color(0.62f, 0.34f, 0.11f, 0.16f);

	// For colouring part of a line of text: "<color=" + Theme.Tag(Theme.Muted) + ">...</color>"
	public static string Tag(Color color)
	{
		return "#" + ColorUtility.ToHtmlStringRGBA(color);
	}

	public static ColorBlock Colors(Color normal, Color hover, Color pressed, Color disabled)
	{
		ColorBlock colors = ColorBlock.defaultColorBlock;
		colors.normalColor = normal;
		colors.highlightedColor = hover;
		colors.pressedColor = pressed;
		colors.selectedColor = normal;
		colors.disabledColor = disabled;
		colors.colorMultiplier = 1f;
		colors.fadeDuration = 0.08f;
		return colors;
	}
}

// Builds the panel's controls from plain shapes in a flat Valheim palette, with Valheim's TextMeshPro font
internal static class Widgets
{
	private static TMP_FontAsset s_font;

	public static TMP_FontAsset Font
	{
		get
		{
			if (s_font == null)
			{
				s_font = GUIManager.Instance.TMP_AveriaSansLibre;
			}
			if (s_font == null)
			{
				s_font = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.name.Contains("Averia"));
			}
			if (s_font == null)
			{
				s_font = TMP_Settings.defaultFontAsset;
			}
			return s_font;
		}
	}

	public static RectTransform Place(GameObject go, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
	{
		RectTransform rect = (RectTransform)go.transform;
		rect.anchorMin = anchorMin;
		rect.anchorMax = anchorMax;
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.offsetMin = offsetMin;
		rect.offsetMax = offsetMax;
		return rect;
	}

	public static void Fill(GameObject go, float inset = 0f)
	{
		Place(go, Vector2.zero, Vector2.one, new Vector2(inset, inset), new Vector2(-inset, -inset));
	}

	public static GameObject Child(Transform parent, string name)
	{
		GameObject go = new GameObject(name, typeof(RectTransform));
		go.transform.SetParent(parent, worldPositionStays: false);
		return go;
	}

	public static Image Box(Transform parent, string name, Color color, Color? outline = null, float outlineWidth = 1f)
	{
		GameObject go = Child(parent, name);
		Image image = go.AddComponent<Image>();
		image.color = color;
		if (outline.HasValue)
		{
			Outline line = go.AddComponent<Outline>();
			line.effectColor = outline.Value;
			line.effectDistance = new Vector2(outlineWidth, -outlineWidth);
		}
		return image;
	}

	public static TMP_Text Label(Transform parent, string text, float size, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft, bool bold = false)
	{
		// Built switched off: TextMeshPro looks for a font as soon as it is switched on, and Valheim does not
		// have its default one, so it would log a "LiberationSans SDF" warning before ours was set
		GameObject go = Child(parent, "Text");
		go.SetActive(false);
		TextMeshProUGUI label = go.AddComponent<TextMeshProUGUI>();
		label.font = Font;
		label.fontSize = size;
		label.color = color;
		label.alignment = alignment;
		label.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
		label.richText = true;
		label.textWrappingMode = TextWrappingModes.NoWrap;
		label.overflowMode = TextOverflowModes.Ellipsis;
		label.raycastTarget = false;
		label.text = text;
		go.SetActive(true);
		return label;
	}

	public static Button Button(Transform parent, string text, float fontSize = 16f)
	{
		Image background = Box(parent, "Button", Color.white, Theme.ButtonOutline);
		Button button = background.gameObject.AddComponent<Button>();
		button.targetGraphic = background;
		button.colors = Theme.Colors(Theme.Button, Theme.ButtonHover, Theme.ButtonPressed, Theme.ButtonDisabled);
		AddClickSound(button.gameObject);

		TMP_Text label = Label(background.transform, text, fontSize, Theme.Text, TextAlignmentOptions.Center);
		Fill(label.gameObject, 4f);
		return button;
	}

	// A button showing a drawn icon rather than text, so it does not depend on the font having the character
	public static Button IconButton<T>(Transform parent, float inset) where T : MaskableGraphic
	{
		Button button = Button(parent, "", 14f);
		GameObject icon = Child(button.transform, typeof(T).Name);
		icon.AddComponent<CanvasRenderer>();
		T graphic = icon.AddComponent<T>();
		graphic.color = Theme.Accent;
		graphic.raycastTarget = false;
		Fill(icon, inset);
		return button;
	}

	public static Toggle Toggle(Transform parent)
	{
		Image box = Box(parent, "Toggle", Color.white, Theme.FieldOutline);
		Toggle toggle = box.gameObject.AddComponent<Toggle>();
		toggle.targetGraphic = box;
		toggle.colors = Theme.Colors(Theme.Field, Theme.FieldHover, Theme.ButtonPressed, Theme.ButtonDisabled);
		Image check = Box(box.transform, "Check", Theme.Accent);
		Fill(check.gameObject, 5f);
		check.raycastTarget = false;
		toggle.graphic = check;
		AddClickSound(toggle.gameObject);
		return toggle;
	}

	public static Slider Slider(Transform parent)
	{
		GameObject root = Child(parent, "Slider");
		Slider slider = root.AddComponent<Slider>();

		Image track = Box(root.transform, "Track", Theme.Track, Theme.FieldOutline);
		Place(track.gameObject, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, -4f), new Vector2(0f, 4f));

		GameObject fillArea = Child(root.transform, "Fill Area");
		Place(fillArea, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, -4f), new Vector2(0f, 4f));
		Image fill = Box(fillArea.transform, "Fill", Theme.Accent);
		Fill(fill.gameObject);
		fill.raycastTarget = false;

		GameObject handleArea = Child(root.transform, "Handle Slide Area");
		Place(handleArea, Vector2.zero, Vector2.one, new Vector2(6f, 3f), new Vector2(-6f, -3f));
		Image handle = Box(handleArea.transform, "Handle", Color.white, Theme.SliderHandleOutline);
		RectTransform handleRect = (RectTransform)handle.transform;
		handleRect.anchorMin = new Vector2(0f, 0f);
		handleRect.anchorMax = new Vector2(0f, 1f);
		handleRect.sizeDelta = new Vector2(12f, 0f);

		slider.fillRect = (RectTransform)fill.transform;
		slider.handleRect = handleRect;
		slider.targetGraphic = handle;
		slider.colors = Theme.Colors(Theme.Accent, Theme.AccentBright, Theme.AccentBright, Theme.Muted);
		slider.direction = UnityEngine.UI.Slider.Direction.LeftToRight;
		return slider;
	}

	public static TMP_InputField Input(Transform parent, float fontSize, string placeholder = "")
	{
		Image background = Box(parent, "Input", Color.white, Theme.FieldOutline);
		TMP_InputField input = background.gameObject.AddComponent<TMP_InputField>();
		input.targetGraphic = background;
		input.colors = Theme.Colors(Theme.Field, Theme.FieldHover, Theme.FieldHover, Theme.ButtonDisabled);

		GameObject area = Child(background.transform, "Text Area");
		area.AddComponent<RectMask2D>();
		Place(area, Vector2.zero, Vector2.one, new Vector2(8f, 2f), new Vector2(-8f, -2f));

		TMP_Text text = Label(area.transform, "", fontSize, Theme.Text);
		Fill(text.gameObject);
		text.overflowMode = TextOverflowModes.Overflow;
		TMP_Text hint = Label(area.transform, placeholder, fontSize, Theme.Muted);
		Fill(hint.gameObject);
		hint.fontStyle = FontStyles.Italic;

		input.textViewport = (RectTransform)area.transform;
		input.textComponent = text;
		input.placeholder = hint;
		input.fontAsset = Font;
		input.pointSize = fontSize;
		input.richText = false;
		input.lineType = TMP_InputField.LineType.SingleLine;
		input.customCaretColor = true;
		input.caretColor = Theme.Caret;
		input.caretWidth = 2;
		input.selectionColor = Theme.TextSelection;
		input.restoreOriginalTextOnEscape = true;
		return input;
	}

	public static TMP_Dropdown Dropdown(Transform parent, float fontSize)
	{
		GameObject go = WithDefaultFont(() => TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources()));
		go.name = "Dropdown";
		go.transform.SetParent(parent, worldPositionStays: false);
		TMP_Dropdown dropdown = go.GetComponent<TMP_Dropdown>();

		// The default dropdown is drawn with Unity's stock sprites; repaint it flat
		Image background = go.GetComponent<Image>();
		Flatten(background, Color.white);
		AddOutline(go, Theme.FieldOutline);
		dropdown.colors = Theme.Colors(Theme.Field, Theme.FieldHover, Theme.FieldHover, Theme.ButtonDisabled);

		Transform arrow = go.transform.Find("Arrow");
		if (arrow != null)
		{
			UnityEngine.Object.DestroyImmediate(arrow.GetComponent<Image>());
			arrow.gameObject.AddComponent<CanvasRenderer>();
			DownArrowIcon icon = arrow.gameObject.AddComponent<DownArrowIcon>();
			icon.color = Theme.Accent;
			icon.raycastTarget = false;
			Place(arrow.gameObject, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-22f, -4f), new Vector2(-10f, 4f));
		}

		RectTransform template = dropdown.template;
		if (template != null)
		{
			// The default list moves 1 pixel per wheel notch; about three options is more usable
			ScrollRect list = template.GetComponent<ScrollRect>();
			if (list != null)
			{
				list.scrollSensitivity = 60f;
			}
			Flatten(template.GetComponent<Image>(), Theme.Panel);
			AddOutline(template.gameObject, Theme.PanelOutline);
			// The default list clips its options with a stencil Mask, which Valheim's text and flat images do not draw through, so
			// the list opened as an empty box. It clips with a RectMask2D instead, as the panel's own list does.
			Transform viewportTransform = template.Find("Viewport");
			Mask stencil = viewportTransform != null ? viewportTransform.GetComponent<Mask>() : null;
			if (stencil != null)
			{
				UnityEngine.Object.DestroyImmediate(stencil);
				viewportTransform.gameObject.AddComponent<RectMask2D>();
			}
			Image viewport = viewportTransform != null ? viewportTransform.GetComponent<Image>() : null;
			if (viewport != null)
			{
				Flatten(viewport, Color.clear);   // (the Mask hid it; now it would show, so it is see-through)
			}
			Scrollbar scrollbar = template.GetComponentInChildren<Scrollbar>(includeInactive: true);
			if (scrollbar != null)
			{
				StyleScrollbar(scrollbar);
			}
			Toggle item = template.GetComponentInChildren<Toggle>(includeInactive: true);
			if (item != null)
			{
				Image itemBackground = item.targetGraphic as Image;
				Flatten(itemBackground, Color.white);
				item.colors = Theme.Colors(Theme.Panel, Theme.Selected, Theme.ButtonPressed, Theme.ButtonDisabled);
				if (item.graphic is Image checkmark)
				{
					Flatten(checkmark, Theme.Accent);
					((RectTransform)checkmark.transform).sizeDelta = new Vector2(8f, 8f);
				}
			}
		}

		foreach (TMP_Text text in go.GetComponentsInChildren<TMP_Text>(includeInactive: true))
		{
			text.font = Font;
			text.fontSize = fontSize;
			text.color = Theme.Text;
			text.textWrappingMode = TextWrappingModes.NoWrap;
			text.overflowMode = TextOverflowModes.Ellipsis;
			text.alignment = TextAlignmentOptions.MidlineLeft;
		}

		// The default sizes leave the texts about 15 pixels tall: too short for a line of Valheim's font, and a line that does not fit
		// is cut off whole, so the options showed as an empty list. The texts get the full height of their box, and the options
		// are made taller.
		const float itemHeight = 28f;
		if (dropdown.captionText != null)
		{
			RectTransform caption = dropdown.captionText.rectTransform;
			caption.offsetMin = new Vector2(10f, 0f);
			caption.offsetMax = new Vector2(-25f, 0f);
		}
		if (dropdown.itemText != null)
		{
			RectTransform label = dropdown.itemText.rectTransform;
			label.offsetMin = new Vector2(22f, 0f);
			label.offsetMax = new Vector2(-8f, 0f);
			RectTransform item = label.parent as RectTransform;
			if (item != null)
			{
				item.sizeDelta = new Vector2(item.sizeDelta.x, itemHeight);
				if (item.parent is RectTransform content)
				{
					content.sizeDelta = new Vector2(content.sizeDelta.x, itemHeight + 8f);
				}
			}
		}
		if (template != null)
		{
			template.sizeDelta = new Vector2(template.sizeDelta.x, itemHeight * 7f + 8f);   // (up to seven options before it scrolls)
		}
		return dropdown;
	}

	public static ScrollRect ScrollView(Transform parent)
	{
		GameObject root = Child(parent, "Scroll View");
		ScrollRect scroll = root.AddComponent<ScrollRect>();

		Image viewport = Box(root.transform, "Viewport", Theme.ListBackground);
		viewport.gameObject.AddComponent<RectMask2D>();
		Place(viewport.gameObject, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(-16f, 0f));

		GameObject content = Child(viewport.transform, "Content");
		RectTransform contentRect = (RectTransform)content.transform;
		contentRect.anchorMin = new Vector2(0f, 1f);
		contentRect.anchorMax = new Vector2(1f, 1f);
		contentRect.pivot = new Vector2(0.5f, 1f);
		contentRect.offsetMin = Vector2.zero;
		contentRect.offsetMax = Vector2.zero;
		VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
		layout.childControlWidth = true;
		layout.childControlHeight = true;
		layout.childForceExpandWidth = true;
		layout.childForceExpandHeight = false;
		layout.spacing = 2f;
		layout.padding = new RectOffset(2, 2, 2, 2);
		content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

		Image track = Box(root.transform, "Scrollbar", Color.white);
		Place(track.gameObject, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(-12f, 0f), Vector2.zero);
		Scrollbar scrollbar = track.gameObject.AddComponent<Scrollbar>();
		GameObject slidingArea = Child(track.transform, "Sliding Area");
		Fill(slidingArea, 2f);
		Image handle = Box(slidingArea.transform, "Handle", Color.white);
		Fill(handle.gameObject);
		scrollbar.handleRect = (RectTransform)handle.transform;
		scrollbar.targetGraphic = handle;
		scrollbar.direction = Scrollbar.Direction.BottomToTop;
		StyleScrollbar(scrollbar);

		scroll.viewport = (RectTransform)viewport.transform;
		scroll.content = contentRect;
		scroll.horizontal = false;
		scroll.vertical = true;
		scroll.movementType = ScrollRect.MovementType.Clamped;
		scroll.inertia = false;
		scroll.scrollSensitivity = 40f;
		scroll.verticalScrollbar = scrollbar;
		scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
		scroll.verticalScrollbarSpacing = 4f;
		return scroll;
	}

	public static GameObject Row(Transform parent, float height)
	{
		GameObject row = Child(parent, "Row");
		LayoutElement layout = row.AddComponent<LayoutElement>();
		layout.minHeight = height;
		layout.preferredHeight = height;
		return row;
	}

	private static void StyleScrollbar(Scrollbar scrollbar)
	{
		Flatten(scrollbar.GetComponent<Image>(), Theme.Track);
		if (scrollbar.targetGraphic is Image handle)
		{
			Flatten(handle, Color.white);
		}
		scrollbar.colors = Theme.Colors(Theme.ScrollHandle, Theme.Accent, Theme.AccentBright, Theme.Muted);
	}

	// Unity's dropdown builder creates its text already switched on, so TextMeshPro looks for its default font
	// (which Valheim does not have) and logs a warning. Valheim's font is made the default just while it runs.
	private static T WithDefaultFont<T>(Func<T> create)
	{
		FieldInfo field = typeof(TMP_Settings).GetField("m_defaultFontAsset", BindingFlags.NonPublic | BindingFlags.Instance);
		TMP_Settings settings = TMP_Settings.instance;
		if (field == null || settings == null || Font == null)
		{
			return create();
		}
		object previous = field.GetValue(settings);
		field.SetValue(settings, Font);
		try
		{
			return create();
		}
		finally
		{
			field.SetValue(settings, previous);
		}
	}

	private static void Flatten(Image image, Color color)
	{
		if (image == null)
		{
			return;
		}
		image.sprite = null;
		image.type = Image.Type.Simple;
		image.color = color;
	}

	private static void AddOutline(GameObject go, Color color)
	{
		Outline line = go.GetComponent<Outline>();
		if (line == null)
		{
			line = go.AddComponent<Outline>();
		}
		line.effectColor = color;
		line.effectDistance = new Vector2(1f, -1f);
	}

	// Valheim's own click sound; only the click, as a mouse click also selects and a select sound would double it up
	private static void AddClickSound(GameObject go)
	{
		ButtonSfx sfx = go.GetComponent<ButtonSfx>();
		if (sfx == null)
		{
			sfx = go.AddComponent<ButtonSfx>();
		}
		sfx.m_sfxPrefab = PrefabManager.Cache.GetPrefab<GameObject>("sfx_gui_button");
		sfx.m_selectSfxPrefab = null;
	}
}

// Reports pointer enter and exit only. An EventTrigger would also swallow scroll-wheel events and stop the list scrolling.
internal sealed class HoverRelay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public System.Action OnEnter;

	public System.Action OnExit;

	public void OnPointerEnter(PointerEventData eventData)
	{
		OnEnter?.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		OnExit?.Invoke();
	}
}

// Lets the panel be moved by dragging any part of it that does not handle dragging itself, kept on screen
internal sealed class DragMove : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
	public RectTransform Target;

	public System.Action<Vector2> OnMoved;

	private Canvas m_canvas;

	public void OnBeginDrag(PointerEventData eventData)
	{
		m_canvas = GetComponentInParent<Canvas>()?.rootCanvas;
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (Target == null || eventData.button != PointerEventData.InputButton.Left)
		{
			return;
		}
		float scale = m_canvas != null ? m_canvas.scaleFactor : 1f;
		Target.anchoredPosition = Clamp(Target.anchoredPosition + eventData.delta / scale);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (Target != null)
		{
			OnMoved?.Invoke(Target.anchoredPosition);
		}
	}

	// Keeps at least part of the panel's top inside the screen, so it can always be grabbed again
	public Vector2 Clamp(Vector2 position)
	{
		RectTransform parent = Target.parent as RectTransform;
		if (parent == null)
		{
			return position;
		}
		Vector2 half = parent.rect.size / 2f;
		Vector2 size = Target.rect.size;
		const float keep = 80f;
		position.x = Mathf.Clamp(position.x, -half.x - size.x / 2f + keep, half.x + size.x / 2f - keep);
		position.y = Mathf.Clamp(position.y, -half.y + size.y / 2f - size.y + keep, half.y - size.y / 2f);
		return position;
	}
}
