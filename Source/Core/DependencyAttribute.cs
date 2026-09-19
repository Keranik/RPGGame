namespace RPGGame.Core;

[AttributeUsage(validOn: AttributeTargets.Class, Inherited = false)]
public class DependencyAttribute : Attribute
{
	public RegistrationType RegistrationType { get; }

	public DependencyAttribute(RegistrationType registrationType)
	{
		RegistrationType = registrationType;
	}
}