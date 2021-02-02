using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralClasses : MonoBehaviour
{
    
}


[CreateAssetMenu(fileName = "Settings", menuName = "Hexfall/GameSettings", order = 1)]
public class GameSettings : ScriptableObject
{
    public int Row;
    public int Column;
}
