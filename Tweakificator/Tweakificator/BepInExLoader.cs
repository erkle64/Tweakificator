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
            VERSION = "1.2.0";

        public static BepInEx.Logging.ManualLogSource log;

        public static ConfigEntry<bool> forceDump;
        public static ConfigEntry<bool> dumpIcons;

        public static string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string dumpFolder = Path.Combine(assemblyFolder, "Tweakificator");
        public static string itemsDumpFolder = Path.Combine(dumpFolder, "Items");
        public static string recipesDumpFolder = Path.Combine(dumpFolder, "Recipes");
        public static string terrainBlocksDumpFolder = Path.Combine(dumpFolder, "TerrainBlocks");
        public static string iconsDumpFolder = Path.Combine(dumpFolder, "Icons");
        public static string tweaksFolder = Path.Combine(assemblyFolder, "..\\tweaks");

        public static bool firstRun = false;

        public static JObject patchData = null;
        public static JObject patchDataItemChanges = new JObject();
        public static JObject patchDataRecipeChanges = new JObject();
        public static JObject patchDataItemAdditions = new JObject();
        public static JObject patchDataRecipeAdditions = new JObject();

        public BepInExLoader()
        {
            PluginComponent.log = log = Log;
        }

        public override void Load()
        {
            forceDump = Config.Bind("Dump", "forceDump", false, "Overwrite existing dump files.");
            dumpIcons = Config.Bind("Dump", "dumpIcons", false, "Dump icon files. (very slow)");

            if (!Directory.Exists(dumpFolder))
            {
                Directory.CreateDirectory(dumpFolder);
                firstRun = true;
            }

            if (!Directory.Exists(itemsDumpFolder)) Directory.CreateDirectory(itemsDumpFolder);
            if (!Directory.Exists(recipesDumpFolder)) Directory.CreateDirectory(recipesDumpFolder);
            if (!Directory.Exists(terrainBlocksDumpFolder)) Directory.CreateDirectory(terrainBlocksDumpFolder);
            if (!Directory.Exists(iconsDumpFolder)) Directory.CreateDirectory(iconsDumpFolder);
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
                if (patchData.ContainsKey("changes"))
                {
                    var changes = (JObject)patchData["changes"];
                    if(changes.ContainsKey("items")) patchDataItemChanges = changes["items"] as JObject;
                    if(changes.ContainsKey("recipes")) patchDataRecipeChanges = changes["recipes"] as JObject;
                }
                if (patchData.ContainsKey("additions"))
                {
                    var additions = (JObject)patchData["additions"];
                    if(additions.ContainsKey("items")) patchDataItemAdditions = additions["items"] as JObject;
                    if(additions.ContainsKey("recipes")) patchDataRecipeAdditions = additions["recipes"] as JObject;
                }
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

                var original = AccessTools.Method(typeof(ItemTemplate), "onLoad");
                var pre = AccessTools.Method(typeof(PluginComponent), "onLoadItemTemplate");
                harmony.Patch(original, prefix: new HarmonyMethod(pre));

                original = AccessTools.Method(typeof(ItemTemplate), "LoadAllItemTemplatesInBuild");
                var post = AccessTools.Method(typeof(PluginComponent), "LoadAllItemTemplatesInBuild");
                harmony.Patch(original, postfix: new HarmonyMethod(post));

                original = AccessTools.Method(typeof(CraftingRecipe), "onLoad");
                pre = AccessTools.Method(typeof(PluginComponent), "onLoadRecipe");
                harmony.Patch(original, prefix: new HarmonyMethod(pre));

                original = AccessTools.Method(typeof(CraftingRecipe), "LoadAllCraftingRecipesInBuild");
                post = AccessTools.Method(typeof(PluginComponent), "LoadAllCraftingRecipesInBuild");
                harmony.Patch(original, postfix: new HarmonyMethod(post));

                original = AccessTools.Method(typeof(ResourceDB), "InitOnApplicationStart");
                post = AccessTools.Method(typeof(PluginComponent), "ResourceDBInitOnApplicationStart");
                harmony.Patch(original, postfix: new HarmonyMethod(post));

                original = AccessTools.Method(typeof(TerrainBlockType), "onLoad");
                pre = AccessTools.Method(typeof(PluginComponent), "onLoadTerrainBlockType");
                harmony.Patch(original, prefix: new HarmonyMethod(pre));
            }
            catch
            {
                log.LogError("Harmony - FAILED to Apply Patch's!");
            }
        }
    }
}