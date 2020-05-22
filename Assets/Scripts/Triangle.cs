using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Triangle
{
	public List<Room> rooms = new List<Room>();
	public List<Corridor> corridors = new List<Corridor>();

	private Vector3 circumcenter = Vector3.zero;
	private float radius;

	public Triangle(Room r1, Room r2, Room r3)
	{
		rooms.Add(r1);
		rooms.Add(r2);
		rooms.Add(r3);

		corridors.Add(r1.CreateCorridor(r2));
		corridors[0].triangles.Add(this);
		corridors.Add(r2.CreateCorridor(r3));
		corridors[1].triangles.Add(this);
		corridors.Add(r3.CreateCorridor(r1));
		corridors[2].triangles.Add(this);
	}

	public bool IsContaining(Room room)
	{
		if (circumcenter == Vector3.zero)
		{
			Vector2Int[] vertices = new Vector2Int[3];
			for (int i = 0; i < rooms.Count; i++)
			{
				vertices[i].x = rooms[i].xPos;
				vertices[i].y = rooms[i].yPos;
			}

			int a = vertices[1].x - vertices[0].x;
			int b = vertices[1].y - vertices[0].y;
			int c = vertices[2].x - vertices[0].x;
			int d = vertices[2].y - vertices[0].y;

			int aux1 = a * (vertices[0].x + vertices[1].x) + b * (vertices[0].y + vertices[1].y);
			int aux2 = c * (vertices[0].x + vertices[2].x) + d * (vertices[0].y + vertices[2].y);
			int div = 2 * (a * (vertices[2].y - vertices[1].y) - b * (vertices[2].x - vertices[1].x));

			if (Mathf.Abs(div) < float.Epsilon)
			{
				Debug.Log("Divided by Zero: " + div);
				return false;
			}

			circumcenter = new Vector3((d * aux1 - b * aux2) / div, 0, (a * aux2 - c * aux1) / div);
			radius = Mathf.Sqrt((circumcenter.x - vertices[0].x) * (circumcenter.x - vertices[0].x) + (circumcenter.z - vertices[0].y) * (circumcenter.z - vertices[0].y));
		}

		if (Vector3.Distance(new Vector3(room.xPos, 0, room.yPos), circumcenter) > radius)
			return false;
		return true;
	}

}
