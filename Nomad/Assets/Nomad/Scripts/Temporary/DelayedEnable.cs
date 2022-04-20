using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class DelayedEnable : MonoBehaviour
{
	[SerializeField, Required] private MonoBehaviour target;

	private IEnumerator Start()
	{
		yield return null;
		target.enabled = true;
	}
}
