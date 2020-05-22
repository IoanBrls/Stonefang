using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pathfinding : MonoBehaviour
{
	private GameObject dungeonObject;
	private GameObject cavernObject;

	private CavernGenerator cavern;
	private DungeonGenerator dungeon;

	private bool cavernActive;
	private bool dungeonActive;

	public List<Tile> path;

	private GameObject player;

	private void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player");
	}

	private void GetActiveDungeon()
	{
		dungeonObject = GameObject.FindGameObjectWithTag("Dungeon Generator");
		cavernObject = GameObject.FindGameObjectWithTag("Cavern Generator");

		if (dungeonObject != null)
		{
			dungeonActive = true;
			cavernActive = false;
			dungeon = dungeonObject.GetComponent<DungeonGenerator>();
		}
		else if (cavernObject != null)
		{
			dungeonActive = false;
			cavernActive = true;
			cavern = cavernObject.GetComponent<CavernGenerator>();
		}
	}

	public void FindPath(Vector3 startPos, Vector3 targetPos)
	{
		GetActiveDungeon();
		if (cavernActive)
		{
			Tile startTile = cavern.TileFromWorldPoint(startPos);
			Tile targetTile = cavern.TileFromWorldPoint(targetPos);

			Heap<Tile> openSet = new Heap<Tile>(cavern.MaxSize);
			HashSet<Tile> closedSet = new HashSet<Tile>();
			openSet.Add(startTile);

			while (openSet.Count > 0)
			{
				Tile currentTile = openSet.RemoveFirst();
				
				closedSet.Add(currentTile);

				if (currentTile == targetTile)
				{
					RetracePath(startTile, targetTile);
					return;
				}

				foreach (Tile neighbour in cavern.GetNeighbours(currentTile))
				{
					if (!neighbour.walkable || closedSet.Contains(neighbour))
					{
						continue;
					}

					int newCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);
					if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetTile);
						neighbour.parent = currentTile;

						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
					}
				}
			}
		}
		else if (dungeonActive)
		{
			Tile startTile = dungeon.TileFromWorldPoint(startPos);
			Tile targetTile = dungeon.TileFromWorldPoint(targetPos);

			Heap<Tile> openSet = new Heap<Tile>(dungeon.MaxSize);
			HashSet<Tile> closedSet = new HashSet<Tile>();
			openSet.Add(startTile);

			while (openSet.Count > 0)
			{
				Tile currentTile = openSet.RemoveFirst();
				closedSet.Add(currentTile);

				if (currentTile == targetTile)
				{
					RetracePath(startTile, targetTile);
					return;
				}

				foreach (Tile neighbour in dungeon.GetNeighbours(currentTile))
				{
					if (!neighbour.walkable || closedSet.Contains(neighbour))
					{
						continue;
					}

					int newCostToNeighbour = currentTile.gCost + GetDistance(currentTile, neighbour);
					if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
					{
						neighbour.gCost = newCostToNeighbour;
						neighbour.hCost = GetDistance(neighbour, targetTile);
						neighbour.parent = currentTile;

						if (!openSet.Contains(neighbour))
							openSet.Add(neighbour);
					}
				}
			}
		}

	}

	void RetracePath(Tile startTile, Tile endTile)
	{
		if (cavernActive)
		{
			path = new List<Tile>();
			Tile currentTile = endTile;

			while (currentTile != startTile)
			{
				path.Add(currentTile);
				currentTile = currentTile.parent;
			}
			path.Reverse();

			cavern.path = path;
		}
		else if (dungeonActive)
		{
			path = new List<Tile>();
			Tile currentTile = endTile;

			while (currentTile != startTile)
			{
				path.Add(currentTile);
				currentTile = currentTile.parent;
			}
			path.Reverse();

			dungeon.path = path;
		}
		

	}

	int GetDistance(Tile TileA, Tile TileB)
	{
		int dstX = Mathf.Abs(TileA.coordX - TileB.coordX);
		int dstY = Mathf.Abs(TileA.coordY - TileB.coordY);

		if (dstX > dstY)
			return 14 * dstY + 10 * (dstX - dstY);
		return 14 * dstX + 10 * (dstY - dstX);
	}
	
}
