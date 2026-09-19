namespace RPGGame.Core.Audio;

[Dependency(registrationType: RegistrationType.Singleton)]
public class AudioManager
{
	public AudioManager() {
		UnityEngine.Debug.Log("Audio Manager Initialized");
	}
	public void PlaySound(string sound)
	{
		Console.WriteLine(value: $"Playing sound: {sound}");
	}
}
