using Game.Common;
using Game.Scene;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Overworld
{
    public class MapRunController : Singleton<MapRunController>
    {
        private RunMapState runState;

        public event Action<RunNode> OnCurrentNodeChanged;
        public event Action<RunNode> OnNodeRevealed;
        public event Action<int> OnActChanged;
        public event Action OnRunMapInitialized;

        public RunMapGraph CurrentAct =>
        runState.acts[runState.currentActIndex];

        public RunNode CurrentNode =>
            CurrentAct.nodes[CurrentAct.currentNodeId];

        public bool IsInitialized => runState != null;

        protected override void Awake()
        {
            base.Awake();
        }

        public void Initialize(RunMapState state)
        {
            runState = state;

            // Ensure current act index is valid
            runState.currentActIndex = Mathf.Clamp(
                runState.currentActIndex, 0, runState.acts.Count - 1
            );

            RevealStartNodeIfNeeded();
          
            OnRunMapInitialized?.Invoke();
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

            ApplyCorruption(node);
            RevealChildren(node);

            if (node.nodeType == LevelType.Boss)
            {
                AdvanceToNextAct();
            }
        }

        private void ApplyCorruption(RunNode node)
        {
            float corruption = node.levelData.corruptionIncrease;
            Debug.Log($"[MapRun] Apply corruption: {corruption}");
            // Hook PlayerCorruption here later
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
            if (runState.currentActIndex + 1 >= runState.acts.Count)
            {
                Debug.Log("[MapRun] Run complete!");
                return;
            }

            runState.currentActIndex++;
            OnActChanged?.Invoke(runState.currentActIndex);

            RevealStartNodeIfNeeded();
        }
    }
}
