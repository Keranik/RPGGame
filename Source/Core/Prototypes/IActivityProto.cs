using RPGGame.Core.Activities;
using RPGGame.Core.Prototypes.Activities;

namespace RPGGame.Core.Prototypes;
public interface IActivityProto : IProto {
	Type ActivityType { get; }
}
