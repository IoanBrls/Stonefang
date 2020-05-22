using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	private GameObject dungeonObject;
	private GameObject cavernObject;

	private DungeonGenerator dungeon;
	private CavernGenerator cavern;

	private bool dungeonActive;
	private bool cavernActive;

	private int waitUntilMove;

	public int speed;
	public int melee;
	public int ranged;
	public int magic;
	public int defense;
	public GameObject enemy2D;

	private void Start()
	{
		waitUntilMove = 0;
		GameObject newEnemy = Instantiate(enemy2D, new Vector3(transform.position.x - 400, 0.001f, transform.position.z), Quaternion.Euler(new Vector3(90, 0, 0)));
		newEnemy.transform.parent = transform;
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

	public void FollowPath()
	{
		waitUntilMove++;
		GetActiveDungeon();

		if (dungeonActive)
		{
			if (waitUntilMove >= speed)
			{
				waitUntilMove = 0;
				if (dungeon == null)
					Debug.Log("NO dungeon");
				if (GetComponent<Pathfinding>().path == null)
					Debug.Log("NO PATH");
				if (dungeon.TileFromWorldPoint(GetComponent<Pathfinding>().path[0].worldPos) == null)
					Debug.Log("NO TILE");
				if(!dungeon.TileFromWorldPoint(GetComponent<Pathfinding>().path[0].worldPos).isPlayer)
					transform.position = GetComponent<Pathfinding>().path[0].worldPos + new Vector3(0, 1, 0);
			}
		}
		else if (cavernActive)
		{
			if (waitUntilMove >= speed)
			{
				waitUntilMove = 0;
				if (!cavern.TileFromWorldPoint(GetComponent<Pathfinding>().path[0].worldPos).isPlayer)
					transform.position = GetComponent<Pathfinding>().path[0].worldPos + new Vector3(0, 1, 0);
			}
		}
		
	}
}
