using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room
{
	public int xPos;
	public int yPos;
	public int width;
	public int height;
	public Vector2Int centroid;

	public Dictionary<Room, Corridor> RoomCorridor = new Dictionary<Room, Corridor>();

	public Room(int x, int y, int width, int height)
	{
		xPos = x;
		yPos = y;
		this.width = width;
		this.height = height;
		centroid.x = Mathf.RoundToInt(x + width / 2);
		centroid.y = Mathf.RoundToInt(y + height / 2);
	}

	public Corridor CreateCorridor(Room otherRoom)
	{
		if (RoomCorridor.ContainsKey(otherRoom))
			return RoomCorridor[otherRoom];

		Corridor newCorridor = new Corridor();
		newCorridor.coordinates = new Vector2Int(xPos + width / 2, otherRoom.yPos + otherRoom.height / 2);
		newCorridor.connectedRooms[0] = otherRoom;
		newCorridor.connectedRooms[1] = this;
		newCorridor.length = Mathf.Abs(otherRoom.xPos - xPos) + Mathf.Abs(otherRoom.yPos - yPos);
		otherRoom.RoomCorridor.Add(this, newCorridor);
		RoomCorridor.Add(otherRoom, newCorridor);

		return newCorridor;
	}
}
