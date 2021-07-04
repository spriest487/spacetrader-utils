using System.Collections.Generic;
using UnityEngine;

namespace SpaceTrader.Util {
    public class SubmeshTextureMap {
        private readonly Dictionary<Texture2D, int> textureSubmeshes;

        private List<Texture2D> textures;
        public IReadOnlyList<Texture2D> Textures => this.textures;

        private int nextIndex;

        public SubmeshTextureMap() {
            this.textureSubmeshes = new Dictionary<Texture2D, int>();
            this.textures = new List<Texture2D>();

            this.nextIndex = 0;
        }
        
        public int GetSubmeshIndex(Texture2D texture) {
            if (this.textureSubmeshes.TryGetValue(texture, out var index)) {
                return index;
            }

            index = this.nextIndex;
            this.textureSubmeshes.Add(texture, index);
            this.textures.Add(texture);

            this.nextIndex += 1;
            return index;
        }
    }
}
