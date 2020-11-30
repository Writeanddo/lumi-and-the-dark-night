using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Achievements
{
    public enum Type
    {
        L1_ORBS,
        L1_DEATH,
        L1_TIME,
        L2_ORBS,
        L2_DEATH,
        L2_TIME,
        L3_ORBS,
        L3_DEATH,
        L3_TIME,
        L4_ORBS,
        L4_DEATH,
        L4_TIME,
        L5_ORBS,
        L5_DEATH,
        L5_TIME,
        L6_ORBS,
        L6_DEATH,
        L6_TIME,
    }

    public int status
    {
        get;
        set;
    }

    public Achievements(int _status)
    {
        status = _status;
    }

    public bool getStatus(Type t)
    {
        int i = (int) t;
        return ((status & (0x1 << i)) != 0);
    }

    public void setStatus(Type t)
    {
        int i = (int) t;
        status |= (0x1 << i);
    }

    public string getName(Type t)
    {
        switch(t)
        {
            case Type.L1_ORBS:
                return "Level 1 Orb Collector";
            case Type.L1_DEATH:
                return "Level 1 Deathless";
            case Type.L1_TIME:
                return "Level 1 Speed Run";
            case Type.L2_ORBS:
                return "Level 2 Orb Collector";
            case Type.L2_DEATH:
                return "Level 2 Deathless";
            case Type.L2_TIME:
                return "Level 2 Speed Run";
            case Type.L3_ORBS:
                return "Level 3 Orb Collector";
            case Type.L3_DEATH:
                return "Level 3 Deathless";
            case Type.L3_TIME:
                return "Level 3 Speed Run";
            case Type.L4_ORBS:
                return "Level 4 Orb Collector";
            case Type.L4_DEATH:
                return "Level 4 Deathless";
            case Type.L4_TIME:
                return "Level 4 Speed Run";
            case Type.L5_ORBS:
                return "Level 5 Orb Collector";
            case Type.L5_DEATH:
                return "Level 5 Deathless";
            case Type.L5_TIME:
                return "Level 5 Speed Run";
            case Type.L6_ORBS:
                return "Level 6 Orb Collector";
            case Type.L6_DEATH:
                return "Level 6 Deathless";
            case Type.L6_TIME:
                return "Level 6 Speed Run";
            default:
                return "";
        }
    }
}
