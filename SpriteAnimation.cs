using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SpaceTrader.Util {
    [CreateAssetMenu(menuName = "SpaceTrader/Sprite Animation")]
    public class SpriteAnimation : ScriptableObject {
        [SerializeField]
        private Sprite[] frames;
        public IReadOnlyList<Sprite> Frames => this.frames;
        
        [SerializeField]
        private float framesPerSecond;
        public float FramesPerSecond => this.framesPerSecond;

        [SerializeField]
        private bool loop;
        public bool Loop => this.loop;

        public static SpriteAnimation Create(IEnumerable<Sprite> frames, float framesPerSecond, bool loop) {
            var spriteAnimation = CreateInstance<SpriteAnimation>();
            spriteAnimation.frames = frames.ToArray();
            spriteAnimation.framesPerSecond = framesPerSecond;
            spriteAnimation.loop = loop;
            return spriteAnimation;
        }
    }
}
