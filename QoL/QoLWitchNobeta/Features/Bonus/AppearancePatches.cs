using System;
using HarmonyLib;
using QoLWitchNobeta.Utils;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

namespace QoLWitchNobeta.Features.Bonus;

public static class AppearancePatches
{
    public static GameSkin SelectedSkin;
    public static readonly string[] AvailableSkins = Enum.GetNames<GameSkin>();

    public static bool HideBagEnabled;
    public static bool HideStaffEnabled;
    public static bool HideHatEnabled;

    public static InputActionMap InputActionMap { get; set; }
    public static InputAction GamepadSelectAction { get; set; }
    public static InputAction InputActionPrevSkin { get; set; }
    public static InputAction InputActionNextSkin { get; set; }
    public static InputAction InputActionToggleHat { get; set; }
    public static InputAction InputActionToggleBag { get; set; }

    private static void UpdateSelectedSkin()
    {
        var gameSkin = SelectedSkin;

        Singletons.Dispatcher.Enqueue(() =>
        {
            Game.Collection.UpdateSkin(gameSkin);

            Plugin.Log.LogDebug($"Skin updated to: {gameSkin}");
        });
    }

    // Skin loader, hide bag, staff and hat
    public static void InitAppearance()
    {
        if (Singletons.NobetaSkin is not { } skin)
        {
            return;
        }

        if (skin.bagMesh is not null)
        {
            skin.bagMesh.enabled = !HideBagEnabled;
        }

        if (skin.weaponMesh is not null)
        {
            skin.weaponMesh.enabled = !HideStaffEnabled;
        }

        if (skin.storyHatMesh is not null)
        {
            skin.storyHatMesh.enabled = !HideHatEnabled;
        }

        // Second pass to remove from other skins
        foreach (var meshRenderer in skin.bodyMesh)
        {
            if (HideBagEnabled)
            {
                if (meshRenderer.name.Contains("bag", StringComparison.OrdinalIgnoreCase))
                {
                    meshRenderer.enabled = false;
                }
            }
            else
            {
                if (meshRenderer.name.Contains("bag", StringComparison.OrdinalIgnoreCase))
                {
                    meshRenderer.enabled = true;
                }
            }

            if (HideHatEnabled)
            {
                if (meshRenderer.name.Contains("hat", StringComparison.OrdinalIgnoreCase))
                {
                    meshRenderer.enabled = false;
                }
            }
            else
            {
                if (meshRenderer.name.Contains("hat", StringComparison.OrdinalIgnoreCase))
                {
                    meshRenderer.enabled = true;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Game), nameof(Game.SwitchScene))]
    [HarmonyPrefix]
    private static void SwitchScenePrefix()
    {
        UpdateSelectedSkin();
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Update))]
    [HarmonyPostfix]
    private static void PlayerControllerUpdatePostfix()
    {
        InitAppearance();
    }

    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.Update))]
    [HarmonyPrefix]
    private static void PlayerControllerUpdatePrefix()
    {
        if (InputActionMap == null)
        {
            Plugin.Log.LogInfo("Initializing custom input actions");
            InputActionMap = new("QoLWitchNobeta");

            InputActionPrevSkin = InputActionMap.AddAction(nameof(InputActionPrevSkin), InputActionType.Button, "<Keyboard>/PageDown");
            InputActionNextSkin = InputActionMap.AddAction(nameof(InputActionNextSkin), InputActionType.Button, "<Keyboard>/PageUp");
            InputActionToggleHat = InputActionMap.AddAction(nameof(InputActionToggleHat), InputActionType.Button, "<Keyboard>/Home");
            InputActionToggleBag = InputActionMap.AddAction(nameof(InputActionToggleBag), InputActionType.Button, "<Keyboard>/End");
            InputActionMap.Enable();
        }

        if (Singletons.PlayerController.state == NobetaState.Normal)
        {
            if (InputActionPrevSkin.triggered || InputActionNextSkin.triggered)
            {
                Plugin.Log.LogInfo("Action is triggered");
                int dir = 1;
                if (InputActionPrevSkin.triggered)
                    dir = -1;
                int newSkin = (int)SelectedSkin + dir;
                if (newSkin < 0)
                    newSkin = AvailableSkins.Length - 1;
                if (newSkin >= AvailableSkins.Length)
                    newSkin = 0;
                SelectedSkin = (GameSkin)newSkin;

                if (Singletons.WizardGirl != null)
                {
                    Singletons.Dispatcher.Enqueue(() =>
                    {
                        Singletons.WizardGirl.PreloadSkin(SelectedSkin);
                        var assetKey = Singletons.WizardGirl.GetSkinAssetKey(SelectedSkin);

                        // Need to keep the object in a variable to avoid getting GC'd before the call to ReplaceActiveSkin
                        var _ = Addressables.LoadAsset<GameObject>(assetKey).WaitForCompletion();

                        Singletons.WizardGirl.ReplaceActiveSkin(SelectedSkin);

                        // Also update skin in GameCollection for reload
                        Game.Collection.UpdateSkin(SelectedSkin);

                        Plugin.Log.LogDebug($"Skin updated to: {SelectedSkin}");

                        Game.AppearEventPrompt($"Skin: {SelectedSkin}");
                    });
                }
            }

            if (InputActionToggleHat.triggered)
            {
                if (Singletons.WizardGirl != null)
                {
                    Singletons.Dispatcher.Enqueue(() =>
                    {
                        HideHatEnabled = !HideHatEnabled;
                        //Game.AppearEventPrompt($"Hide Hat: {HideHatEnabled}");
                    });
                }
            }

            if (InputActionToggleBag.triggered)
            {
                if (Singletons.WizardGirl != null)
                {
                    Singletons.Dispatcher.Enqueue(() =>
                    {
                        HideBagEnabled = !HideBagEnabled;
                        //Game.AppearEventPrompt($"Hide Bag: {HideBagEnabled}");
                    });
                }
            }
        }
    }

}