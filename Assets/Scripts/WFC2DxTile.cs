using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WFC2DxTile : WaveFunctionCollapse<Vector2Int, Tile>
{
    public int layer;

    [SerializeField]
    int lastAddedWave;

    public Tilemap tilemap;

    public override void Start()
    {
        lastAddedWave = -1;
        PopulateClosedMap();
        base.Start();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            Debug.Log("started");
            Tilemap.tilemapTileChanged += OnTilemapTileChanged;
        }
#endif
    }

#if UNITY_EDITOR
    private void OnTilemapTileChanged(Tilemap tilemap, Tilemap.SyncTile[] syncTiles)
    {
        if (Application.isPlaying)
            return;

        if (tilemap != this.tilemap)
            return;

        foreach (Tilemap.SyncTile syncTile in syncTiles)
        {
            Vector3Int position3 = syncTile.position;
            if (position3.z == layer && (syncTile.tile is Tile || syncTile.tile == null))
            {

                Vector2Int position2 = (Vector2Int)syncTile.position;
                Tile tile = (Tile)syncTile.tile;
                if (tile == null)
                {
                    closedMap.Remove(position2);
                }
                else
                {
                    if (!IsDetermined(position2) || closedMap[position2] != tile)
                    {
                        Debug.Log("on tilemap tile changed");
                        DefineStateAndCollapse(position2, tile);
                    }
                }
            }
        }
    }
#endif

    public void CreateTile(Vector2Int ipos, Tile tile)
    {
        Vector3Int ipos3 = new Vector3Int(ipos.x, ipos.y, 0);
        tilemap.SetTile(ipos3, tile);
        tilemap.SetTileFlags(ipos3, TileFlags.None);
    }

    public override void OnCollapse(List<Vector2Int> closedPositions)
    {
        if (lastAddedWave != nextWave)
        {
            lastAddedWave = nextWave;
            foreach (Vector2Int pos in closedPositions)
            {
                CreateTile(pos, closedMap[pos]);
            }
        }
    }

    public override void OnResetBegin()
    {
        lastAddedWave = -1;
    }

    public override void OnResetEnd()
    {
        PopulateClosedMap();
    }

    public void PopulateClosedMap()
    {
        TileBase[] tiles = tilemap.GetTilesBlock(tilemap.cellBounds);

        BoundsInt bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int pos3 = new Vector3Int(x, y, 0);
                TileBase tileBase = tilemap.GetTile(pos3);
                if (tileBase is Tile)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    Tile tile = (Tile)tileBase;
                    DefineStateAndCollapse(pos, tile);
                }

            }
        }
    }
}
