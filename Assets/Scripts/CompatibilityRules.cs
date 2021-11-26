using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CompatibilityRules<Position, State> : ScriptableObject
{
    public bool useLazyBaking;
    
    public bool AreCompatible(Position p1, State s1, Position p2, State s2)
    {
        bool areCompatible = false;

        if (!useLazyBaking)
        {
            areCompatible = AreCompatibleCompute(p1, s1, p2, s2);
        }
        else
            areCompatible = AreCompatibleLazy(p1, s1, p2, s2);

        return areCompatible;
    }

    public bool AreCompatibleLazy(Position p1, State s1, Position p2, State s2)
    {
        bool areCompatible = false;
        if (TryGetRegisteredRule(p1, s1, p2, s2, out areCompatible))
        {
            return areCompatible;
        }
        else
        {
            areCompatible = AreCompatibleCompute(p1, s1, p2, s2);
            RegisterRule(p1, s1, p2, s2, areCompatible);
            return areCompatible;
        } 
    }

    public virtual void RegisterRule(Position p1, State s1, Position p2, State s2, bool compatibility) { }

    public virtual bool TryGetRegisteredRule(Position p1, State s1, Position p2, State s2, out bool areCompatible) {
        areCompatible = false;
        return false;
    }
    public abstract bool AreCompatibleCompute(Position p1, State s1, Position p2, State s2);
}