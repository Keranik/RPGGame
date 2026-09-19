using UnityEngine;

namespace RPGGame.UI.Styles;

#nullable disable

public class UserOverrideSettings {
	private readonly Dictionary<string, Color> k_colorOverrides = new();
	private readonly Dictionary<string, float> k_sizeOverrides = new();
	private readonly Dictionary<string, int> k_intOverrides = new();
	private readonly Dictionary<string, string> k_stringOverrides = new();

	public bool Enabled { get; set; } = true;

	public void SetColor(string key, Color value) {
		k_colorOverrides[key] = value;
		GameTheme.NotifyPropertyChanged($"Colors.{key}");
	}

	public bool TryGetColor(string key, out Color value) {
		if (Enabled && k_colorOverrides.TryGetValue(key, out value)) {
			return true;
		}
		value = default;
		return false;
	}

	public void ClearColor(string key) {
		k_colorOverrides.Remove(key);
		GameTheme.NotifyPropertyChanged($"Colors.{key}");
	}

	public Color GetColorOr(string key, Color defaultValue) {
		return TryGetColor(key, out var value) ? value : defaultValue;
	}

	public void SetSize(string key, float value) {
		k_sizeOverrides[key] = value;
		GameTheme.NotifyPropertyChanged($"Sizes.{key}");
	}

	public bool TryGetSize(string key, out float value) {
		if (Enabled && k_sizeOverrides.TryGetValue(key, out value)) {
			return true;
		}
		value = default;
		return false;
	}

	public float GetSizeOr(string key, float defaultValue) {
		return TryGetSize(key, out var value) ? value : defaultValue;
	}

	public void SetInt(string key, int value) {
		k_intOverrides[key] = value;
		GameTheme.NotifyPropertyChanged($"Ints.{key}");
	}

	public bool TryGetInt(string key, out int value) {
		if (Enabled && k_intOverrides.TryGetValue(key, out value)) {
			return true;
		}
		value = default;
		return false;
	}

	public int GetIntOr(string key, int defaultValue) {
		return TryGetInt(key, out var value) ? value : defaultValue;
	}

	public void SetString(string key, string value) {
		k_stringOverrides[key] = value;
		GameTheme.NotifyPropertyChanged($"Strings.{key}");
	}

	public bool TryGetString(string key, out string value) {
		if (Enabled && k_stringOverrides.TryGetValue(key, out value)) {
			return true;
		}
		value = default;
		return false;
	}

	public string GetStringOr(string key, string defaultValue) {
		return TryGetString(key, out var value) ? value : defaultValue;
	}

	public void ClearAll() {
		k_colorOverrides.Clear();
		k_sizeOverrides.Clear();
		k_intOverrides.Clear();
		k_stringOverrides.Clear();
		GameTheme.NotifyThemeChanged();
	}

	public Dictionary<string, object> ExportOverrides() {
		var result = new Dictionary<string, object>();
		foreach (var kvp in k_colorOverrides) {
			result[$"color:{kvp.Key}"] = ColorToHex(kvp.Value);
		}
		foreach (var kvp in k_sizeOverrides) {
			result[$"size:{kvp.Key}"] = kvp.Value;
		}
		foreach (var kvp in k_intOverrides) {
			result[$"int:{kvp.Key}"] = kvp.Value;
		}
		foreach (var kvp in k_stringOverrides) {
			result[$"string:{kvp.Key}"] = kvp.Value;
		}
		return result;
	}

	public void ImportOverrides(Dictionary<string, object> data) {
		ClearAll();
		foreach (var kvp in data) {
			string[] parts = kvp.Key.Split(':');
			if (parts.Length != 2) {
				continue;
			}
			string type = parts[0], key = parts[1];

			switch (type) {
				case "color" when kvp.Value is string hex && TryParseHexColor(hex, out var color): k_colorOverrides[key] = color; break;
				case "size" when kvp.Value is float f: k_sizeOverrides[key] = f; break;
				case "size" when kvp.Value is double d: k_sizeOverrides[key] = (float)d; break;
				case "int" when kvp.Value is int i: k_intOverrides[key] = i; break;
				case "int" when kvp.Value is long l: k_intOverrides[key] = (int)l; break;
				case "string" when kvp.Value is string s: k_stringOverrides[key] = s; break;
			}
		}
		GameTheme.NotifyThemeChanged();
	}

	private static string ColorToHex(Color c) => $"#{(int)(c.r * 255):X2}{(int)(c.g * 255):X2}{(int)(c.b * 255):X2}{(int)(c.a * 255):X2}";

	private static bool TryParseHexColor(string hex, out Color color) {
		color = Color.white;
		if (string.IsNullOrEmpty(hex)) {
			return false;
		}
		hex = hex.TrimStart('#');
		if (hex.Length == 6) {
			hex += "FF";
		}
		if (hex.Length != 8) {
			return false;
		}
		try {
			color = new Color(
				int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
				int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
				int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f,
				int.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber) / 255f);
			return true;
		} catch { return false; }
	}
}