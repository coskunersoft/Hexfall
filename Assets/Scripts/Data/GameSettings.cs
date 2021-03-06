﻿using UnityEngine;

[CreateAssetMenu(fileName = "Settings", menuName = "Hexfall/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    public int row;
    public int column;
    public Color32[] colorScale;
    public int ItemCount { get { return row * column; } }
    public int scoreReward;

    [Range(0,75)]
    public float starItemFrequency=10;
}
