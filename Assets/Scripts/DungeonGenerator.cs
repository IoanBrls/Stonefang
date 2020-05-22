using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
	//public LayerMask unwalkableLayer;
	private Vector2 dungeonWorldSize;
	private float tileRadius;

	public GameObject dungeonObject;
	public GameObject dungeonObject2D;
	public GameObject[] wall;
	public GameObject floor;
	public GameObject player;
	public GameObject exit;
	public GameObject ceiling;
	public GameObject floor2D;
	public GameObject wall2D;
	public GameObject player2D;
	public GameObject exit2D;

	public GameObject[] enemy;
	public int maxEnemies;
	//public GameObject enemy2D;

	public GameObject controller;

	public int maxRoomSize;
	public int minRoomSize;
	public int maxRooms;

	private Tile[,] dungeon;
	private int[,] dungeonInt;
	private List<Room> rooms;
	private List<Corridor> corridors;
	private List<Corridor> MSTcorridors;
	private List<Triangle> triangulation;

	private float tileDiameter;
	private int dungeonSizeX, dungeonSizeY;

	private bool showDelaunay;
	private bool showMST;
	private bool showMix;

	public List<Tile> path;

	private void Start()
	{
		dungeonWorldSize = new Vector2(256, 256);
		tileRadius = 0.5f;
		tileDiameter = tileRadius * 2;
		dungeonSizeX = Mathf.RoundToInt(dungeonWorldSize.x / tileDiameter);
		dungeonSizeY = Mathf.RoundToInt(dungeonWorldSize.y / tileDiameter);
		//Debug.Log("Dungeon Width: " + dungeonSizeX);
		//Debug.Log("Dungeon Height: " + dungeonSizeY);
		//GenerateRooms();
		//CreateDungeon();
		//VisualizeDungeon();
		//VisualizeDungeon2D();
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
			return dungeonSizeX * dungeonSizeY;
		}
	}

	public void GenerateNextDungeon()
	{
		GenerateRooms();
		CreateDungeon();
		VisualizeDungeon();
		VisualizeDungeon2D();
	}

	private void GenerateRooms()
	{
		dungeonInt = new int[dungeonSizeX, dungeonSizeY];
		rooms = new List<Room>();

		for (int i = 0; i < dungeonSizeX; i++)
		{
			for (int j = 0; j < dungeonSizeY; j++)
			{
				dungeonInt[i, j] = 1;
			}
		}

		for (int i = 0; i < maxRooms; i++)
		{
			Room roomInstance = CreateRoom();

			if (roomInstance == null)
			{
				Debug.Log("Cannot make more rooms");
				Debug.Log("Created rooms: " + rooms.Count);
				break;
			}
			else
			{
				rooms.Add(roomInstance);
			}
		}

		foreach (Room room in rooms)
		{
			for (int i = 0; i < room.width; i++)
			{
				for (int j = 0; j < room.height; j++)
				{
					dungeonInt[room.xPos + i, room.yPos + j] = 0;
				}
			}
		}

		CreateCorridors();

		int randomRoomPlayer = Random.Range(0, rooms.Count - 1);
		int randomWidth = Random.Range(1, rooms[randomRoomPlayer].width - 1);
		int randomHeight = Random.Range(1, rooms[randomRoomPlayer].height - 1);
		Vector2Int playerPosition = new Vector2Int(rooms[randomRoomPlayer].xPos + randomWidth, rooms[randomRoomPlayer].yPos + randomHeight);

		int randomRoomExit = Random.Range(0, rooms.Count - 1);
		while (randomRoomExit == randomRoomPlayer)
		{
			randomRoomExit = Random.Range(0, rooms.Count - 1);
		}
		int randomWidthExit = Random.Range(1, rooms[randomRoomExit].width - 1);
		int randomHeightExit = Random.Range(1, rooms[randomRoomExit].height - 1);
		Vector2Int exitPosition = new Vector2Int(rooms[randomRoomExit].xPos + randomWidthExit, rooms[randomRoomExit].yPos + randomHeightExit);

		for (int i = 0; i < maxEnemies; i++)
		{
			int randomRoomEnemy = Random.Range(0, rooms.Count - 1);
			while (randomRoomEnemy == randomRoomPlayer)
			{
				randomRoomEnemy = Random.Range(0, rooms.Count - 1);
			}
			int randomWidthEnemy = Random.Range(0, rooms[randomRoomEnemy].width - 1);
			int randomHeightEnemy = Random.Range(0, rooms[randomRoomEnemy].height - 1);
			Vector2Int enemyPosition = new Vector2Int(rooms[randomRoomEnemy].xPos + randomWidthEnemy, rooms[randomRoomEnemy].yPos + randomHeightEnemy);

			dungeonInt[enemyPosition.x, enemyPosition.y] = 5;
		}

		dungeonInt[playerPosition.x, playerPosition.y] = 3;
		dungeonInt[exitPosition.x, exitPosition.y] = 4;

	}

	private Room CreateRoom()
	{
		Room newRoom = null;

		for (int i=0; i < Mathf.Pow(maxRooms, 2); i++)
		{
			int roomSizeX = Random.Range(minRoomSize, maxRoomSize + 1);
			int roomSizeY = Random.Range(minRoomSize, maxRoomSize + 1);
			int xCoord = Random.Range(1, dungeonSizeX - roomSizeX);
			int yCoord = Random.Range(1, dungeonSizeY - roomSizeY);

			if (!CheckOverlap(xCoord, yCoord, roomSizeX, roomSizeY))
			{
				newRoom = new Room(xCoord, yCoord, roomSizeX, roomSizeY);
				break;
			}
		}

		return newRoom;
	}

	private bool CheckOverlap(int xCoord, int yCoord, int roomSizeX, int roomSizeY)
	{
		foreach (Room room in rooms)
		{
			if (Mathf.Abs(room.xPos - xCoord + (room.width - roomSizeX) * 0.5f) < (room.width + roomSizeX)*0.7f &&
				Mathf.Abs(room.yPos - yCoord + (room.height - roomSizeY) * 0.5f) < (room.height + roomSizeY) * 0.7f)
			{
				return true;
			}
		}
		return false;
	}

	private void CreateCorridors()
	{
		DelaunayTriangles();
		//Debug.Log("Triangles: " + triangulation.Count);
		MinimumSpanningTree();
		//Debug.Log("Corridors: " + corridors.Count);
		
		foreach(Triangle triangle in triangulation)
		{
			foreach (Corridor corridor in triangle.corridors)
			{
				if (!corridors.Contains(corridor) && Random.Range(0, 100) < 35)
				{
					corridors.Add(corridor);
				}
			}
		}

		//Debug.Log("New Corridors: " + corridors.Count);

		foreach(Corridor corridor in corridors)
		{
			Vector2Int correction = new Vector2Int(0, 0);

			if (corridor.connectedRooms[0].xPos == corridor.coordinates.x + 1)
				correction.x = 2;
			else if (corridor.connectedRooms[0].xPos + corridor.connectedRooms[0].width == corridor.coordinates.x)
				correction.x = -2;
			else if (corridor.connectedRooms[0].xPos == corridor.coordinates.x)
				correction.x = 1;
			else if (corridor.connectedRooms[0].xPos + corridor.connectedRooms[0].width == corridor.coordinates.x + 1)
				correction.x = -1;

			if (corridor.connectedRooms[1].yPos == corridor.coordinates.y + 1)
				correction.y = 2;
			else if (corridor.connectedRooms[1].yPos + corridor.connectedRooms[1].height == corridor.coordinates.y)
				correction.y = -2;
			else if (corridor.connectedRooms[1].yPos == corridor.coordinates.y)
				correction.y = 1;
			else if (corridor.connectedRooms[1].yPos + corridor.connectedRooms[1].height == corridor.coordinates.y + 1)
				correction.y = -1;

			corridor.coordinates += correction;

			int startX = corridor.connectedRooms[0].xPos + corridor.connectedRooms[0].width / 2;
			int endX = corridor.coordinates.x;

			if( startX > endX)
			{
				int temp = startX;
				startX = endX;
				endX = temp;
			}

			int startY = corridor.connectedRooms[1].yPos + corridor.connectedRooms[1].height / 2;
			int endY = corridor.coordinates.y;

			if (startY > endY)
			{
				int temp = startY;
				startY = endY;
				endY = temp;
			}

			for (int x = startX; x <= endX; x++)
			{
				dungeonInt[x, corridor.coordinates.y] = 0;
			}

			for(int y = startY; y <= endY; y++)
			{
				dungeonInt[corridor.coordinates.x, y] = 0;
			}

		}

		foreach (Room room in rooms)
		{
			dungeonInt[room.centroid.x, room.centroid.y] = 2;
		}
	}

	private void DelaunayTriangles()
	{
		triangulation = new List<Triangle>();
		Triangle loot = LootTriangle();
		triangulation.Add(loot);

		foreach (Room room in rooms)
		{
			List<Triangle> badTriangles = new List<Triangle>();

			foreach(Triangle triangle in triangulation)
			{
				if (triangle.IsContaining(room))
				{
					badTriangles.Add(triangle);
				}
			}

			List<Corridor> polygon = new List<Corridor>();
			foreach(Triangle badTriangle in badTriangles)
			{
				foreach(Corridor corridor in badTriangle.corridors)
				{
					if (corridor.triangles.Count == 1)
					{
						polygon.Add(corridor);
						corridor.triangles.Remove(badTriangle);
						continue;
					}

					foreach(Triangle triangle in corridor.triangles)
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

			foreach(Corridor corridor in polygon)
			{
				Triangle newTriangle = new Triangle(corridor.connectedRooms[0], corridor.connectedRooms[1], room);
				triangulation.Add(newTriangle);
			}
		}
		//Debug.Log("Triangles before: " + triangulation.Count);

		for (int i = triangulation.Count - 1; i >= 0; i--){
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
		MSTcorridors = new List<Corridor>();

		connectedRooms.Add(rooms[0]);

		while(connectedRooms.Count < rooms.Count)
		{
			var minLength = new KeyValuePair<Room, Corridor>();
			List<Corridor> deleteList = new List<Corridor>();

			foreach (Room room in connectedRooms)
			{
				foreach(var pair in room.RoomCorridor)
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
			{
				MSTcorridors.Add(minLength.Value);
				corridors.Add(minLength.Value);
			}
		}
	}

	private Triangle LootTriangle()
	{
		Vector2Int[] vertices = {new Vector2Int(dungeonSizeX*2, dungeonSizeY),
								 new Vector2Int(-dungeonSizeX*2, dungeonSizeY),
								 new Vector2Int(0, -2*dungeonSizeY)};

		Room[] tempRooms = new Room[3];

		for (int i = 0; i < 3; i++)
		{
			tempRooms[i] = new Room(vertices[i].x, vertices[i].y, 2, 2);
		}

		return new Triangle(tempRooms[0], tempRooms[1], tempRooms[2]);

	}

	private void CreateDungeon()
	{
		dungeon = new Tile[dungeonSizeX, dungeonSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * dungeonWorldSize.x / 2 - Vector3.forward * dungeonWorldSize.y / 2;


		for (int x = 0; x < dungeonSizeX; x++)
		{
			for (int y = 0; y < dungeonSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * tileDiameter + tileRadius) + Vector3.forward * (y * tileDiameter + tileRadius);
				dungeon[x, y] = new Tile(worldPoint, x, y);
				
				if (dungeonInt[x, y] == 1)
				{
					dungeon[x, y].walkable = false;
				}
				else
				{
					dungeon[x, y].walkable = true;
				}

				if (dungeonInt[x, y] == 2)
				{
					dungeon[x, y].isCentroid = true;
				}
				else
				{
					dungeon[x, y].isCentroid = false;
				}

				if (dungeonInt[x, y] == 3)
				{
					dungeon[x, y].isPlayer = true;
				}
				else
				{
					dungeon[x, y].isPlayer = false;
				}

				if (dungeonInt[x, y] == 4)
				{
					dungeon[x, y].isExit = true;
				}
				else
				{
					dungeon[x, y].isExit = false;
				}

				if (dungeonInt[x, y] == 5)
				{
					dungeon[x, y].isEnemy = true;
				}
				else
				{
					dungeon[x, y].isEnemy= false;
				}
			}
		}
	}

	public Tile TileFromWorldPoint(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + dungeonWorldSize.x / 2) / dungeonWorldSize.x;
		float percentY = (worldPosition.z + dungeonWorldSize.y / 2) / dungeonWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((dungeonSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((dungeonSizeY - 1) * percentY);
		return dungeon[x, y];
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

				if (checkX >= 0 && checkX < dungeonSizeX && checkY >= 0 && checkY < dungeonSizeY)
				{
					neighbours.Add(dungeon[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	private void VisualizeDungeon()
	{
		if( dungeonObject.transform.childCount != 0)
		{
			for (int i = 0; i < dungeonObject.transform.childCount; i++)
			{
				Destroy(dungeonObject.transform.GetChild(i).gameObject);
			}
		}

		if (dungeon != null) 
		{

			for (int x = 0; x < dungeonSizeX; x++)
			{
				for (int y = 0; y < dungeonSizeY; y++)
				{
					if (x==0 || y==0 || x == dungeonSizeX -1 || y == dungeonSizeY - 1)
					{
						GameObject newWall = Instantiate(wall[0], new Vector3(dungeon[x, y].worldPos.x, 2f, dungeon[x, y].worldPos.z), Quaternion.identity);
						newWall.transform.parent = dungeonObject.transform;
						continue;
					}

					if (!dungeon[x,y].walkable && CheckNeightBoursWalkbability(x, y))
					{
						int wallIndex = Random.Range(0, wall.Length - 1);
						GameObject newWall = Instantiate(wall[wallIndex], new Vector3(dungeon[x,y].worldPos.x, 2f, dungeon[x,y].worldPos.z), Quaternion.identity);
						newWall.transform.parent = dungeonObject.transform;
					}

					if (dungeon[x, y].walkable)
					{
						GameObject newCeiling = Instantiate(ceiling, new Vector3(dungeon[x, y].worldPos.x, 4f, dungeon[x, y].worldPos.z), Quaternion.identity);
						newCeiling.transform.parent = dungeonObject.transform;
					}

					if (dungeon[x, y].isPlayer)
					{
						player.transform.position = new Vector3(dungeon[x, y].worldPos.x, 1f, dungeon[x, y].worldPos.z);
					}

					if (dungeon[x, y].isExit)
					{
						GameObject newExit = Instantiate(exit, new Vector3(dungeon[x, y].worldPos.x, 1f, dungeon[x, y].worldPos.z), Quaternion.identity);
						newExit.transform.parent = dungeonObject.transform;
					}

					if (dungeon[x, y].isEnemy)
					{
						int enemyIndex = Random.Range(0, enemy.Length - 1);
						GameObject newEnemy = Instantiate(enemy[enemyIndex], new Vector3(dungeon[x, y].worldPos.x, 1f, dungeon[x, y].worldPos.z), Quaternion.identity);
						newEnemy.transform.parent = dungeonObject.transform;
					}
				}
			}
		}
		GenerateGrid();
	}

	private void VisualizeDungeon2D()
	{
		if (dungeonObject2D.transform.childCount != 0)
		{
			for (int i = 0; i < dungeonObject2D.transform.childCount; i++)
			{
				Destroy(dungeonObject2D.transform.GetChild(i).gameObject);
			}
		}

		if (dungeon != null)
		{
			for (int x = 0; x < dungeonSizeX; x++)
			{
				for (int y = 0; y < dungeonSizeY; y++)
				{
					if (x == 0 || y == 0 || x == dungeonSizeX - 1 || y == dungeonSizeY - 1)
					{
						GameObject newWall = Instantiate(wall2D, new Vector3(dungeon[x, y].worldPos.x - 400, 0f, dungeon[x, y].worldPos.z), Quaternion.Euler(new Vector3(90,0,0)));
						newWall.transform.parent = dungeonObject2D.transform;
						continue;
					}

					if (!dungeon[x, y].walkable)
					{
						GameObject newWall = Instantiate(wall2D, new Vector3(dungeon[x, y].worldPos.x - 400, 0f, dungeon[x, y].worldPos.z), Quaternion.Euler(new Vector3(90, 0, 0)));
						newWall.transform.parent = dungeonObject2D.transform;
					}

					if (dungeon[x, y].walkable)
					{
						GameObject newFloor = Instantiate(floor2D, new Vector3(dungeon[x, y].worldPos.x - 400, 0f, dungeon[x, y].worldPos.z), Quaternion.Euler(new Vector3(90, 0, 0)));
						newFloor.transform.parent = dungeonObject2D.transform;
					}

					if (dungeon[x, y].isPlayer)
					{
						player2D.transform.position = new Vector3(dungeon[x, y].worldPos.x - 400, 0f, dungeon[x, y].worldPos.z);
					}

					if (dungeon[x, y].isExit)
					{
						GameObject newExit = Instantiate(exit2D, new Vector3(dungeon[x, y].worldPos.x - 400, 0f, dungeon[x, y].worldPos.z), Quaternion.Euler(new Vector3(90, 0, 0)));
						newExit.transform.parent = dungeonObject2D.transform;
					}
				}
			}
		}
	}

	private void GenerateGrid()
	{
		MeshRenderer floorRenderer = floor.GetComponent<MeshRenderer>();
		//Texture2D gridImage = (Texture2D)floorRenderer.sharedMaterial.mainTexture;
		Texture gridImage3D = floorRenderer.material.mainTexture;
		Texture2D gridImage = new Texture2D(dungeonSizeX, dungeonSizeY, TextureFormat.RGBA32, false);
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture renderTexture = new RenderTexture(dungeonSizeX, dungeonSizeY, 32);
		Graphics.Blit(gridImage3D, renderTexture);
		RenderTexture.active = renderTexture;
		gridImage.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		gridImage.Apply();

		Color gridColor = Color.white;
		Color borderColor = Color.black;
		Collider floorCollider = floor.GetComponent<Collider>();
		//Vector3 floorSize = new Vector3(floorCollider.bounds.size.x, floorCollider.bounds.size.z);
		for (int x = 0; x < dungeonSizeX; x++)
		{
			for (int y = 0; y < dungeonSizeY; y++)
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
		for (int i = -1; i < 2; i++)
		{
			for (int j = -1; j< 2; j++)
			{
				if ((i == 0 && j == 0) || !dungeon[x + i, y + j].walkable)
					continue;

				if (dungeon[x + i, y + j].walkable)
					return true;
			}
		}
		return false;
	}

	public void MovePlayer(Transform playerTransform)
	{
		Vector3 playerRotation = playerTransform.rotation.eulerAngles;
		int playerX = -1;
		int playerY = -1;

		for (int x = 0; x < dungeonSizeX; x++)
		{
			for (int y = 0; y < dungeonSizeY; y++)
			{
				if (dungeon[x, y].isPlayer)
				{
					playerX = x;
					playerY = y;
					x = dungeonSizeX;
					y = dungeonSizeY;
					break;
				}
			}
		}
		//Debug.Log("Player Rotation:" + playerRotation.y);
		if (playerRotation.y > 179 && playerRotation.y < 181)
		{
			if (dungeon[playerX, playerY - 1].isExit)
			{
				controller.GetComponent<MainController>().GenerateNextDungeon();
			}
			else if (dungeon[playerX, playerY - 1].walkable)
			{
				dungeon[playerX, playerY].isPlayer = false;
				dungeon[playerX, playerY - 1].isPlayer = true;
				player.transform.position = new Vector3(dungeon[playerX, playerY - 1].worldPos.x, 1f, dungeon[playerX, playerY - 1].worldPos.z);
				player2D.transform.position = new Vector3(dungeon[playerX, playerY - 1].worldPos.x - 400, 0f, dungeon[playerX, playerY - 1].worldPos.z);
			}
		}
		else if (playerRotation.y > 89 && playerRotation.y < 91)
		{
			if (dungeon[playerX + 1, playerY].isExit)
			{
				controller.GetComponent<MainController>().GenerateNextDungeon();
			}
			else if (dungeon[playerX + 1, playerY].walkable)
			{
				dungeon[playerX, playerY].isPlayer = false;
				dungeon[playerX + 1, playerY].isPlayer = true;
				player.transform.position = new Vector3(dungeon[playerX + 1, playerY].worldPos.x, 1f, dungeon[playerX + 1, playerY].worldPos.z);
				player2D.transform.position = new Vector3(dungeon[playerX + 1, playerY].worldPos.x - 400, 0f, dungeon[playerX + 1, playerY].worldPos.z);
			}
		}
		else if (playerRotation.y > 269 && playerRotation.y < 271)
		{
			if (dungeon[playerX - 1, playerY].isExit)
			{
				controller.GetComponent<MainController>().GenerateNextDungeon();
			}
			else if (dungeon[playerX - 1, playerY].walkable)
			{
				dungeon[playerX, playerY].isPlayer = false;
				dungeon[playerX - 1, playerY].isPlayer = true;
				player.transform.position = new Vector3(dungeon[playerX - 1, playerY].worldPos.x, 1f, dungeon[playerX - 1, playerY].worldPos.z);
				player2D.transform.position = new Vector3(dungeon[playerX - 1, playerY].worldPos.x - 400, 0f, dungeon[playerX - 1, playerY].worldPos.z);
			}
		}
		else if (playerRotation.y < 1)
		{
			if (dungeon[playerX, playerY + 1].isExit)
			{
				controller.GetComponent<MainController>().GenerateNextDungeon();
			}
			else if (dungeon[playerX, playerY + 1].walkable)
			{
				dungeon[playerX, playerY].isPlayer = false;
				dungeon[playerX, playerY + 1].isPlayer = true;
				player.transform.position = new Vector3(dungeon[playerX, playerY + 1].worldPos.x, 1f, dungeon[playerX, playerY + 1].worldPos.z);
				player2D.transform.position = new Vector3(dungeon[playerX, playerY + 1].worldPos.x - 400, 0f, dungeon[playerX, playerY + 1].worldPos.z);
			}
		}
	}
	
	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(dungeonWorldSize.x, 1, dungeonWorldSize.y));

		if (dungeon != null)
		{
			Tile playerTile = TileFromWorldPoint(player.transform.position);
			foreach(Tile t in dungeon)
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
				Vector3 worldBottomLeft = transform.position - Vector3.right * dungeonWorldSize.x / 2 - Vector3.forward * dungeonWorldSize.y / 2;
				foreach (Triangle t in triangulation)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(worldBottomLeft + new Vector3(t.rooms[0].centroid.x, 0f, t.rooms[0].centroid.y), worldBottomLeft + new Vector3(t.rooms[1].centroid.x, 0f, t.rooms[1].centroid.y));
					Gizmos.DrawLine(worldBottomLeft + new Vector3(t.rooms[1].centroid.x, 0f, t.rooms[1].centroid.y), worldBottomLeft + new Vector3(t.rooms[2].centroid.x, 0f, t.rooms[2].centroid.y));
					Gizmos.DrawLine(worldBottomLeft + new Vector3(t.rooms[2].centroid.x, 0f, t.rooms[2].centroid.y), worldBottomLeft + new Vector3(t.rooms[0].centroid.x, 0f, t.rooms[0].centroid.y));
				}
			}

			if (corridors != null && showMST)
			{
				Vector3 worldBottomLeft = transform.position - Vector3.right * dungeonWorldSize.x / 2 - Vector3.forward * dungeonWorldSize.y / 2;
				foreach (Corridor c in MSTcorridors)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(worldBottomLeft + new Vector3(c.connectedRooms[0].centroid.x, 0f, c.connectedRooms[0].centroid.y), worldBottomLeft + new Vector3(c.connectedRooms[1].centroid.x, 0f, c.connectedRooms[1].centroid.y));
				}
			}

			if (corridors != null && showMix)
			{
				Vector3 worldBottomLeft = transform.position - Vector3.right * dungeonWorldSize.x / 2 - Vector3.forward * dungeonWorldSize.y / 2;
				foreach (Corridor c in corridors)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawLine(worldBottomLeft + new Vector3(c.connectedRooms[0].centroid.x, 0f, c.connectedRooms[0].centroid.y), worldBottomLeft + new Vector3(c.connectedRooms[1].centroid.x, 0f, c.connectedRooms[1].centroid.y));
				}
			}
		}

		
	}
	
}


