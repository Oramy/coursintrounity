using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Borders2DTileCompatibilityRules", menuName = "WFC/2D/TiledMap/CompatibilityRules/Borders", order = 1)]
public class Borders2DTileCompatibilityRules : CompatibilityRules<Vector2Int, Tile>
{
    private Dictionary<(Vector2Int, Tile, Tile), bool> seenCompatibilityRules;

    public Borders2DTileCompatibilityRules()
    {
        seenCompatibilityRules = new Dictionary<(Vector2Int, Tile, Tile), bool>();
    }

    public override void RegisterRule(Vector2Int p1, Tile s1, Vector2Int p2, Tile s2, bool compatibility)
    {
        seenCompatibilityRules[(p1 - p2, s1, s2)] = compatibility;
        seenCompatibilityRules[(p2 - p1, s2, s1)] = compatibility;
    }

    public override bool TryGetRegisteredRule(Vector2Int p1, Tile s1, Vector2Int p2, Tile s2, out bool areCompatible)
    {
        return seenCompatibilityRules.TryGetValue((p1 - p2, s1, s2), out areCompatible);
    }

    public override bool AreCompatibleCompute(Vector2Int p1, Tile s1, Vector2Int p2, Tile s2)
    {
        Texture2D t1 = s1.sprite.texture;
        Texture2D t2 = s2.sprite.texture;
        if (t1.width != t2.width || t1.height != t2.height)
        {
            return false;
        }

        if (p1.x != p2.x && p1.y != p2.y)
        {
            Debug.LogError("Margin2DCompatibilityRules must be used with a von neumann neighborhood");
            return false;
        }

        bool vertical = p1.x != p2.x;
        bool positive = p2.y == p1.y + 1 || p2.x == p1.x + 1;

        int imageSizeParallel = (vertical ? t1.height : t1.width);
        int imageSizeTangent = (vertical ? t1.width : t1.height);
        for (int j = 0; j < imageSizeParallel; j++)
        {
            int i1 = positive ? imageSizeTangent - 1 : 0;
            int i2 = positive ? 0 : imageSizeTangent - 1;

            int x1 = vertical ? i1 : j;
            int x2 = vertical ? i2 : j;
            int y1 = vertical ? j : i1;
            int y2 = vertical ? j : i2;

            if (t1.GetPixel(x1, y1) != t2.GetPixel(x2, y2))
            {
                return false;
            }
        }

        return true;
    }
}
    