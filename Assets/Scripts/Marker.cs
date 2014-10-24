using UnityEngine;
using System.Collections;
using System;

public enum EffectType
{
    HPF,
    LPF,
    Echo,
    Chorus,
}

public class MarkerInfo
{
    public MarkerDefinition Up;
    public MarkerDefinition Down;
    public EffectType Effect;
}

public class MarkerDefinition
{
    public float TimingSec { get; private set; }
    public Vector2 Position { get; private set; }

    public MarkerDefinition(float bpm, int measures, int beats, Vector2 position)
    {
        TimingSec = ((measures * 4) + beats) * (60f / bpm);
        Position = position;
    }
}