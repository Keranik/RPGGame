using RPGGame.Core.Characters;
using RPGGame.Core.Combat;
using RPGGame.Core.Simulation;
using UnityEngine;

namespace RPGGame.Core.Input;

/// <summary>
/// Input context for combat phase.
/// Handles target selection, ability usage, etc.
/// </summary>
public class CombatInputContext : IInputContext {
	private readonly CombatManager k_combatManager;
	private readonly GameStateManager k_stateManager;

	private bool k_isSubscribed;

	// Pending action state
	private CombatActionType? k_pendingActionType;
	private CombatAbility? k_pendingAbility;

	public CombatInputContext(CombatManager combatManager, GameStateManager stateManager) {
		k_combatManager = combatManager;
		k_stateManager = stateManager;
	}

	public void OnActivate() {
		if (k_isSubscribed) return;

		var inputManager = GameInputManager.Instance;
		if (inputManager != null) {
			inputManager.OnPrimaryAction += OnPrimaryAction;
			inputManager.OnSecondaryAction += OnSecondaryAction;
			inputManager.OnCancel += OnCancel;
			inputManager.OnConfirm += OnConfirm;
			k_isSubscribed = true;
		}

		// Reset pending state
		k_pendingActionType = null;
		k_pendingAbility = null;

		Debug.Log("CombatInputContext: Activated");
	}

	public void OnDeactivate() {
		if (!k_isSubscribed) return;

		var inputManager = GameInputManager.Instance;
		if (inputManager != null) {
			inputManager.OnPrimaryAction -= OnPrimaryAction;
			inputManager.OnSecondaryAction -= OnSecondaryAction;
			inputManager.OnCancel -= OnCancel;
			inputManager.OnConfirm -= OnConfirm;
			k_isSubscribed = false;
		}

		Debug.Log("CombatInputContext: Deactivated");
	}

	public void ProcessInput(GameInputManager inputManager) {
		// Could handle number key presses for ability slots
	}

	private void OnPrimaryAction(Vector2 screenPosition) {
		// Select target or confirm action
		// TODO: Raycast to find character under cursor
		Debug.Log($"CombatInputContext: Primary action at {screenPosition}");
	}

	private void OnSecondaryAction(Vector2 screenPosition) {
		// Cancel current selection
		CancelPendingAction();
	}

	private void OnConfirm() {
		// Confirm current action if waiting on player
		if (k_combatManager.Phase == CombatPhase.PlayerTurn) {
			// If we have a pending action, execute it
			// Otherwise, execute basic attack on first enemy
			if (k_pendingActionType == null) {
				var target = k_combatManager.LivingEnemies.FirstOrDefault();
				var player = k_combatManager.Player;
				if (target != null && player != null) {
					var action = CombatAction.Attack(player, target);
					k_combatManager.ExecuteAction(action);
				}
			}
		} else if (k_combatManager.Phase == CombatPhase.ShowingResults) {
			// Continue after showing results
			k_combatManager.ContinueAfterAction();
		}
	}

	private void OnCancel() {
		// Cancel current action or try to flee
		if (k_pendingActionType != null) {
			CancelPendingAction();
		}
		// Note: Fleeing should be done through the UI, not just pressing cancel
	}

	/// <summary>
	/// Sets a pending action type that needs a target.
	/// </summary>
	public void SetPendingAction(CombatActionType actionType, CombatAbility? ability = null) {
		k_pendingActionType = actionType;
		k_pendingAbility = ability;
		Debug.Log($"CombatInputContext: Pending action set to {actionType}");
	}

	/// <summary>
	/// Cancels the pending action.
	/// </summary>
	public void CancelPendingAction() {
		k_pendingActionType = null;
		k_pendingAbility = null;
		Debug.Log("CombatInputContext: Pending action cancelled");
	}

	/// <summary>
	/// Selects a target for the pending action.
	/// </summary>
	public void SelectTarget(LiveCharacter target) {
		if (k_pendingActionType == null) return;

		var player = k_combatManager.Player;
		if (player == null) return;

		CombatAction? action = k_pendingActionType switch {
			CombatActionType.Attack => CombatAction.Attack(player, target),
			CombatActionType.HeavyAttack => CombatAction.HeavyAttack(player, target),
			CombatActionType.QuickAttack => CombatAction.QuickAttack(player, target),
			CombatActionType.UseAbility when k_pendingAbility != null =>
				CombatAction.UseAbility(player, k_pendingAbility, target),
			CombatActionType.CastSpell when k_pendingAbility != null =>
				CombatAction.CastSpell(player, k_pendingAbility, target),
			_ => null
		};

		if (action != null) {
			k_combatManager.ExecuteAction(action);
		}

		CancelPendingAction();
	}
}