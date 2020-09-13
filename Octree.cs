using System;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    public class Octree<T> {
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

        public Octree(Bounds region, float minSize) {
            this.root = new Node(region, minSize);
            this.MinSize = minSize;
        }

        public void Add(Vector3 point, T item) {
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
                this.root = new Node(parentRegion, this.MinSize);

                var childSegment = GetChildSegment(parentRegion, rootRegion.center);
                this.root.children[(int)childSegment] = oldRoot;
            }

            this.root.Add(point, item, this.MinSize);
        }

        public bool Remove(T item) {
            return this.root.Remove(item);
        }

        public IEnumerable<Bounds> GetAllRegions() {
            return this.root.GetAllRegions();
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

        private class Node {
            internal Bounds Region { get; }

            internal readonly Node[] children;

            private readonly List<KeyValuePair<Vector3, T>> items;

            internal Node(Bounds region, float minSize) {

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
                    this.items = new List<KeyValuePair<Vector3, T>>(0);
                }
            }

            private Node GetOrAddChild(Segment childSegment, float minSize) {
                var childIndex = (int)childSegment;
                if (this.children[childIndex] == null) {
                    var childRegion = this.GetChildRegion(childSegment);
                    this.children[childIndex] = new Node(childRegion, minSize);
                }

                return this.children[childIndex];
            }

            internal void Add(Vector3 position, T item, float minSize) {
                if (this.children != null) {
                    var childIndex = GetChildSegment(this.Region, position);
                    var child = this.GetOrAddChild(childIndex, minSize);

                    child.Add(position, item, minSize);
                } else {
                    this.items.Add(new KeyValuePair<Vector3, T>(position, item));
                }
            }

            public bool Remove(T item) {
                if (this.items != null) {
                    var itemIndex = this.items.FindIndex(i => i.Value.Equals(item));
                    if (itemIndex == -1) {
                        return false;
                    }

                    this.items.RemoveAt(itemIndex);
                    return true;
                }

                foreach (var child in this.children) {
                    if (child != null && child.Remove(item)) {
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

            internal IEnumerable<Bounds> GetAllRegions() {
                yield return this.Region;

                if (this.children == null) {
                    yield break;
                }

                foreach (var child in this.children) {
                    if (child != null) {
                        foreach (var childRegion in child.GetAllRegions()) {
                            yield return childRegion;
                        }
                    }
                }
            }
        }
    }
}
