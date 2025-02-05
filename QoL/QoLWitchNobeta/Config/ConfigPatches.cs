﻿using System;
using HarmonyLib;

namespace QoLWitchNobeta.Config;

public static class ConfigPatches
{
    [HarmonyPatch(typeof(Game), nameof(Game.WriteGameSave), [])]
    [HarmonyPostfix]
    private static void WriteGameSavePostfix()
    {
        Plugin.Log.LogDebug("Triggered Config save on Game save");

        Plugin.SaveConfigs();
    }
}