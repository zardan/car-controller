using Mellis;
using Mellis.Core.Interfaces;
using UnityEngine;

public abstract class MoveFunction : ClrYieldingFunction
{
    protected MoveFunction(string name) : base(name)
    {
    }

    public override void InvokeEnter(params IScriptType[] arguments)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (!player)
        {
            Debug.LogError("Did not find player. Check if the map loaded from JSON correctly, and if it is missing a player.");
            PMWrapper.RaiseError("Internt fel: Finns ingen spelare i staden");
            return;
        }

        var playerMovement = player.GetComponent<PlayerMovement>();
        if (!playerMovement)
        {
            Debug.LogError("Player missing movement script. Wrong gameobject with \"Player\" tag?", player);
            PMWrapper.RaiseError("Internt fel: Spelaren saknar rörelseskript");
            return;
        }

        PerformMove(playerMovement);
    }

    protected abstract void PerformMove(PlayerMovement player);
}

public class MoveEast : MoveFunction
{
    public MoveEast() : base("åk_mot_öst")
    {
    }

    protected override void PerformMove(PlayerMovement player)
    {
        player.MoveEast();
    }
}

public class MoveNorth : MoveFunction
{
    public MoveNorth() : base("åk_mot_norr")
    {
    }

    protected override void PerformMove(PlayerMovement player)
    {
        player.MoveNorth();
    }
}

public class MoveSouth : MoveFunction
{
    public MoveSouth() : base("åk_mot_syd")
    {
    }

    protected override void PerformMove(PlayerMovement player)
    {
        player.MoveSouth();
    }
}

public class MoveWest : MoveFunction
{
    public MoveWest() : base("åk_mot_väst")
    {
    }

    protected override void PerformMove(PlayerMovement player)
    {
        player.MoveWest();
    }
}