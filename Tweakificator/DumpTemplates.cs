using System.Collections.Generic;
using TinyJSON;
using UnityEngine;

namespace Tweakificator
{
    public struct ItemDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public bool includeInDemo;
        public bool includeInCreativeMode;
        public string creativeModeCategory_str;
        public string entitlementIdentifier;
        public string icon_identifier;
        public string __conveyorItemTexture;
        public uint stackSize;
        public bool isHiddenItem;
        public ItemTemplate.ItemTemplateFlags flags;
        public ItemTemplate.ItemTemplateToggleableModeTypes toggleableModeType;
        public Dictionary<string, ItemModeProxy> toggleableModes;
        public ItemTemplate.GenericItemActionButtons[] genericItemActionButtons;
        public bool skipForRunningIdxGeneration;
        public ItemTemplate.ItemDestroyFlags itemDestroyFlags;
        public Vector3 meshScale;
        public Quaternion meshRotation;
        public string buildableObjectIdentifer;
        public ItemTemplate.HandheldSubType handheldSubType;
        public bool handheld_miner_shakeRightArmOnUse;
        public string handheld_defaultPowerPoleItemTemplate_str;
        public bool supportsFocusMode;
        public float miningTimeReductionInSec;
        public float miningRange;
        public int explosionRadius;
        public string burnable_fuelValueKJ_str;
        public string burnable_residualItemTemplate_str;
        public uint burnable_residualItemTemplate_count;
        public string salesItem_currencyIdentifier;
        public uint salesItem_currencyAmount;
        public long salesItem_skyPlatformBaseCapacity;
        public long salesItem_sortOrderASC;
        public string salesItem_hiddenByResearchIdentifier;
        public long salesCurrency_skyPlatformBaseCapacity;
        public long salesCurrency_sortOrderASC;
        public string salesCurrency_hiddenByResearchIdentifier;
        public int sciencePack_researchFrameSortingOrder;
        public Color sciencePack_color;
        public int railminer_slotLength;
        public int railminer_speedInSlotsPerTick;
        public int railminer_minecartSpeedInSlotsPerTick;
        public string[] railMiner_terrainTargetList_str;
        public bool isMinecartItem;
        public string trainVehicle_templateIdentifier;
        public string alStarter_alotIdentifier;
        public string alStarter_sellItemTemplateIdentifier;
        public uint warehouse_stackSize;
        public string rubble_recyclingResultIdentifier;
        public ItemTemplate.AutoProducerAction[] autoProducerRecipes;
    }

    public struct ElementDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public string icon_identifier;
        public ElementTemplate.ElementTemplateFlags flags;
        public Color pipeContentColor;
        public int pipeContentType;
        public string fuel_fuelValueKJPerL_str;
        public string fuel_residualTemplate_identifier;
        public float fuel_residualAmountPerL;
        public ElementTemplate.ElementTemplateFuelFlags fuel_flags;
        public float fuel_minViableVolumeL;
    }

    public struct RecipeDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public string entitlementIdentifier;
        public string icon_identifier;
        public string category_identifier;
        public string rowGroup_identifier;
        public bool hideInDemo;
        public bool isHiddenInCharacterCraftingFrame;
        public bool isHiddenByNarrativeTrigger;
        public bool isNeverUnseenRecipe;
        public string narrativeTrigger;
        public Dictionary<string, CraftingRecipeItemInputProxy> input_data;
        public Dictionary<string, CraftingRecipeItemInputProxy> output_data;
        public Dictionary<string, CraftingRecipeElementalInputProxy> inputElemental_data;
        public Dictionary<string, CraftingRecipeElementalInputProxy> outputElemental_data;
        public string relatedItemTemplateIdentifier;
        public int sortingOrderWithinRowGroup;
        public int timeMs;
        public string[] tags;
        public bool forceShowOutputsAtTooltips;
        public int recipePriority;
        public string extraInfoTooltipText;
    }

    public struct RecipeCategoryDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public string icon_identifier;
        public string identifier_defaultRowGroup;
    }

    public struct RecipeCategoryRowDump
    {
        public string modIdentifier;
        public string identifier;
        public int sortingOrder;
        public string identifier_category;
    }

    public struct CraftingTagDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public string slotLockDescription;
    }

    public struct TerrainBlockDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public TerrainBlockType.TerrainTypeFlags flags;
        public float movementSpeedModifier;
        public Color shatterColor;
        public Color scanColor;
        public bool destructible;
        public string yieldItemOnDig;
        public float miningTimeInSec;
        public int requiredMiningHardnessLevel;
        //public MovementSoundPack movementSoundPack;
        public Color mapColor;
        public TerrainBlockType.DecorFlags decorFlags;
        public bool hasTrim;
        public bool useTextureTiling;
        public bool useLayerNoise;
        public bool geologicalScanner_showOnScan;
        public bool unlockedForOreScannerByDefault;
        public string ore_yield_id;
        public int maxHeight;
        public int minHeight;
        public uint oreSpawn_chancePerChunk_ground;
        public uint oreSpawn_chancePerChunk_surface;
        public TerrainBlockType.OreSpawnFlags oreSpawnFlags;
        public Vector3IntProxy minSize;
        public Vector3IntProxy maxSize;
        public int sizeRng;
        public int averageYield;
        public string yieldVarietyPercent_str;
        public int depthIncreasePerTile;
        public WorldDecorSpawnInfo[] surfaceOre_worldDecor;
        public TerrainBlockType.FixedOrePatchData[] fixedOrePatchDataArray;
        public int rmd_ticksPerItem;
        public string rmd_miningYield;
        public int rmd_totalYield;
        public string oreVeinMineable_yieldItem_identifier;
        public int oreVeinMineable_averageYield;
        public int oreVeinMineable_yieldVariety;
        public Texture2DProxy texture_abledo;
        public Texture2DProxy texture_metalHeightSmoothHeight;
        public float blendSmoothness;
        public bool hasSideTextures;
        public Texture2DProxy texture_side_abledo;
        public Texture2DProxy texture_side_metalSmoothHeight;
        public Texture2DProxy texture_side_mask;
        public Texture2DProxy texture_side_mask_metalSmoothHeight;
        public bool hasBottomTextures;
        public Texture2DProxy texture_bottom_abledo;
        public Texture2DProxy texture_bottom_metalSmoothHeight;
    }

    public struct BuildableObjectDump
    {
        //public ulong id;
        public string modIdentifier;
        public string identifier;
        public BuildableObjectTemplate.BuildableObjectType type;
        public BuildableObjectTemplate.SimulationType simulationType;
        public BuildableObjectTemplate.SimulationSleepFlags simulationSleepFlags;
        public bool simTypeSleep_initial;
        public bool isSuperBuilding;
        public float __conveyorTextureSpeed;
        public Vector3IntProxy size;
        public bool validateTerrainTileColliders;
        public ProxyObject prefabOnDisk;
        public ProxyObject debrisWithExplosionForcePrefab;
        public ProxyObject droneMiner_dronePrefab;
        public ProxyObject droneTransport_dronePrefab;
        public ProxyObject loader_prefabOutputDummy;
        public ProxyObject loader_prefabAutoDummy;
        public ProxyObject voxelMeshCorner;
        public ProxyObject worldDecor_despawnPrefab;
        public ProxyObject minecartDepot_railMinerPrefab;
        public ProxyObject freightElevator_middlePartGOH1;
        public ProxyObject freightElevator_middlePartGOH2;
        public ProxyObject elevatorStation_cabinPrefab;
        public ProxyObject elevatorStation_closeOffGO_top;
        public ProxyObject elevatorStation_closeOffGO_bottom;
        public ProxyObject constructionDronePort_dronePrefab;
        public ProxyObject transportDronePort_transportDronePrefab;
        public ProxyObject emergencyBeacon_dropPodPrefab;
        public ProxyObject emergencyBeacon_customDemolitionPrefab;
        public BuildableObjectTemplate.DragBuildType dragBuildType;
        public float dragModeOrientationSlope_planeAngle;
        public Quaternion dragModeOrientationSlope_planeAngleQuaternion;
        public int dragModeOrientationSlope_yOrientationModifier;
        public int dragModeOrientationSlope_yOffsetPerInstance;
        public bool dragModeOrientationSlope_allowSideways;
        public BuildableObjectTemplate.DragAxis dragModeLine_dragAxis;
        public float dragModeLine_dragPlaneMultipler;
        public float dragModeLine_nonUnlockedYCorrection;
        public BuildableObjectTemplate.DragAxis dragModePlane_dragAxis01;
        public BuildableObjectTemplate.DragAxis dragModePlane_dragAxis02;
        public BuildableObjectTemplate.DragMode[] dragModes;
        public BuildableObjectTemplate.CustomSnapMode customSnapMode;
        public float demolitionTimeSec;
        public bool canBeDestroyedByDynamite;
        public string conversionGroup_str;
        public bool isVisibleOnMap;
        public byte mapColorPriority;
        public bool skipForRunningIdxGeneration;
        //public AudioClip audioClip_customBuildSound;
        public bool hasNameOverride;
        public string nameOverride;
        public bool canBeCopiedByTablet;
        public Color defaultObjectColor;
        public BuildableObjectTemplate.AnalyticsTrackerId analyticsTrackerId;
        public bool hasToBeOnFoundation;
        public bool floorShouldOutlineBuilding;
        public BuildableObjectTemplate.FoundationConnectorType foundationConnection;
        public int loaderLevel;
        public bool disableLoaders;
        public bool hasPipeLoaderSupport;
        public v3i[] blockedLoaderPositions;
        public bool rotationAllowed;
        public bool canBeRotatedAroundXAxis;
        public BuildableObjectTemplate.AdditionalAABB3D[] additionalAABBs_input;
        public bool isModularBuilding;
        public BuildableObjectTemplate.ModularBuildingType modularBuildingType;
        public string modularBuildingModule_descriptionName;
        public uint modularBuildingModule_amountItemCost;
        public string modularBuildingModule_unlockedByResearchTemplateIdentifier;
        public ModularBuildingConnectionNode[] modularBuildingConnectionNodes;
        public ModularBuildingModuleLimit[] modularBuildingLimits;
        public bool modularBuilding_forceRequirePCM;
        public Vector3IntProxy modularBuildingLocalSearchAnchor;
        public ModularEntityItemCosts[] modularBuildingItemCost;
        public ModularEntityItemCosts[] modularBuildingRubble;
        public bool modularBuilding_hasNoEnabledState;
        public BuildableObjectTemplate.PowerComponentType powerComponentType;
        public BuildableObjectTemplate.BuildableObjectPowerSubType powerSubType;
        public string energyConsumptionKW_str;
        public int powerProducer_drawPriority;
        public bool spp_showToggleButton;
        public bool spp_showPowerButton;
        public bool spp_disablePowerCheck;
        public bool spp_disableGridCheck;
        public ScreenPanelProfile_Native.SPP_GridCheckType spp_gridCheckType;
        public bool hasEnergyGridConnection;
        public bool hasPoleGridConnection;
        public int poleGrid_connectionRange;
        public Vector3IntProxy poleGrid_connectorOffset;
        public int poleGrid_maxConnections;
        public int poleGrid_reservedConnections;
        public BuildableObjectTemplate.PoleGridTypes poleGridType;
        public BuildableObjectTemplate.PoleGridTypes poleGridConnectionMatrix;
        public bool hasFuelManagerSolid;
        public bool hasFuelManagerElemental;
        public ElementTemplate.ElementTemplateFuelFlags fme_fuelFlags;
        public string fme_lockedElementTemplateIdentifier;
        public bool hasLightSource;
        public BuildableObjectTemplate.LightEmitter[] lightEmitters;
        public bool spawnFlyingDebrisWhenExploding;
        public BuildableObjectTemplate.ItemBufferUIRow[] itemBufferSlotMap;
        public bool hasRailings;
        //public WalkwayMeshEntry walkway_mesh_noRailings;
        //public WalkwayMeshEntry walkway_mesh_xPos;
        //public WalkwayMeshEntry walkway_mesh_xNeg;
        //public WalkwayMeshEntry walkway_mesh_zPos;
        //public WalkwayMeshEntry walkway_mesh_zNeg;
        //public WalkwayMeshEntry walkway_mesh_xPos_xNeg;
        //public WalkwayMeshEntry walkway_mesh_z_Pos_zNeg;
        //public WalkwayMeshEntry walkway_mesh_xPos_zPos;
        //public WalkwayMeshEntry walkway_mesh_xPos_zNeg;
        //public WalkwayMeshEntry walkway_mesh_xNeg_zPos;
        //public WalkwayMeshEntry walkway_mesh_xNeg_zNeg;
        //public WalkwayMeshEntry walkway_mesh_xPos_zNeg_xNeg;
        //public WalkwayMeshEntry walkway_mesh_zPos_xPos_zNeg;
        //public WalkwayMeshEntry walkway_mesh_xPos_zPos_xNeg;
        //public WalkwayMeshEntry walkway_mesh_zPos_xNeg_zNeg;
        //public WalkwayMeshEntry walkway_mesh_allRailings;
        public bool hasIntraBuildingWalkways;
        public BuildableObjectTemplate.IntraBuildingWalkwayData[] intraBuildingWalkwayData;
        public bool hasAdjacentWalkwayOverrides;
        public BuildableObjectTemplate.IntraBuildingWalkwayData[] adjacentWalkwayOverridesPos;
        public bool hasConveyorConnectionManager;
        public BuildableObjectTemplate.ModularBuildingConveyorConnectionData[] ccm_connectionData;
        public bool hasStrutManager;
        public BuildableObjectTemplate.AdditionalAABB3D[] sm_strutPillars;
        public string sm_strutPillarBotIdentifier;
        public bool hasFluidBoxManager;
        public bool fbm_sendUpdateEventsForRegularFluidBoxes;
        public BuildableObjectTemplate.FluidBoxData[] fbm_fluidBoxes;
        public IOFluidBoxData[] fbm_ioFluidBoxes;
        public int droneMiner_oreSearchRadius;
        public int droneMiner_itemCapacityPerDrone;
        public string droneMiner_miningSpeed_str;
        public string droneMiner_droneCharge_str;
        public int droneMiner_droneCount;
        public Vector3IntProxy droneMiner_dockPositionInside;
        public Vector3IntProxy droneMiner_dockPositionOutside;
        //public AudioClip droneMiner_audioClip_droneHover;
        //public AudioClip droneMiner_audioClip_droneMining;
        public List<Vector3IntProxy> droneMiner_list_localBlocksAllowedToBeTraversed;
        public float dissolver_elementalConsumptionPerSecond;
        public string dissolver_solidsPerSecond_str;
        public bool dissolver_isElemental;
        public bool droneTransport_isStartStation;
        public string droneTransport_energyConsumptionPerSecondFlightTime_kj_str;
        public string droneTransport_energyRechargeRatePerSecond_kj_str;
        public float droneTransport_travelSpeed_mPerSec;
        public float droneTransport_rotationSpeedDegreePerSec;
        public float droneTransport_droneYOffset;
        public Vector3IntProxy loader_localBeltOffset;
        public int loader_ticksPerAction;
        public string loader_idlePowerConsumption_kjPerS_str;
        public bool loader_isFilter;
        public string pipeLoader_maxThroughputPerTickInLiter_str;
        public string pipeLoader_idlePowerConsumption_kjPerS_str;
        public List<string> pipeLoader_allowedPipeGroupIdentifiers;
        public v3i pipeLoader_fluidBoxPositionOrigin_localOffset;
        public v3i pipeLoader_fluidBoxPositionTarget_localOffset;
        public v3i pipeLoader_buildingPosition_localOffset;
        public uint storage_slotSize;
        public BuildableObjectTemplate.ProducerRecipeType producerRecipeType;
        public string producer_recipeTimeModifier_str;
        //public AudioClip producer_audioClip_active;
        public string producer_recipeType_fixed;
        public string[] producer_recipeType_tags;
        public string autoProducer_recipeTimeModifier_str;
        //public AudioClip autoProducer_audioClip_active;
        public string autoProducer_recipeType_tag;
        //public AudioClip producer_audioClip_customItemFinishSound;
        public bool conveyor_isSlope;
        public string conveyor_slopePartner_str;
        public int conveyor_speed_slotsPerTick;
        public Texture2DProxy buildingPart_texture_albedo;
        public bool buildingPart_hasSideTextures;
        public Texture2DProxy buildingPart_texture_side_albedo;
        public bool buildingPart_hasBottomTextures;
        public Texture2DProxy buildingPart_texture_bottom_albedo;
        public string battery_capacityKJ_str;
        public int shippingPad_inventorySlotCount;
        public string shippingPad_requiredChargeKJ_str;
        public string shippingPad_chargePerSecondKJ_str;
        public int shippingPad_timeInSpaceSec;
        public bool pipe_hasVisualUpdate;
        public uint pipe_visualUpdateFluidBoxTemplateIdx;
        public string transformer_transmissionRate_kjPerS_str;
        //public AudioClip transformer_audioClip_active;
        public string lvgGenerator_transmissionRate_kjPerS_str;
        public string lvgGenerator_lockedItemTemplate_identifier;
        //public AudioClip lvgGenerator_audioClip_active;
        public string lvgGenerator_efficiency_str;
        public string solarPanel_outputMax_str;
        public string solarPanel_outputMin_str;
        public bool solarPanel_rotatingPart;
        public WorldDecorMiningYield[] worldDecor_miningYield;
        public float worldDecor_miningTimeSec;
        //public AudioClip worldDecor_audioClip_afterHarvesting;
        public bool worldDecor_isDebris;
        public WorldDecorSpecialDrop[] worldDecor_specialDrops;
        public Color worldDecor_scratchColor;
        public bool worldDecor_canGrow;
        public int worldDecor_growTimeSec;
        public float worldDecor_growStartScale;
        public string worldDecor_plantSeedIdentifier;
        public string[] worldDecor_allowedGrowableTerrainBlocks;
        public SuperBuildingLevel[] superBuilding_levels;
        public string[] superBuilding_researchUnlocks_str;
        public Vector3IntProxy[] superBuilding_allowedLoaderPositions;
        public int superBuilding_loaderIndicator_rotY;
        public bool superBuilding_refundOnDemolish;
        public string researchLab_sciencePack_str;
        public string[] researchEntity_scienceItems;
        public string[] researchEntity_scienceItemsVisibilityResearch;
        public string researchEntity_powerDemand_kjPerSecond_data;
        public string pumpjack_amountPerSec_str;
        public Vector3IntProxy pumpjack_drillOffset;
        public int pumpjack_maxDrillDepth;
        //public AudioClip pumpjack_audioClip_active;
        public string burnerGenerator_powerGenertaionRate_kjPerS_str;
        public string burnerGenerator_efficiency_str;
        public string terrainBlock_tbtIdentifier;
        public Vector3IntProxy minecartDepot_connectionPoint;
        public BuildingManager.BuildOrientation minecartDepot_connectionSearchDirection;
        public uint minecartDepot_miningInventorySlots;
        public uint minecartDepot_cartInterval_sec;
        public string minecartDepot_autobuildTrackTemplate_str;
        public Vector3IntProxy[] minecartTracks_connectionPoints;
        public BuildingManager.BuildOrientation[] minecartTracks_connectionSearchDirection;
        public int minecartTracks_slotLength;
        public int freightContainer_speedPerTick;
        public long freightElevator_tierID;
        public string elevatorStation_structureBOT_str;
        //public AudioClip elevatorStation_audioClip_cabinMoving;
        //public AudioClip elevatorStation_audioClip_doorsOpening;
        //public AudioClip elevatorStation_audioClip_doorsClosing;
        //public AudioClip elevatorStation_audioClip_bell;
        public float door_secondsToOpen;
        //public AudioClip door_audioClip_openClose_trigger;
        //public AudioClip door_audioClip_openClose_loop;
        //public AudioClip geologicalScanner_audioClip_active;
        public string blastFurnace_speedModifier;
        public string blastFurnace_outputMultiplier;
        public string blastFurnace_optimalRunningTemp;
        public string blastFurnace_minRunningTemp;
        public string blastFurnace_hotAirTemplateIdentifier;
        public int blastFurnace_shutdownTimer_base_sec;
        public int blastFurnace_shutdownTimer_temp_sec;
        public string blastFurnace_towerModule_capacity;
        public string blastFurnace_towerModule_speedIncrease;
        public string blastFurnace_towerModuleBotIdentifier;
        public string blastFurnace_gasExhaustDrainModuleBotIdentifier;
        public string blastFurnace_towerModule_hotAirConsumptionPercentIncrease;
        public string blastFurnace_baseHotAirConsumptionPerTick;
        public string blastFurnace_maxHeatLossPerTick;
        public string blastFurnace_heatGainPerTick;
        public string blastFurnace_speedPercentageAtMinRunningTemp;
        public string resourceConverter_powerConsumption_kjPerSec;
        public CraftingRecipe.CraftingRecipeElementalInput[] resourceConverter_input_elemental;
        public CraftingRecipe.CraftingRecipeElementalInput[] resourceConverter_output_elemental;
        public ResourceConverterModuleSpeedBonus[] resourceConverter_speedBonusModules;
        public bool resourceConverter_hasAdjacencyBonus;
        public string resourceConverter_powerDecreasePerAdjacentResourceConverter;
        public byte resourceConverter_adjacencyBonusAxis;
        //public AudioClip resourceConverter_audioClip_active;
        public v3i frackingTower_drillOffset;
        public uint frackingTower_maxDrillLength;
        public uint frackingTower_powerConsumptionPercentagePerOptionalTowerModule;
        public float frackingTower_fluidThroughputPerTowerPerSecond;
        public string frackingTower_towerModuleBOT_identifier;
        public float modularFluidTank_baseFluidCapacity;
        public string modularFluidTank_towerModuleBOT_identifier;
        public string modularFluidTank_topModuleBOT_identifier;
        public float modularFluidTank_fluidCapacityPerTower;
        public int trainLoadingStation_itemType;
        public Vector3IntProxy trainLoadingStation_trackSearchOffset01;
        public Vector3IntProxy trainLoadingStation_trackSearchOffset02;
        //public TrainLoadingStationCompatibleTrackTemplate[] trainLoadingStation_compatibleTrackTemplates;
        public uint al_start_slotWidth;
        public uint al_start_speedInSlotsPerTick;
        public Vector3IntProxy al_start_localOffset_outputOrigin;
        public Vector3IntProxy al_start_localOffset_outputTarget;
        public string alStart_requiredPowerPerAction_kj_str;
        public SplineDataContainer al_start_splineDataContainer;
        public uint al_rail_slotWidth;
        public uint al_rail_speedInSlotsPerTick;
        public Vector3IntProxy al_rail_localOffset_inputOrigin;
        public Vector3IntProxy al_rail_localOffset_inputTarget;
        public Vector3IntProxy al_rail_localOffset_outputOrigin;
        public Vector3IntProxy al_rail_localOffset_outputTarget;
        public SplineDataContainer al_rail_splineDataContainer;
        public int al_rail_csm_outputOrientationModifier;
        public BuildableObjectTemplate.AL_ProducerMachineType al_producer_machineType;
        public uint al_producer_slotWidth_input;
        public uint al_producer_slotWidth_output;
        public uint al_producer_speedInSlotsPerTick;
        public Vector3IntProxy al_producer_localOffset_inputOrigin;
        public Vector3IntProxy al_producer_localOffset_inputTarget;
        public Vector3IntProxy al_producer_localOffset_outputOrigin;
        public Vector3IntProxy al_producer_localOffset_outputTarget;
        public SplineDataContainer al_producer_splineDataContainer_input;
        public SplineDataContainer al_producer_splineDataContainer_output;
        public uint al_endConsumer_itemCapacity;
        public BuildableObjectTemplate.InteractionPointData[] al_endConsumer_interactionPoints;
        public uint al_endConsumer_slotWidth;
        public uint al_endConsumer_speedInSlotsPerTick;
        public Vector3IntProxy al_endConsumer_localOffset_inputOrigin;
        public Vector3IntProxy al_endConsumer_localOffset_inputTarget;
        public SplineDataContainer al_endConsumer_splineDataContainer;
        public uint al_merger_speedInSlotsPerTick;
        public uint al_merger_slotWidth_input01;
        public uint al_merger_slotWidth_input02;
        public uint al_merger_slotWidth_input03;
        public uint al_merger_slotWidth_input01_internal;
        public uint al_merger_slotWidth_input02_internal;
        public uint al_merger_slotWidth_input03_internal;
        public uint al_merger_slotWidth_output;
        public Vector3IntProxy al_merger_localOffset_inputOrigin01;
        public Vector3IntProxy al_merger_localOffset_inputTarget01;
        public Vector3IntProxy al_merger_localOffset_inputOrigin02;
        public Vector3IntProxy al_merger_localOffset_inputTarget02;
        public Vector3IntProxy al_merger_localOffset_inputOrigin03;
        public Vector3IntProxy al_merger_localOffset_inputTarget03;
        public Vector3IntProxy al_merger_localOffset_outputOrigin;
        public Vector3IntProxy al_merger_localOffset_outputTarget;
        public SplineDataContainer al_merger_splineDataContainer_input01;
        public SplineDataContainer al_merger_splineDataContainer_input02;
        public SplineDataContainer al_merger_splineDataContainer_input03;
        public SplineDataContainer al_merger_splineDataContainer_input01_internal;
        public SplineDataContainer al_merger_splineDataContainer_input02_internal;
        public SplineDataContainer al_merger_splineDataContainer_input03_internal;
        public SplineDataContainer al_merger_splineDataContainer_output;
        public uint al_splitter_speedInSlotsPerTick;
        public uint al_splitter_slotWidth_output01;
        public uint al_splitter_slotWidth_output02;
        public uint al_splitter_slotWidth_output03;
        public uint al_splitter_slotWidth_input;
        public Vector3IntProxy al_splitter_localOffset_outputOrigin01;
        public Vector3IntProxy al_splitter_localOffset_outputTarget01;
        public Vector3IntProxy al_splitter_localOffset_outputOrigin02;
        public Vector3IntProxy al_splitter_localOffset_outputTarget02;
        public Vector3IntProxy al_splitter_localOffset_outputOrigin03;
        public Vector3IntProxy al_splitter_localOffset_outputTarget03;
        public Vector3IntProxy al_splitter_localOffset_inputOrigin;
        public Vector3IntProxy al_splitter_localOffset_inputTarget;
        public SplineDataContainer al_splitter_splineDataContainer_output01;
        public SplineDataContainer al_splitter_splineDataContainer_output02;
        public SplineDataContainer al_splitter_splineDataContainer_output03;
        public SplineDataContainer al_splitter_splineDataContainer_input;
        public BuildingManager.BuildOrientation[] escalator_localForwardDirection;
        public EscalatorGO.EscalatorType escalator_type;
        public string[] baseStation_craftingRecipeIdentifier;
        public uint oreVeinMiner_railMinerSlotLength;
        public uint oreVeinMiner_railMinerDrillSlotPadding;
        public uint oreVeinMiner_railMinerMoveSpeedSlotsPerTick;
        public string oreVeinMiner_ticksPerOre_str;
        public uint oreVeinMiner_minecartInventorySlotSize;
        public uint oreVeinMiner_minecartMoveSpeedSlotsPerTick;
        public uint oreVeinMiner_minecartSpawnOffset_toDepot;
        public uint oreVeinMiner_minecartSpawnOffset_toMiner;
        public uint oreVeinMiner_minecartDespawnDeadzone_toDepot;
        public uint oreVeinMiner_minecartDespawnDeadzone_toMiner;
        public float oreVeinMiner_minecartRenderingOffsetToDepot;
        public float oreVeinMiner_minecartRenderingOffsetToMiner;
        public uint oreVeinMiner_powerConsumptionBase_kjPerSec;
        public uint oreVeinMiner_powerConsumptionMining_kjPerSec;
        public string boiler_elementTemplateIdentifier_source;
        public string boiler_elementTemplateIdentifier_output;
        public float boiler_consumptionPerSecond;
        public float boiler_outputPerSecond;
        public string constructionDronePort_itemContributionPerSecond_str;
        public float constructionDronePort_droneYOffset;
        public float constructionDronePort_droneTravelHeight;
        public float constructionDronePort_droneTravelSpeed_mPerS;
        public float constructionDronePort_droneTravelRotation_degPerS;
        public string constructionDronePort_energyBuffer_kj_str;
        public string constructionDronePort_energyConsumptionPerItem_kj_str;
        public string constructionDronePort_energyRechargeRate_kjPerS_str;
        public v3i[] transportDronePort_dronePositions;
        public uint transportDronePort_droneInventoryCapacity;
        public float transportDronePort_droneTravelHeight;
        public float transportDronePort_droneTravelSpeed_mPerS;
        public float transportDronePort_droneTravelRotation_degPerS;
        public BuildableObjectTemplate.InteractionPointData[] constructionWarehouse_interactionPoints;
        public uint constructionWarehouse_recyclingSpeedItemsPerSecond;
        public string constructionWarehouse_powerDemandRecycling_kjPerS_str;
        public BuildableObjectTemplate.GreenhouseInput[] greenhouse_arrayInputs;
        public uint greenhouse_doubleOutputPercentChance;
        public uint greenhouse_craftingTimeMs;
        public BuildableObjectTemplate.InteractionPointData[] salesItemWarehouse_interactionPoints;
        public uint salesItemWarehouse_itemCapacity;
        public BuildableObjectTemplate.InteractionPointData[] salesCurrencyWarehouse_interactionPoints;
        public uint salesCurrencyWarehouse_itemCapacity;
        public string emergencyBeacon_yieldItemTemplateIdentifier;
        public string buddyKennel_critterTemplateIdentifier;
        public v3i buddyKennel_critterPos_localOffset;
        public uint scanningEntity_scanRange;
        public uint scanningEntity_secondsPerChunk;
        public uint scanningEntity_scanRangeAddPerModule;
        public uint scanningEntity_additionalScanSpeedPercentagePerModule;
        public string scanningEntity_speedModuleIdentifier;
        public string scanningEntity_distanceModuleIdentifier;
        public uint scanningEntity_additionalPowerConsumptionPercentagePerOptionalModule;
    }

    public struct ResearchDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public string icon_identifier;
        public bool includeInBuild;
        public bool includeInDemo;
        public string entitlementIdentifier;
        public ResearchTemplate.ResearchTemplateFlags flags;
        public bool isInternal;
        public string internalResearchUnlockDescription;
        public string manualEndlessResearchTemplate_str;
        public string description;
        public string researchContext;
        public uint secondsPerScienceItem;
        public Dictionary<string, ResearchTemplateItemInputProxy> input_data;
        public List<string> list_researchDependencies_str;
        public List<string> list_craftingUnlocks_str;
        public List<string> list_blastFurnaceModes_str;
        public List<string> list_alots_str;
        public List<string> list_oreScannerUnlocks_str;
        public string mapScanner_ore_identifier;
        public string mapScanner_reservoir_identifier;
        public int inventorySize_additionalInventorySlots;
        public int endlessResearch_amountOfManualEndlessResearches;
        public string characterCraftingSpeed_additionalDecrementPercentage_str;
        public string miningDrillSpeed_miningTimeMultiplier_str;
        public int miningHardness_unlockedLevel;
        public string jetpackSpeed_speedIncreasmentPercent_str;
    }

    public struct BiomeDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public Color32 gridColor;
        public bool isHeightBased;
        public int lowestPossibleHeight;
        public string surfaceBlock_identifier;
        public string groundBlock_identifier;
        public EnvironmentEffectSpawnInfo[] effects;
        public uint effectChancePerBlockPercent;
        public uint temperature_min;
        public uint precipitation_min;
        public EnvironmentEvent environmentEvent;
        public BiomeLayerTemplate biomeLayer;
    }

    public struct BlastFurnaceModeDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public string icon_identifier;
        public Dictionary<string, CraftingRecipeItemInputProxy> input_data;
        public Dictionary<string, CraftingRecipeElementalInputProxy> output_data_elemental;
        public CraftingRecipe.CraftingRecipeElementalInput waste_gas_data;
        public string slagTemplateIdentifierForShutdown;
    }

    public struct AssemblyLineObjectDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public bool includeInBuild;
        public string icon_identifier;
        public string description;
        public AssemblyLineObjectTemplate.AssemblyLineObjectPart[] objectParts;
        public AssemblyLineObjectTemplate.AssemblyStage[] stages;
        public Vector3 boxColliderSize;
        public Vector3 boxColliderOffset;
    }

    public struct ReservoirDump
    {
        public string modIdentifier;
        public string identifier;
        public string name;
        public string terrainBlockIdentifier;
        public string elementIdentifier;
        public Vector3IntProxy minSize;
        public Vector3IntProxy maxSize;
        public uint maxHeight;
        public uint minHeight;
        public string minContent_str;
        public string maxContent_str;
        public string contentIncreasePerTile_str;
        public Color mapColor;
        public Sprite icon;
        public TerrainBlockType.OreSpawnFlags oreSpawnFlags;
        public uint chancePerChunk_ground;
        public uint chancePerChunk_surface;
        public int ore_startChunkX;
        public int ore_startChunkZ;
    }

    public struct ModularBuildingConnectionNode
    {
        public ModularBuildingConnectionNodeData[] nodeData;
        public Vector3 attachmentPointPreviewPosition;
    }

    public struct ModularBuildingConnectionNodeData
    {
        public string bot_identifier;
        public BuildableObjectTemplate.OffsetAndOrientationData positionData;
    }

    public struct ResourceConverterModuleSpeedBonus
    {
        public string bot_identifier;
        public string speedBonus;
        public int numberOfIgnoredModules;
    }

    public struct WorldDecorMiningYield
    {
        public string it_identifier;
        public uint amount;
    }

    public struct WorldDecorSpecialDrop
    {
        public string it_identifier;
        public uint amount;
        public string dropChancePercentage_str;
    }

    public struct ModularBuildingModuleLimit
    {
        public string bot_identifier;
        public int minAmount;
        public int maxAmount;
    }

    public struct ModularEntityItemCosts
    {
        public string itemTemplateIdentifier;
        public uint itemCost;
    }

    public struct IOFluidBoxData
    {
        public struct IOFluidBoxConnectionData
        {
            public string groupIdentifier;

            public Vector3Int localOffset_origin;

            public Vector3Int localOffset_target;
        }

        public BuildableObjectTemplate.IOFluidBoxData.IOFBType type;

        public BuildableObjectTemplate.IOFluidBoxData.IOFBTransferSpeedType transferSpeedType;

        public bool isInput;

        public float capacity_l;

        public float transferRatePerSecond_l;

        public string fixedElementTemplateIdentifier;

        public IOFluidBoxConnectionData[] connectors;
    }

    public struct SuperBuildingLevel
    {
        public SuperBuildingRequirement[] requirements;
        public int constructionTimeSec;
    }

    public struct SuperBuildingRequirement
    {
        public string identifier;
        public uint amount;
    }
}
