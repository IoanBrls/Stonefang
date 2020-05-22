using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
	private GameObject dungeonObject;
	private GameObject cavernObject;

	private DungeonGenerator dungeon;
	private CavernGenerator cavern;

	private bool dungeonActive;
	private bool cavernActive;
	private bool GameIsPaused;
	private bool PauseMenuActive;

	private int strength;
	private int dexterity;
	private int intelligence;
	private int constitution;
	private int luck;

	private int HP;
	private int carryWeight;
	private bool hunger;
	private bool thirst;
	private int hours;
	private int minutes;
	private bool daytime;
	private int thirstMinutes;
	private int hungerMinutes;

	public Text HPtext;
	public Text hungerText;
	public Text thirstText;
	public Text timeText;
	public Text daytimeText;

	public Text strengthText;
	public Text dexterityText;
	public Text intelligenceText;
	public Text constitutionText;
	public Text luckText;

	public GameObject characterUI;
	public GameObject pauseMenuUI;
	public GameObject player2D;

	private void Start()
	{
		hunger = false;
		thirst = false;
		minutes = 0;
		hours = 5;
		daytime = true;

		HPtext.text = "Health: " + HP.ToString();
		hungerText.text = "Hunger: NO";
		thirstText.text = "Thirst: NO";
		timeText.text = "Time: " + hours.ToString() + ":" + minutes.ToString() + "0";
		daytimeText.text = "(Day)";
	}

	public void GetActiveDungeon()
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

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
			if (GameIsPaused)
			{
				Resume();
			}
			else
			{
				Show();
			}
		}

		if (pauseMenuUI.GetComponent<PauseMenu>().IsGamePaused())
		{
			PauseMenuActive = true;
		}
		else
		{
			PauseMenuActive = false;
		}

		if (!GameIsPaused && !PauseMenuActive)
		{
			if (dungeonActive)
			{
				if (Input.GetKeyDown(KeyCode.A))
				{
					transform.Rotate(new Vector3(0, -90, 0), Space.World);
					player2D.transform.Rotate(new Vector3(0, -90, 0), Space.World);
				}

				if (Input.GetKeyDown(KeyCode.D))
				{
					transform.Rotate(new Vector3(0, 90, 0), Space.World);
					player2D.transform.Rotate(new Vector3(0, 90, 0), Space.World);
				}

				if (Input.GetKeyDown(KeyCode.W))
				{
					dungeon.MovePlayer(transform);
					foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
					{
						enemy.GetComponent<Pathfinding>().FindPath(enemy.transform.position, transform.position);
						enemy.GetComponent<Enemy>().FollowPath();
					}
					PlayTurn();

				}
			}
			else if (cavernActive)
			{
				if (Input.GetKeyDown(KeyCode.A))
				{
					transform.Rotate(new Vector3(0, -90, 0), Space.World);
					player2D.transform.Rotate(new Vector3(0, -90, 0), Space.World);
				}

				if (Input.GetKeyDown(KeyCode.D))
				{
					transform.Rotate(new Vector3(0, 90, 0), Space.World);
					player2D.transform.Rotate(new Vector3(0, 90, 0), Space.World);
				}

				if (Input.GetKeyDown(KeyCode.W))
				{
					cavern.MovePlayer(transform);
					foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
					{
						enemy.GetComponent<Pathfinding>().FindPath(enemy.transform.position, transform.position);
						enemy.GetComponent<Enemy>().FollowPath();
					}

					PlayTurn();
				}
			}
		}
		
	}

	public void Resume()
	{
		characterUI.SetActive(false);
		Time.timeScale = 1f;
		GameIsPaused = false;
	}

	public void Show()
	{
		characterUI.SetActive(true);
		strengthText.text = "Strength: " + strength.ToString();
		dexterityText.text = "Dexterity: " + dexterity.ToString();
		intelligenceText.text = "Intelligence: " + intelligence.ToString();
		constitutionText.text = "Constitution: " + constitution.ToString();
		luckText.text = "Luck: " + luck.ToString();
		Time.timeScale = 0f;
		GameIsPaused = true;
	}

	private void PlayTurn()
	{
		minutes += 10;
		thirstMinutes += 10;
		hungerMinutes += 10;

		if (minutes == 60)
		{
			hours += 1;
			minutes = 0;

			if(hours == 5)
			{
				daytime = true;
			}

			if(hours == 18)
			{
				daytime = false;
			}

			if(hours == 24)
			{
				hours = 0;
			}
		}

		if(thirstMinutes >= 120)
		{
			thirst = true;
			thirstText.text = "Thirst: YES";
		}
		else
		{
			thirstText.text = "Thirst: NO";
		}

		if(hungerMinutes >= 240)
		{
			hunger = true;
			hungerText.text = "Hunger: YES";
		}
		else
		{
			hungerText.text = "Hunger: NO";
		}

		HPtext.text = "Health: " + HP.ToString();
		if (minutes == 0)
		{
			timeText.text = "Time: " + hours.ToString() + ":" + minutes.ToString() + "0";
		}
		else
		{
			timeText.text = "Time: " + hours.ToString() + ":" + minutes.ToString();
		}

		if (daytime)
		{
			daytimeText.text = "(Day)";
		}
		else
		{
			daytimeText.text = "(Night)";
		}
		
	}

	public void AllocateSpiritPoints(int classIndex)
	{
		int initialSpiritPoints = 30;
		bool done = false;
		//KNIGHT
		if (classIndex == 1)
		{
			while (!done)
			{
				strength = Random.Range(1, 6) + 5;
				initialSpiritPoints -= strength;
				dexterity = Random.Range(1, 6) + 5;
				initialSpiritPoints -= dexterity;
				constitution = Random.Range(1, 6) + 5;
				initialSpiritPoints -= constitution;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += constitution;
					}
				}

				intelligence = Random.Range(1, 3) + 3;
				initialSpiritPoints -= intelligence;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += intelligence;
					}
				}

				luck = Random.Range(1, 3) + 3;
				initialSpiritPoints -= luck;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += luck;
					}
				}
				else
				{
					done = true;
				}
			}
		}
		//ROGUE
		else if (classIndex == 2)
		{
			while (!done)
			{
				luck = Random.Range(1, 6) + 7;
				initialSpiritPoints -= luck;
				dexterity = Random.Range(1, 6) + 5;
				initialSpiritPoints -= dexterity;
				strength = Random.Range(1, 3) + 3;
				initialSpiritPoints -= strength;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += strength;
					}
				}

				intelligence = Random.Range(1, 3) + 3;
				initialSpiritPoints -= intelligence;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += intelligence;
					}
				}

				constitution = Random.Range(1, 3) + 1;
				initialSpiritPoints -= constitution;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += constitution;
					}
				}
				else
				{
					done = true;
				}
			}
		}
		//SORCERER
		else if (classIndex == 3)
		{
			while (!done)
			{
				intelligence = Random.Range(1, 6) + 7;
				initialSpiritPoints -= intelligence;
				luck = Random.Range(1, 6) + 5;
				initialSpiritPoints -= luck;
				strength = Random.Range(1, 3) + 3;
				initialSpiritPoints -= strength;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += strength;
					}
				}

				dexterity = Random.Range(1, 3) + 3;
				initialSpiritPoints -= dexterity;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += dexterity;
					}
				}

				constitution = Random.Range(1, 3) + 3;
				initialSpiritPoints -= constitution;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += constitution;
					}
				}
				else
				{
					done = true;
				}
			}
		}
		//BARBARIAN
		else if (classIndex == 4)
		{
			while (!done)
			{
				strength = Random.Range(1, 6) + 7;
				initialSpiritPoints -= strength;
				constitution = Random.Range(1, 6) + 7;
				initialSpiritPoints -= constitution;
				dexterity = Random.Range(1, 3) + 3;
				initialSpiritPoints -= dexterity;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += dexterity;
					}
				}

				luck = Random.Range(1, 3) + 3;
				initialSpiritPoints -= luck;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += luck;
					}
				}

				intelligence = Random.Range(1, 3) + 1;
				initialSpiritPoints -= intelligence;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += intelligence;
					}
				}
				else
				{
					done = true;
				}
			}

		}
		//ADVENTURER
		else if (classIndex == 5)
		{
			while (!done)
			{
				luck = Random.Range(1, 6) + 7;
				initialSpiritPoints -= luck;
				intelligence = Random.Range(1, 6) + 5;
				initialSpiritPoints -= intelligence;
				dexterity = Random.Range(1, 3) + 3;
				initialSpiritPoints -= dexterity;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += dexterity;
					}
				}

				constitution = Random.Range(1, 3) + 3;
				initialSpiritPoints -= constitution;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += constitution;
					}
				}

				strength = Random.Range(1, 3) + 1;
				initialSpiritPoints -= strength;

				if (initialSpiritPoints < 0)
				{
					int extrordinaryCheck = Random.Range(1, 5);
					if (extrordinaryCheck != 1)
					{
						done = false;
						initialSpiritPoints = 30;
						continue;
					}
					else
					{
						initialSpiritPoints += strength;
					}
				}
				else
				{
					done = true;
				}
			}

		}

		HP = 30 + constitution * 4;

		HPtext.text = "Health: " + HP.ToString();
		carryWeight = 20 + constitution * 3;
	}
}
