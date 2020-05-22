using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corridor
{
	public Vector2Int coordinates;
	public float length;
	public Room[] connectedRooms = new Room[2];
	public List<Triangle> triangles = new List<Triangle>(); 
}
