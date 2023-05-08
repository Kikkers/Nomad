using System;
using System.Collections.Generic;

public class MenuStack
{
	private readonly List<MenuNode> activeStack = new();
	private MenuNode root;

	public bool AnyMenuOpened => activeStack.Count > 0;

	public MenuNode ActiveMenuNode
	{
		get
		{
			if (activeStack.Count == 0)
			{
				return null;
			}
			return activeStack[activeStack.Count - 1];
		}
	}

	public void SetRoot(MenuNode newRoot)
	{
		if (root != null)
		{
			throw new InvalidOperationException();
		}
		root = newRoot;
	}

	public void ClearRoot()
	{
		if (root == null)
		{
			throw new InvalidOperationException();
		}
		root = null;
	}

	public void PushMenu(MenuNode newNode)
	{
		if (activeStack.Count == 0 && newNode != null && newNode != root)
		{
			throw new InvalidOperationException("first node must be the root");
		}

		MenuNode previousActiveNode = ActiveMenuNode;
		if (previousActiveNode != null)
		{
			previousActiveNode.MoveToBackground();
		}
		activeStack.Add(newNode);
		newNode.MoveToForeground();
	}

	public void PopMenu()
	{
		MenuNode toBePoppedNode = ActiveMenuNode;
		if (toBePoppedNode == null)
		{
			return;
		}
		toBePoppedNode.MoveToDisabled();
		activeStack.RemoveAt(activeStack.Count - 1);

		MenuNode newTopNode = ActiveMenuNode;
		if (newTopNode != null)
		{
			newTopNode.MoveToForeground();
		}
	}

	public void Clear()
	{
		foreach (MenuNode node in activeStack)
		{
			node.MoveToDisabled();
		}
	}
}
