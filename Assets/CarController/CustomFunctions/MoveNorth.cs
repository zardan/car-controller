using Mellis;
using Mellis.Core.Interfaces;
using UnityEngine;

public class MoveNorth : ClrYieldingFunction
{
    public MoveNorth() : base("åk_mot_norr")
    {
    }

    public override void InvokeEnter(params IScriptType[] arguments)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().MoveNorth();
    }
}