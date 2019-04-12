using System.Collections.Generic;
using UnityEngine;
using PM;

public class GameController : MonoBehaviour, IPMCompilerStopped, IPMCaseSwitched
{
	static GameController()
	{
		Main.RegisterFunction(new Charge());
		Main.RegisterFunction(new MoveNorth());
		Main.RegisterFunction(new MoveEast());
		Main.RegisterFunction(new MoveSouth());
		Main.RegisterFunction(new MoveWest());

		Main.RegisterLevelDefinitionContract<CarLevelDefinition>();
	}

	[Header("Prefabs")]
	public GameObject PlayerPrefab;
	public GameObject ObstaclePrefab;
	public GameObject ChargeStationPrefab;

	[HideInInspector]
	public List<GameObject> Obstacles = new List<GameObject>();

	[HideInInspector]
	public List<GameObject> ChargeStations = new List<GameObject>();

	private GameObject playerObject;

	public void OnPMCompilerStopped(StopStatus status)
	{
		PlayerMovement playerMovement = null;
		if (playerObject != null)
			playerMovement = playerObject.GetComponent<PlayerMovement>();
		
		if (status == StopStatus.Finished)
		{
			if (playerMovement != null && playerMovement.AtChargeStation)
			{
				if (!playerMovement.isCharging)
					PMWrapper.RaiseTaskError("Podden laddades inte. Kom ihåg att ladda().");
			}
			else
			{
				PMWrapper.RaiseTaskError("Podden kom inte hela vägen fram.");
			}
		}
		if (playerMovement != null && !playerMovement.isCharging)
			playerMovement.Reset();
	}

	public void OnPMCaseSwitched(int caseNumber)
	{
		DeleteLastLevel();
		CreateAssets();
	}

	private void CreateAssets()
	{
		var levelDefinition = (CarLevelDefinition)PMWrapper.currentLevel.levelDefinition;

		foreach (Car car in levelDefinition.cars)
		{
			Vector3 worldPosition = CityGrid.GetWorldPosition(car.position);
			Vector3 positionWithOffset = new Vector3(worldPosition.x, worldPosition.y, -0.18f);
			playerObject = Instantiate(PlayerPrefab, positionWithOffset, Quaternion.Euler(new Vector3(0, 180, 0)));
			playerObject.GetComponent<PlayerMovement>().Init(car);
		}

		foreach (Station station in levelDefinition.stations)
		{
			Vector3 position = CityGrid.GetWorldPosition(station.position);
			GameObject stationObj = Instantiate(ChargeStationPrefab, position, Quaternion.Euler(new Vector3(0, 0, 0)));
			stationObj.GetComponent<ChargeStation>().position = new Vector2(position.x, position.y);
			ChargeStations.Add(stationObj);
		}

		if (levelDefinition.obstacles != null)
		{
			foreach (Obstacles obstacle in levelDefinition.obstacles)
			{
				Vector3 position = CityGrid.GetWorldPosition(obstacle.position);
				GameObject obstacleObj = Instantiate(ObstaclePrefab, position, Quaternion.identity);
				Obstacles.Add(obstacleObj);
			}
		}
	}

	private void DeleteLastLevel()
	{
		Destroy(playerObject);

		foreach (GameObject obj in ChargeStations)
		{
			Destroy(obj);
		}
		ChargeStations.Clear();

		foreach (GameObject obj in Obstacles)
		{
			Destroy(obj);
		}
		Obstacles.Clear();
	}
}
