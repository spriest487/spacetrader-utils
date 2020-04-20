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

        public Pathfinder() {
            this.cameFrom = new Dictionary<TNode, TNode>();

            this.openSet = new HashSet<TNode>();
            this.closedSet = new HashSet<TNode>();

            this.gScore = new Dictionary<TNode, float>();
            this.fScore = new Dictionary<TNode, float>();

            this.neighbors = new List<Neighbor>();
        }

        public void FilterReachable(
            TNode origin,
            IList<TNode> tiles,
            Func<TNode, bool> tileFilter,
            int maxLength = -1
        ) {
            for (var i = tiles.Count - 1; i >= 0; --i) {
                if (!this.FindPath(origin, tiles[i], tileFilter, maxLength)) {
                    tiles.RemoveAt(i);
                }
            }
        }

        public bool FindPath(
            TNode origin,
            TNode destination,
            Func<TNode, bool> tileFilter = null,
            int maxLength = -1,
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
                    return this.ReconstructPath(current, outPath, maxLength);
                }

                this.openSet.Remove(current);
                this.closedSet.Add(current);

                this.neighbors.Clear();
                this.FindNeighbors(current, tileFilter, this.neighbors);
                foreach (var neighbor in this.neighbors) {
                    if (this.closedSet.Contains(neighbor.Node)) {
                        // neighbor is already evaluated
                        continue;
                    }

                    // the distance from start to a neighbor
                    var tentativeGScore = ScoreOrInfinity(this.gScore, current) + neighbor.Distance;

                    if (!this.openSet.Contains(neighbor.Node)) {
                        this.openSet.Add(neighbor.Node);
                    } else if (tentativeGScore >= ScoreOrInfinity(this.gScore, neighbor.Node)) {
                        // this is not a better path
                        continue;
                    }

                    // this path is the best until now, record it
                    this.cameFrom[neighbor.Node] = current;
                    this.gScore[neighbor.Node] = tentativeGScore;
                    this.fScore[neighbor.Node] = tentativeGScore + this.Heuristic(neighbor.Node, destination);
                }
            }

            Profiler.EndSample();

            return false;
        }

        private bool ReconstructPath(
            TNode current,
            List<TNode> outPath,
            int maxLength
        ) {
            Profiler.BeginSample("Pathfinder.ReconstructPath");

            outPath?.Clear();
            outPath?.Add(current);

            /* outPath is null if we're checking without storing the result,
            or we could just use outPath.Count, so count the number of steps
            separately. this starts at 0 instead of 1 because we consider a path
            of one point to have length 0 */
            var count = 0;

            while (this.cameFrom.TryGetValue(current, out current)) {
                // reached max length, shortest path is too long
                if (maxLength >= 0 && count == maxLength) {
                    outPath?.Clear();
                    Profiler.EndSample();
                    return false;
                }

                outPath?.Add(current);
                ++count;
            }

            outPath?.Reverse();

            Profiler.EndSample();

            return true;
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
            Func<TNode, bool> tileFilter,
            List<Neighbor> outNeighbors
        );
    }
}