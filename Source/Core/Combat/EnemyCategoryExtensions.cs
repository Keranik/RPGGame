using System;
using System.Collections.Generic;
using System.Text;

namespace RPGGame.Core.Combat;

public static class EnemyCategoryExtensions {
	/// <summary>
	/// Gets the icon for this category.
	/// </summary>
	public static string GetIconName(this EnemyCategory category) {
		return $"icon_enemy_{category.ToString().ToLower()}";
	}
}
