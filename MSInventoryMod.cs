using System;
using System.Collections.Generic;
using UnityEngine;

public class MSInventoryMod : FortressCraftMod
{
    public static ushort paneltype = ModManager.mModMappings.CubesByKey["steveman0.MSInventoryPanel"].CubeType;

    public override ModRegistrationData Register()
    {
        ModRegistrationData modRegistrationData = new ModRegistrationData();
        modRegistrationData.RegisterEntityHandler("steveman0.MSInventoryPanel");

        Debug.Log("Mass Storage Inventory Panel Mod v4 registered");

        return modRegistrationData;
    }

    public override ModCreateSegmentEntityResults CreateSegmentEntity(ModCreateSegmentEntityParameters parameters)
    {
        ModCreateSegmentEntityResults result = new ModCreateSegmentEntityResults();

        if (parameters.Cube == paneltype)
        {
            parameters.ObjectType = SpawnableObjectEnum.ServerMonitor;
            result.Entity = new MSInventoryPanel(parameters);
        }
        return result;
    }
}