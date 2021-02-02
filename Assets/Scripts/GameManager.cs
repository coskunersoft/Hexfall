using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameSettings gameSettings;
    public RuntimeVariables runtimeVars;

    public void StartGame()
    {
        
    }

    private void SetupGame()
    {

    }

    [System.Serializable]
    public struct RuntimeVariables
    {
        public bool IsGameStarted;
        public GameItem[,] gameGrid;
       
    }

    

}
