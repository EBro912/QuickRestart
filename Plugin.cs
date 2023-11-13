using BepInEx;
using HarmonyLib;
using System.Reflection;

namespace QuickRestart;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static bool verifying = false;

    private Harmony harmony;
    private static MethodInfo chat;

    private void Awake()
    {
        harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        chat = AccessTools.Method(typeof(HUDManager), "AddChatMessage");

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} loaded!");
    }

    public static void SendChatMessage(string message)
    {
        chat?.Invoke(HUDManager.Instance, new object[] { message, "" });
        HUDManager.Instance.lastChatMessage = "";
    }

    public static void ConfirmRestart()
    {
        verifying = true;
        SendChatMessage("Are you sure? Type CONFIRM or DENY.");
    }

    public static void AcceptRestart(StartOfRound manager)
    {
        SendChatMessage("Restart confirmed.");
        verifying = false;

        int[] stats = new int[]
        {
                manager.gameStats.daysSpent,
                manager.gameStats.scrapValueCollected,
                manager.gameStats.deaths,
                manager.gameStats.allStepsTaken
        };
        manager.FirePlayersAfterDeadlineClientRpc(stats);
    }

    public static void DeclineRestart()
    {
        SendChatMessage("Restart aborted.");
        verifying = false;
    }
}
