using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseWaveFunctionCollapse), true)]
public class WFCEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BaseWaveFunctionCollapse wfc = (BaseWaveFunctionCollapse)target;


        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        if (wfc.maxWaveCount != 0)
        {
            wfc.editorStepCount = EditorGUILayout.IntSlider("Number of steps to collapse", wfc.editorStepCount, 1, wfc.maxWaveCount);
        }
        else
        {
            wfc.editorStepCount = EditorGUILayout.IntField("Number of steps to collapse", wfc.editorStepCount);
            wfc.editorStepCount = wfc.editorStepCount < 1 ? 1 : wfc.editorStepCount;
        }
        
        string buttonText;
        switch (wfc.editorStepCount)
        {
            case 0:
                buttonText = "Max steps";
                break;
            case 1:
                buttonText = $"1 step";
                break;
            default:
                buttonText = $"{wfc.editorStepCount} steps";
                break;
        }

        if (GUILayout.Button(buttonText))
        {
            Undo.RecordObject(target, "Collapsed one step");
            if (wfc.editorStepCount == 0 && wfc.maxWaveCount != 0)
                wfc.TotalCollapse();
            else
                wfc.CollapseSteps(wfc.editorStepCount);
            EditorUtility.SetDirty(wfc);
        }

        if (GUILayout.Button("1 step"))
        {
            Undo.RecordObject(target, "Collapsed one step");
            wfc.CollapseSteps(1);
            EditorUtility.SetDirty(wfc);
        }

        GUILayout.EndHorizontal();
    }
}
