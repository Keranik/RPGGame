using RPGGame.Core.Prototypes.Item;

namespace RPGGame.Core.Items;
public interface IItem {
	ItemId Id { get; }

	ItemProto Prototype { get; }
}
