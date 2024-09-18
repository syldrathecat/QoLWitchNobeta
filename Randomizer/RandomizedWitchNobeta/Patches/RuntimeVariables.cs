﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RandomizedWitchNobeta.Config.Serialization;
using RandomizedWitchNobeta.Generation;
using RandomizedWitchNobeta.Generation.Models;
using RandomizedWitchNobeta.Shared;
using RandomizedWitchNobeta.Utils;

namespace RandomizedWitchNobeta.Patches;

public class RuntimeVariables
{
    public static string SavePath { get; } = Path.Combine(Plugin.ConfigDirectory.FullName, "RuntimeVariables.json");

    public SeedSettings Settings { get; }

    // Timers
    public TimeSpan ElapsedRealTime { get; set; } = TimeSpan.Zero;
    public TimeSpan ElapsedLoadRemoved { get; set; } = TimeSpan.Zero;

    // Data used to resume a randomized game
    public int GlobalMagicLevel { get; set; } = 1;

    public bool CatLootObtained { get; set; } = false;

    public HashSet<string> KilledBosses { get; } = new();
    public HashSet<string> OpenedTrials { get; } = new();

    // Generated by the randomizer
    public int StartScene { get; }
    public Dictionary<RegionExit, (int sceneNumberOverride, int savePointOverride)> ExitsOverrides { get; } = new();
    public Dictionary<ChestOverride, ItemSystem.ItemType> ChestOverrides { get; } = new();
    public ItemSystem.ItemType CatOverride { get; }

    public RuntimeVariables(SeedSettings settings, int startScene, Dictionary<RegionExit, int> exitsOverrides, List<ItemLocation> itemLocations)
    {
        Settings = settings;
        StartScene = startScene;

        // Generate exit overrides
        foreach (var (regionExit, destinationScene) in exitsOverrides)
        {
            ExitsOverrides[regionExit] =
                (destinationScene, SceneUtils.SceneStartSavePoint(destinationScene));
        }

        // Generate chest content overrides
        foreach (var chestItemLocation in itemLocations.OfType<ChestItemLocation>())
        {
            ChestOverrides[new ChestOverride(chestItemLocation.ChestName, chestItemLocation.SceneNumber)] = chestItemLocation.ItemType;
        }

        // Get cat item override
        CatOverride = itemLocations.OfType<CatItemLocation>().Single().ItemType;
    }

    private RuntimeVariables(SerializableRuntimeVariables serializable)
    {
        Settings = serializable.Settings;

        ElapsedRealTime = serializable.ElapsedRealTime;
        ElapsedLoadRemoved = serializable.ElapsedLoadRemoved;

        GlobalMagicLevel = serializable.GlobalMagicLevel;

        CatLootObtained = serializable.CatLootObtained;

        KilledBosses = serializable.KilledBosses;
        OpenedTrials = serializable.OpenedTrials;

        StartScene = serializable.StartScene;

        foreach (var (key, value) in serializable.ExitsOverrides)
        {
            ExitsOverrides[key] = value;
        }

        foreach (var (key, value) in serializable.ChestOverrides)
        {
            ChestOverrides[key] = value;
        }

        CatOverride = serializable.CatOverride;
    }

    public void Save()
    {
        File.WriteAllText(SavePath, SerializeUtils.SerializeIndented(ToSerializable()));
    }

    private SerializableRuntimeVariables ToSerializable()
    {
        return new SerializableRuntimeVariables
        {
            Settings = Settings,

            ElapsedRealTime = ElapsedRealTime,
            ElapsedLoadRemoved = ElapsedLoadRemoved,

            GlobalMagicLevel = GlobalMagicLevel,

            CatLootObtained = CatLootObtained,

            KilledBosses = KilledBosses,
            OpenedTrials = OpenedTrials,

            StartScene = StartScene,
            ChestOverrides = ChestOverrides.ToList(),
            ExitsOverrides = ExitsOverrides.ToList(),
            CatOverride = CatOverride
        };
    }

    public static bool TryLoad(out RuntimeVariables runtimeVariables)
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                runtimeVariables = null;
                return false;
            }

            runtimeVariables = new RuntimeVariables(SerializeUtils.Deserialize<SerializableRuntimeVariables>(File.ReadAllText(SavePath)));
        }
        catch (Exception)
        {
            // File is not recoverable, maybe after an update
            runtimeVariables = null;
            return false;
        }

        return true;
    }
}