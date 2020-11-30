using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameData
{
    static GameData()
    {
        orbCount = 0;
        musicMuted = false;
        musicVolume = 0.5f;
        achieveStatus = 0;
        respawned = false;
    }

    public static int orbCount
    {
        get;
        set;
    }

    public static bool musicMuted
    {
        get;
        set;
    }

    public static float musicVolume
    {
        get;
        set;
    }

    public static int achieveStatus
    {
        get;
        set;
    }

    public static bool respawned
    {
        get;
        set;
    }
}
