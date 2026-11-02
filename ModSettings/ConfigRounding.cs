using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace MagicMike.ModSettings;

// Keeps a mod's decimal settings (float and double) tidy: rounded to a few decimal places and inside their range, however
// they were set. A slider or a typed value can leave long tails such as 0.30000001 in the config file; this rounds them as
// soon as the value changes, whatever changed it (Configuration Manager, the Mod Settings panel, the config file edited and
// reloaded, or code), and once when it is called for what was read from the file.
//
// It hooks the config file's own SettingChanged event rather than a custom range type, so the settings keep their ordinary
// AcceptableValueRange and still get sliders in every config editor.
//
// Call RoundAll(config) once the mod's settings are bound; call it again after binding more later (it hooks the file once).
// Settings that need a different number of decimals are set with Round(entry, decimals), e.g. 0 for a position in pixels.
internal static class ConfigRounding
{
	internal const int DefaultDecimals = 2;

	// The decimals for each config file this mod rounds, and for the settings given their own
	private static readonly Dictionary<ConfigFile, int> s_files = new Dictionary<ConfigFile, int>();
	private static readonly Dictionary<ConfigEntryBase, int> s_entries = new Dictionary<ConfigEntryBase, int>();

	// Set while a rounded value is being written back, so the change that write causes is not rounded again
	private static bool s_writing;

	/// <summary>Rounds every decimal setting in the config file to <paramref name="decimals"/> places, now and whenever one changes.</summary>
	internal static void RoundAll(ConfigFile config, int decimals = DefaultDecimals)
	{
		if (config == null)
		{
			return;
		}
		if (!s_files.ContainsKey(config))
		{
			config.SettingChanged += OnSettingChanged;
		}
		s_files[config] = Math.Max(0, decimals);

		bool changed = false;
		foreach (ConfigEntryBase entry in ((IDictionary<ConfigDefinition, ConfigEntryBase>)config).Values)
		{
			changed |= Normalize(entry);
		}
		SaveIfNeeded(config, changed);
	}

	/// <summary>Gives one setting its own number of decimal places (0 rounds it to whole numbers), and rounds it now.</summary>
	internal static void Round(ConfigEntryBase entry, int decimals)
	{
		if (entry == null)
		{
			return;
		}
		s_entries[entry] = Math.Max(0, decimals);
		SaveIfNeeded(entry.ConfigFile, Normalize(entry));
	}

	private static void OnSettingChanged(object sender, SettingChangedEventArgs args)
	{
		if (!s_writing && args?.ChangedSetting != null)
		{
			Normalize(args.ChangedSetting);
		}
	}

	// Rounds the setting if it is a decimal one; true when its value was changed
	private static bool Normalize(ConfigEntryBase entry)
	{
		if (entry == null || (entry.SettingType != typeof(float) && entry.SettingType != typeof(double)))
		{
			return false;
		}
		int decimals;
		if (!s_entries.TryGetValue(entry, out decimals) && (entry.ConfigFile == null || !s_files.TryGetValue(entry.ConfigFile, out decimals)))
		{
			return false;
		}

		double value = Convert.ToDouble(entry.BoxedValue);
		double rounded = double.IsNaN(value) || double.IsInfinity(value)
			? Convert.ToDouble(entry.DefaultValue)
			: Math.Round(value, Math.Min(decimals, 15), MidpointRounding.AwayFromZero);
		object boxed = entry.SettingType == typeof(float) ? (object)(float)rounded : rounded;

		// Rounded first, then kept in range, so a range edge with more decimals (0.001) can still be reached
		AcceptableValueBase acceptable = entry.Description?.AcceptableValues;
		if (acceptable != null)
		{
			boxed = acceptable.Clamp(boxed);
		}
		if (Equals(boxed, entry.BoxedValue))
		{
			return false;
		}

		s_writing = true;
		try
		{
			entry.BoxedValue = boxed;
		}
		finally
		{
			s_writing = false;
		}
		return true;
	}

	// The file is saved by BepInEx on each change unless the mod turned that off; then it is saved here
	private static void SaveIfNeeded(ConfigFile config, bool changed)
	{
		if (changed && config != null && !config.SaveOnConfigSet)
		{
			config.Save();
		}
	}
}
