using System;
using UnhollowerBaseLib;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace Tweakificator
{
    public class PluginComponent : MonoBehaviour
    {
        public PluginComponent (IntPtr ptr) : base(ptr)
        {
        }

        //[HarmonyPrefix]
        //public static void templateManager_registerItemTemplate(
        //    ulong id,
        //    string identifier,
        //    ulong modId,
        //    string modIdentifier,
        //    string name,
        //    string nameLocalized,
        //    ref uint stackSize,
        //    uint toggleableModeType,
        //    IOBool skipForRunningIdxGeneration,
        //    int runningIdx_all,
        //    long burnable_fuelValueKJ_fpm,
        //    ulong burnable_residualItemTemplateId,
        //    uint burnable_residualItemTemplate_count,
        //    Vector3 meshScale,
        //    uint minecartMaterialIdx)
        //{
        public static void onLoadItemTemplatePostProcess(ItemTemplate __instance)
        {
            var path = Path.Combine(BepInExLoader.itemsFolder, __instance.identifier+".json");
            if(!File.Exists(path))
            {
                var itemDump = new ItemDump();
                itemDump.name = __instance.name;
                itemDump.stackSize = __instance.stackSize;
                itemDump.burnable_fuelValueKJ_fpm = __instance.burnable_fuelValueKJ_fpm;
                itemDump.burnable_residualItemTemplate_str = __instance.burnable_residualItemTemplate_str;
                itemDump.burnable_residualItemTemplate_count = __instance.burnable_residualItemTemplate_count;
                itemDump.explosionRadius = __instance.explosionRadius;
                itemDump.miningRange = __instance.miningRange;
                itemDump.miningTimeReductionInSec = __instance.miningTimeReductionInSec;
                File.WriteAllText(path, JsonConvert.SerializeObject(itemDump, Formatting.Indented));
            }

            if(BepInExLoader.patchDataItemChanges != null && BepInExLoader.patchDataItemChanges.ContainsKey(__instance.identifier))
            {
                BepInExLoader.log.LogMessage(string.Format("Patching item {0}", __instance.identifier));
                var changes = BepInExLoader.patchDataItemChanges[__instance.identifier] as JObject;
                if (changes.ContainsKey("name")) __instance.name = changes.Value<string>("name");
                if (changes.ContainsKey("stackSize")) __instance.stackSize = changes.Value<uint>("stackSize");
                if (changes.ContainsKey("burnable_fuelValueKJ_fpm")) __instance.burnable_fuelValueKJ_fpm = changes.Value<long>("burnable_fuelValueKJ_fpm");
                if (changes.ContainsKey("burnable_residualItemTemplate_str")) __instance.burnable_residualItemTemplate_str = changes.Value<string>("burnable_residualItemTemplate_str");
                if (changes.ContainsKey("burnable_residualItemTemplate_count")) __instance.burnable_residualItemTemplate_count = changes.Value<uint>("burnable_residualItemTemplate_count");
                if (changes.ContainsKey("explosionRadius")) __instance.explosionRadius = changes.Value<int>("explosionRadius");
                if (changes.ContainsKey("miningRange")) __instance.miningRange = changes.Value<float>("miningRange");
                if (changes.ContainsKey("miningTimeReductionInSec")) __instance.miningTimeReductionInSec = changes.Value<float>("miningTimeReductionInSec");
            }

            //BepInExLoader.log.LogMessage(string.Format("onLoadItemTemplate: {0} {1} {2} {3} {4}", __instance.name, __instance.identifier, __instance.id, __instance.includeInBuild, __instance.isHiddenItem));
            //BepInExLoader.log.LogMessage("onLoadItemTemplate");
            //BepInExLoader.log.LogMessage(string.Format(" - name: {0}", __instance.name));
            //BepInExLoader.log.LogMessage(string.Format(" - stackSize: {0}", __instance.stackSize));
            //BepInExLoader.log.LogMessage(string.Format(" - burnable_fuelValueKJ_fpm: {0}", __instance.burnable_fuelValueKJ_fpm));
            //BepInExLoader.log.LogMessage(string.Format(" - burnable_residualItemTemplateId: {0}", __instance.burnable_residualItemTemplate_str));
            //BepInExLoader.log.LogMessage(string.Format(" - burnable_residualItemTemplate_count: {0}", __instance.burnable_residualItemTemplate_count));
            //BepInExLoader.log.LogMessage(string.Format(" - explosionRadius: {0}", __instance.explosionRadius));
            //BepInExLoader.log.LogMessage(string.Format(" - miningRange: {0}", __instance.miningRange));
            //BepInExLoader.log.LogMessage(string.Format(" - miningTimeReductionInSec: {0}", __instance.miningTimeReductionInSec));
        }

        private struct ItemDump
        {
            public string name;
            public uint stackSize;
            public long burnable_fuelValueKJ_fpm;
            public string burnable_residualItemTemplate_str;
            public uint burnable_residualItemTemplate_count;
            public int explosionRadius;
            public float miningRange;
            public float miningTimeReductionInSec;
        }
    }
}