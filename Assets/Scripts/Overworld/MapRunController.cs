using Game.Combat;
using Game.Common;
using Game.Saving;
using Game.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld
{
    public class MapRunController : Singleton<MapRunController>
    {

        private RunMapState runState;
        private RunMapGraph currentRunMapState;

        public event Action<RunNode> OnCurrentNodeChanged;
        public event Action<RunNode> OnNodeRevealed;
        public event Action<int> OnActChanged;
        public event Action OnRunMapInitialized;

        public RunMapGraph CurrentAct => currentRunMapState;

        public RunNode CurrentNode =>
            CurrentAct.nodes[CurrentAct.currentNodeId];

        public bool IsInitialized => runState != null;
        // Set once the last act's boss is cleared, so the finished run isn't saved again
        public bool IsRunComplete { get; private set; }

        protected override void Awake()
        {
            base.Awake();
        }

        public void Initialize(RunMapState state)
        {
            runState = state;
            IsRunComplete = false;

            SetCurrentMapGraph(1);

            RevealStartNodeIfNeeded();

            OnRunMapInitialized?.Invoke();
        }

        // --- Save / load ---

        public RunMapSaveData CaptureSaveData()
        {
            var data = new RunMapSaveData
            {
                seed = runState.seed,
                actNumber = CurrentAct.actNumber,
                mapId = CurrentAct.mapId,
                currentNodeId = CurrentAct.currentNodeId
            };

            foreach (var node in CurrentAct.nodes.Values)
            {
                data.nodes.Add(new NodeSaveEntry { id = node.id, state = node.state.ToString() });
                foreach (var edge in node.outgoingEdges)
                {
                    data.edges.Add(new EdgeSaveEntry { fromNodeId = edge.fromNodeId, toNodeId = edge.toNodeId, enabled = edge.enabled });
                }
            }

            return data;
        }

        /// <summary>
        /// Checks that a save matches a run regenerated from its seed (same act map and node ids).
        /// </summary>
        public static bool CanRestore(RunMapState state, RunMapSaveData save)
        {
            var graph = state?.acts?.Find(a => a.mapId == save.mapId && a.actNumber == save.actNumber);
            if (graph == null)
            {
                Debug.LogWarning($"[RunSave] Map '{save.mapId}' (act {save.actNumber}) no longer exists.");
                return false;
            }

            if (string.IsNullOrEmpty(save.currentNodeId) || !graph.nodes.ContainsKey(save.currentNodeId))
            {
                Debug.LogWarning($"[RunSave] Current node '{save.currentNodeId}' no longer exists.");
                return false;
            }

            foreach (var node in save.nodes)
            {
                if (!graph.nodes.ContainsKey(node.id) || !Enum.TryParse(node.state, out RunNodeState _))
                {
                    Debug.LogWarning($"[RunSave] Node '{node.id}' no longer exists or has an unknown state.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Initializes the run from a regenerated RunMapState and applies the saved progress on top.
        /// Call CanRestore first.
        /// </summary>
        public void RestoreFromSave(RunMapState state, RunMapSaveData save)
        {
            runState = state;
            IsRunComplete = false;
            currentRunMapState = runState.acts.Find(a => a.mapId == save.mapId && a.actNumber == save.actNumber);

            foreach (var entry in save.nodes)
            {
                if (currentRunMapState.nodes.TryGetValue(entry.id, out RunNode node) &&
                    Enum.TryParse(entry.state, out RunNodeState nodeState))
                {
                    node.state = nodeState;
                }
            }

            foreach (var entry in save.edges)
            {
                if (!currentRunMapState.nodes.TryGetValue(entry.fromNodeId, out RunNode from))
                    continue;

                var edge = from.outgoingEdges.Find(e => e.toNodeId == entry.toNodeId);
                if (edge != null)
                {
                    edge.enabled = entry.enabled;
                }
            }

            currentRunMapState.currentNodeId = save.currentNodeId;

            RevealStartNodeIfNeeded();

            OnRunMapInitialized?.Invoke();
        }

        public void ChangeAct(int newActNumber)
        {
            SetCurrentMapGraph(newActNumber);

            RevealStartNodeIfNeeded();

            OnActChanged?.Invoke(newActNumber);
        }

        private void SetCurrentMapGraph(int actNumber)
        {
            var actMaps = runState.acts.FindAll(a => a.actNumber == actNumber);

            if (actMaps.Count == 0)
            {
                Debug.LogError($"[MapRunController] No maps found for act {actNumber}.");
                return;
            }

            currentRunMapState = actMaps[UnityEngine.Random.Range(0, actMaps.Count)];
        }
        public IReadOnlyList<RunNode> GetVisibleNodes()
        {
            List<RunNode> result = new();

            foreach (var node in CurrentAct.nodes.Values)
            {
                if (node.state == RunNodeState.Revealed ||
                    node.state == RunNodeState.Cleared)
                {
                    result.Add(node);
                }
            }

            return result;
        }

        public IReadOnlyList<RunEdge> GetVisibleEdges()
        {
            List<RunEdge> result = new();

            foreach (var node in CurrentAct.nodes.Values)
            {
                if (node.state == RunNodeState.LockedHidden)
                    continue;

                foreach (var edge in node.outgoingEdges)
                {
                    if (!edge.enabled)
                        continue;

                    RunNode target = CurrentAct.nodes[edge.toNodeId];
                    if (target.state != RunNodeState.LockedHidden)
                    {
                        result.Add(edge);
                    }
                }
            }

            return result;
        }

        public IReadOnlyList<RunNode> GetAvailableMoves()
        {
            List<RunNode> result = new();

            foreach (var node in CurrentAct.nodes.Values)
            {
                if (node.id == CurrentNode.id)
                    continue;

                if (CanTravel(CurrentNode, node))
                    result.Add(node);
            }

            return result;
        }

        public bool TryMoveTo(string nodeId)
        {
            if (!CurrentAct.nodes.TryGetValue(nodeId, out RunNode target))
                return false;

            if (!CanTravel(CurrentNode, target))
                return false;

            CurrentAct.currentNodeId = target.id;
            OnCurrentNodeChanged?.Invoke(target);
            // Moves only happen on the map, where the session is a clean snapshot
            RunSaveService.SaveCurrentRun();
            return true;
        }

        private bool CanTravel(RunNode from, RunNode to)
        {
            if (to.state != RunNodeState.Revealed &&
                to.state != RunNodeState.Cleared)
                return false;

            // forward
            if (HasEnabledEdge(from.id, to.id))
                return true;

            // backward (implicit)
            if (HasEnabledEdge(to.id, from.id))
                return true;

            return false;
        }

        private bool HasEnabledEdge(string fromId, string toId)
        {
            var fromNode = CurrentAct.nodes[fromId];
            foreach (var edge in fromNode.outgoingEdges)
            {
                if (edge.enabled && edge.toNodeId == toId)
                    return true;
            }
            return false;
        }

        // --- Level resolution ---

        public void OnLevelCleared()
        {
            RunNode node = CurrentNode;

            if (node.state == RunNodeState.Cleared)
                return;

            node.state = RunNodeState.Cleared;

            RevealChildren(node);

            if (node.nodeType == LevelType.Boss)
            {
                AdvanceToNextAct();
            }
        }

        private void RevealChildren(RunNode node)
        {
            foreach (var edge in node.outgoingEdges)
            {
                if (!edge.enabled || !edge.revealOnClear)
                    continue;

                RunNode child = CurrentAct.nodes[edge.toNodeId];
                if (child.state == RunNodeState.LockedHidden)
                {
                    child.state = RunNodeState.Revealed;
                    OnNodeRevealed?.Invoke(child);
                }
            }
        }

        private void RevealStartNodeIfNeeded()
        {
            if (!CurrentAct.nodes.TryGetValue(CurrentAct.currentNodeId, out RunNode start))
                return;

            if (start.state == RunNodeState.LockedHidden)
                start.state = RunNodeState.Revealed;
        }

        // --- Act progression ---

        private void AdvanceToNextAct()
        {
            int nextActNumber = CurrentAct.actNumber + 1;

            if (!runState.acts.Exists(a => a.actNumber == nextActNumber))
            {
                Debug.Log("[MapRun] Run complete!");
                IsRunComplete = true;
                RunSaveService.DeleteSave();
                return;
            }

            ChangeAct(nextActNumber);
        }
    }
}
