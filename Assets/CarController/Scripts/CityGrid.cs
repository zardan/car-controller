/*
 * Place this script on the parent object of all objects that decide the size of the grid. 
 */

using System;
using UnityEngine;
using UnityEngine.Serialization;

public class CityGrid : MonoBehaviour
{
	public static float distanceBetweenPoints;

	static float xMin;
	static float yMin;
	static float zMax;

	[Header("Grid dimensions in number of houses")]
	[FormerlySerializedAs("M")]
	public int horizontal = 9;

	[Space]
	[FormerlySerializedAs("Padding")]
	public float padding = 0.5f;

	[FormerlySerializedAs("N")]
	public int vertical = 8;

	public static Vector3 GetWorldPosition(Position position)
	{
		float worldX = xMin + position.x * distanceBetweenPoints;
		float worldY = yMin + position.y * distanceBetweenPoints;
		float worldZ = -0.01f; //zMax + position.z;

		return new Vector3(worldX, worldY, worldZ);
	}

	public void Awake()
	{
		InitBounds();
	}

	void InitBounds()
	{
		Bounds bounds = CalculateBoundsInChildren(gameObject);
		xMin = bounds.min.x - padding;
		yMin = bounds.min.y - padding;
		zMax = bounds.max.z;

		distanceBetweenPoints = (bounds.size.x + 2 * padding) / horizontal;
	}

	Bounds CalculateBoundsInChildren(GameObject obj)
	{
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

		if (renderers.Length == 0)
		{
			throw new Exception("Could not find any renderers in children of gameobject \"" + obj.name + "\".");
		}

		var bounds = new Bounds(obj.transform.position, Vector3.zero);
		foreach (Renderer rend in renderers)
		{
			bounds.Encapsulate(rend.bounds);
		}

		return bounds;
	}
}