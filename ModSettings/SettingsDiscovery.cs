using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;

namespace MagicMike.ModSettings;

internal sealed class ModInfo
{
	public string Name;

	public string Version;

	public ConfigFile Config;

	public List<SettingInfo> Settings;
}

internal sealed class SettingInfo
{
	public ConfigEntryBase Entry;

	public string Section;

	public string Name;

	public string Description;

	public bool Synced;

	// Jotunn's attributes object, kept so the read-only state can be re-checked as admin status changes
	public object Attributes;

	public bool IsReadOnly => SettingsDiscovery.GetField<bool?>(Attributes, "ReadOnly") == true;
}

internal static class SettingsDiscovery
{
	// Jotunn's BindConfigInOrder numbers sections ("2 - Chain Towers") to keep them in order
	private static readonly Regex s_sectionNumber = new Regex(@"^\s*(\d+)\s*-\s*");

	private static readonly Regex s_syncTag = new Regex(@"\s*\[(Not )?Synced with Server\]\s*$");

	public static List<ModInfo> FindMods()
	{
		List<ModInfo> mods = new List<ModInfo>();
		foreach (PluginInfo plugin in Chainloader.PluginInfos.Values)
		{
			if (plugin.Instance == null || !plugin.Metadata.GUID.StartsWith(ModSettings.GuidPrefix, StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			mods.Add(Describe(plugin.Metadata.Name, plugin.Metadata.Version.ToString(), plugin.Instance.Config));
		}
		mods.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase));
		if (ModSettings.Config != null)
		{
			mods.Add(Describe("Mod Settings Panel", ModSettings.Version.ToString(), ModSettings.Config));
		}
		return mods;
	}

	private static ModInfo Describe(string name, string version, ConfigFile config)
	{
		List<(SettingInfo setting, int section, int order, int index)> found = new List<(SettingInfo, int, int, int)>();
		int index = 0;
		foreach (KeyValuePair<ConfigDefinition, ConfigEntryBase> pair in config)
		{
			ConfigEntryBase entry = pair.Value;
			object attributes = entry.Description?.Tags?.FirstOrDefault(t => t != null && t.GetType().Name == "ConfigurationManagerAttributes");
			if (GetField<bool?>(attributes, "Browsable") == false)
			{
				continue;
			}
			Match match = s_sectionNumber.Match(entry.Definition.Section);
			SettingInfo setting = new SettingInfo
			{
				Entry = entry,
				Section = match.Success ? entry.Definition.Section.Substring(match.Length) : entry.Definition.Section,
				Name = GetField<string>(attributes, "DispName") ?? entry.Definition.Key,
				Description = s_syncTag.Replace(entry.Description?.Description ?? "", "").Trim(),
				Synced = GetField<bool>(attributes, "IsAdminOnly"),
				Attributes = attributes
			};
			int section = match.Success ? int.Parse(match.Groups[1].Value) : int.MaxValue;
			found.Add((setting, section, GetField<int?>(attributes, "Order") ?? 0, index++));
		}

		// Section number first, then ConfigurationManager's order (highest first), then the order they were bound in
		List<SettingInfo> settings = found
			.OrderBy(f => f.section)
			.ThenBy(f => f.setting.Section, StringComparer.OrdinalIgnoreCase)
			.ThenByDescending(f => f.order)
			.ThenBy(f => f.index)
			.Select(f => f.setting)
			.ToList();
		return new ModInfo { Name = name, Version = version, Config = config, Settings = settings };
	}

	// Read by name so any ConfigurationManagerAttributes class works, not just Jotunn's
	public static T GetField<T>(object target, string name)
	{
		if (target == null)
		{
			return default;
		}
		Type type = target.GetType();
		const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
		object value = type.GetField(name, flags)?.GetValue(target) ?? type.GetProperty(name, flags)?.GetValue(target);
		return value is T typed ? typed : default;
	}
}
