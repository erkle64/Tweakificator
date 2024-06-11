using UnityEngine;

namespace Tweakificator
{
    internal static class FoundryExtensions
    {
        internal static MaterialSwapManager GetMaterialSwapManager(this BuildableObjectGO buildableObjectGO)
        {
            if (buildableObjectGO is AutoProducerGO autoProducerGO) return autoProducerGO.MaterialSwapManager;
            if (buildableObjectGO is BaseStationGO baseStationGO) return baseStationGO.MaterialSwapManager;
            if (buildableObjectGO is BiomassBurnerGO biomassBurnerGO) return biomassBurnerGO.MaterialSwapManager;
            if (buildableObjectGO is BurnerGeneratorGO burnerGeneratorGO) return burnerGeneratorGO.MaterialSwapManager;
            if (buildableObjectGO is ProducerGO producerGO) return producerGO.MaterialSwapManager;
            if (buildableObjectGO is PumpGO pumpGO) return pumpGO.MaterialSwapManager;
            return null;
        }

        internal static MaterialSwapManager GetMaterialSwapManager(this GameObject gameObject)
        {
            if (!gameObject.TryGetComponent<BuildableObjectGO>(out var buildableObjectGO)) return null;
            return buildableObjectGO.GetMaterialSwapManager();
        }

        internal static bool TryGetMaterialSwapManager(this GameObject gameObject, out MaterialSwapManager materialSwapManager)
        {
            materialSwapManager = gameObject.GetMaterialSwapManager();
            return materialSwapManager != null;
        }

        internal static void SetMaterialSwapManager(this BuildableObjectGO buildableObjectGO, MaterialSwapManager materialSwapManager)
        {
            if (buildableObjectGO is AutoProducerGO autoProducerGO) autoProducerGO._materialSwapManager = materialSwapManager;
            if (buildableObjectGO is BaseStationGO baseStationGO) baseStationGO._materialSwapManager = materialSwapManager;
            if (buildableObjectGO is BiomassBurnerGO biomassBurnerGO) biomassBurnerGO._materialSwapManager = materialSwapManager;
            if (buildableObjectGO is BurnerGeneratorGO burnerGeneratorGO) burnerGeneratorGO._materialSwapManager = materialSwapManager;
            if (buildableObjectGO is ProducerGO producerGO) producerGO._materialSwapManager = materialSwapManager;
            if (buildableObjectGO is PumpGO pumpGO) pumpGO._materialSwapManager = materialSwapManager;
        }

        internal static bool SetMaterialSwapManager(this GameObject gameObject, MaterialSwapManager materialSwapManager)
        {
            if (!gameObject.TryGetComponent<BuildableObjectGO>(out var buildableObjectGO)) return true;
            buildableObjectGO.SetMaterialSwapManager(materialSwapManager);
            return false;
        }
    }
}
