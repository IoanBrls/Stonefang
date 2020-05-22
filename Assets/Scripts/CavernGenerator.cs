using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CavernGenerator : MonoBehaviour
{
	private Vector2 cavernWorldSize;
	private float tileRadius;

	public GameObject dungeonObject;
	public GameObject dungeonObject2D;
	public GameObject[] rock;
	public GameObject player;
	public GameObject exit;
	public GameObject ground;
	public GameObject ground2D;
	public GameObject rock2D;
	public GameObject player2D;
	public GameObject exit2D;

	public GameObject[] enemy;
	public int maxEnemies;
	//public GameObject enemy2D;

	public GameObject controller;

	private Tile[,] cavern;
	private int[,] cavernInt;
	private List<Room> rooms;
	private List<Cave> caves;
	private List<Corridor> corridors;
	private List<Triangle> triangulation;

	private float tileDiameter;
	private int cavernSizeX, cavernSizeY;

	private bool showDelaunay;
	private bool showMST;
	private bool showMix;

	public int fillPercent = 45;

	public List<Tile> path;
	

	private void Start()
	{
		cavernWorldSize = new Vector2(256, 256);
		tileRadius = 0.5f;
		tileDiameter = tileRadius * 2;
		cavernSizeX = Mathf.RoundToInt(cavernWorldSize.x / tileDiameter);
		cavernSizeY = Mathf.RoundToInt(cavernWorldSize.y / tileDiameter);
		//Debug.Log("Width: " + cavernSizeX);
		//Debug.Log("Height: " + cavernSizeY);
		//GenerateCavern();
		//CreateCavern();
		//VisualizeCavern();
		//VisualizeCavern2D();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.I))
		{
			showDelaunay = true;
			showMix = false;
			showMST = false;
		}
		if (Input.GetKeyDown(KeyCode.O))
		{
			showDelaunay = false;
			showMix = false;
			showMST = true;
		}
		if (Input.GetKeyDown(KeyCode.P))
		{
			showDelaunay = false;
			showMix = true; 
			showMST = false;
		}
	}
	public int MaxSize
	{
		get
		{
			return cavernSizeX * cavernSizeY;
		}
	}

	public void GenerateNextCavern()
	{
		GenerateCavern();
		CreateCavern();
		VisualizeCavern();
		VisualizeCavern2D();
	}

	private void GenerateCavern()
	{
		cavernInt = new int[cavernSizeX, cavernSizeY];

		for (int x = 0; x < cavernSizeX; x++)
		{
			for (int y = 0; y < cavernSizeY; y++) 
			{
				if (x <= 1 || x >= cavernSizeX - 2 || y <= 1 || y >= cavernSizeY - 2)
				{
					cavernInt[x, y] = 1;
					continue;
				}
				if(UnityEngine.Random.Range(0,100) < fillPercent)
				{
					cavernInt[x, y] = 1;
				}
				else
				{
					cavernInt[x, y] = 0;
				}
			}
		}
		
		for (int i =0; i < 10; i++)
		{
			CellularAutomaton();
		}
		ProcessMap();
	}

	private void CellularAutomaton()
	{
		int[,] destination = new int[256, 256];

		for (int x = 0; x < cavernSizeX; x++)
		{
			for (int y = 0; y < cavernSizeY; y++)
			{
				int aliveNeighbours = GetAliveNeighboursCount(x, y);

				if (cavernInt[x, y] == 0)
				{
					if (aliveNeighbours < 4)
					{
						destination[x, y] = 1;
					}
					else if (aliveNeighbours > 8)
					{
						destination[x, y] = 1;
					}
					else
					{
						destination[x, y] = cavernInt[x, y];
					}
				}
				else
				{
					if (aliveNeighbours >= 5 && aliveNeighbours <= 7)
					{
						destination[x, y] = 0;
					}
					else
					{
						destination[x, y] = cavernInt[x, y];
					}
				}
			}
		}

		for (int x = 0; x < cavernSizeX; x++)
		{
			for (int y = 0; y < cavernSizeY; y++)
			{
				cavernInt[x, y] = destination[x, y];
			}
		}

	}

	private int GetAliveNeighboursCount(int x, int y)
	{
		int aliveCount = 0;

		for (int neighbourX = x -1; neighbourX <= x + 1; neighbourX++)
		{
			for (int neighbourY = y - 1; neighbourY <= y + 1; neighbourY++)
			{
				if (neighbourX >=0 && neighbourX < cavernSizeX && neighbourY >= 0 && neighbourY < cavernSizeY)
				{
					if (neighbourX != x || neighbourY != y)
					{
						if (cavernInt[neighbourX,neighbourY] == 0)
						{
							aliveCount++;
						}
					}
				}
			}
		}

		return aliveCount;
	}

	private void ProcessMap()
	{
		rooms = new List<Room>();
		List<List<Coord>> wallRegions = GetRegions(1);

		int wallThresholdSize = 50;
		foreach (List<Coord> wallRegion in wallRegions)
		{
			if(wallRegion.Count < wallThresholdSize)
			{
				foreach(Coord tile in wallRegion)
				{
					cavernInt[tile.tileX, tile.tileY] = 0;
				}
			}
		}
		 
		List<List<Coord>> caveRegions = GetRegions(0);
		caves = new List<Cave>();
		int caveThresholdSize = 50;

		foreach (List<Coord> caveRegion in caveRegions)
		{
			if (caveRegion.Count < caveThresholdSize)
			{
				foreach (Coord tile in caveRegion)
				{
					cavernInt[tile.tileX, tile.tileY] = 1;
				}
			}
			else
			{
				caves.Add(new Cave(caveRegion, cavernInt));
			}
		}

		foreach (Cave cave in caves)
		{
			cavernInt[cave.centroid.tileX, cave.centroid.tileY] = 2;
			Room newRoom = new Room(cave.tiles[0].tileX, cave.tiles[0].tileY, cave.caveSize, cave.caveSize);
			newRoom.centroid = new Vector2Int(cave.centroid.tileX, cave.centroid.tileY);
			rooms.Add(newRoom);
		}
		//Debug.Log(caves.Count);


		GeneratePassageways();

		int randomPlayer = UnityEngine.Random.Range(0, caves.Count - 1);
		int randomPlayerTile = UnityEngine.Random.Range(0, caves[randomPlayer].caveSize - 1);

		while (true)
		{
			int unwalkableCount = 0;
			for (int i = caves[randomPlayer].tiles[randomPlayerTile].tileX - 1; i <= caves[randomPlayer].tiles[randomPlayerTile].tileX + 1; i++)
			{
				for (int j = caves[randomPlayer].tiles[randomPlayerTile].tileY - 1; j <= caves[randomPlayer].tiles[randomPlayerTile].tileY + 1; j++)
				{
					if (cavernInt[i, j] == 1)
					{
						unwalkableCount++;
					}
				}
			}

			if (unwalkableCount > 0)
			{
				randomPlayerTile = UnityEngine.Random.Range(0, caves[randomPlayer].caveSize - 1);
			}
			else
			{
				break;
			}
		}
		Vector2Int playerPosition = new Vector2Int(caves[randomPlayer].tiles[randomPlayerTile].tileX, caves[randomPlayer].tiles[randomPlayerTile].tileY);


		int randomExit = UnityEngine.Random.Range(0, caves.Count - 1);
		while (randomExit == randomPlayer)
		{
			randomExit = UnityEngine.Random.Range(0, caves.Count - 1);
		}
		int randomExitTile = UnityEngine.Random.Range(0, caves[randomExit].caveSize - 1);
		Vector2Int exitPosition = new Vector2Int(caves[randomExit].tiles[randomExitTile].tileX, caves[randomExit].tiles[randomExitTile].tileY);

		for(int i = 0; i < maxEnemies; i++)
		{
			int randomEnemy = UnityEngine.Random.Range(0, caves.Count - 1);
			while (randomEnemy == randomPlayer)
			{
				randomEnemy = UnityEngine.Random.Range(0, caves.Count - 1);
			}
			int randomEnemyTile = UnityEngine.Random.Range(0, caves[randomEnemy].caveSize - 1);
			Vector2Int enemyPosition = new Vector2Int(caves[randomEnemy].tiles[randomEnemyTile].tileX, caves[randomEnemy].tiles[randomEnemyTile].tileY);

			cavernInt[enemyPosition.x, enemyPosition.y] = 5;
		}
		

		cavernInt[playerPosition.x, playerPosition.y] = 3;
		cavernInt[exitPosition.x, exitPosition.y] = 4;

	}

	private void GeneratePassageways()
	{
		DelaunayTriangles();
		MinimumSpanningTree();

		//foreach (Triangle triangle in triangulation)
		//{
		//	foreach (Corridor corridor in triangle.corridors)
		//	{
		//		if (!corridors.Contains(corridor) && UnityEngine.Random.Range(0, 100) < 35)
		//		{
		//			corridors.Add(corridor);
		//		}
		//	}
		//}

		foreach (Corridor corridor in corridors)
		{
			Cave caveA = FindCaveFromCentroid(corridor.connectedRooms[0].centroid);
			Cave caveB = FindCaveFromCentroid(corridor.connectedRooms[1].centroid);

			Coord bestTileA = new Coord();
			Coord bestTileB = new Coord();

			int bestDistance = 100000;
			

			for (int tileIndexA = 0; tileIndexA < caveA.edgeTiles.Count; tileIndexA++)
			{
				for (int tileIndexB = 0; tileIndexB < caveB.edgeTiles.Count; tileIndexB++)
				{
					Coord tileA = caveA.edgeTiles[tileIndexA];
					Coord tileB = caveB.edgeTiles[tileIndexB];

					int distanceBetweenCaves = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

					if (distanceBetweenCaves < bestDistance)
					{
						bestDistance = distanceBetweenCaves;
						bestTileA = tileA;
						bestTileB = tileB;
					}
				}
			}

			CreatePassage(caveA, caveB, bestTileA, bestTileB);
		}
	}

	private Cave FindCaveFromCentroid(Vector2Int centroid)
	{
		Cave outputCave = new Cave();

		foreach(Cave cave in caves)
		{
			if (cave.centroid.tileX == centroid.x && cave.centroid.tileY == centroid.y)
			{
				outputCave = cave;
				break;
			}
		}

		return outputCave;
	}

	private Triangle LootTriangle()
	{
		Vector2Int[] vertices = {new Vector2Int(cavernSizeX*2, cavernSizeY),
								 new Vector2Int(-cavernSizeX*2, cavernSizeY),
								 new Vector2Int(0, -2*cavernSizeY)};

		Room[] tempRooms = new Room[3];

		for (int i = 0; i < 3; i++)
		{
			tempRooms[i] = new Room(vertices[i].x, vertices[i].y, 2, 2);
		}

		return new Triangle(tempRooms[0], tempRooms[1], tempRooms[2]);

	}

	private void DelaunayTriangles()
	{
		triangulation = new List<Triangle>();
		Triangle loot = LootTriangle();
		triangulation.Add(loot);

		foreach (Room room in rooms)
		{
			List<Triangle> badTriangles = new List<Triangle>();

			foreach (Triangle triangle in triangulation)
			{
				if (triangle.IsContaining(room))
				{
					badTriangles.Add(triangle);
				}
			}

			List<Corridor> polygon = new List<Corridor>();
			foreach (Triangle badTriangle in badTriangles)
			{
				foreach (Corridor corridor in badTriangle.corridors)
				{
					if (corridor.triangles.Count == 1)
					{
						polygon.Add(corridor);
						corridor.triangles.Remove(badTriangle);
						continue;
					}

					foreach (Triangle triangle in corridor.triangles)
					{
						if (triangle == badTriangle)
							continue;

						if (badTriangles.Contains(triangle))
						{
							corridor.connectedRooms[0].RoomCorridor.Remove(corridor.connectedRooms[1]);
							corridor.connectedRooms[1].RoomCorridor.Remove(corridor.connectedRooms[0]);
						}
						else
						{
							polygon.Add(corridor);
						}
						break;
					}
				}
			}

			for (int i = badTriangles.Count - 1; i >= 0; --i)
			{
				Triangle triangle = badTriangles[i];
				badTriangles.RemoveAt(i);
				triangulation.Remove(triangle);
				foreach (Corridor corridor in triangle.corridors)
					corridor.triangles.Remove(triangle);
			}

			foreach (Corridor corridor in polygon)
			{
				Triangle newTriangle = new Triangle(corridor.connectedRooms[0], corridor.connectedRooms[1], room);
				triangulation.Add(newTriangle);
			}
		}
		//Debug.Log("Triangles before: " + triangulation.Count);

		for (int i = triangulation.Count - 1; i >= 0; i--)
		{
			if (triangulation[i].rooms.Contains(loot.rooms[0]) || triangulation[i].rooms.Contains(loot.rooms[1])
				|| triangulation[i].rooms.Contains(loot.rooms[2]))
			{
				triangulation.RemoveAt(i);
			}
		}

		foreach (Room room in loot.rooms)
		{
			List<Corridor> deleteList = new List<Corridor>();

			foreach (var pair in room.RoomCorridor)
				deleteList.Add(pair.Value);

			for (int i = deleteList.Count - 1; i >= 0; i--)
			{
				Corridor corridor = deleteList[i];
				corridor.connectedRooms[0].RoomCorridor.Remove(corridor.connectedRooms[1]);
				corridor.connectedRooms[1].RoomCorridor.Remove(corridor.connectedRooms[0]);
			}
		}
	}

	private void MinimumSpanningTree()
	{
		List<Room> connectedRooms = new List<Room>();
		corridors = new List<Corridor>();

		connectedRooms.Add(rooms[0]);

		while (connectedRooms.Count < rooms.Count)
		{
			var minLength = new KeyValuePair<Room, Corridor>();
			List<Corridor> deleteList = new List<Corridor>();

			foreach (Room room in connectedRooms)
			{
				foreach (var pair in room.RoomCorridor)
				{
					if (connectedRooms.Contains(pair.Key))
						continue;
					if (minLength.Value == null || minLength.Value.length > pair.Value.length)
						minLength = pair;
				}
			}

			foreach (var pair in minLength.Key.RoomCorridor)
			{
				if (connectedRooms.Contains(pair.Key) && minLength.Value != pair.Value)
					deleteList.Add(pair.Value);
			}

			for (int i = deleteList.Count - 1; i >= 0; i--)
			{
				Corridor corridor = deleteList[i];
				corridor.connectedRooms[0].RoomCorridor.Remove(corridor.connectedRooms[1]);
				corridor.connectedRooms[1].RoomCorridor.Remove(corridor.connectedRooms[0]);
				deleteList.RemoveAt(i);
			}

			if (!connectedRooms.Contains(minLength.Key))
				connectedRooms.Add(minLength.Key);
			if (!corridors.Contains(minLength.Value))
				corridors.Add(minLength.Value);
		}
	}
	
	private void CreatePassage(Cave caveA, Cave caveB, Coord tileA, Coord tileB)
	{
		List<Coord> line = GetLine(tileA, tileB);
		foreach (Coord c in line)
		{
			DrawCircle(c, 1);
		}
	}

	private void DrawCircle(Coord c, int r)
	{
		for (int x = -r; x <= r; x++)
		{
			for (int y = -r; y <= r; y++)
			{
				if (x * x + y * y <= r * r)
				{
					int drawX = c.tileX + x;
					int drawY = c.tileY + y;

					if (drawX >= 0 && drawX < cavernSizeX && drawY >= 0 && drawY < cavernSizeY)
					{
						cavernInt[drawX, drawY] = 0;
					}
				}
			}
		}
	}

	private List<Coord> GetLine(Coord from, Coord to)
	{
		List<Coord> line = new List<Coord>();

		int x = from.tileX;
		int y = from.tileY;

		int dx = to.tileX - from.tileX;
		int dy = to.tileY - from.tileY;

		bool inverted = false;
		int step = Math.Sign(dx);
		int gradientStep = Math.Sign(dy);

		int longest = Mathf.Abs(dx);
		int shortest = Mathf.Abs(dy);

		if (longest < shortest)
		{
			inverted = true;
			longest = Mathf.Abs(dy);
			shortest = Mathf.Abs(dx);

			step = Math.Sign(dy);
			gradientStep = Math.Sign(dx);
		}

		int gradientAccumulation = longest / 2;

		for(int i = 0; i < longest; i++)
		{
			line.Add(new Coord(x, y));

			if (inverted)
			{
				y += step;
			}
			else
			{
				x += step;
			}

			gradientAccumulation += shortest;

			if(gradientAccumulation >= longest)
			{
				if (inverted)
				{
					x += gradientStep;
				}
				else
				{
					y += gradientStep;
				}
				gradientAccumulation -= longest;
			}
		}

		return line;
	}

	private Vector3 CoordToWorldPoint(Coord tile)
	{
		return new Vector3(-cavernSizeX / 2 + 0.5f + tile.tileX, 2, -cavernSizeY / 2 + 0.5f + tile.tileY);
	}

	private List<List<Coord>> GetRegions (int tileType)
	{
		List<List<Coord>> regions = new List<List<Coord>>();
		int[,] mapFlags = new int[cavernSizeX, cavernSizeY];

		for (int x = 0; x < cavernSizeX; x++)
		{
			for (int y = 0; y < cavernSizeY; y++)
			{
				if (mapFlags[x,y] == 0 && cavernInt[x,y] == tileType)
				{
					List<Coord> newRegion = GetRegionTiles(x, y);
					regions.Add(newRegion);

					foreach (Coord tile in newRegion)
					{
						mapFlags[tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}

		return regions;
	}

	private List<Coord> GetRegionTiles(int startX, int startY)
	{
		List<Coord> tiles = new List<Coord>();
		int[,] mapFlags = new int[cavernSizeX, cavernSizeY];
		int tileType = cavernInt[startX, startY];

		Queue<Coord> queue = new Queue<Coord>();
		queue.Enqueue(new Coord(startX, startY));
		mapFlags[startX, startY] = 1;

		while (queue.Count > 0)
		{
			Coord tile = queue.Dequeue();
			tiles.Add(tile);

			for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
			{
				for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
				{
					if (x >= 0 && x < cavernSizeX && y >= 0 && y < cavernSizeY)
					{
						if(y == tile.tileY || x == tile.tileX)
						{
							if (mapFlags[x,y] == 0 && cavernInt[x, y] == tileType)
							{
								mapFlags[x, y] = 1;
								queue.Enqueue(new Coord(x, y));
							}
						}
					}
				}
			}
		}

		return tiles;
	}

	private void CreateCavern()
	{
		cavern = new Tile[cavernSizeX, cavernSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * cavernWorldSize.x / 2 - Vector3.forward * cavernWorldSize.y / 2;

		for (int x = 0; x < cavernSizeX; x++)
		{
			for (int y = 0; y < cavernSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * tileDiameter + tileRadius) + Vector3.forward * (y * tileDiameter + tileRadius);
				cavern[x, y] = new Tile(worldPoint, x, y);

				if(cavernInt[x,y] == 1)
				{
					cavern[x, y].walkable = false;
				}
				else
				{
					cavern[x, y].walkable = true;
				}

				if (cavernInt[x, y] == 2)
				{
					cavern[x, y].isCentroid = true;
				}
				else
				{
					cavern[x, y].isCentroid = false;
				}

				if (cavernInt[x, y] == 3)
				{
					cavern[x, y].isPlayer = true;
				}
				else
				{
					cavern[x, y].isPlayer = false;
				}

				if (cavernInt[x, y] == 4)
				{
					cavern[x, y].isExit = true;
				}
				else
				{
					cavern[x, y].isExit = false;
				}

				if (cavernInt[x, y] == 5)
				{
					cavern[x, y].isEnemy = true;
				}
				else
				{
					cavern[x, y].isEnemy = false;
				}
			}
		}
	}

	public Tile TileFromWorldPoint(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + cavernWorldSize.x / 2) / cavernWorldSize.x;
		float percentY = (worldPosition.z + cavernWorldSize.y / 2) / cavernWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((cavernSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((cavernSizeY - 1) * percentY);
		return cavern[x, y];
	}

	public List<Tile> GetNeighbours(Tile node)
	{
		List<Tile> neighbours = new List<Tile>();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == y || x == -y)
					continue;

				int checkX = node.coordX + x;
				int checkY = node.coordY + y;

				if (checkX >= 0 && checkX < cavernSizeX && checkY >= 0 && checkY < cavernSizeY)
				{
					neighbours.Add(cavern[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	private void VisualizeCavern()
	{
		if (dungeonObject.transform.childCount != 0)
		{
			for (int i = 0; i < dungeonObject.transform.childCount; i++)
			{
				Destroy(dungeonObject.transform.GetChild(i).gameObject);
			}
		}

		if (cavern != null)
		{
			for (int x = 0; x < cavernSizeX; x++)
			{
				for (int y = 0; y < cavernSizeY; y++)
				{
					if (x == 0 || y == 0 || x == cavernSizeX - 1 || y == cavernSizeY - 1)
					{
						GameObject newRock = Instantiate(rock[0], new Vector3(cavern[x, y].worldPos.x, 0f, cavern[x, y].worldPos.z), Quaternion.identity);
						newRock.transform.parent = dungeonObject.transform;
						continue;
					}

					if (!cavern[x, y].walkable && CheckNeightBoursWalkbability(x, y))
					{
						int rockIndex = UnityEngine.Random.Range(0, rock.Length - 1);
						GameObject newRock = Instantiate(rock[rockIndex], new Vector3(cavern[x, y].worldPos.x, 0f, cavern[x, y].worldPos.z), Quaternion.identity);
						newRock.transform.parent = dungeonObject.transform;
					}

					if (cavern[x, y].isPlayer)
					{
						player.transform.position = new Vector3(cavern[x, y].worldPos.x, 1f, cavern[x, y].worldPos.z);
					}

					if (cavern[x, y].isExit)
					{
						GameObject newExit = Instantiate(exit, new Vector3(cavern[x, y].worldPos.x, 1f, cavern[x, y].worldPos.z), Quaternion.identity);
						newExit.transform.parent = dungeonObject.transform;
					}

					if (cavern[x, y].isEnemy)
					{
						int enemyIndex = UnityEngine.Random.Range(0, enemy.Length - 1);
						GameObject newEnemy = Instantiate(enemy[enemyIndex], new Vector3(cavern[x, y].worldPos.x, 1f, cavern[x, y].worldPos.z), Quaternion.identity);
						newEnemy.transform.parent = dungeonObject.transform;
					}

				}
			}
		}
		GenerateGrid();
	}

	private void VisualizeCavern2D()
	{
		if (dungeonObject2D.transform.childCount != 0)
		{
			for (int i = 0; i < dungeonObject2D.transform.childCount; i++)
			{
				Destroy(dungeonObject2D.transform.GetChild(i).gameObject);
			}
		}

		if (cavern != null)
		{
			for (int x = 0; x < cavernSizeX; x++)
			{
				for (int y = 0; y < cavernSizeY; y++)
				{
					if (x == 0 || y == 0 || x == cavernSizeX - 1 || y == cavernSizeY - 1)
					{
						GameObject newRock = Instantiate(rock2D, new Vector3(cavern[x, y].worldPos.x - 400, 0f, cavern[x, y].worldPos.z), Quaternion.Euler(new Vector3(90, 0, 0)));
						newRock.transform.parent = dungeonObject2D.transform;
						continue;
					}

					if (!cavern[x, y].walkable)
					{
						GameObject newRock = Instantiate(rock2D, new Vector3(cavern[x, y].worldPos.x - 400, 0f, cavern[x, y].worldPos.z), Quaternion.Euler(new Vector3(90, 0, 0)));
						newRock.transform.parent = dungeonObject2D.transform;
					}

					if (cavern[x, y].walkable)
					{
						GameObject newFloor = Instantiate(ground2D, new Vector3(cavern[x, y].worldPos.x - 400, 0f, cavern[x, y].worldPos.z), Quaternion.Euler(new Vector3(90, 0, 0)));
						newFloor.transform.parent = dungeonObject2D.transform;
					}

					if (cavern[x, y].isPlayer)
					{
						player2D.transform.position = new Vector3(cavern[x, y].worldPos.x - 400, 0f, cavern[x, y].worldPos.z);
					}

					if (cavern[x, y].isExit)
					{
						GameObject newExit = Instantiate(exit2D, new Vector3(cavern[x, y].worldPos.x - 400, 0f, cavern[x, y].worldPos.z), Quaternion.Euler(new Vector3(90, 0, 0)));
						newExit.transform.parent = dungeonObject2D.transform;
					}

					//if (cavern[x, y].isEnemy)
					//{
					//	GameObject newEnemy = Instantiate(enemy2D, new Vector3(cavern[x, y].worldPos.x - 400, 0f, cavern[x, y].worldPos.z), Quaternion.Euler(new Vector3(90, 0, 0)));
					//	newEnemy.transform.parent = dungeonObject.transform;
					//}
				}
			}
		}
	}

	private void GenerateGrid()
	{
		MeshRenderer floorRenderer = ground.GetComponent<MeshRenderer>();
		//Texture2D gridImage = (Texture2D)floorRenderer.sharedMaterial.mainTexture;
		Texture gridImage3D = floorRenderer.material.mainTexture;
		Texture2D gridImage = new Texture2D(cavernSizeX, cavernSizeY, TextureFormat.RGBA32, false);
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture renderTexture = new RenderTexture(cavernSizeX, cavernSizeY, 32);
		Graphics.Blit(gridImage3D, renderTexture);
		RenderTexture.active = renderTexture;
		gridImage.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		gridImage.Apply();

		Color gridColor = Color.white;
		Color borderColor = Color.black;
		Collider floorCollider = ground.GetComponent<Collider>();
		//Vector3 floorSize = new Vector3(floorCollider.bounds.size.x, floorCollider.bounds.size.z);
		for (int x = 0; x < cavernSizeX; x++)
		{
			for (int y = 0; y < cavernSizeY; y++)
			{
				if (x < 3 || x > gridImage.width - 3 || y < 3 || y > gridImage.height - 3)
				{
					gridImage.SetPixel(x, y, new Color(borderColor.r, borderColor.g, borderColor.b, 50));
				}
			}
			gridImage.Apply();
		}
		gridImage.wrapMode = TextureWrapMode.Repeat;
		floorRenderer.material.mainTexture = gridImage;
		floorRenderer.material.mainTextureScale = new Vector2(floorCollider.bounds.size.x, floorCollider.bounds.size.z);
		floorRenderer.material.mainTextureOffset = new Vector2(1f, 1f);

		RenderTexture.active = currentRT;
	}

	private bool CheckNeightBoursWalkbability(int x, int y)
	{
		for (int i = -2; i < 3; i++)
		{
			for (int j = -2; j < 3; j++)
			{
				if (x+i >= 0 && x+i < cavernSizeX && y+j >= 0 && y+j < cavernSizeY)
				{
					if ((i == 0 && j == 0) || !cavern[x + i, y + j].walkable)
						continue;

					if (cavern[x + i, y + j].walkable)
						return true;
				}
			}
		}
		return false;
	}

	public void MovePlayer(Transform playerTransform)
	{
		Vector3 playerRotation = playerTransform.rotation.eulerAngles;
		int playerX = -1;
		int playerY = -1;

		for (int x = 0; x < cavernSizeX; x++)
		{
			for (int y = 0; y < cavernSizeY; y++)
			{
				if (cavern[x, y].isPlayer)
				{
					playerX = x;
					playerY = y;
					x = cavernSizeX;
					y = cavernSizeY;
					break;
				}
			}
		}
		//Debug.Log("Player Rotation:" + playerRotation.y);
		if (playerRotation.y > 179 && playerRotation.y < 181)
		{
			if (cavern[playerX, playerY - 1].isExit)
			{
				controller.GetComponent<MainController>().GenerateNextDungeon();
			}
			else if (cavern[playerX, playerY - 1].walkable)
			{
				cavern[playerX, playerY].isPlayer = false;
				cavern[playerX, playerY - 1].isPlayer = true;
				player.transform.position = new Vector3(cavern[playerX, playerY - 1].worldPos.x, 1f, cavern[playerX, playerY - 1].worldPos.z);
				player2D.transform.position = new Vector3(cavern[playerX, playerY - 1].worldPos.x - 400, 0f, cavern[playerX, playerY - 1].worldPos.z);
			}
		}
		else if (playerRotation.y > 89 && playerRotation.y < 91)
		{
			if (cavern[playerX + 1, playerY].isExit)
			{
				controller.GetComponent<MainController>().GenerateNextDungeon();
			}
			else if (cavern[playerX + 1, playerY].walkable)
			{
				cavern[playerX, playerY].isPlayer = false;
				cavern[playerX + 1, playerY].isPlayer = true;
				player.transform.position = new Vector3(cavern[playerX + 1, playerY].worldPos.x, 1f, cavern[playerX + 1, playerY].worldPos.z);
				player2D.transform.position = new Vector3(cavern[playerX + 1, playerY].worldPos.x - 400, 0f, cavern[playerX + 1, playerY].worldPos.z);
			}
		}
		else if (playerRotation.y > 269 && playerRotation.y < 271)
		{
			if (cavern[playerX - 1, playerY].isExit)
			{
				controller.GetComponent<MainController>().GenerateNextDungeon();
			}
			else if (cavern[playerX - 1, playerY].walkable)
			{
				cavern[playerX, playerY].isPlayer = false;
				cavern[playerX - 1, playerY].isPlayer = true;
				player.transform.position = new Vector3(cavern[playerX - 1, playerY].worldPos.x, 1f, cavern[playerX - 1, playerY].worldPos.z);
				player2D.transform.position = new Vector3(cavern[playerX - 1, playerY].worldPos.x - 400, 0f, cavern[playerX - 1, playerY].worldPos.z);
			}
		}
		else if (playerRotation.y < 1)
		{
			if (cavern[playerX, playerY + 1].isExit)
			{
				controller.GetComponent<MainController>().GenerateNextDungeon();
			}
			else if (cavern[playerX, playerY + 1].walkable)
			{
				cavern[playerX, playerY].isPlayer = false;
				cavern[playerX, playerY + 1].isPlayer = true;
				player.transform.position = new Vector3(cavern[playerX, playerY + 1].worldPos.x, 1f, cavern[playerX, playerY + 1].worldPos.z);
				player2D.transform.position = new Vector3(cavern[playerX, playerY + 1].worldPos.x - 400, 0f, cavern[playerX, playerY + 1].worldPos.z);
			}
		}
	}
	
	private struct Coord
	{
		public int tileX;
		public int tileY;

		public Coord(int x, int y)
		{
			tileX = x;
			tileY = y;
		}
	}

	private class Cave : IComparable<Cave>
	{
		public List<Coord> tiles;
		public List<Coord> edgeTiles;
		public List<Cave> connectedCaves;
		public int caveSize;
		public Coord centroid;

		public Cave()
		{

		}

		public Cave(List<Coord> roomTiles, int[,] cavern)
		{
			tiles = roomTiles;
			caveSize = tiles.Count;
			connectedCaves = new List<Cave>();
			edgeTiles = new List<Coord>();

			foreach(Coord tile in tiles)
			{
				for (int x = tile.tileX -1; x <= tile.tileX +1; x++)
				{
					for (int y = tile.tileY -1; y <= tile.tileY +1; y++)
					{
						if(x==tile.tileX || y == tile.tileY)
						{
							if(cavern[x,y] == 1)
								edgeTiles.Add(tile);
						}
					}
				}
			}
			centroid = tiles[Mathf.RoundToInt(tiles.Count / 2)];
		}

		public bool IsConnected(Cave otherCave)
		{
			return connectedCaves.Contains(otherCave);
		}

		public int CompareTo(Cave otherCave)
		{
			return otherCave.caveSize.CompareTo(caveSize);
		}
	}
	 
	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(cavernWorldSize.x, 1, cavernWorldSize.y));

		if (cavern != null)
		{
			Tile playerTile = TileFromWorldPoint(player.transform.position);
			foreach (Tile t in cavern)
			{
				Gizmos.color = (t.walkable) ? Color.white : Color.black;
				if (t.isCentroid)
					Gizmos.color = Color.red;
				if (t == playerTile)
					Gizmos.color = Color.blue;
				if (t.isExit)
					Gizmos.color = Color.green;
				if (t.isEnemy)
					Gizmos.color = Color.yellow;
				Gizmos.DrawCube(t.worldPos, Vector3.one * tileDiameter);
			}

			if (triangulation != null && showDelaunay)
			{
				Vector3 worldBottomLeft = transform.position - Vector3.right * cavernWorldSize.x / 2 - Vector3.forward * cavernWorldSize.y / 2;
				foreach (Triangle t in triangulation)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(worldBottomLeft + new Vector3(t.rooms[0].centroid.x, 0f, t.rooms[0].centroid.y), worldBottomLeft + new Vector3(t.rooms[1].centroid.x, 0f, t.rooms[1].centroid.y));
					Gizmos.DrawLine(worldBottomLeft + new Vector3(t.rooms[1].centroid.x, 0f, t.rooms[1].centroid.y), worldBottomLeft + new Vector3(t.rooms[2].centroid.x, 0f, t.rooms[2].centroid.y));
					Gizmos.DrawLine(worldBottomLeft + new Vector3(t.rooms[2].centroid.x, 0f, t.rooms[2].centroid.y), worldBottomLeft + new Vector3(t.rooms[0].centroid.x, 0f, t.rooms[0].centroid.y));
				}
			}

			if(corridors != null && showMST)
			{
				Vector3 worldBottomLeft = transform.position - Vector3.right * cavernWorldSize.x / 2 - Vector3.forward * cavernWorldSize.y / 2;
				foreach(Corridor c in corridors)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(worldBottomLeft + new Vector3(c.connectedRooms[0].centroid.x, 0f, c.connectedRooms[0].centroid.y), worldBottomLeft + new Vector3(c.connectedRooms[1].centroid.x, 0f, c.connectedRooms[1].centroid.y));
				}
			}
		}
	}
}
