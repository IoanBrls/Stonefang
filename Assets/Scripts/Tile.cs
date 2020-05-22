using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : IHeapItem<Tile>
{
	public int coordX;
	public int coordY;

	public bool walkable;
	public bool isCentroid;
	public bool isPlayer;
	public bool isExit;
	public bool isEnemy;
	public bool isItem;

	public Vector3 worldPos;

	public int gCost;
	public int hCost;
	public Tile parent;
	private int heapIndex;

	public Tile(Vector3 worldPos, int x, int y)
	{
		this.worldPos = worldPos;
		coordX = x;
		coordY = y;
	}

	public int fCost
	{
		get
		{
			return gCost + hCost;
		}
	}

	public int HeapIndex
	{
		get
		{
			return heapIndex;
		}
		set
		{
			heapIndex = value;
		}
	}

	public int CompareTo(Tile nodeToCompare)
	{
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0)
		{
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}
