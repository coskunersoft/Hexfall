using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameSettings gameSettings;
    public RuntimeVariables runtimeVars;
    public GeneralGameVariables generalGameVars;

    private void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        SetupGame();
    }

    /// <summary>
    /// The first installation of the game is done in this function. 
    /// </summary>
    private void SetupGame()
    {
        #region Control Settings Asset
        if (!gameSettings)gameSettings = Resources.Load<GameSettings>("Settings");
        if (!gameSettings) throw new System.Exception("You have drop a settings file to gameSettings");
        #endregion

        #region First Step
        runtimeVars.grid = new GameGrid(gameSettings.row, gameSettings.column);
        runtimeVars.highScore = PlayerPrefs.GetInt("HighScore", 0);
        runtimeVars.isGameStarted = true;
        runtimeVars.isPaused = false;
        runtimeVars.canUndo = false;
        runtimeVars.bombScore = 1000;
        runtimeVars.tempscore = 0;
        runtimeVars.deltaScore = 0;
        SetScore(0);
        SetMoves(0);
        SetUndoRight(5);
        UIManager.instance.ShowHideUndoButton(false);
        #endregion

        #region Creation
        Vector3 objectpos = generalGameVars.gameItemPosition.position;
        if (!runtimeVars.selectionCenter)
        {
            runtimeVars.selectionCenter = Instantiate(generalGameVars.selectionCenterObject, Vector3.zero, Quaternion.identity).transform;
        }

        GameObject temp = Instantiate(generalGameVars.gameItemPrefab, Vector3.zero, Quaternion.identity);
        runtimeVars.movementMultipery = temp.transform.localScale.y*0.71f;
        runtimeVars.movementMultiperx= temp.transform.localScale.x*0.59f;
        Destroy(temp);

        bool y_ofset = true;
        IEnumerator creation()
        {
            GridItemTemp[,] colormap = new GridItemTemp[gameSettings.column, gameSettings.row];
            for (int i = 0; i < gameSettings.column; i++)
            {
                y_ofset = !y_ofset;
                GameItem created = null;
                for (int j = 0; j <gameSettings.row; j++)
                {
                    #region Get Object from pool and place
                    created = ObjectCamp.instance.GetObject<GameItem>();
                    created.transform.position = objectpos;
                    if (j == 0 && y_ofset)
                    {
                        objectpos.y -= (runtimeVars.movementMultipery) / 2;
                        created.transform.position = objectpos;
                    }
                    created.FallDown();
                    objectpos.y += runtimeVars.movementMultipery;
                    created.gridIndex = new Vector2(i, j);
                    runtimeVars.grid.AddItem(created);
                    #endregion

                    #region Color settings
                    List<int> ignorelist = new List<int>();
                    if (i>0) ignorelist.Add(colormap[i - 1, j].color);
                    created.ItemColor = Utilities.RandomintegerWithIgnore(0,gameSettings.colorScale.Length,ignorelist);
                    #endregion

                    colormap[i, j] = new GridItemTemp(created.ItemColor, -1, created.HaveStar);

                    yield return new WaitForSeconds(0.035f);
                }
                objectpos.y = generalGameVars.gameItemPosition.position.y;
                objectpos.x += runtimeVars.movementMultiperx;
            }
            runtimeVars.grid.isBusy = false;
            runtimeVars.lastMoves = colormap;
        }
        creation().InvokeIE();

        #region Setup filling action
        runtimeVars.grid.ColumnFillAction = (int column) =>
        {
            List<GameItem> columnItems = runtimeVars.grid.ColumnItems(column);
            columnItems.Sort((x, y) => x.gridIndex.y.CompareTo(y.gridIndex.y));
            int reqcount = runtimeVars.grid.row - columnItems.Count;
            GameItem created = null;
            GameItem lastincolumn = null;
            if (columnItems.Count>0)
            {
                lastincolumn = columnItems[columnItems.Count - 1];
            }

            float starty = -1;
            Vector3 pos = generalGameVars.gameItemPosition.position + (new Vector3(runtimeVars.movementMultiperx,0,0)*column);
            if (lastincolumn!=null)
            {
                pos = lastincolumn.transform.position;
                starty = lastincolumn.gridIndex.y;
            }
            for (int i = 0; i < reqcount; i++)
            {
                pos.y += runtimeVars.movementMultipery;
                created = ObjectCamp.instance.GetObject<GameItem>();
                created.transform.position = pos;
                created.FallDown();
                created.ItemColor = Random.Range(0, gameSettings.colorScale.Length);
                created.gridIndex = new Vector2(column, starty + (i + 1));
                if (runtimeVars.score>runtimeVars.bombScore)
                {
                    runtimeVars.bombScore += 1000;
                    created.SetBombStyle(true,Random.Range(6,9));
                }
                runtimeVars.grid.AddItem(created);
            }
        };
        #endregion

        #endregion
    }

    private void Update()
    {
        SwipteController();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log((int)runtimeVars.grid[100]);
            //   runtimeVars.grid.DestructionControl ();
           
        }
    }

    /// <summary>
    /// Detecting player's finger movement
    /// </summary>
    private void SwipteController()
    {
        if (runtimeVars.isPaused) return;
        if (Input.GetMouseButtonDown(0))
        {
            runtimeVars.deltaClickPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            if (Mathf.Abs(Input.mousePosition.x-runtimeVars.deltaClickPos.x)>Screen.width*0.05)
            {
                Vector2 selectioncenter = Camera.main.WorldToScreenPoint(runtimeVars.selectionCenter.position);
                if (Input.mousePosition.y<selectioncenter.y)
                {
                    runtimeVars.grid.Swipe(runtimeVars.deltaClickPos.x > Input.mousePosition.x ? SwipeDirection.Left : SwipeDirection.Right);
                }
                else
                {
                    runtimeVars.grid.Swipe(runtimeVars.deltaClickPos.x > Input.mousePosition.x ? SwipeDirection.Right : SwipeDirection.Left);
                }
                runtimeVars.deltaClickPos = Input.mousePosition;
            }
            else if (Mathf.Abs(Input.mousePosition.y - runtimeVars.deltaClickPos.y) > Screen.height * 0.05)
            {
                Vector2 selectioncenter = Camera.main.WorldToScreenPoint(runtimeVars.selectionCenter.position);
                if (Input.mousePosition.x < selectioncenter.x)
                {
                    runtimeVars.grid.Swipe(runtimeVars.deltaClickPos.y < Input.mousePosition.y ? SwipeDirection.Left : SwipeDirection.Right);
                }
                else
                {
                    runtimeVars.grid.Swipe(runtimeVars.deltaClickPos.y < Input.mousePosition.y ? SwipeDirection.Right : SwipeDirection.Left);
                }
                runtimeVars.deltaClickPos = Input.mousePosition;
            }
        }
    }

    /// <summary>
    /// Called on moves
    /// </summary>
    /// <param name="moves"></param>
    public void SetMoves(int moves)
    {
        runtimeVars.movesCount=moves;
        UIManager.instance.SyncMovesField();


    }
    /// <summary>
    /// Called on score changed
    /// </summary>
    /// <param name="score"></param>
    public void SetScore(int score)
    {
        runtimeVars.score = score;
        if (runtimeVars.score > runtimeVars.highScore) runtimeVars.highScore = runtimeVars.score;
        UIManager.instance.SyncScoreFields();
    }
    /// <summary>
    /// Called on undoRight Changed
    /// </summary>
    public void SetUndoRight(int right)
    {
        runtimeVars.undoRight = right;
        UIManager.instance.SyncUndoRigth();
    }
    /// <summary>
    /// Undo to previous move 
    /// </summary>
    public void UndoMove()
    {
        //Check can undo controls
        if (runtimeVars.undoRight <= 0 || !runtimeVars.canUndo || runtimeVars.grid.isBusy) return;

        UIManager.instance.ShowDialogWindow("Are you sure?", "You will undo last move", new UIManager.DWButtonData("Yes", () =>
          {
              //Minus right
              SetUndoRight(runtimeVars.undoRight - 1);
              //minus moves
              SetMoves(runtimeVars.movesCount - 1);
              //minus score
              SetScore(runtimeVars.score - runtimeVars.tempscore);
              UIManager.instance.ShowHideUndoButton(false);
              IEnumerator progress()
              {
                  runtimeVars.grid.isBusy = true;
                  runtimeVars.grid.RestoreGrid(runtimeVars.lastMoves);
                  yield return new WaitForSeconds(0.3f);
                  runtimeVars.grid.isBusy = false;
              }
              progress().InvokeIE();

          }),new UIManager.DWButtonData("No",()=>
          {

          }));
       
    }
    /// <summary>
    /// Calls on Slicing end
    /// </summary>
    public void OnSliceEnd()
    {
        runtimeVars.lastMoves = runtimeVars.lastMovesTemp;
        runtimeVars.canUndo = true;
        runtimeVars.tempscore = runtimeVars.deltaScore;
        runtimeVars.deltaScore = 0;
        UIManager.instance.ShowHideUndoButton(runtimeVars.undoRight > 0);
        Debug.Log("Slice end");
    }
    /// <summary>
    /// When the player destroys any group 
    /// </summary>
    public void OnSliced(int sliceCount,Vector3 centerOfGroup)
    {
        FlyingScore flyScore = ObjectCamp.instance.GetObject<FlyingScore>();
        int scorereward = gameSettings.scoreReward * sliceCount;
        flyScore.transform.position = centerOfGroup;
        flyScore += (scorereward.ToString());
        runtimeVars.deltaScore += scorereward;
        SetScore(runtimeVars.score+scorereward);
    }
    /// <summary>
    /// When the player selected any group on grid
    /// </summary>
    public void OnSelectGroup(Vector3 center)
    {
        runtimeVars.selectionCenter.gameObject.SetActive(true);
        runtimeVars.selectionCenter.position = center;
    }
    /// <summary>
    /// When selection canceled on grid
    /// </summary>
    public void OnGroupSelectCancel()
    {
        runtimeVars.selectionCenter.gameObject.SetActive(false);
    }
    /// <summary>
    /// Game restarts when called
    /// </summary>
    public void RestartGame()
    {
        runtimeVars.grid.Finish();
        SetupGame();
    }
    /// <summary>
    /// 
    /// </summary>
    public void GameOver(string reason)
    {
        if (runtimeVars.score>=runtimeVars.highScore)
        {
            PlayerPrefs.SetInt("HighScore",runtimeVars.score);
        }

        UIManager.instance.ShowDialogWindow("Game Over", reason,
            new UIManager.DWButtonData("Restart", () =>
          {
              RestartGame();
             
          }),
            new UIManager.DWButtonData("Exit", () =>
          {
              Application.Quit();
          }));
    }

    /// <summary>
    /// Variables that change dynamically during playback
    /// </summary>
    [System.Serializable]
    public struct RuntimeVariables
    {
        public bool isGameStarted;
        public bool isPaused;
        public GameGrid grid;
        public Pattern selectedPattern;
        [HideInInspector]
        public float movementMultiperx;
        [HideInInspector]
        public float movementMultipery;
        [HideInInspector]
        public Transform selectionCenter;
        [HideInInspector]
        public Vector3 deltaClickPos;

        public int score;
        public int highScore;
        [HideInInspector]
        public int deltaScore;
        public int movesCount;
        public bool canUndo;
        public int undoRight;
        public int bombScore;

        public GridItemTemp[,] lastMoves;
        public GridItemTemp[,] lastMovesTemp;
        public int tempscore;
    }
    /// <summary>
    /// Constant variables required for the game 
    /// </summary>
    [System.Serializable]
    public struct GeneralGameVariables
    {
        public GameObject gameItemPrefab;
        public Transform gameItemPosition;
        public GameObject selectionCenterObject;
        public GameObject flyingScorePrefab;
    }

  
}

/// <summary>
/// Game Patterns for grouping and destroying items
/// </summary>
public enum Pattern
{
    Diamond = 0,
}

/// <summary>
/// For marking the which direction items will turn 
/// </summary>
public enum SwipeDirection
{
    Right=0,Left=1,
}