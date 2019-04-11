using Mellis;
using Mellis.Core.Interfaces;
using UnityEngine;

public class MoveEast : ClrYieldingFunction
{
    public MoveEast() : base("åk_mot_öst")
    {
    }

    public override void InvokeEnter(params IScriptType[] arguments)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().MoveEast();
    }
}