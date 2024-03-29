﻿using BepInEx;
using BepInEx.Configuration;
using UnhollowerRuntimeLib;
using HarmonyLib;
using UnityEngine;
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
            VERSION = "1.7.1";

        public static BepInEx.Logging.ManualLogSource log;

        public static ConfigEntry<bool> forceDump;
        public static ConfigEntry<bool> dumpIcons;
        public static ConfigEntry<bool> verbose;

        public static string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static string dumpFolder = Path.Combine(assemblyFolder, MODNAME);
        public static string itemsDumpFolder = Path.Combine(dumpFolder, "Items");
        public static string elementsDumpFolder = Path.Combine(dumpFolder, "Elements");
        public static string recipesDumpFolder = Path.Combine(dumpFolder, "Recipes");
        public static string terrainBlocksDumpFolder = Path.Combine(dumpFolder, "TerrainBlocks");
        public static string buildingsDumpFolder = Path.Combine(dumpFolder, "Buildings");
        public static string researchDumpFolder = Path.Combine(dumpFolder, "Research");
        public static string biomeDumpFolder = Path.Combine(dumpFolder, "Biomes");
        public static string iconsDumpFolder = Path.Combine(dumpFolder, "Icons");
        public static string tweaksFolder = Path.Combine(assemblyFolder, "..\\tweaks");
        public static string iconsFolder = Path.Combine(tweaksFolder, "icons");
        public static string texturesFolder = Path.Combine(tweaksFolder, "textures");

        public static bool firstRun = false;

        public static JObject patchData = null;
        public static JObject patchDataItemChanges = new JObject();
        public static JObject patchDataElementChanges = new JObject();
        public static JObject patchDataRecipeChanges = new JObject();
        public static JObject patchDataTerrainChanges = new JObject();
        public static JObject patchDataBuildingChanges = new JObject();
        public static JObject patchDataResearchChanges = new JObject();
        public static JObject patchDataBiomeChanges = new JObject();
        public static JObject patchDataItemAdditions = new JObject();
        public static JObject patchDataElementAdditions = new JObject();
        public static JObject patchDataRecipeAdditions = new JObject();
        public static JObject patchDataTerrainAdditions = new JObject();
        public static JObject patchDataBuildingAdditions = new JObject();
        public static JObject patchDataResearchAdditions = new JObject();
        public static JObject patchDataBiomeAdditions = new JObject();

        public BepInExLoader()
        {
            PluginComponent.log = log = Log;
        }

        public override void Load()
        {
            forceDump = Config.Bind("Dump", "forceDump", false, "Overwrite existing dump files.");
            dumpIcons = Config.Bind("Dump", "dumpIcons", false, "Dump icon files. (very slow)");
            verbose = Config.Bind("Log", "verbose", false, "Log extra information.");

            if (!Directory.Exists(dumpFolder))
            {
                Directory.CreateDirectory(dumpFolder);
                firstRun = true;
            }

            if (!Directory.Exists(itemsDumpFolder)) Directory.CreateDirectory(itemsDumpFolder);
            if (!Directory.Exists(elementsDumpFolder)) Directory.CreateDirectory(elementsDumpFolder);
            if (!Directory.Exists(recipesDumpFolder)) Directory.CreateDirectory(recipesDumpFolder);
            if (!Directory.Exists(terrainBlocksDumpFolder)) Directory.CreateDirectory(terrainBlocksDumpFolder);
            if (!Directory.Exists(buildingsDumpFolder)) Directory.CreateDirectory(buildingsDumpFolder);
            if (!Directory.Exists(researchDumpFolder)) Directory.CreateDirectory(researchDumpFolder);
            if (!Directory.Exists(biomeDumpFolder)) Directory.CreateDirectory(biomeDumpFolder);
            if (!Directory.Exists(iconsDumpFolder)) Directory.CreateDirectory(iconsDumpFolder);
            if (!Directory.Exists(tweaksFolder)) Directory.CreateDirectory(tweaksFolder);
            if (!Directory.Exists(iconsFolder)) Directory.CreateDirectory(iconsFolder);
            if (!Directory.Exists(texturesFolder)) Directory.CreateDirectory(texturesFolder);

            foreach (var path in Directory.GetFiles(tweaksFolder, "*.json"))
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
                    if(changes.ContainsKey("elements")) patchDataItemChanges = changes["elements"] as JObject;
                    if(changes.ContainsKey("recipes")) patchDataRecipeChanges = changes["recipes"] as JObject;
                    if(changes.ContainsKey("terrain")) patchDataTerrainChanges = changes["terrain"] as JObject;
                    if(changes.ContainsKey("buildings")) patchDataBuildingChanges = changes["buildings"] as JObject;
                    if(changes.ContainsKey("research")) patchDataResearchChanges = changes["research"] as JObject;
                    if(changes.ContainsKey("biomes")) patchDataResearchChanges = changes["biomes"] as JObject;
                }
                if (patchData.ContainsKey("additions"))
                {
                    var additions = (JObject)patchData["additions"];
                    if(additions.ContainsKey("items")) patchDataItemAdditions = additions["items"] as JObject;
                    if(additions.ContainsKey("elements")) patchDataItemAdditions = additions["elements"] as JObject;
                    if(additions.ContainsKey("recipes")) patchDataRecipeAdditions = additions["recipes"] as JObject;
                    if(additions.ContainsKey("terrain")) patchDataTerrainAdditions = additions["terrain"] as JObject;
                    if(additions.ContainsKey("buildings")) patchDataBuildingAdditions = additions["buildings"] as JObject;
                    if(additions.ContainsKey("research")) patchDataResearchAdditions = additions["research"] as JObject;
                    if(additions.ContainsKey("biomes")) patchDataResearchAdditions = additions["biomes"] as JObject;
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

                original = AccessTools.Method(typeof(ElementTemplate), "onLoad");
                pre = AccessTools.Method(typeof(PluginComponent), "onLoadElementTemplate");
                harmony.Patch(original, prefix: new HarmonyMethod(pre));

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

                original = AccessTools.Method(typeof(ItemTemplateManager._InitOnApplicationStart_d__34), "MoveNext");
                pre = AccessTools.Method(typeof(PluginComponent), "onItemTemplateManagerInitOnApplicationStart");
                harmony.Patch(original, prefix: new HarmonyMethod(pre));

                original = AccessTools.Method(typeof(BuildableObjectTemplate), "onLoad");
                pre = AccessTools.Method(typeof(PluginComponent), "onLoadBuildableObjectTemplate");
                harmony.Patch(original, prefix: new HarmonyMethod(pre));

                original = AccessTools.Method(typeof(BuildableObjectTemplate), "LoadAllBuildableObjectTemplatesInBuild");
                post = AccessTools.Method(typeof(PluginComponent), "LoadAllBuildableObjectTemplatesInBuild");
                harmony.Patch(original, postfix: new HarmonyMethod(post));

                original = AccessTools.Method(typeof(ResearchTemplate), "onLoad");
                pre = AccessTools.Method(typeof(PluginComponent), "onLoadResearchTemplate");
                harmony.Patch(original, prefix: new HarmonyMethod(pre));

                original = AccessTools.Method(typeof(BiomeTemplate), "onLoad");
                pre = AccessTools.Method(typeof(PluginComponent), "onLoadBiomeTemplate");
                harmony.Patch(original, prefix: new HarmonyMethod(pre));
            }
            catch
            {
                log.LogError("Harmony - FAILED to Apply Patch's!");
            }
        }
    }
}