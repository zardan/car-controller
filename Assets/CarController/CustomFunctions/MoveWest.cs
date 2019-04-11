using Mellis;
using Mellis.Core.Interfaces;
using UnityEngine;

public class MoveWest : ClrYieldingFunction
{
    public MoveWest() : base("åk_mot_väst")
    {
    }

    public override void InvokeEnter(params IScriptType[] arguments)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().MoveWest();
    }
}