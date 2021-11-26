using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public abstract class BaseWaveFunctionCollapse : MonoBehaviour
{
    
    [Tooltip("When true, collapse all steps on start.")]
    public bool totalCollapseOnStart = false;

    [Tooltip("When true, collapse one step on each update in play mode.")]
    public bool incremental = false;
    [Min(0)]
    public int maxWaveCount;

    public int nextWave;

    [HideInInspector]
    public int editorStepCount;

    public virtual void Start()
    {
        nextWave = 0;
        if (totalCollapseOnStart && Application.isPlaying)
        {
            CollapseSteps(maxWaveCount);
        }
    }

    public virtual void Update()
    {
        if (incremental && Application.isPlaying)
        {
            CollapseSteps(1);
        }
    }

    public void TotalCollapse()
    {
        CollapseSteps(maxWaveCount - nextWave);
    }
    public abstract void CollapseSteps(int stepCount);
    protected abstract void CollapseOneStepNoCheck();

    public abstract void Reset();
}
