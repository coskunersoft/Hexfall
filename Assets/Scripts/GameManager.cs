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
        runtimeVars.movementMultipery = temp.transform.localScale.y*0.725f;
        runtimeVars.movementMultiperx= temp.transform.localScale.x*0.6f;
        Destroy(temp);

        bool y_ofset = true;
        IEnumerator creation()
        {
            for (int i = 0; i < gameSettings.column; i++)
            {
                y_ofset = !y_ofset;
                GameItem created = null;
                for (int j = 0; j <gameSettings.row; j++)
                {
                    created = Instantiate(generalGameVars.gameItemPrefab, objectpos, Quaternion.identity).GetComponent<GameItem>();
                    if (j == 0 && y_ofset)
                    {
                        objectpos.y -= (runtimeVars.movementMultipery) / 2;
                        created.transform.position = objectpos;
                    }
                    objectpos.y += runtimeVars.movementMultipery;

                    created.ItemColor = Random.Range(0, gameSettings.colorScale.Length);
                    created.gridIndex = new Vector2(i, j);
                    runtimeVars.grid.AddItem(created);
                    yield return new WaitForSeconds(0.035f);
                }
                objectpos.y = generalGameVars.gameItemPosition.position.y;
                objectpos.x += runtimeVars.movementMultiperx;
            }
        }
        creation().InvokeIE();

        runtimeVars.grid.ColumnFillAction = (int column) =>
        {
            List<GameItem> columnItems = runtimeVars.grid.ColumnItems(column);
            columnItems.Sort((x, y) => x.gridIndex.y.CompareTo(y.gridIndex.y));
            int reqcount = runtimeVars.grid.row - columnItems.Count;
            GameItem created = null;
            GameItem lastincolumn = columnItems[columnItems.Count - 1];
            Vector3 pos = lastincolumn.transform.position;
            for (int i = 0; i < reqcount; i++)
            {
                pos.y += runtimeVars.movementMultipery;
                created = Instantiate(generalGameVars.gameItemPrefab, pos, Quaternion.identity).GetComponent<GameItem>();
                created.ItemColor = Random.Range(0, gameSettings.colorScale.Length);
                created.gridIndex = new Vector2(column, lastincolumn.gridIndex.y + (i + 1));
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
        }
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
        public Vector3 deltaClickPos;
    }
    [System.Serializable]
    public struct GeneralGameVariables
    {
        public GameObject gameItemPrefab;
        public Transform gameItemPosition;
        public GameObject selectionCenterObject;
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