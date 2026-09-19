namespace RPGGame.Core;

public abstract class GameIdFactoryBase<TId> where TId : struct
{
	private TId k_previousId;

	public TId GetNextId()
	{
		k_previousId = GenerateNewId(k_previousId);
		return k_previousId;
	}

	protected abstract TId GenerateNewId(TId previousId);	
}
