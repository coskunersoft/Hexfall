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

    private void SetupGame()
    {
        if (!gameSettings)gameSettings = Resources.Load<GameSettings>("Settings");
        if (!gameSettings) throw new System.Exception("You have drop a settings file to gameSettings");
        runtimeVars.grid = new GameGrid(gameSettings.row,gameSettings.column);
        Vector3 objectpos = generalGameVars.gameItemPosition.position;

        runtimeVars.selectionCenter = Instantiate(generalGameVars.selectionCenterObject, Vector3.zero, Quaternion.identity).transform;

        GameObject temp = Instantiate(generalGameVars.gameItemPrefab, Vector3.zero, Quaternion.identity);
        runtimeVars.movementMultipery = temp.transform.localScale.y*0.71f;
        runtimeVars.movementMultiperx= temp.transform.localScale.x*0.59f;
        Destroy(temp);

        bool y_ofset = true;
        IEnumerator creation()
        {
            int[,] colormap = new int[gameSettings.column, gameSettings.row];
            for (int i = 0; i < gameSettings.column; i++)
            {
                y_ofset = !y_ofset;
                GameItem created = null;
                for (int j = 0; j <gameSettings.row; j++)
                {
                    created = ObjectCamp.instance.GetObject<GameItem>();
                    created.transform.position = objectpos;
                    //created = Instantiate(generalGameVars.gameItemPrefab, objectpos, Quaternion.identity).GetComponent<GameItem>();
                    if (j == 0 && y_ofset)
                    {
                        objectpos.y -= (runtimeVars.movementMultipery) / 2;
                        created.transform.position = objectpos;
                    }
                    objectpos.y += runtimeVars.movementMultipery;
                   

                    List<int> ignorelist = new List<int>();
                    if (i>0)
                    {
                        ignorelist.Add(colormap[i - 1, j]);
                    }
                    created.ItemColor = Utilities.RandomintegerWithIgnore(0,gameSettings.colorScale.Length,ignorelist);
                    colormap[i, j] = created.ItemColor;

                    created.gridIndex = new Vector2(i, j);
                    runtimeVars.grid.AddItem(created);
                    yield return new WaitForSeconds(0.035f);
                }
                objectpos.y = generalGameVars.gameItemPosition.position.y;
                objectpos.x += runtimeVars.movementMultiperx;
            }
            runtimeVars.grid.isBusy = false;
        }
        creation().InvokeIE();

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
                // created = Instantiate(generalGameVars.gameItemPrefab, pos, Quaternion.identity).GetComponent<GameItem>();
                created.ItemColor = Random.Range(0, gameSettings.colorScale.Length);
                created.gridIndex = new Vector2(column, starty + (i + 1));
                runtimeVars.grid.AddItem(created);
            }
        };
    }

    private void Update()
    {
        SwipteController();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log((int)runtimeVars.grid[100]);
            runtimeVars.grid.SliceControl();
        }
    }

    private void SwipteController()
    {
        if (Input.GetMouseButtonDown(0))
        {
            runtimeVars.deltaClickPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            if (Mathf.Abs(Input.mousePosition.x-runtimeVars.deltaClickPos.x)>Screen.width*0.05)
            {
                runtimeVars.grid.Swipe(runtimeVars.deltaClickPos.x > Input.mousePosition.x? SwipeDirection.Left : SwipeDirection.Right);
                runtimeVars.deltaClickPos = Input.mousePosition;
            }
            else if (Mathf.Abs(Input.mousePosition.y - runtimeVars.deltaClickPos.y) > Screen.height * 0.05)
            {
                runtimeVars.grid.Swipe(runtimeVars.deltaClickPos.y < Input.mousePosition.y ? SwipeDirection.Left : SwipeDirection.Right);
                runtimeVars.deltaClickPos = Input.mousePosition;
            }
        }
    }
    
    public void OnSliced(int sliceCount,Vector3 centerOfGroup)
    {
        FlyingScore flyScore = ObjectCamp.instance.GetObject<FlyingScore>();
        float scorereward = gameSettings.scoreReward * sliceCount;
        flyScore.transform.position = centerOfGroup;
        flyScore += (scorereward.ToString());

    }

    public void OnSelectGroup(Vector3 center)
    {
        runtimeVars.selectionCenter.gameObject.SetActive(true);
        runtimeVars.selectionCenter.position = center;
    }
    public void OnGroupSelectCancel()
    {
        runtimeVars.selectionCenter.gameObject.SetActive(false);
    }

    [System.Serializable]
    public struct RuntimeVariables
    {
        public bool isGameStarted;
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

    }
    [System.Serializable]
    public struct GeneralGameVariables
    {
        public GameObject gameItemPrefab;
        public Transform gameItemPosition;
        public GameObject selectionCenterObject;
        public GameObject flyingScorePrefab;
    }

  
}
public enum Pattern
{
    Diamond = 0,
}
public enum SwipeDirection
{
    Right=0,Left=1,
}