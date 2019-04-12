using UnityEngine;

public class Danger : MonoBehaviour
{
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			// TODO play crash animation
			PMWrapper.RaiseTaskError("Podden kraschade. Undvik vägarbeten.");
			PMWrapper.StopCompiler();
		}
	}
}