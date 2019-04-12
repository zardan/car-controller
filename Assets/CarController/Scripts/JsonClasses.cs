using System.Collections.Generic;

public class Position
{
	public float x { get; set; }
	public float y { get; set; }
	public float z { get; set; }
}

public class Car
{
	public Position position { get; set; }
	public string direction { get; set; }
}

public class Station
{
	public Position position { get; set; }
}

public class Obstacles
{
	public Position position { get; set; }
}

public class CarLevelDefinition : LevelDefinition
{
	public List<Car> cars { get; set; }
	public List<Station> stations { get; set; }
	public List<Obstacles> obstacles { get; set; }
}