using HarmonyLib;

namespace ThaumielMapEditor.HarmonyPatches
{
    [HarmonyPatch]
    public static class ServerNamePatch
    {
        [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
        public static void Postfix()
        {
            if (!Main.Instance.Config.EnableServerTracking)
                return;
            
            ServerConsole.ServerName += $"<color=#00000000><size=1>TME {Main.Instance.Version}</size></color>";
        }
    }
}