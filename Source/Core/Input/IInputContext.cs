namespace RPGGame.Core.Input;

/// <summary>
/// Interface for input contexts that handle input for specific game phases.
/// </summary>
public interface IInputContext {
	/// <summary>
	/// Called when this context becomes active.
	/// </summary>
	void OnActivate();

	/// <summary>
	/// Called when this context is deactivated.
	/// </summary>
	void OnDeactivate();

	/// <summary>
	/// Called each frame to process input specific to this context.
	/// </summary>
	void ProcessInput(GameInputManager inputManager);
}