using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WaveFunctionCollapseConfig<Position, State> : ScriptableObject
{
    public List<State> allStates;
    public Topology<Position> neighborhood;
    public CompatibilityRules<Position, State> compatibilityRules;
    public Position defaultPosition;
}

public abstract class WaveFunctionCollapse<Position, State> : BaseWaveFunctionCollapse, ISerializationCallbackReceiver
{
    [System.Serializable]
    public class PartialState
    {
        [SerializeField]
        public Position position;
        public List<State> possibleStates;

        public PartialState(Position position, List<State> possibleStates) {
            this.position = position;
            this.possibleStates = possibleStates;
        }
    }
    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize() { }

    [HideInInspector]
    [SerializeField]
    protected GenericDictionary<Position, State> closedMap;
    [HideInInspector]
    [SerializeField]
    protected GenericDictionary<Position, PartialState> partialMap;

    [SerializeField]
    public WaveFunctionCollapseConfig<Position, State> config;

    [SerializeField]
    private List<Position> closedPositionsOnCurrentWave;


    public void EnsureValidState()
    {
        if (closedMap == null)
            closedMap = new GenericDictionary<Position, State>();
        if (partialMap == null)
            partialMap = new GenericDictionary<Position, PartialState>();
        if (closedPositionsOnCurrentWave == null)
            closedPositionsOnCurrentWave = new List<Position>();
    }
    public void Awake()
    {
        EnsureValidState();
    }
    public override void Start()
    {
        Reset();
        base.Start();
    }

    public override void Update()
    {
        if (!Application.isPlaying)
        {
            EnsureValidState();
        }
        base.Update();
    }

    private void ClosePosition(Position position, State state)
    {
        partialMap.Remove(position);
        closedMap[position] = state;
        closedPositionsOnCurrentWave.Add(position);
    }

    bool FindEntropyMinima(List<PartialState> entropyMinima)
    {
        if (partialMap.Count == 0)
        {
            Debug.LogError("Partial map is empty");
            return false;
        }

        int minimumEntropy = Int32.MaxValue;
        foreach (PartialState pState in partialMap.Values)
        {
            int stateCount = pState.possibleStates.Count;
            if (stateCount <= 1)
            {
                Debug.LogError($"Partial state is totally defined or impossible to reach (state count : {stateCount}) at position {pState.position}");
            }
            else if (stateCount < minimumEntropy)
            {
                minimumEntropy = stateCount;
                entropyMinima.Clear();
                entropyMinima.Add(pState);
            }
            else if (stateCount == minimumEntropy)
            {
                entropyMinima.Add(pState);
            }
        }

        return true;
    }

    protected bool IsDetermined(Position position)
    {
        return closedMap.ContainsKey(position);
    }

    void Collapse(Position position, List<State> possibleStates, HashSet<Position> toCollapse)
    {

        List<Position> neighbors = config.neighborhood.ComputeNeighborhood(position);
        List<State> toRemove = new List<State>();
        foreach (Position neighbor in neighbors)
        {
            if (IsDetermined(neighbor))
                continue;

            PartialState pStateNeighbor;
            if (!partialMap.TryGetValue(neighbor, out pStateNeighbor))
            {
                pStateNeighbor = AddToPartialMap(neighbor);
            }

            toRemove.Clear();
            foreach (State possibleStateNeighbor in pStateNeighbor.possibleStates)
            {
                bool compatible = false;
                foreach (State possibleState in possibleStates)
                {
                    if (config.compatibilityRules.AreCompatible(position, possibleState, neighbor, possibleStateNeighbor))
                    {
                        compatible = true;
                        break;
                    }
                }

                if (!compatible)
                {
                    toRemove.Add(possibleStateNeighbor);
                }
            }

            if (toRemove.Count > 0)
            {
                foreach (State s in toRemove)
                {
                    pStateNeighbor.possibleStates.Remove(s);
                }

                if (pStateNeighbor.possibleStates.Count == 1) {
                    ClosePosition(pStateNeighbor.position, pStateNeighbor.possibleStates[0]);
                }

                if (pStateNeighbor.possibleStates.Count != 0)
                    toCollapse.Add(neighbor);
                else
                {
                    Debug.Log($"Failed to collapse position {pStateNeighbor.position}");
                    OnFailedToCollapse(pStateNeighbor.position);
                }
            }
        }
    }

    public override sealed void CollapseSteps(int stepCount) {
        if (stepCount == 0)
            return;

        int maxWaveCount = this.maxWaveCount == 0 ? Int32.MaxValue : this.maxWaveCount;
        if (partialMap.Count == 0 && !closedMap.ContainsKey(config.defaultPosition))
        {
            AddToPartialMap(config.defaultPosition);
        }

        List<Position> closedPositions = new List<Position>();
        int i = 0;
        while (partialMap.Count > 0 && nextWave < maxWaveCount && i < stepCount)
        {
            i++;
            CollapseOneStepNoCheck();
            closedPositions.AddRange(closedPositionsOnCurrentWave);
        }
        OnCollapse(closedPositions);
    }

    protected override sealed void CollapseOneStepNoCheck()
    {
        closedPositionsOnCurrentWave.Clear();
        if (partialMap.Count == 0 && nextWave == 0)
        {
            AddToPartialMap(config.defaultPosition);
        }

        List<PartialState> entropyMinima = new List<PartialState>();
        if (FindEntropyMinima(entropyMinima))
        {
            nextWave++;
            int randomIndex = UnityEngine.Random.Range(0, entropyMinima.Count);
            DefineRandomStateAndCollapse(entropyMinima[randomIndex].position);
            entropyMinima.Clear();
        }
        else
        {
            Debug.LogWarning($"Trying to collapse wave {nextWave} but there is nothing to collapse.");
        }
    }

    void StartCollapse(Position position) {
        HashSet<Position> toCollapse = new HashSet<Position>();
        HashSet<Position> nextWaveToCollapse = new HashSet<Position>();
        toCollapse.Add(position);

        while (toCollapse.Count != 0)
        {
            foreach (Position positionToCollapse in toCollapse)
            {
                PartialState pState;
                State stateToCollapse;
                if (partialMap.TryGetValue(positionToCollapse, out pState))
                {
                    Collapse(positionToCollapse, pState.possibleStates, nextWaveToCollapse);
                }
                else if (closedMap.TryGetValue(positionToCollapse, out stateToCollapse))
                {
                    List<State> possibleStates = new List<State>();
                    possibleStates.Add(stateToCollapse);
                    Collapse(positionToCollapse, possibleStates, nextWaveToCollapse);
                }
                else
                {

                }
            }
            toCollapse.Clear();

            // Switching both lists
            HashSet<Position> tmp = nextWaveToCollapse;
            nextWaveToCollapse = toCollapse;
            toCollapse = tmp;
            nextWaveToCollapse.Clear();
        }
    }

    void DefineState(Position position, State state) {
        PartialState pState;
        if (partialMap.TryGetValue(position, out pState))
        {
            if (!pState.possibleStates.Contains(state)) {
                Debug.LogError($"Defined state {state} to an impossible value at position {position}");
            }
        }
        ClosePosition(position, state);
    }

    protected void DefineStateAndCollapse(Position position, State state)
    {
        DefineState(position, state);
        StartCollapse(position);
    }

    void DefineRandomStateAndCollapse(Position position)
    {
        List<State> possibleStates;
        PartialState pState;
        if (partialMap.TryGetValue(position, out pState))
        {
            possibleStates = pState.possibleStates;
        }
        else
        {
            possibleStates = config.allStates;
        }

        int randomIndex = UnityEngine.Random.Range(0, possibleStates.Count);
        State state = possibleStates[randomIndex];

        DefineStateAndCollapse(position, state);
    }

    PartialState AddToPartialMap(Position position)
    {
        List<State> possibleStates = new List<State>(config.allStates);
        return DefinePartialState(position, possibleStates);
    }

    PartialState DefinePartialState(Position position, List<State> possibleStates)
    {
        PartialState pState;
        if (!partialMap.TryGetValue(position, out pState))
        {
            pState = new PartialState(position, possibleStates);
            partialMap[position] = pState;
        }
        else
        {
            pState.possibleStates.Clear();
            pState.possibleStates.AddRange(possibleStates);
        }

        return pState;
    }


    public virtual void OnFailedToCollapse(Position position) { }
    public virtual void OnCollapse(List<Position> closedPositions) { }
    public virtual void OnResetBegin() { }

    public virtual void OnResetEnd() { }

    public sealed override void Reset()
    {
        OnResetBegin();
        if (closedMap != null)
            closedMap.Clear();
        else
            closedMap = new GenericDictionary<Position, State>();

        if (partialMap != null)
            partialMap.Clear();
        else
            partialMap = new GenericDictionary<Position, PartialState>();

        nextWave = 0;

        OnResetEnd();
    }
}
