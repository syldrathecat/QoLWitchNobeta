﻿using System.Collections.Generic;
using HarmonyLib;
using RandomizedWitchNobeta.Utils;
using RandomizedWitchNobeta.Utils.Extensions;
using UnityEngine;

namespace RandomizedWitchNobeta.Runtime.Gameplay;

public static class TriforceQuestPatches
{
    private static readonly List<MultipleEventOpen> _openers = new();

    // Disable auto-open of trials
    [HarmonyPatch(typeof(MultipleEventOpen), nameof(MultipleEventOpen.InitData))]
    [HarmonyPostfix]
    private static void MultipleEventOpenInitPostfix(MultipleEventOpen __instance)
    {
        // Only check in last stage
        if (Game.sceneManager.stageId != 7)
        {
            return;
        }

        if (__instance.name is "OpenLightRoomStart01" or "OpenLightRoomStart02" or "OpenLightRoomStart03")
        {
            // Reopen it if it has already been opened
            if (Singletons.RuntimeVariables.OpenedTrials.Contains(__instance.name))
            {
                __instance.OpenEvent();

                return;
            }

            __instance.CheckPlayerEnter = false;

            // Make collider smaller so they don't overlap
            var extents = __instance.g_BC.extents - new Vector3(3f, 0f, 3f);
            __instance.g_BC.extents = extents;

            _openers.Add(__instance);
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterLoaderScene))]
    [HarmonyPrefix]
    private static void EnterLoaderScenePostfix()
    {
        _openers.Clear();
    }

    // Open trial on token drop
    [HarmonyPatch(typeof(PlayerItem), nameof(PlayerItem.DiscardItemSuccess))]
    [HarmonyPostfix]
    private static void DiscardItemPostfix(IItemController __instance)
    {
        if (Game.sceneManager.stageId != 7)
        {
            return;
        }

        var items = UnityUtils.FindComponentsByTypeForced<Item>();

        // Check if any token is in a trial open bound
        foreach (var item in items)
        {
            if (item.currentItemType == ItemSystem.ItemType.SPMaxAdd)
            {
                foreach (var eventOpen in _openers)
                {
                    if (!eventOpen.g_AllOpen && eventOpen.g_BC.Contains(item.transform.position))
                    {
                        eventOpen.OpenEvent();
                        Singletons.RuntimeVariables.OpenedTrials.Add(eventOpen.name);

                        Object.Destroy(item.gameObject);

                        return;
                    }
                }
            }
        }
    }

    // Disable usage of tokens (can only drop)
    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnUseItemHotKeyDown))]
    [HarmonyPrefix]
    private static bool UseItemPrefix(PlayerController __instance, int index)
    {
        return CheckUseItem(__instance.g_Item.GetSelectItemType(index));
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.OnUseItemKeyDown))]
    [HarmonyPrefix]
    private static bool UseItemPrefix(PlayerController __instance)
    {
        return CheckUseItem(__instance.g_Item.GetSelectItemType(Game.GetItemSelectPos()));
    }

    private static bool CheckUseItem(ItemSystem.ItemType itemType)
    {
        if (itemType == ItemSystem.ItemType.SPMaxAdd)
        {
            Game.AppearEventPrompt("Tokens can only be dropped, not used.");

            return false;
        }

        return true;
    }
}