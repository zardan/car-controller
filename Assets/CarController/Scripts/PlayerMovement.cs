using System;
using System.Collections;
using PM;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	public float PlayerSpeed = 4;
	public bool isCharging;

	private Vector2 currentGridPosition;
	private Vector3 startPosition;
	private Vector3 lastPosition;
	private string startDirection;

	private bool isMoving;
	private Direction currentDirection;

	public bool AtChargeStation;

	private void Update()
	{
		if (!isMoving) return;

		transform.position += transform.up * Time.deltaTime * PMWrapper.speedMultiplier * PlayerSpeed;

		// Calculate differance in distance without sqrt
		if (Mathf.Pow(transform.position.x - lastPosition.x, 2) + Mathf.Pow(transform.position.y - lastPosition.y, 2) > Mathf.Pow(CityGrid.DistanceBetweenPoints, 2))
		{
			transform.position = lastPosition + transform.up * CityGrid.DistanceBetweenPoints;
			isMoving = false;
			lastPosition = transform.position;
			PMWrapper.UnpauseWalker();
		}
	}

	public void Reset()
	{
		transform.position = startPosition;
		isMoving = false;
		SetDirection(startDirection);
		AtChargeStation = false;
	}

	private void SetDirection(string direction)
	{
		if (direction == null)
			direction = "north";

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

	public void Init(Car carData)
	{
		startPosition = transform.position;
		startDirection = carData.direction;

		currentGridPosition = new Vector2(carData.position.x, carData.position.y);
		Reset();
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("ChargeStation"))
		{
			AtChargeStation = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("ChargeStation"))
		{
			AtChargeStation = false;
		}
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
		if (AtChargeStation)
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

	private IEnumerator PlayChargeAnimation()
	{
		var animator = GameObject.FindGameObjectWithTag("ChargeStation").GetComponent<Animator>();
		animator.SetTrigger("Charge");

		yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

		PMWrapper.SetCaseCompleted();
		isCharging = false;
	}
}

public enum Direction
{
	East,
	West,
	North,
	South
}
