using System;
using System.Collections.Generic;
using UnityEngine;

public class MSInventoryMod : FortressCraftMod
{

    public override ModRegistrationData Register()
    {
        ModRegistrationData modRegistrationData = new ModRegistrationData();
        modRegistrationData.RegisterEntityHandler("steveman0.MSInventoryPanel");


        Debug.Log("Mass Storage Inventory Panel Mod v1.2 registered");

        return modRegistrationData;
    }

    public override ModCreateSegmentEntityResults CreateSegmentEntity(ModCreateSegmentEntityParameters parameters)
    {
        ModCreateSegmentEntityResults result = new ModCreateSegmentEntityResults();

        foreach (ModCubeMap cubeMap in ModManager.mModMappings.CubeTypes)
        {
            if (cubeMap.CubeType == parameters.Cube)
            {
                if (cubeMap.Key.Equals("steveman0.MSInventoryPanel"))
                    result.Entity = new MSInventoryPanel(parameters.Segment, parameters.X, parameters.Y, parameters.Z, parameters.Cube, parameters.Flags, parameters.Value, parameters.LoadFromDisk);
            }
        }
        return result;
    }
}