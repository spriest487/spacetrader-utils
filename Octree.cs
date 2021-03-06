﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    public delegate void OctreeNodeVisitorDelegate<TItem, TTime>(
        Bounds region,
        IEnumerable<TItem> items,
        TTime lastModified
    );

    public class Octree<TItem, TTime> where TTime : IComparable<TTime> {
        private enum Segment {
            WestTopNorth = 0,
            EastTopNorth = 1,
            WestTopSouth = 2,
            EastTopSouth = 3,
            WestBottomNorth = 4,
            EastBottomNorth = 5,
            WestBottomSouth = 6,
            EastBottomSouth = 7,
        }

        private Node root;

        public float MinSize { get; }

        public Bounds Region => this.root.Region;

        public Func<TTime> Clock { get; }

        public Octree(Bounds region, float minSize, Func<TTime> clock) {
            this.Clock = clock;
            this.MinSize = minSize;

            this.root = new Node(null, region, minSize, clock());
        }

        public void Add(Vector3 point, TItem item) {
            var timeNow = this.Clock();

            if (!this.root.Region.Contains(point)) {
                var rootRegion = this.root.Region;

                // min pos
                var rootExtents = rootRegion.extents;
                var parentX = rootRegion.center.x + Mathf.Sign(point.x - rootRegion.min.x) * rootExtents.x;
                var parentY = rootRegion.center.y + Mathf.Sign(point.y - rootRegion.min.y) * rootExtents.y;
                var parentZ = rootRegion.center.z + Mathf.Sign(point.z - rootRegion.min.z) * rootExtents.z;

                var parentSize = rootRegion.size * 2;
                var parentRegion = new Bounds(new Vector3(parentX, parentY, parentZ), parentSize);
                var oldRoot = this.root;
                this.root = new Node(null, parentRegion, this.MinSize, timeNow);

                var childSegment = GetChildSegment(parentRegion, rootRegion.center);
                this.root.children[(int)childSegment] = oldRoot;
                oldRoot.parent = this.root;
            }

            this.root.Add(point, item, this.MinSize, timeNow);
        }

        public bool Remove(TItem item) {
            return this.root.Remove(item, this.Clock());
        }

        public void Move(TItem item, Vector3 newPosition) {
            this.Remove(item);
            this.Add(newPosition, item);
        }

        public void Clear() {
            this.root = new Node(null, this.root.Region, this.MinSize, this.Clock());
        }

        public void Shrink(TTime cutoff) {
            this.root.Shrink(cutoff);
        }

        public void Visit(OctreeNodeVisitorDelegate<TItem, TTime> visitor) {
            this.root.Visit(visitor);
        }

        private Node GetNode(Vector3 position) {
            var node = this.root;
            while (node.children != null) {
                var segment = GetChildSegment(node.Region, position);

                if (node.children[(int)segment] is {} child) {
                    node = child;
                } else {
                    return node;
                }
            }

            return node;
        }

        private static Segment GetChildSegment(Bounds region, Vector3 point) {
            var east = point.x >= region.center.x;
            var top = point.y >= region.center.y;
            var north = point.z >= region.center.z;

            return east
                ? top
                    ? north
                        ? Segment.EastTopNorth
                        : Segment.EastTopSouth
                    : north
                        ? Segment.EastBottomNorth
                        : Segment.EastBottomSouth
                : top
                    ? north
                        ? Segment.WestTopNorth
                        : Segment.WestTopSouth
                    : north
                        ? Segment.WestBottomNorth
                        : Segment.WestBottomSouth;
        }

        public IEnumerable<TItem> Find(Vector3 origin, float range) {
            var rangeSqr = range * range;

            // origin node (most specific node at this position)
            var node = this.GetNode(origin);

            // go up the tree until none of our parent's siblings are in range - this is the broadest
            // parent node that encompasses all possible search results
            var parent = node.parent;
            while (parent != null) {
                var hasSiblingsInRange = false;

                // are any of the other children (current node's siblings) of this node in range?
                foreach (var child in parent.children) {
                    if (child == null || parent == node) {
                        continue;
                    }

                    var distSqr = child.Region.SqrDistance(origin);
                    if (!(distSqr <= rangeSqr)) {
                        continue;
                    }

                    hasSiblingsInRange = true;
                }

                if (hasSiblingsInRange) {
                    node = parent;
                    parent = node.parent;
                } else {
                    break;
                }
            }

            foreach (var (position, item) in node.GetEntriesDeep()) {
                if (Vector3.SqrMagnitude(position - origin) <= range) {
                    yield return item;
                }
            }
        }

        public IEnumerable<KeyValuePair<Vector3, TItem>> FindNearest(Vector3 origin) {
            var priorityQueue = new LinkedList<(Node node, float distSqr, int itemIndex)>();

            void EnqueueNode(Node node, int itemIndex) {
                var distSqr = itemIndex >= 0
                    ? Vector3.SqrMagnitude(origin - node.items[itemIndex].Key)
                    : node.Region.SqrDistance(origin);

                // find insertion point
                var insertAt = priorityQueue.First;
                if (insertAt == null) {
                    priorityQueue.AddFirst((node, distSqr, itemIndex));
                    return;
                }

                while (insertAt != null && insertAt.Value.distSqr < distSqr) {
                    insertAt = insertAt.Next;
                }

                if (insertAt == null) {
                    priorityQueue.AddLast((node, distSqr, itemIndex));
                } else {
                    priorityQueue.AddBefore(insertAt, (node, distSqr, itemIndex));
                }
            }

            EnqueueNode(this.root, -1);
            while (priorityQueue.Count > 0) {
                var head = priorityQueue.First.Value;
                priorityQueue.RemoveFirst();

                if (head.itemIndex >= 0) {
                    var item = head.node.items[head.itemIndex];
                    yield return item;
                } else {
                    foreach (var child in head.node.children) {
                        if (child != null) {
                            EnqueueNode(child, -1);
                        }
                    }
                }
            }
        }

        private class Node {
            internal Bounds Region { get; }

            internal Node parent;
            internal readonly Node[] children;

            internal readonly List<KeyValuePair<Vector3, TItem>> items;

            private TTime lastModified;

            internal Node(Node parent, Bounds region, float minSize, TTime now) {
                this.parent = parent;
                this.lastModified = now;

                var regionSize = region.size;
                if (regionSize.x < minSize || regionSize.y < minSize || regionSize.z < minSize) {
                    throw new ArgumentException($"bad octree node: region provided was smaller than min size {minSize}");
                }

                this.Region = region;

                var minPartitionSize = minSize * 2f;
                var canPartition = regionSize.x > minPartitionSize
                    && regionSize.y > minPartitionSize
                    && regionSize.z > minPartitionSize;

                if (canPartition) {
                    this.children = new Node[8];
                    this.items = null;
                } else {
                    this.children = null;
                    this.items = new List<KeyValuePair<Vector3, TItem>>(0);
                }
            }

            private Node GetOrAddChild(Segment childSegment, float minSize, TTime now) {
                var childIndex = (int)childSegment;
                if (this.children[childIndex] == null) {
                    var childRegion = this.GetChildRegion(childSegment);
                    this.children[childIndex] = new Node(this, childRegion, minSize, now);
                }

                return this.children[childIndex];
            }

            internal void Add(Vector3 position, TItem item, float minSize, TTime now) {
                if (this.children != null) {
                    var childIndex = GetChildSegment(this.Region, position);
                    var child = this.GetOrAddChild(childIndex, minSize, now);

                    child.Add(position, item, minSize, now);
                } else {
                    this.items.Add(new KeyValuePair<Vector3, TItem>(position, item));
                    this.lastModified = now;
                }
            }

            public bool Remove(TItem item, TTime now) {
                if (this.items != null) {
                    var itemIndex = this.items.FindIndex(i => i.Value.Equals(item));
                    if (itemIndex == -1) {
                        return false;
                    }

                    this.items.RemoveAt(itemIndex);
                    this.lastModified = now;
                    return true;
                }

                foreach (var child in this.children) {
                    if (child != null && child.Remove(item, now)) {
                        return true;
                    }
                }

                return false;
            }

            private Bounds GetChildRegion(Segment childSegment) {
                var halfExtents = this.Region.extents * 0.5f;
                var childSize = this.Region.size * 0.5f;
                var center = this.Region.center;

                switch (childSegment) {
                    case Segment.EastTopNorth:
                        return new Bounds(center + new Vector3(halfExtents.x, halfExtents.y, halfExtents.z), childSize);

                    case Segment.WestTopNorth:
                        return new Bounds(center + new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z), childSize);

                    case Segment.EastBottomNorth:
                        return new Bounds(center + new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z), childSize);

                    case Segment.WestBottomNorth:
                        return new Bounds(center + new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z), childSize);

                    case Segment.EastTopSouth:
                        return new Bounds(center + new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z), childSize);

                    case Segment.WestTopSouth:
                        return new Bounds(center + new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z), childSize);

                    case Segment.EastBottomSouth:
                        return new Bounds(center + new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z), childSize);

                    case Segment.WestBottomSouth:
                        return new Bounds(center + new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z), childSize);

                    default:
                        throw new ArgumentException($"octree child index out of range: {childSegment}");
                }
            }

            private bool IsEmpty() {
                if (this.children != null) {
                    foreach (var child in this.children) {
                        if (child != null) {
                            return false;
                        }
                    }
                    return true;
                }

                return this.items.Count == 0;
            }

            public void Shrink(TTime cutoff) {
                if (this.children == null) {
                    return;
                }

                for (var i = 0; i < this.children.Length; i += 1) {
                    var child = this.children[i];
                    if (this.children[i] == null) {
                        continue;
                    }

                    this.children[i].Shrink(cutoff);

                    if (child.IsEmpty() && cutoff.CompareTo(child.lastModified) >= 0) {
                        this.children[i] = null;
                    }
                }
            }

            public void Visit(OctreeNodeVisitorDelegate<TItem, TTime> visitor) {
                var items = this.items?.Values();

                visitor(this.Region, items, this.lastModified);

                if (this.children != null) {
                    foreach (var child in this.children) {
                        if (child != null) {
                            child.Visit(visitor);
                        }
                    }
                }
            }

            // all item values in range from all nodes beneath the top node
            public IEnumerable<KeyValuePair<Vector3, TItem>> GetEntriesDeep() {
                if (this.items != null) {
                    foreach (var entry in this.items) {
                        yield return entry;
                    }
                } else {
                    foreach (var child in this.children) {
                        if (child == null) {
                            continue;
                        }

                        foreach (var entry in child.GetEntriesDeep()) {
                            yield return entry;
                        }
                    }
                }
            }
        }
    }
}
