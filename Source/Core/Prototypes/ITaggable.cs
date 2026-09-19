using RPGGame.Core.Generation;
using RPGGame.Core.Prototypes.Stats;
using RPGGame.Core.Stats;

namespace RPGGame.Core.Prototypes;

/// <summary>
/// Interface for any Proto that can have tags applied to it.
/// </summary>
public interface ITaggable {
	/// <summary>The tag IDs applied to this prototype.</summary>
	List<TagProto.ID> Tags { get; }
}

/// <summary>
/// Extension methods for ITaggable.
/// </summary>
public static class TaggableExtensions {
	extension(ITaggable taggable) {
		/// <summary>Checks if this has a specific tag.</summary>
		public bool HasTag(TagProto.ID tagId) {
			return taggable.Tags.Any(t => t == tagId);
		}
		/// <summary>Checks if this has a specific tag (by Proto.ID).</summary>
		public bool HasTag(Proto.ID tagId) {
			return taggable.Tags.Any(t => t.Value == tagId.Value);
		}
		/// <summary>Checks if this has any of the specified tags.</summary>
		public bool HasAnyTag(params TagProto.ID[] tagIds) {
			return taggable.Tags.Any(t => tagIds.Any(id => id == t));
		}
		/// <summary>Checks if this has all of the specified tags.</summary>
		public bool HasAllTags(params TagProto.ID[] tagIds) {
			return tagIds.All(id => taggable.Tags.Any(t => t == id));
		}
		/// <summary>Gets all tags that match a category prefix.</summary>
		public IEnumerable<TagProto.ID> GetTagsByPrefix(string prefix) {
			return taggable.Tags.Where(t => t.Value.StartsWith(prefix, StringComparison.Ordinal));
		}
		/// <summary>Checks if this has any tags with the given prefix.</summary>
		public bool HasTagWithPrefix(string prefix) {
			return taggable.Tags.Any(t => t.Value.StartsWith(prefix, StringComparison.Ordinal));
		}
		/// <summary>
		/// Collects all modifiers for a specific stat from all tags on this entity.
		/// Requires GameDb to look up TagProtos.
		/// </summary>
		public List<ValueModifier> GetTagModifiers(StatProto.ID statId, GameDb gameDb) {
			var modifiers = new List<ValueModifier>();

			foreach (var tagId in taggable.Tags) {
				if (gameDb.TryGetProto<TagProto>(tagId, out var tagProto)) {
					// Check if this tag has a modifier for the target stat
					Proto.ID targetId = statId;
					if (tagProto.Modifiers.TryGetValue(targetId, out var mod)) {
						modifiers.Add(mod);
					}
				}
			}

			return modifiers;
		}
		/// <summary>
		/// Gets all modifiers from all tags, grouped by stat.
		/// </summary>
		public Dictionary<Proto.ID, List<ValueModifier>> GetAllTagModifiers(GameDb gameDb) {
			var result = new Dictionary<Proto.ID, List<ValueModifier>>();

			foreach (var tagId in taggable.Tags) {
				if (gameDb.TryGetProto<TagProto>(tagId, out var tagProto)) {
					foreach (var (target, mod) in tagProto.Modifiers) {
						if (!result.ContainsKey(target)) {
							result[target] = [];
						}
						result[target].Add(mod);
					}
				}
			}

			return result;
		}
		/// <summary>
		/// Gets all modifiers for stats in a specific category.
		/// </summary>
		public List<(StatProto.ID StatId, ValueModifier Modifier)> GetTagModifiersForCategory(StatCategoryProto.ID categoryId,
			GameDb gameDb) {

			var result = new List<(StatProto.ID, ValueModifier)>();

			foreach (var tagId in taggable.Tags) {
				if (gameDb.TryGetProto<TagProto>(tagId, out var tagProto)) {
					foreach (var (target, mod) in tagProto.Modifiers) {
						// Try to get the stat proto to check its category
						var statId = new StatProto.ID(target.Value);
						if (gameDb.TryGetProto<StatProto>(statId, out var statProto)) {
							if (statProto.Category == categoryId) {
								result.Add((statId, mod));
							}
						}
					}
				}
			}

			return result;
		}
	}

}