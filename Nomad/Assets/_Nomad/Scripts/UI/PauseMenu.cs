using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(MenuNode))]
public class PauseMenu : MonoBehaviour
{
	[SerializeField, Required] private MenuNode menuNode;

	private void Reset()
	{
		menuNode = GetComponent<MenuNode>();
	}

	private void Awake()
	{
		Systems.Session.UI.PauseMenuStack.SetRoot(menuNode);
	}

	private void OnDestroy()
	{
		Systems.Session.UI.PauseMenuStack.ClearRoot();
	}
}
