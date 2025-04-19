using HarmonyLib;
using MelonLoader;
using System.Reflection;
using UnityEngine;

[assembly: MelonInfo(typeof(StoragePlus.Core), "StoragePlus", "1.0.3", "Denveous", null)] 
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: MelonColor(255, 255, 0, 255)]
[assembly: MelonAuthorColor(255, 0, 255, 0)]

namespace StoragePlus {
    internal class ModConfig {
        public int SafeRows = 4;
        public int SafeSlots = 16;
        public int SmallRackRows = 2; 
        public int SmallRackSlots = 6;
        public int MediumRackRows = 2;
        public int MediumRackSlots = 8;
        public int LargeRackRows = 3; 
        public int LargeRackSlots = 12;
        public int WallShelfRows = 2;
        public int WallShelfSlots = 6;
        public int VanRows = 4;
        public int VanSlots = 20;
    }
    
    public class Core : MelonMod {
        private static ModConfig config = new ModConfig();
        private HarmonyLib.Harmony harmony;
        private const int GAME_MAX_SLOTS = 20;
        private static Type storageEntityType;
        
        public override void OnInitializeMelon() { SetupConfig(); SetupPatches(); }
        public override void OnSceneWasInitialized(int buildIndex, string sceneName) { if (sceneName == "Main") SetupPatches(); }

        private void SetupConfig() {
            MelonPreferences_Category category = MelonPreferences.CreateCategory("StoragePlus", "Settings");
            category.CreateEntry<int>("SafeSlots", config.SafeSlots, "Safe Slots", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("SafeRows", config.SafeRows, "Safe Rows", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("SmallRackSlots", config.SmallRackSlots, "Small Rack Slots", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("SmallRackRows", config.SmallRackRows, "Small Rack Rows", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("MediumRackSlots", config.MediumRackSlots, "Medium Rack Slots", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("MediumRackRows", config.MediumRackRows, "Medium Rack Rows", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("LargeRackSlots", config.LargeRackSlots, "Large Rack Slots", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("LargeRackRows", config.LargeRackRows, "Large Rack Rows", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("WallShelfSlots", config.WallShelfSlots, "Wall Shelf Slots", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("WallShelfRows", config.WallShelfRows, "Wall Shelf Rows", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("VanSlots", config.VanSlots, "Van Slots", "Valid number must be between 1 to 20", false, false, null, null);
            category.CreateEntry<int>("VanRows", config.VanRows, "Van Rows", "Valid number must be between 1 to 20", false, false, null, null);
            LoadConfig();
        }
        
        private void SetupPatches() {
            try {
                storageEntityType = Type.GetType("Il2CppScheduleOne.Storage.StorageEntity, Assembly-CSharp");
                if (storageEntityType == null) { MelonLogger.Error("Could not find StorageEntity type!"); return; }
                
                if (harmony == null) harmony = new HarmonyLib.Harmony("com.scheduleone.StoragePlus");
                
                string[] methodsToTry = { "Awake", "dll", "Start", "Update", "OnEnable", "Initialize", "Init", "Refresh", "RefreshUI", "LoadData" };
                foreach (string methodName in methodsToTry) {
                    try {
                        MethodInfo method = storageEntityType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (method != null) {
                            MelonLogger.Msg($"Found method to patch: {methodName}");
                            
                            harmony.Patch(
                                method,
                                new HarmonyMethod(
                                    typeof(Core).GetMethod(nameof(PatchStorageEntity), 
                                    BindingFlags.Static | BindingFlags.NonPublic)
                                )
                            );
                        }
                    } catch (Exception ex) { MelonLogger.Error($"Error patching {methodName}: {ex.Message}"); }
                }
            } catch (Exception ex) { MelonLogger.Error($"Error in SetupPatches: {ex.Message}"); }
        }
        
        private void LoadConfig() {
            config.SafeSlots = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "SafeSlots"), GAME_MAX_SLOTS);
            config.SafeRows = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "SafeRows"), GAME_MAX_SLOTS);
            config.SmallRackSlots = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "SmallRackSlots"), GAME_MAX_SLOTS);
            config.SmallRackRows = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "SmallRackRows"), GAME_MAX_SLOTS);
            config.MediumRackSlots = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "MediumRackSlots"), GAME_MAX_SLOTS);
            config.MediumRackRows = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "MediumRackRows"), GAME_MAX_SLOTS);
            config.LargeRackSlots = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "LargeRackSlots"), GAME_MAX_SLOTS);
            config.LargeRackRows = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "LargeRackRows"), GAME_MAX_SLOTS);
            config.WallShelfSlots = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "WallShelfSlots"), GAME_MAX_SLOTS);
            config.WallShelfRows = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "WallShelfRows"), GAME_MAX_SLOTS);
            config.VanSlots = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "VanSlots"), GAME_MAX_SLOTS);
            config.VanRows = Mathf.Min(MelonPreferences.GetEntryValue<int>("StoragePlus", "VanRows"), GAME_MAX_SLOTS);
        }
        
        public override void OnPreferencesSaved() {
            int newSafeSlots = MelonPreferences.GetEntryValue<int>("StoragePlus", "SafeSlots");
            int newSafeRows = MelonPreferences.GetEntryValue<int>("StoragePlus", "SafeRows");
            int newSmallRackSlots = MelonPreferences.GetEntryValue<int>("StoragePlus", "SmallRackSlots");
            int newSmallRackRows = MelonPreferences.GetEntryValue<int>("StoragePlus", "SmallRackRows");
            int newMediumRackSlots = MelonPreferences.GetEntryValue<int>("StoragePlus", "MediumRackSlots");
            int newMediumRackRows = MelonPreferences.GetEntryValue<int>("StoragePlus", "MediumRackRows");
            int newLargeRackSlots = MelonPreferences.GetEntryValue<int>("StoragePlus", "LargeRackSlots");
            int newLargeRackRows = MelonPreferences.GetEntryValue<int>("StoragePlus", "LargeRackRows");
            int newWallShelfSlots = MelonPreferences.GetEntryValue<int>("StoragePlus", "WallShelfSlots");
            int newWallShelfRows = MelonPreferences.GetEntryValue<int>("StoragePlus", "WallShelfRows");
            int newVanSlots = MelonPreferences.GetEntryValue<int>("StoragePlus", "VanSlots");
            int newVanRows = MelonPreferences.GetEntryValue<int>("StoragePlus", "VanRows");
            
            if (newSafeSlots != config.SafeSlots) { if (newSafeSlots < 1 || newSafeSlots > GAME_MAX_SLOTS) return; config.SafeSlots = newSafeSlots; }
            if (newSafeRows != config.SafeRows) { if (newSafeRows < 1 || newSafeRows > GAME_MAX_SLOTS) return; config.SafeRows = newSafeRows; }
            if (newSmallRackSlots != config.SmallRackSlots) { if (newSmallRackSlots < 1 || newSmallRackSlots > GAME_MAX_SLOTS) return; config.SmallRackSlots = newSmallRackSlots; }
            if (newSmallRackRows != config.SmallRackRows) { if (newSmallRackRows < 1 || newSmallRackRows > GAME_MAX_SLOTS) return; config.SmallRackRows = newSmallRackRows; }
            if (newMediumRackSlots != config.MediumRackSlots) { if (newMediumRackSlots < 1 || newMediumRackSlots > GAME_MAX_SLOTS) return; config.MediumRackSlots = newMediumRackSlots; }
            if (newMediumRackRows != config.MediumRackRows) { if (newMediumRackRows < 1 || newMediumRackRows > GAME_MAX_SLOTS) return; config.MediumRackRows = newMediumRackRows; }
            if (newLargeRackSlots != config.LargeRackSlots) { if (newLargeRackSlots < 1 || newLargeRackSlots > GAME_MAX_SLOTS) return; config.LargeRackSlots = newLargeRackSlots; }
            if (newLargeRackRows != config.LargeRackRows) { if (newLargeRackRows < 1 || newLargeRackRows > GAME_MAX_SLOTS) return; config.LargeRackRows = newLargeRackRows; }
            if (newWallShelfSlots != config.WallShelfSlots) { if (newWallShelfSlots < 1 || newWallShelfSlots > GAME_MAX_SLOTS) return; config.WallShelfSlots = newWallShelfSlots; }
            if (newWallShelfRows != config.WallShelfRows) { if (newWallShelfRows < 1 || newWallShelfRows > GAME_MAX_SLOTS) return; config.WallShelfRows = newWallShelfRows; }
            if (newVanSlots != config.VanSlots) { if (newVanSlots < 1 || newVanSlots > GAME_MAX_SLOTS) return; config.VanSlots = newVanSlots; }
            if (newVanRows != config.VanRows) { if (newVanRows < 1 || newVanRows > GAME_MAX_SLOTS) return; config.VanRows = newVanRows; }
        }
        
        private static bool PatchStorageEntity(object __instance) {
            try {
                if (storageEntityType == null) return true;
                
                MonoBehaviour monoBehaviour = __instance as MonoBehaviour;
                if (monoBehaviour == null) return true;
                
                string lowerName = monoBehaviour.gameObject.name.ToLower();
                int slotCount = 0;
                int displayRowCount = 0;
                
                if (lowerName.Contains("storagerack_small")) { slotCount = config.SmallRackSlots; displayRowCount = config.SmallRackRows; }
                else if (lowerName.Contains("storagerack_medium")) { slotCount = config.MediumRackSlots; displayRowCount = config.MediumRackRows; }
                else if (lowerName.Contains("storagerack_large")) { slotCount = config.LargeRackSlots; displayRowCount = config.LargeRackRows; }
                else if (lowerName.Contains("safe_built")) { slotCount = config.SafeSlots; displayRowCount = config.SafeRows; }
                else if (lowerName.Contains("wallmountedshelf_built")) { slotCount = config.WallShelfSlots; displayRowCount = config.WallShelfRows; }
                else if (lowerName.Contains("van")) { slotCount = config.VanSlots; displayRowCount = config.VanRows; }
                else return true;
                
                slotCount = Mathf.Min(slotCount, GAME_MAX_SLOTS);
                
                try {
                    PropertyInfo property = storageEntityType.GetProperty("SlotCount");
                    FieldInfo field = storageEntityType.GetField("SlotCount");
                    if (property != null) property.SetValue(__instance, slotCount);
                    else if (field != null) field.SetValue(__instance, slotCount);
                    else MelonLogger.Error("Property or field 'SlotCount' not found in StorageEntity.");
                } catch (Exception ex) { MelonLogger.Error($"Error setting SlotCount: {ex.Message}"); }
                
                try {
                    PropertyInfo property = storageEntityType.GetProperty("DisplayRowCount");
                    FieldInfo field = storageEntityType.GetField("DisplayRowCount");
                    if (property != null) property.SetValue(__instance, displayRowCount);
                    else if (field != null) field.SetValue(__instance, displayRowCount);
                    else MelonLogger.Error("Property or field 'DisplayRowCount' not found in StorageEntity.");
                } catch (Exception ex) { MelonLogger.Error($"Error setting DisplayRowCount: {ex.Message}"); }
                
                return true;
            }
            catch (Exception ex) { MelonLogger.Error($"Error in PatchStorageEntity: {ex.Message}"); return true; }
        }
    }
}