using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    public class SubmeshTextureMap {
        private readonly Dictionary<Texture, int> submeshByTexture;

        private readonly List<Texture> textures;
        public IReadOnlyList<Texture> Textures => this.textures;

        private int nextIndex;

        public SubmeshTextureMap() {
            this.submeshByTexture = new Dictionary<Texture, int>();
            this.textures = new List<Texture>();

            this.nextIndex = 0;
        }
        
        public int GetSubmeshIndex(Texture texture) {
            if (this.submeshByTexture.TryGetValue(texture, out var index)) {
                return index;
            }

            index = this.nextIndex;
            this.submeshByTexture.Add(texture, index);
            this.textures.Add(texture);

            this.nextIndex += 1;
            return index;
        }

        public void Clear() {
            this.textures.Clear();
            this.submeshByTexture.Clear();

            this.nextIndex = 0;
        }
    }
}
