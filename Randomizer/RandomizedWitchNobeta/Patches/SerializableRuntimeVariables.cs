﻿using System;
using System.Collections.Generic;
using RandomizedWitchNobeta.Generation;
using RandomizedWitchNobeta.Generation.Models;
using RandomizedWitchNobeta.Shared;

namespace RandomizedWitchNobeta.Patches;

public class SerializableRuntimeVariables
{
    public SeedSettings Settings { get; set; }

    public TimeSpan ElapsedRealTime { get; set; }
    public TimeSpan ElapsedLoadRemoved { get; set; }

    public int GlobalMagicLevel { get; set; }

    public bool CatLootObtained { get; set; } = false;

    public HashSet<string> KilledBosses { get; set; }
    public HashSet<string> OpenedTrials { get; set; }

    // Generated by the randomizer
    public int StartScene { get; set; }
    public List<KeyValuePair<RegionExit, (int sceneNumberOverride, int savePointOverride)>> ExitsOverrides { get; set; }
    public List<KeyValuePair<ChestOverride, ItemSystem.ItemType>> ChestOverrides { get; set; }
    public ItemSystem.ItemType CatOverride { get; set; }
}