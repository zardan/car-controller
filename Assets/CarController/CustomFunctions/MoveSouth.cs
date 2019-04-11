using Mellis;
using Mellis.Core.Interfaces;
using UnityEngine;

public class MoveSouth : ClrYieldingFunction
{
    public MoveSouth() : base("åk_mot_syd")
    {
    }

    public override void InvokeEnter(params IScriptType[] arguments)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().MoveSouth();
    }
}