/*
 * Place this script on the parent object of all objects that decide the size of the grid. 
 */

using UnityEngine;
using UnityEngine.Serialization;

public class CityGrid : MonoBehaviour
{
	[Header("Grid dimensions in number of houses")]
	[FormerlySerializedAs("M")]
	public int horizontal = 9;
	[FormerlySerializedAs("N")]
	public int vertical = 8;

	[Space]
	[FormerlySerializedAs("Padding")]
	public float padding = 0.5f;

	private static float xMin;
	private static float yMin;
	private static float zMax;

	public static float distanceBetweenPoints;

	private void InitBounds()
	{
		Bounds bounds = CalculateBoundsInChildren(gameObject);
		xMin = bounds.min.x - padding;
		yMin = bounds.min.y - padding;
		zMax = bounds.max.z;

		distanceBetweenPoints = (bounds.size.x + 2 * padding) / horizontal;
	}

	public static Vector3 GetWorldPosition(Position position)
	{
		float worldX = xMin + position.x * distanceBetweenPoints;
		float worldY = yMin + position.y * distanceBetweenPoints;
		float worldZ = -0.01f;//zMax + position.z;

		return new Vector3(worldX, worldY, worldZ);
	}

	public void Awake()
	{
		InitBounds();
	}

	private Bounds CalculateBoundsInChildren(GameObject obj)
	{
		Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

		if (renderers.Length == 0)
			throw new System.Exception("Could not find any renderers in children of gameobject \"" + obj.name + "\".");

		var bounds = new Bounds(obj.transform.position, Vector3.zero);
		foreach (Renderer rend in renderers)
		{
			bounds.Encapsulate(rend.bounds);
		}

		return bounds;
	}
}
