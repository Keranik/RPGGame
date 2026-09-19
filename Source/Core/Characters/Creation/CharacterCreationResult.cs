using System.Collections.Generic;

namespace RPGGame.Core.Characters.Creation;

/// <summary>
/// Result of character creation, containing the final state or validation errors.
/// </summary>
public class CharacterCreationResult {
	/// <summary>Whether character creation was successful.</summary>
	public bool Success { get; init; }

	/// <summary>The final character creation state (null if failed).</summary>
	public CharacterCreationState? State { get; init; }

	/// <summary>Validation errors if creation failed.</summary>
	public List<string> Errors { get; init; } = [];

	/// <summary>Whether this character was created via random reroll (gets hidden bonus).</summary>
	public bool WasRandomReroll { get; init; }
}