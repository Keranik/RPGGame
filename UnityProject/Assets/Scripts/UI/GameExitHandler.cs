using UnityEngine;

public class GameExitHandler : MonoBehaviour
{
	public void ExitGame()
	{
		Debug.Log("Exit button clicked - Exiting Game.");
		Application.Quit();
	}
}
