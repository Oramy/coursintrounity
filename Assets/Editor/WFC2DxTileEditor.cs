using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WFC2DxTile), true)]
public class WFC2DxTileEditor : WFCEditor
{
    public override void OnInspectorGUI()
    {
        WFC2DxTile wfc = (WFC2DxTile)target;
        Undo.RecordObject(wfc.tilemap, "Modified tilemap");
        base.OnInspectorGUI();
        EditorUtility.SetDirty(wfc.tilemap);
    }
 }
