using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFC2DxSprite : WaveFunctionCollapse<Vector2Int, Sprite> {

    int lastAddedWave;
    public override void Start()
    {
        lastAddedWave = -1;
        base.Start();
    }

    public void CreateGameObject(Vector2Int ipos, Sprite sprite)
    {
        GameObject obj = new GameObject($"sprite_{ipos.x}_{ipos.y}");
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = new Vector3(sprite.bounds.size.x * ipos.x, sprite.bounds.size.y * ipos.y, 0);
        SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
    }

    public override void OnCollapse(List<Vector2Int> closedPositions)
    {
        if (lastAddedWave != nextWave)
        {
            lastAddedWave = nextWave;
            foreach (Vector2Int pos in closedPositions)
            {
                CreateGameObject(pos, closedMap[pos]);
            }
        }
    }

    public override void OnResetEnd()
    {
        lastAddedWave = -1;
        for(int i = this.transform.childCount; i > 0; --i)
            DestroyImmediate(this.transform.GetChild(0).gameObject);
    }
}
