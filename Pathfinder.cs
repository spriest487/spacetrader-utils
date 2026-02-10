using System;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace SpaceTrader.Util {
    public abstract class Pathfinder<TNode>
        where TNode : IEquatable<TNode> {
        protected struct Neighbor {
            public float Distance { get; }
            public TNode Node { get; }

            public Neighbor(TNode node, float distance) {
                this.Node = node;
                this.Distance = distance;
            }
        }
        
        private readonly Dictionary<TNode, TNode> cameFrom;

        // the set of nodes already evaluated
        private readonly HashSet<TNode> closedSet;

        // for each node, the total cost of getting from the start node to the
        // goal by passing by that node. that value is partly known, partly
        // heuristic
        private readonly Dictionary<TNode, float> fScore;

        // for each node, the cost of getting from the start node to that node
        private readonly Dictionary<TNode, float> gScore;

        private readonly List<Neighbor> neighbors;

        // the set of currently discovered nodes that are not evaluated yet
        // initially, only the start node is known
        private readonly HashSet<TNode> openSet;

        protected Pathfinder() {
            this.cameFrom = new Dictionary<TNode, TNode>();

            this.openSet = new HashSet<TNode>();
            this.closedSet = new HashSet<TNode>();

            this.gScore = new Dictionary<TNode, float>();
            this.fScore = new Dictionary<TNode, float>();

            this.neighbors = new List<Neighbor>();
        }

        public bool FindPath(
            TNode origin,
            TNode destination,
            float maxDist,
            List<TNode> outPath = null
        ) {
            Profiler.BeginSample("Pathfinder.FindPath");

            this.cameFrom.Clear();
            this.closedSet.Clear();

            this.openSet.Clear();
            this.openSet.Add(origin);

            this.gScore.Clear();
            this.gScore.Add(origin, 0);

            this.fScore.Clear();
            this.fScore.Add(origin, this.Heuristic(origin, destination));

            while (this.openSet.Count > 0) {
                // point with lowest fscore is current point
                var minFScore = float.PositiveInfinity;
                var current = destination;
                foreach (var point in this.openSet) {
                    var pointFScore = ScoreOrInfinity(this.fScore, point);
                    if (pointFScore < minFScore) {
                        current = point;
                        minFScore = pointFScore;
                    }
                }
                
                if (current.Equals(destination)) {
                    Profiler.EndSample();

                    if (outPath != null) {
                        this.ReconstructPath(current, outPath);
                    }

                    return true;
                }

                this.openSet.Remove(current);
                this.closedSet.Add(current);

                this.neighbors.Clear();
                this.FindNeighbors(current, this.neighbors);
                foreach (var neighbor in this.neighbors) {
                    if (this.closedSet.Contains(neighbor.Node)) {
                        // neighbor is already evaluated
                        continue;
                    }

                    // the distance from start to a neighbor
                    var neighborGScore = ScoreOrInfinity(this.gScore, current) + neighbor.Distance;

                    if (!this.openSet.Contains(neighbor.Node) && neighborGScore <= maxDist) {
                        this.openSet.Add(neighbor.Node);
                    } else if (neighborGScore >= ScoreOrInfinity(this.gScore, neighbor.Node)) {
                        // this is not a better path
                        continue;
                    }

                    // this path is the best until now, record it
                    this.cameFrom[neighbor.Node] = current;
                    this.gScore[neighbor.Node] = neighborGScore;
                    this.fScore[neighbor.Node] = neighborGScore + this.Heuristic(neighbor.Node, destination);
                }
            }

            Profiler.EndSample();

            return false;
        }

        public void FindOpenNodes(TNode origin, float maxDist, List<TNode> outNodes) {
            outNodes.Clear();

            this.closedSet.Clear();
            this.openSet.Clear();
            this.gScore.Clear();

            this.openSet.Add(origin);
            this.gScore[origin] = 0f;

            while (true) {
                TNode current;
                using (var openSetIt = this.openSet.GetEnumerator()) {
                    if (!openSetIt.MoveNext()) {
                        break;
                    }
                    current = openSetIt.Current;
                }

                this.openSet.Remove(current);
                // this.closedSet.Add(current);
                var currentScore = this.gScore[current!];

                this.neighbors.Clear();
                this.FindNeighbors(current, this.neighbors);
                foreach (var neighbor in this.neighbors) {
                    var neighborScore = currentScore + neighbor.Distance;

                    if (this.gScore.TryGetValue(neighbor.Node, out var prevScore) && prevScore <= neighborScore) {
                        continue;
                    }

                    if (neighborScore <= maxDist) {
                        this.openSet.Add(neighbor.Node);
                        this.gScore[neighbor.Node] = neighborScore;

                        outNodes.Add(neighbor.Node);
                    }
                }
            }
        }

        private void ReconstructPath(
            TNode current,
            List<TNode> outPath
        ) {
            Profiler.BeginSample("Pathfinder.ReconstructPath");

            outPath.Clear();
            outPath.Add(current);

            while (this.cameFrom.TryGetValue(current, out current)) {
                outPath.Add(current);
            }

            outPath.Reverse();

            Profiler.EndSample();
        }

        private static float ScoreOrInfinity(
            Dictionary<TNode, float> dict,
            TNode at
        ) {
            if (dict.TryGetValue(at, out var val)) {
                return val;
            }

            dict.Add(at, float.PositiveInfinity);
            return float.PositiveInfinity;
        }

        protected abstract float Heuristic(TNode a, TNode b);

        protected abstract void FindNeighbors(
            TNode point,
            List<Neighbor> outNeighbors
        );
    }
}
