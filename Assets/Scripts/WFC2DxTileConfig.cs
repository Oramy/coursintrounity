using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "WFC2DxTileConfig", menuName = "WFC/2D/TiledMap/WFC2DxTileConfig", order = 1)]
public class WFC2DxTileConfig : WaveFunctionCollapseConfig<Vector2Int, Tile> { }
