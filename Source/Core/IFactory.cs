namespace RPGGame.Core;
public interface IFactory<in TArg, out TResult>
{
	TResult Create(TArg arg);
}
