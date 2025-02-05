﻿using System.Linq;
using HarmonyLib;
using QoLWitchNobeta.Utils;
using UnityEngine;

namespace QoLWitchNobeta.Features.General;

public static class StatueUnlockPatches
{
    public const float UnlockDistance = 8f;

    private static SavePoint[] _statues;

    [HarmonyPatch(typeof(SceneManager), nameof(SceneManager.OnSceneInitComplete))]
    [HarmonyPostfix]
    private static void OnSceneInitCompletePostfix()
    {
        _statues = UnityUtils.FindComponentsByTypeForced<SavePoint>().Where(savePoint => savePoint.EventType == PassiveEvent.PassiveEventType.SavePoint).ToArray();
    }

    [HarmonyPatch(typeof(Game), nameof(Game.EnterLoaderScene))]
    [HarmonyPrefix]
    private static void EnterLoaderScenePostfix()
    {
        _statues = null;
    }

    [HarmonyPatch(typeof(WizardGirlManage), nameof(WizardGirlManage.Update))]
    [HarmonyPostfix]
    private static void OnWizardGirlUpdate(WizardGirlManage __instance)
    {
        if (_statues is null)
            return;

        if (Singletons.GameSave.basic.difficulty == GameDifficulty.BossRush)
            return;

        var gameSave = Game.GameSave.basic;
        var stageName = Game.sceneManager.stageName;
        var gameStage = gameSave.GetStage(stageName);

        foreach (var statue in _statues)
        {
            var savePointNumber = Game.sceneManager.GetSavePointNumber(statue);

            if (gameSave.HasSavePointUnlocked(gameStage, savePointNumber))
                continue;

            var distance = Vector3.Distance(statue.transform.position, __instance.transform.position);

            if (distance < UnlockDistance)
            {
                Plugin.Log.LogDebug($"Statue '{statue.name}#{statue.TransferLevelNumber}#{savePointNumber}' auto-unlocked");

                gameSave.AddNewSavePoint(stageName, savePointNumber);
                gameSave.stage = gameStage;
                gameSave.savePoint = savePointNumber;

                Game.AppearEventPrompt($"Nearby statue unlocked: {Game.GetLocationText(gameStage, savePointNumber)}");
            }
        }
    }
}