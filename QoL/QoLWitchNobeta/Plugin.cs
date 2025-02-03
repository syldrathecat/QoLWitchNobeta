using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using QoLWitchNobeta.Behaviours;
using QoLWitchNobeta.Config;
using QoLWitchNobeta.Utils;
using UnityEngine;

namespace QoLWitchNobeta;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("LittleWitchNobeta")]
public class Plugin : BasePlugin
{
    internal new static ManualLogSource Log;

    public static DirectoryInfo ConfigDirectory;
    public static DirectoryInfo PluginInstallationDirectory;
    public static ConfigFile ConfigFile;

    private static Harmony _harmony;

    private static AutoConfigManager AutoConfigManager;

    public override void Load()
    {
        Log = base.Log;
        Log.LogMessage($"Plugin {MyPluginInfo.PLUGIN_GUID} is loading...");

        Application.quitting += (Action) (() =>
        {
            Unload();
        });

        // Plugin startup logic
        ConfigDirectory = new DirectoryInfo(Path.Combine(Path.GetDirectoryName(Config.ConfigFilePath)!, "QoLWitchNobeta"));
        ConfigDirectory.Create();

        PluginInstallationDirectory = new DirectoryInfo(Path.Combine(ConfigDirectory.FullName, "../../plugins/QoLWitchNobeta"));

        ConfigFile = new ConfigFile(Path.Combine(ConfigDirectory.FullName, "QoLWitchNobeta.cfg"), true, GetType().GetCustomAttribute<BepInPlugin>());

        AutoConfigManager = new AutoConfigManager(ConfigFile);
        AutoConfigManager.LoadValuesToFields();

        // Fetch Nobeta process early to get game window handle
        NobetaProcessUtils.NobetaProcess = Process.GetProcessesByName("LittleWitchNobeta")[0];
        NobetaProcessUtils.GameWindowHandle = NobetaProcessUtils.FindWindow(null, "Little Witch Nobeta");

        // Apply patches
        ApplyPatches();

        // Add required Components
        AddComponent<UnityMainThreadDispatcher>();

        Log.LogMessage($"Plugin {MyPluginInfo.PLUGIN_GUID} successfully loaded!");
    }

    public override bool Unload()
    {
        Log.LogMessage($"Plugin {MyPluginInfo.PLUGIN_GUID} unloading...");

        SaveConfigs();

        Log.LogMessage($"Plugin {MyPluginInfo.PLUGIN_GUID} successfully unloaded");

        return false;
    }

    public static void SaveConfigs()
    {
        Log.LogInfo("Saving configs...");

        // Save BepInEx config
        AutoConfigManager.FetchValuesFromFields();
        ConfigFile.Save();

        Log.LogInfo("Configs saved");
    }

    public static void ApplyPatches()
    {
        _harmony = new Harmony(nameof(QoLWitchNobeta));

        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

            if (methods.Any(method => method.GetCustomAttribute<HarmonyPatch>() is not null))
            {
                _harmony.PatchAll(type);
            }
        }
    }
}