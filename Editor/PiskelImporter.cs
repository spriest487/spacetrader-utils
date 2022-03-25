using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

using UnityEngine;

namespace SpaceTrader.Util {
    [UnityEditor.AssetImporters.ScriptedImporter(1, ".piskel")]
    public class PiskelImporter : UnityEditor.AssetImporters.ScriptedImporter {
        // ReSharper disable InconsistentNaming
        [DataContract]
        private class PiskelFile {
            [DataMember]
            public int modelVersion;

            [DataMember]
            public PiskelData piskel;
        }

        [DataContract]
        private class PiskelData {
            [DataMember]
            public string name;
            
            [DataMember]
            public string description;
            
            [DataMember]
            public float fps;
            
            [DataMember]
            public int height;
            
            [DataMember]
            public int width;
            
            [DataMember]
            public string[] layers;
        }

        [DataContract]
        private class PiskelLayer {
            [DataMember]
            public string name;

            [DataMember]
            public float opacity;

            [DataMember]
            public int frameCount;

            [DataMember]
            public PiskelChunk[] chunks;
        }

        [DataContract]
        private class PiskelChunk {
            [DataMember]
            public int[][] layout;

            [DataMember]
            public string base64PNG;
        }
        // ReSharper restore InconsistentNaming

        [SerializeField]
        private TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        [SerializeField]
        private bool alphaIsTransparency = true;

        [SerializeField]
        private Vector2 spritePivot = new Vector2(0.5f, 0.5f);

        [SerializeField]
        private float pixelsPerUnit = 100;

        [SerializeField]
        private int extrude = 1;

        [SerializeField]
        private SpriteMeshType meshType = SpriteMeshType.FullRect;

        [SerializeField]
        private Vector2 borderMin;

        [SerializeField]
        private Vector2 borderMax;

        [SerializeField]
        private bool importAnimation = true;

        [SerializeField]
        private bool loopAnimation = true;
        
        public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx) {
            var serializer = new DataContractJsonSerializer(typeof(PiskelFile));
            var layerSerializer = new DataContractJsonSerializer(typeof(PiskelLayer));

            using (var assetFile = File.OpenRead(ctx.assetPath)) {
                var piskelFile = (PiskelFile)serializer.ReadObject(assetFile);
                var frameWidth = piskelFile.piskel.width;
                var frameHeight = piskelFile.piskel.height;

                if (piskelFile.modelVersion != 2) {
                    ctx.LogImportWarning("expected piskel model version 2");
                }
                
                var layers = piskelFile.piskel.layers
                    .Select(layerJson => {
                        using (var textReader = new MemoryStream(Encoding.Unicode.GetBytes(layerJson))) {
                            return layerSerializer.ReadObject(textReader);
                        }
                    })
                    .Cast<PiskelLayer>()
                    .ToList();

                var layerTextures = layers
                    .Select((layer, l) => {
                        if (layer.chunks.Length != 1) {
                            ctx.LogImportWarning("not supported: piskel layer with no chunks, or more than one chunk:"
                                + layer.name);
                            return default;
                        }
                        if (!ReadChunkTexture(layer.chunks[0], ctx, out var texture)) {
                            return default;
                        }

                        return (layerIndex: l, texture);
                    })
                    .Where(item => item.texture)
                    .ToList();

                var layerWidth = 0;
                var layerHeight = 0;
                if (layerTextures.Count > 0) {
                    layerWidth = layerTextures[0].texture.width;
                    layerHeight = layerTextures[0].texture.height;
                }

                var compositeTexture = new Texture2D(layerWidth, layerHeight, TextureFormat.RGBA32, false) {
                    filterMode = FilterMode.Point,
                    wrapMode = this.wrapMode,
                    alphaIsTransparency = this.alphaIsTransparency,
                };
                    
                for (var y = 0; y < compositeTexture.height; y += 1)
                for (var x = 0; x < compositeTexture.width; x += 1) {
                    compositeTexture.SetPixel(x, y, Color.clear);
                }

                foreach (var (layerIndex, texture) in layerTextures) {
                    BlendTexture(compositeTexture, texture, layers[layerIndex].opacity);
                }

                compositeTexture.Apply();
                ctx.AddObjectToAsset($"{piskelFile.piskel.name}_texture", compositeTexture);
                
                // build sprites
                var frames = new List<Sprite>();
                for (var y = 0; y < compositeTexture.height; y += frameHeight)
                for (var x = 0; x < compositeTexture.width; x += frameWidth) {
                    var rect = new Rect(x, y, frameWidth, frameHeight);
                    var border = new Vector4(this.borderMin.x, this.borderMin.y, this.borderMax.x, this.borderMax.y);

                    var sprite = Sprite.Create(compositeTexture,
                        rect,
                        this.spritePivot,
                        this.pixelsPerUnit,
                        (uint)this.extrude,
                        this.meshType,
                        border
                    );
                    var frameIndex = frames.Count;
                    
                    frames.Add(sprite);

                    sprite.name = $"{piskelFile.piskel.name}_{frameIndex}";
                    ctx.AddObjectToAsset(sprite.name, sprite);
                }

                if (this.importAnimation) {
                    var animation = SpriteAnimation.Create(frames, piskelFile.piskel.fps, this.loopAnimation);
                    animation.name = piskelFile.piskel.name;
                    ctx.AddObjectToAsset($"{piskelFile.piskel.name}_animation", animation);
                }

                ctx.SetMainObject(compositeTexture);
            }
        }

        private static bool ReadChunkTexture(PiskelChunk chunk, UnityEditor.AssetImporters.AssetImportContext ctx, out Texture2D texture) {
            texture = default;
            try {
                const string imageType = "data:image/png;base64,";
                if (string.IsNullOrEmpty(chunk.base64PNG) || !chunk.base64PNG.StartsWith(imageType)) {
                    throw new InvalidDataException("missing image data property");
                }

                texture = new Texture2D(1, 1, TextureFormat.RGBA32, 0, false) {
                    filterMode = FilterMode.Point,
                    alphaIsTransparency = true,
                };

                var pngBytes = Convert.FromBase64String(chunk.base64PNG.Substring(imageType.Length));
                if (!texture.LoadImage(pngBytes)) {
                    throw new InvalidDataException("bad PNG data");
                }

                return true;
            } catch (Exception e) {
                ctx.LogImportError($"Reading chunk texture failed: {e}");
                if (texture) {
                    DestroyImmediate(texture);
                }
                return false;
            }
        }

        private static void BlendTexture(Texture2D dst, Texture2D src, float fgAlpha) {
            for (var y = 0; y < src.height; y += 1)
            for (var x = 0; x < src.width; x += 1) {
                var dstColor = dst.GetPixel(x, y);
                var srcColor = src.GetPixel(x, y);
                
                var alpha = fgAlpha * srcColor.a;
                float Blend(float a, float b) => (1 - alpha) * a + alpha * b; 
                
                var result = new Color(
                    Blend(dstColor.r, srcColor.r),
                    Blend(dstColor.g, srcColor.g),
                    Blend(dstColor.b, srcColor.b),
                    alpha + dstColor.a
                );
                dst.SetPixel(x, y, result);
            }
        }
    }
}
