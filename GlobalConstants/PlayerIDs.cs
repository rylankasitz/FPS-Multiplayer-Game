using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerId
{
    public enum PLAYERID { ONE = 0, TWO = 1, THREE = 2, FOUR = 4, NONE = -1};
    public enum TEAM { ONE = 0, TWO = 1 };

    public static int CurrentPlayer = 0;

    public static int MaxPlayers = 2;
}
