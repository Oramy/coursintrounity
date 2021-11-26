using System.Collections.Generic;
using UnityEngine;

public abstract class Topology<Position> : ScriptableObject
{
    public abstract List<Position> ComputeNeighborhood(Position position);
}
