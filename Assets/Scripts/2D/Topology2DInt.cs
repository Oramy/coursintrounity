using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Topology2DInt", menuName = "WFC/2D/2D Grid Topology", order = 1)]
public class Topology2DInt : Topology<Vector2Int>
{
    public bool infinite = false;
    public Vector2Int minimum;
    public Vector2Int maximum;

    public Vector2Int[] deltas = { new Vector2Int(-1, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
    public override List<Vector2Int> ComputeNeighborhood(Vector2Int position)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        foreach (Vector2Int delta in deltas)
        {
            Vector2Int newPosition = position + delta;
            if (infinite || (newPosition.x >= minimum.x && newPosition.y >= minimum.y
                && newPosition.x <= maximum.x && newPosition.y <= maximum.y))
            {
                neighbors.Add(newPosition);
            }
        }

        return neighbors;
    }
}
