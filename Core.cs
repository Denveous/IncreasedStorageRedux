using HarmonyLib;
using Il2CppInterop.Runtime;
using MelonLoader;
using System;
using System.Reflection;
using UnityEngine; 

[assembly: MelonInfo(typeof(IncreasedStorageRedux.Core), "IncreasedStorageRedux", "1.0.1", "Denveous", null)] 
[assembly: MelonGame("TVGS", "Schedule I")]

namespace IncreasedStorageRedux {
    public class Core : MelonMod {
        private static ModConfig config = new ModConfig();
        private HarmonyLib.Harmony harmony;
        private const int GAME_MAX_SLOTS = 20; private const int SMALL_RACK_SLOTS = 6; private const int MEDIUM_RACK_SLOTS = 8; private const int LARGE_RACK_SLOTS = 12; // TODO: Add settings for all of these.
        private static Type storageEntityType; 
        private static bool isInitialized;
        public override void OnInitializeMelon() { this.SetupConfig(); this.FindAndPatchStorageEntity(); }
        private void SetupConfig() { MelonPreferences_Category category = MelonPreferences.CreateCategory("IncreasedStorageRedux", "Settings"); category.CreateEntry<int>("SafeSlots", config.SafeSlots, "Safe Slots", "Valid number must be between 1 to 20", false, false, null, null); category.CreateEntry<int>("SafeRows", config.SafeRows, "Safe rows", "Valid number must be between 1 to 20", false, false, null, null); this.LoadConfig(); }
        private void FindAndPatchStorageEntity() {
            try {
                storageEntityType = Type.GetType("Il2CppScheduleOne.Storage.StorageEntity, Assembly-CSharp");
                if (storageEntityType == null) {
                    MelonLogger.Error("Could not find StorageEntity type!");
                    return;
                }
                isInitialized = true;
                string[] methodsToTry = { "Awake", "dll", "Start", "Update", "OnEnable", "Initialize", "Init", "Refresh", "RefreshUI", "LoadData" };
                foreach (string methodName in methodsToTry) {
                    try {
                        MethodInfo method = storageEntityType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        if (method != null) {
                            MelonLogger.Msg($"Found additional method to patch: {methodName}");
                            MethodInfo prefixMethod = typeof(Core).GetMethod(nameof(StorageEntityInitialize_Prefix), BindingFlags.Static | BindingFlags.NonPublic);
                            if (harmony == null) harmony = new HarmonyLib.Harmony("com.scheduleone.IncreasedStorageRedux");
                            harmony.Patch(method, new HarmonyMethod(prefixMethod));
                        }
                    } catch (Exception ex) {
                        MelonLogger.Error($"Error patching {methodName}: {ex.Message}");
                    }
                }
            } catch (Exception ex) {
                MelonLogger.Error($"Error in FindAndPatchStorageEntity: {ex.Message}");
            }
        } 
        private void LoadConfig() { config.SafeSlots = Mathf.Min(MelonPreferences.GetEntryValue<int>("IncreasedStorageRedux", "SafeSlots"), GAME_MAX_SLOTS); config.SafeRows = Mathf.Min(MelonPreferences.GetEntryValue<int>("IncreasedStorageRedux", "SafeRows"), GAME_MAX_SLOTS); }
        public override void OnPreferencesSaved() {
          bool changeFlag = false;
          int newSlots = MelonPreferences.GetEntryValue<int>("IncreasedStorageRedux", "SafeSlots");
          int newRows = MelonPreferences.GetEntryValue<int>("IncreasedStorageRedux", "SafeRows");
          if (newSlots != config.SafeSlots) { if (newSlots < 1 || newSlots > GAME_MAX_SLOTS) return; config.SafeSlots = newSlots; changeFlag = true; }
          if (newRows != config.SafeRows) { if (newRows < 1 || newRows > GAME_MAX_SLOTS) return; config.SafeRows = newRows; changeFlag = true; }
          if (changeFlag) { MelonPreferences.Save(); this.FindAndPatchStorageEntity();  }
        }
        private static bool StorageEntityInitialize_Prefix(object __instance) {
            try {
                if (!isInitialized) return true; MonoBehaviour monoBehaviour = __instance as MonoBehaviour; if (monoBehaviour == null) return true;
                string lowerName = monoBehaviour.gameObject.name.ToLower(); int slotCount = 0; int displayRowCount = 0;
                //MelonLogger.Error("Detected: " + lowerName);
                if (lowerName.Contains("storagerack_small")) { slotCount = SMALL_RACK_SLOTS; displayRowCount = 2; }
                else if (lowerName.Contains("storagerack_medium")) { slotCount = MEDIUM_RACK_SLOTS; displayRowCount = 2; }
                else if (lowerName.Contains("storagerack_large")) { slotCount = LARGE_RACK_SLOTS; displayRowCount = 3; }
                else if (lowerName.Contains("safe_built")) { slotCount = config.SafeSlots; displayRowCount = config.SafeRows; }else return true;
                slotCount = Mathf.Min(slotCount, GAME_MAX_SLOTS);
                SetPropertyValue(__instance, "SlotCount", slotCount); SetPropertyValue(__instance, "DisplayRowCount", displayRowCount);
                return true;
            }
            catch (Exception ex) { MelonLogger.Error($"Error in StorageEntityInitialize_Prefix: {ex.Message}"); return true; }
        }
        private static void SetPropertyValue(object instance, string propertyName, object value) {
            try {
                PropertyInfo property = storageEntityType.GetProperty(propertyName); FieldInfo field = storageEntityType.GetField(propertyName);
                if (property != null) property.SetValue(instance, value); else if (field != null) field.SetValue(instance, value);
                else MelonLogger.Error($"Property or field '{propertyName}' not found in StorageEntity.");
            }
            catch (Exception ex) { MelonLogger.Error($"Error setting property '{propertyName}': {ex.Message}"); }
        }
    }
}
