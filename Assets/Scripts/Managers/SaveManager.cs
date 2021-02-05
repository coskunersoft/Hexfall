using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    public void Reset()
    {
        PlayerPrefs.SetString("save", "");
    }

    /// <summary>
    /// Read last recorded save data
    /// </summary>
    public SaveData GetLastRecord()
    {
        string data = PlayerPrefs.GetString("save", "");
        Debug.Log(data);
        return !string.IsNullOrEmpty(data)?JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString("save")):null;
    }
    /// <summary>
    /// Save current game
    /// </summary>
    public void SaveGame()
    {
        SaveData x = new SaveData();
        GameManager.RuntimeVariables runtimeVariables = GameManager.instance.runtimeVars;
        x.bombScore = runtimeVariables.bombScore;
        x.canUndo = runtimeVariables.canUndo;
        x.deltaScore = runtimeVariables.deltaScore;
        x.movesCount = runtimeVariables.movesCount;
        x.score = runtimeVariables.score;
        x.undoRight = runtimeVariables.undoRight;
        x.gridItemTemps = runtimeVariables.grid;
        x.tempscore = runtimeVariables.tempscore;
        if (runtimeVariables.lastMoves!=null)
        {
            x.lastMoves = new List<GridItemTemp>();
            for (int i = 0; i < GameManager.instance.gameSettings.column; i++)
            {
                for (int j = 0; j < GameManager.instance.gameSettings.row; j++)
                {
                    x.lastMoves.Add(runtimeVariables.lastMoves[i, j]);
                }
            }
        }

        x.row = GameManager.instance.gameSettings.row;
        x.column = GameManager.instance.gameSettings.column;
        x.colorscale = GameManager.instance.gameSettings.colorScale.Length;

        PlayerPrefs.SetString("save",JsonUtility.ToJson(x));
    }

   

}
[System.Serializable]
public class SaveData
{
    [SerializeField]
    public List<GridItemTemp> gridItemTemps;

    [SerializeField]
    public List<GridItemTemp> lastMoves;


    public int score;
    public int deltaScore;
    public int movesCount;
    public bool canUndo;
    public int undoRight;
    public int bombScore;
    public int tempscore;


    public int row;
    public int column;
    public int colorscale;

}
