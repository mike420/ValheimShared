using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace MagicMike.ModSettings;

// Shared settings panel for all of MagicMike's mods. Every mod compiles this folder in and calls Init from its
// plugin Awake; the copies agree through the AppDomain on which one runs the panel, so only one is ever shown,
// whichever of the mods are installed.
internal static class ModSettings
{
	// Bump this whenever the shared code changes, so the newest copy installed is the one that runs the panel
	internal const int Version = 32;

	internal const string GuidPrefix = "MagicMike.";

	private const string HostKey = "MagicMike.ModSettings.Host";

	private const string VersionKey = "MagicMike.ModSettings.Version";

	private static readonly string s_id = typeof(ModSettings).Assembly.GetName().Name;

	private static bool s_initialised;

	internal static ConfigFile Config { get; private set; }

	internal static ConfigEntry<KeyboardShortcut> ToggleKey { get; private set; }

	internal static ConfigEntry<float> ScrollSpeed { get; private set; }

	private static ConfigEntry<float> s_panelX;

	private static ConfigEntry<float> s_panelY;

	internal static Vector2 PanelPosition
	{
		get => s_panelX != null ? new Vector2(s_panelX.Value, s_panelY.Value) : Vector2.zero;
		set
		{
			if (s_panelX != null)
			{
				s_panelX.Value = value.x;
				s_panelY.Value = value.y;
			}
		}
	}

	internal static bool IsHost => AppDomain.CurrentDomain.GetData(HostKey) as string == s_id;

	public static void Init(BaseUnityPlugin plugin)
	{
		if (s_initialised)
		{
			return;
		}
		s_initialised = true;

		// The newest version claims the panel; on a tie the first to load keeps it
		AppDomain domain = AppDomain.CurrentDomain;
		if (!(domain.GetData(VersionKey) is int hostVersion) || Version > hostVersion)
		{
			domain.SetData(VersionKey, Version);
			domain.SetData(HostKey, s_id);
		}

		// Every copy adds its driver; by the driver's Start all plugins have loaded, so the losers remove themselves
		plugin.gameObject.AddComponent<ModSettingsDriver>();
	}

	// Called once by the copy that won, after every plugin has loaded
	internal static void StartHost()
	{
		Config = new ConfigFile(Path.Combine(Paths.ConfigPath, "MagicMike.ModSettings.cfg"), saveOnInit: true);
		ToggleKey = Config.Bind("General", "Toggle Key", new KeyboardShortcut(KeyCode.F7), "The key that opens and closes the Mod Settings panel.");
		ScrollSpeed = Config.Bind("General", "Scroll Speed", 3f, new ConfigDescription("How many setting rows the list moves for each notch of the mouse wheel.", new AcceptableValueRange<float>(1f, 10f)));
		// Hidden from the panel's own list; set by dragging the panel. Two numbers, as BepInEx cannot store a Vector2.
		s_panelX = Config.Bind("General", "Panel Position X", 0f, new ConfigDescription("Where the panel was last dragged to, across from the centre of the screen.", null, new ConfigurationManagerAttributes { Browsable = false }));
		s_panelY = Config.Bind("General", "Panel Position Y", 0f, new ConfigDescription("Where the panel was last dragged to, up from the centre of the screen.", null, new ConfigurationManagerAttributes { Browsable = false }));

		// Patched by hand rather than with attributes, so the mods' own PatchAll calls never pick it up
		Harmony harmony = new Harmony("MagicMike.ModSettings");
		harmony.Patch(AccessTools.Method(typeof(Menu), "Update"), prefix: new HarmonyMethod(typeof(ModSettings), nameof(MenuUpdatePrefix)));
		harmony.Patch(AccessTools.Method(typeof(ZInput), nameof(ZInput.GetMouseScrollWheel)), postfix: new HarmonyMethod(typeof(ModSettings), nameof(MouseScrollPostfix)));
	}

	// Escape closes the panel instead of opening the pause menu behind it
	private static bool MenuUpdatePrefix()
	{
		return !SettingsPanel.IsOpen;
	}

	// The mouse wheel scrolls the panel's list, not the game behind it (the camera's zoom, the build menu, the hotbar): BlockInput
	// leaves the wheel to the game. The list scrolls through Unity's own input, which this leaves alone.
	private static void MouseScrollPostfix(ref float __result)
	{
		if (SettingsPanel.IsOpen)
		{
			__result = 0f;
		}
	}
}
