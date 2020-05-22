using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainController : MonoBehaviour
{
	public GameObject championSelectionUI;
	public GameObject player3D;
	public GameObject camera2D;
	public GameObject dungeon;
	public GameObject cavern;
	
	private Camera camera3D;

	private int currentDungeon;

    // Start is called before the first frame update
    void Start()
    {
		camera3D = player3D.GetComponent<Camera>();
    }

	public void InitiateStonefang()
	{
		currentDungeon = 1;
		championSelectionUI.SetActive(false);
		camera2D.SetActive(false);
		cavern.SetActive(true);
		dungeon.SetActive(false);
		cavern.GetComponent<CavernGenerator>().GenerateNextCavern();
		//cavern.GetComponent<Pathfinding>().FindPath(GameObject.FindGameObjectWithTag("Enemy").transform.position, player3D.transform.position);
		//dungeon.GetComponent<DungeonGenerator>().GenerateNextDungeon();
		player3D.GetComponent<Player>().GetActiveDungeon();
		foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
		{
			enemy.GetComponent<Pathfinding>().FindPath(enemy.transform.position, player3D.transform.position);
		}
		//GameObject.FindGameObjectWithTag("Enemy").GetComponent<Pathfinding>().FindPath(GameObject.FindGameObjectWithTag("Enemy").transform.position, player3D.transform.position);
	}

	public void GenerateNextDungeon()
	{
		Debug.Log("Dungeon Number: " + currentDungeon);
		currentDungeon++;
		if (currentDungeon <=10)
		{
			cavern.GetComponent<CavernGenerator>().GenerateNextCavern();
			player3D.GetComponent<Player>().GetActiveDungeon();
			foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
			{
				enemy.GetComponent<Pathfinding>().FindPath(enemy.transform.position, player3D.transform.position);
			}
			//GameObject.FindGameObjectWithTag("Enemy").GetComponent<Pathfinding>().FindPath(GameObject.FindGameObjectWithTag("Enemy").transform.position, player3D.transform.position);

		}
		else if (currentDungeon == 15 || currentDungeon == 20 || currentDungeon == 25)
		{
			dungeon.SetActive(false);
			cavern.SetActive(true);
			cavern.GetComponent<CavernGenerator>().GenerateNextCavern();
			player3D.GetComponent<Player>().GetActiveDungeon();
			foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
			{
				enemy.GetComponent<Pathfinding>().FindPath(enemy.transform.position, player3D.transform.position);
			}
			//GameObject.FindGameObjectWithTag("Enemy").GetComponent<Pathfinding>().FindPath(GameObject.FindGameObjectWithTag("Enemy").transform.position, player3D.transform.position);

		}
		else
		{
			cavern.SetActive(false);
			dungeon.SetActive(true);
			dungeon.GetComponent<DungeonGenerator>().GenerateNextDungeon();
			player3D.GetComponent<Player>().GetActiveDungeon();
			foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
			{
				enemy.GetComponent<Pathfinding>().FindPath(enemy.transform.position, player3D.transform.position);
			}
			//GameObject.FindGameObjectWithTag("Enemy").GetComponent<Pathfinding>().FindPath(GameObject.FindGameObjectWithTag("Enemy").transform.position, player3D.transform.position);

		}
	}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
		{
			if (camera2D.activeSelf == false)
			{
				camera3D.enabled = false;
				camera2D.SetActive(true);
			}
			else
			{
				camera3D.enabled = true;
				camera2D.SetActive(false);
			}
			
		}
    }
}
