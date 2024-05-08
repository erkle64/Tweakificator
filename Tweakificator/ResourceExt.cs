using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Tweakificator
{
    public static class ResourceExt
    {
        static readonly Dictionary<string, Texture2D> loadedTextures = new Dictionary<string, Texture2D>();

        public static void RegisterTexture(string name, Texture2D texture)
        {
            loadedTextures[name] = texture;
        }

        public static Texture2D FindTexture(string name)
        {
            var tweakPath = Path.Combine(Plugin.texturesFolder, name + ".png");
            if (loadedTextures.TryGetValue(name, out Texture2D result))
            {
                return result;
            }
            else if (File.Exists(tweakPath))
            {
                var watch = new System.Diagnostics.Stopwatch();
                watch.Start();
                var tempTexture = new Texture2D(2, 2, TextureFormat.RGBA32, true, true);
                tempTexture.LoadImage(File.ReadAllBytes(tweakPath), false);
                var texture = new Texture2D(tempTexture.width, tempTexture.height, TextureFormat.RGB24, true, true);
                texture.name = name;
                texture.SetPixels(tempTexture.GetPixels());
                texture.Apply(true);
                texture.Compress(false);
                texture.Apply();
                watch.Stop();
                if (Plugin.verbose.Get()) Plugin.log.LogFormat("Loading texture '{0}' from '{1}' took {2}ms", name, tweakPath, watch.ElapsedMilliseconds);
                loadedTextures.Add(name, texture);
                Object.Destroy(tempTexture);
                return texture;
            }
            else
            {
                if (Plugin.verbose.Get()) Plugin.log.LogFormat("Searching for texture '{0}'", name);

                return Unfoundry.ResourceExt.FindTexture(name);
            }
        }
    }
}
