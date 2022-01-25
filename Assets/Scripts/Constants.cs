using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants 
{
    public const int IGNORE_RAYCAST_LAYER = 2;
    public const int PROJECTILE_LAYER     = 6;
    public const int GHOST_LAYER          = 7;
    public const int DEAD_LAYER           = 8;
    public const int PLAYER_LAYER         = 9;
    public const int ENEMY_LAYER          = 10;
    public const int GROUND_LAYER         = 11;
}

public enum State
{
    DEAD,
    DEFAULT,
    BLOCKING,
    STUNNED,
    DASHING,
    ATTACKING
};