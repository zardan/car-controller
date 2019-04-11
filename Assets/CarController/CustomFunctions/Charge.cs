using Mellis;
using Mellis.Core.Interfaces;
using UnityEngine;

public class Charge : ClrFunction
{
    public Charge() : base("ladda")
    {
    }

    public override IScriptType Invoke(params IScriptType[] arguments)
    {
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>().Charge();

        return null;
    }
}