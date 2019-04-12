using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
	static readonly int ANIM_CHARGE = Animator.StringToHash("Charge");

	[FormerlySerializedAs("AtChargeStation")]
	public bool atChargeStation;

	public bool isCharging;

	[FormerlySerializedAs("PlayerSpeed")]
	public float playerSpeed = 4;

	Direction currentDirection;

	Vector2 currentGridPosition;

	bool isMoving;
	Vector3 lastPosition;
	string startDirection;
	Vector3 startPosition;

	public void Reset()
	{
		transform.position = startPosition;
		isMoving = false;
		SetDirection(startDirection);
		atChargeStation = false;
	}

	public void Init(Car carData)
	{
		startPosition = transform.position;
		startDirection = carData.direction;

		currentGridPosition = new Vector2(carData.position.x, carData.position.y);
		Reset();
	}

	void Update()
	{
		if (!isMoving)
		{
			return;
		}

		Transform car = transform;
		Vector3 position = car.position;

		position += car.up * Time.deltaTime * PMWrapper.speedMultiplier * playerSpeed;

		// Calculate difference in distance without sqrt
		if (Mathf.Pow(position.x - lastPosition.x, 2) + Mathf.Pow(position.y - lastPosition.y, 2) >
		    Mathf.Pow(CityGrid.distanceBetweenPoints, 2))
		{
			position = lastPosition + transform.up * CityGrid.distanceBetweenPoints;
			isMoving = false;
			lastPosition = position;
			PMWrapper.ResolveYield();
		}

		transform.position = position;
	}

	void SetDirection(string direction)
	{
		if (direction == null)
		{
			direction = "north";
		}

		switch (direction.ToLower())
		{
		case "east":
			currentDirection = Direction.East;
			transform.localEulerAngles = new Vector3(180, 0, -90);
			break;
		case "west":
			currentDirection = Direction.West;
			transform.localEulerAngles = new Vector3(180, 0, 90);
			break;
		case "north":
			currentDirection = Direction.North;
			transform.localEulerAngles = new Vector3(180, 0, 180);
			break;
		case "south":
			currentDirection = Direction.South;
			transform.localEulerAngles = new Vector3(180, 0, 0);
			break;
		default:
			throw new Exception("The direction \"" + direction + "\" is not supported.");
		}
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("ChargeStation"))
		{
			atChargeStation = true;
		}
	}

	void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("ChargeStation"))
		{
			atChargeStation = false;
		}
	}

	IEnumerator PlayChargeAnimation()
	{
		Animator animator = GameObject.FindGameObjectWithTag("ChargeStation").GetComponent<Animator>();
		animator.SetTrigger(ANIM_CHARGE);

		yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

		PMWrapper.SetCaseCompleted();
		isCharging = false;
	}

	#region Custom functions called from user

	public void MoveEast()
	{
		lastPosition = transform.position;
		currentGridPosition.x += 1;

		SetDirection("east");
		isMoving = true;
	}

	public void MoveWest()
	{
		lastPosition = transform.position;
		currentGridPosition.x -= 1;

		SetDirection("west");
		isMoving = true;
	}

	public void MoveNorth()
	{
		lastPosition = transform.position;
		currentGridPosition.y += 1;

		SetDirection("north");
		isMoving = true;
	}

	public void MoveSouth()
	{
		lastPosition = transform.position;
		currentGridPosition.y -= 1;

		SetDirection("south");
		isMoving = true;
	}

	public void Charge()
	{
		if (atChargeStation)
		{
			isCharging = true;
			StartCoroutine(PlayChargeAnimation());
		}
		else
		{
			PMWrapper.RaiseError("Kan inte ladda här. Se till att köra hela vägen till laddningsstationen.");
		}
	}

	public int CheckPositionX()
	{
		return (int)currentGridPosition.x;
	}

	public int CheckPositionY()
	{
		return (int)currentGridPosition.y;
	}

	#endregion
}

public enum Direction
{
	East,
	West,
	North,
	South
}