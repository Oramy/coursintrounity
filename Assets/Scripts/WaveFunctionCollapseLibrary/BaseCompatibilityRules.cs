using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseCompatibilityRules : ScriptableObject
{
    public bool useLazyBaking = false;

    public abstract bool BakeRules();
}
