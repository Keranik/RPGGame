using RPGGame.Core.Expedition;
using RPGGame.Core.Prototypes.Events;
using RPGGame.Core.Prototypes.Expedition;
using RPGGame.Core.Simulation;

namespace RPGGame.Core.Events;

/// <summary>
/// Runtime instance of an event created from EventProto.
/// EventProto defines the static data, GameEvent holds active event state.
/// </summary>
public class GameEvent {
	#region Identity

	public string Id { get; init; } = "";
	public string Title { get; init; } = "";
	public string Description { get; init; } = "";
	public string? FlavorText { get; init; }
	public string IconName { get; init; } = "icon_event";
	public string? BackgroundImage { get; init; }

	#endregion

	#region Classification

	public EventType Type { get; init; } = EventType.Choice;
	public EventRarity Rarity { get; init; } = EventRarity.Common;
	public List<string> Tags { get; init; } = [];

	#endregion

	#region Spawn Rules

	public List<EventCondition> SpawnConditions { get; init; } = [];
	public List<TerrainProto.ID> ValidTerrains { get; init; } = [];
	public float MinDistance { get; init; }
	public float MaxDistance { get; init; } = -1;
	public float SpawnWeight { get; init; } = 10f;
	public bool IsRepeatable { get; init; } = true;
	public int RepeatCooldown { get; init; }

	#endregion

	#region Behavior

	public bool PausesTravel { get; init; } = true;
	public bool CanSkip { get; init; } = true;
	public bool AutoResolve { get; init; }
	public float TimeLimit { get; init; }

	#endregion

	#region Content

	public List<EventChoiceProto> Choices { get; init; } = [];
	public List<OutcomeEffect> OnStartEffects { get; init; } = [];
	public List<OutcomeEffect> OnEndEffects { get; init; } = [];

	#endregion

	#region Audio/Visual

	public string? MusicId { get; init; }
	public string? AmbientId { get; init; }
	public string? StartSfxId { get; init; }

	#endregion

	#region Factory

	/// <summary>
	/// Creates a GameEvent from an EventProto.
	/// </summary>
	public static GameEvent FromProto(EventProto proto) {
		return new GameEvent {
			Id = proto.Id.Value,
			Title = proto.Title,
			Description = proto.Description,
			FlavorText = proto.FlavorText,
			IconName = proto.IconName,
			BackgroundImage = proto.BackgroundImage,
			Type = proto.Type,
			Rarity = proto.Rarity,
			Tags = [.. proto.Tags],
			SpawnConditions = [.. proto.SpawnConditions],
			ValidTerrains = [.. proto.ValidTerrains],
			MinDistance = proto.MinDistance,
			MaxDistance = proto.MaxDistance,
			SpawnWeight = proto.SpawnWeight,
			IsRepeatable = proto.IsRepeatable,
			RepeatCooldown = proto.RepeatCooldown,
			PausesTravel = proto.PausesTravel,
			CanSkip = proto.CanSkip,
			AutoResolve = proto.AutoResolve,
			TimeLimit = proto.TimeLimit,
			Choices = [.. proto.Choices],
			OnStartEffects = [.. proto.OnStartEffects],
			OnEndEffects = [.. proto.OnEndEffects],
			MusicId = proto.MusicId,
			AmbientId = proto.AmbientId,
			StartSfxId = proto.StartSfxId
		};
	}

	#endregion

	#region Methods

	public float GetEffectiveWeight() {
		return SpawnWeight * Type.GetBaseWeight() * Rarity.GetWeightMultiplier();
	}

	public bool CanSpawn(RunState runState, MetaProgression meta, TerrainProto.ID terrain, float distance) {
		if (ValidTerrains.Count > 0 && !ValidTerrains.Contains(terrain)) {
			return false;
		}

		if (distance < MinDistance) {
			return false;
		}

		if (MaxDistance >= 0 && distance > MaxDistance) {
			return false;
		}

		if (!IsRepeatable && runState.EncounteredEvents.Contains(Id)) {
			return false;
		}

		foreach (var condition in SpawnConditions) {
			if (!condition.Evaluate(runState, meta).Passed) {
				return false;
			}
		}

		return true;
	}

	#endregion
}