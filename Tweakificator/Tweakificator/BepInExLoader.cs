using BepInEx;
using BepInEx.Configuration;
using UnhollowerRuntimeLib;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.IO;

namespace Tweakificator
{
    [BepInPlugin(GUID, MODNAME, VERSION)]
    public class BepInExLoader : BepInEx.IL2CPP.BasePlugin
    {
        public const string
            MODNAME = "Tweakificator",
            AUTHOR = "erkle64",
            GUID = "com." + AUTHOR + "." + MODNAME,
            VERSION = "1.0.0";

        public static BepInEx.Logging.ManualLogSource log;

        public static string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string jsonFolder = Path.Combine(assemblyFolder, "Tweakificator");
        public static string recipeFolder = Path.Combine(jsonFolder, "Recipes");
        public static string itemsFolder = Path.Combine(jsonFolder, "Items");
        public static string tweaksFolder = Path.Combine(assemblyFolder, "..\\tweaks");

        public static bool firstRun = false;

        public static JObject patchData = null;
        public static JObject patchDataItemChanges = null;

        public BepInExLoader()
        {
            log = Log;
        }

        public override void Load()
        {
            if (!Directory.Exists(jsonFolder))
            {
                Directory.CreateDirectory(jsonFolder);
                firstRun = true;
            }

            if (!Directory.Exists(recipeFolder)) Directory.CreateDirectory(recipeFolder);
            if (!Directory.Exists(itemsFolder)) Directory.CreateDirectory(itemsFolder);
            if (!Directory.Exists(tweaksFolder)) Directory.CreateDirectory(tweaksFolder);

            foreach(var path in Directory.GetFiles(tweaksFolder, "*.json"))
            {
                var patch = JObject.Parse(File.ReadAllText(path));
                if(patchData == null)
                {
                    log.LogMessage(string.Format("Loading patch {0}", Path.GetFileName(path)));
                    patchData = patch;
                }
                else
                {
                    log.LogMessage(string.Format("Merging patch {0}", Path.GetFileName(path)));
                    patchData.Merge(patch);
                }
            }

            if(patchData != null)
            {
                patchDataItemChanges = patchData["changes"]["items"] as JObject;
            }

            log.LogMessage("Registering PluginComponent in Il2Cpp");

            try
            {
                ClassInjector.RegisterTypeInIl2Cpp<PluginComponent>();

                var go = new GameObject("Erkle64_Tweakificator_PluginObject");
                go.AddComponent<PluginComponent>();
                Object.DontDestroyOnLoad(go);
            }
            catch
            {
                log.LogError("FAILED to Register Il2Cpp Type: PluginComponent!");
            }

            try
            {
                var harmony = new Harmony(GUID);

                //var original = AccessTools.Method(typeof(NativeWrapper), "templateManager_registerItemTemplate");
                //var pre = AccessTools.Method(typeof(PluginComponent), "templateManager_registerItemTemplate");
                //harmony.Patch(original, prefix: new HarmonyMethod(pre));

                var original = AccessTools.Method(typeof(ItemTemplate), "onLoadPostprocess");
                var post = AccessTools.Method(typeof(PluginComponent), "onLoadItemTemplatePostProcess");
                harmony.Patch(original, postfix: new HarmonyMethod(post));
            }
            catch
            {
                log.LogError("Harmony - FAILED to Apply Patch's!");
            }
        }
    }
}