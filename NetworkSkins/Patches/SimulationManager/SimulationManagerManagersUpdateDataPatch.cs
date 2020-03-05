using HarmonyLib;
using NetworkSkins.Skins;

namespace NetworkSkins.Patches.SimulationManager
{
    [HarmonyPatch(typeof(global::SimulationManager), "Managers_UpdateData")]
    public static class SimulationManagerManagersUpdateDataPatch
    {
        public static void Prefix(global::SimulationManager.UpdateMode mode)
        {
            UnityEngine.Debug.Log(System.Environment.StackTrace);
            NetworkSkinManager.instance.OnPreUpdateData(mode);
        }
    }
}
