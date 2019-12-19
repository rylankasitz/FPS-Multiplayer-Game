using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class GunStats
{

    /* Pistol, AR, MachineGun, Sniper */

    public static float[] FIRE_RATE = { -1, .4f, .2f, .1f, .7f };

    public static float[] RELOAD_TIME = { -1, 2.5f, 4f, 3.5f, 4.6f };

    public static int[] MAG_SIZE = { -1, 10, 20, 50, 6 };

    public static bool[] AUTOMATIC = { false, false, true, true, false };

    public static float[] BULLET_DAMAGE = { -1, 15f, 10f, 4f, 50f };

    public static string[] SCOPE_TEXTS = { "NONE", "SNIPER (8x)", "ACOG (4x)", "CCO (3x)", "HOLO (1x)", "RDS (2x)", "RED DOT (1.5x)" };

    public static string[] WEAPON_NAMES = { "NONE", "PISTOL", "AR", "MACHINE GUN", "SNIPER" };

    public static WeaponStats None = new WeaponStats()
    {
        GunVal = GunVal.None,
        ScopeVal = ScopeVal.None,
        Scopes = new List<ScopeItem>()
    };

    public static WeaponStats Pistol = new WeaponStats()
    {
        GunVal = GunVal.Pistol,
        ScopeVal = ScopeVal.None,
        Price = 100,
        Scopes = new List<ScopeItem>()
        {
            new ScopeItem()
            {
                Scope = ScopeVal.None,
                bought = true
            },
            new ScopeItem()
            {
                Scope = ScopeVal.HOLO,
                bought = false,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.RedDot,
                bought = false,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.RDS,
                bought = false,
                Price = 100
            }
        }
    };

    public static WeaponStats AR = new WeaponStats()
    {
        GunVal = GunVal.AR,
        ScopeVal = ScopeVal.None,
        Price = 400,
        Scopes = new List<ScopeItem>()
        {
            new ScopeItem()
            {
                Scope = ScopeVal.None,
                bought = true
            },
            new ScopeItem()
            {
                Scope = ScopeVal.RDS,
                bought = false,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.RedDot,
                bought = false,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.CCO,
                bought = false,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.Acog,
                bought = false,
                Price = 100
            }
        }
    };

    public static WeaponStats MachineGun = new WeaponStats()
    {
        GunVal = GunVal.MachineGun,
        ScopeVal = ScopeVal.None,
        Price = 500,
        Scopes = new List<ScopeItem>()
        {
            new ScopeItem()
            {
                Scope = ScopeVal.None,
                bought = true,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.HOLO,
                bought = false,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.RDS,
                bought = false,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.RedDot,
                bought = false,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.CCO,
                bought = false,
                Price = 100
            },
        }
    };

    public static WeaponStats Sniper = new WeaponStats()
    {
        GunVal = GunVal.Sniper,
        ScopeVal = ScopeVal.None,
        Price = 800,
        Scopes = new List<ScopeItem>()
        {
            new ScopeItem()
            {
                Scope = ScopeVal.None,
                bought = true,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.Acog,
                bought = false,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.X8,
                bought = false,
                Price = 100
            },
            new ScopeItem()
            {
                Scope = ScopeVal.CCO,
                bought = false,
                Price = 100
            }
        }
    };
}

public enum GunVal { None = 0, Pistol = 1, AR = 2, MachineGun = 3, Sniper = 4 }

public enum ScopeVal { None = 0, X8 = 1, Acog = 2, CCO = 3, HOLO = 4, RDS = 5, RedDot = 6 }

public class WeaponStats
{
    public GunVal GunVal { get; set; }

    public ScopeVal ScopeVal { get; set; }

    public List<ScopeItem> Scopes { get; set; }

    public int Price { get; set; }
}

public class ScopeItem
{
    public bool bought { get; set; }

    public ScopeVal Scope { get; set; }

    public int Price { get; set; }
}
