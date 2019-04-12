using System.Collections.Generic;
using UnityEngine;
using PM;
using UnityEngine.Serialization;

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
	[FormerlySerializedAs("PlayerPrefab")]
	public GameObject playerPrefab;
	[FormerlySerializedAs("ObstaclePrefab")]
	public GameObject obstaclePrefab;
	[FormerlySerializedAs("ChargeStationPrefab")]
	public GameObject chargeStationPrefab;

	[HideInInspector]
	[FormerlySerializedAs("Obstacles")]
	public List<GameObject> obstacles = new List<GameObject>();

	[HideInInspector]
	[FormerlySerializedAs("ChargeStations")]
	public List<GameObject> chargeStations = new List<GameObject>();

	private GameObject playerObject;

	public void OnPMCompilerStopped(StopStatus status)
	{
		PlayerMovement playerMovement = null;
		if (playerObject != null)
		{
			playerMovement = playerObject.GetComponent<PlayerMovement>();
		}

		if (status == StopStatus.Finished)
		{
			if (playerMovement != null && playerMovement.atChargeStation)
			{
				if (!playerMovement.isCharging)
				{
					PMWrapper.RaiseTaskError("Podden laddades inte. Kom ihåg att ladda().");
				}
			}
			else
			{
				PMWrapper.RaiseTaskError("Podden kom inte hela vägen fram.");
			}
		}
		if (playerMovement != null && !playerMovement.isCharging)
		{
			playerMovement.Reset();
		}
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
			var positionWithOffset = new Vector3(worldPosition.x, worldPosition.y, -0.18f);
			playerObject = Instantiate(playerPrefab, positionWithOffset, Quaternion.Euler(new Vector3(0, 180, 0)));
			playerObject.GetComponent<PlayerMovement>().Init(car);
		}

		foreach (Station station in levelDefinition.stations)
		{
			Vector3 position = CityGrid.GetWorldPosition(station.position);
			GameObject stationObj = Instantiate(chargeStationPrefab, position, Quaternion.Euler(new Vector3(0, 0, 0)));
			stationObj.GetComponent<ChargeStation>().position = new Vector2(position.x, position.y);
			chargeStations.Add(stationObj);
		}

		if (levelDefinition.obstacles != null)
		{
			foreach (Obstacles obstacle in levelDefinition.obstacles)
			{
				Vector3 position = CityGrid.GetWorldPosition(obstacle.position);
				GameObject obstacleObj = Instantiate(obstaclePrefab, position, Quaternion.identity);
				obstacles.Add(obstacleObj);
			}
		}
	}

	private void DeleteLastLevel()
	{
		Destroy(playerObject);

		foreach (GameObject obj in chargeStations)
		{
			Destroy(obj);
		}
		chargeStations.Clear();

		foreach (GameObject obj in obstacles)
		{
			Destroy(obj);
		}
		obstacles.Clear();
	}
}
