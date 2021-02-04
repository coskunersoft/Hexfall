using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

public class GameGrid
{

    public GameGrid(int _row, int _column)
    {
        isBusy = true;
        row = _row;
        column = _column;
        items = new List<GameItem>();
    }
    private List<GameItem> items;
    public int row;
    public int column;

    public bool isBusy = false;

    public SelectData selectData;

    public System.Action<int> ColumnFillAction;

    /// <summary>
    /// Remove item from grid
    /// </summary>
    /// <param name="_item"></param>
    public void RemoveItem(GameItem _item)
    {
        items.Remove(_item);
        items.Sort((x, y) => ((int)x).CompareTo(y));
    }
    /// <summary>
    /// Adds an item to the grid's item list 
    /// </summary>
    /// <param name="_item"></param>
    public void AddItem(GameItem _item)
    {
        items.Add(_item);
        items.Sort((x, y) => ((int)x).CompareTo(y));
    }
    /// <summary>
    /// Function that starts to find the groups that will disappear on the game board and start destroying them
    /// </summary>
    /// <returns></returns>
    public bool DestructionControl()
    {
        List<List<int>> allSliceds = new List<List<int>>();
        var sliceRule = GameManager.instance.runtimeVars.selectedPattern.PatternRule();
        foreach (var item in items)
        {
            List<int> Sliceds = sliceRule.Invoke(item, this);
            bool added = false;
            for (int i = 0; i < allSliceds.Count; i++)
            {
                if (allSliceds[i].MatchCount(Sliceds) > 0)
                {
                    Debug.Log("MatchCountFinded");
                    List<int> total = allSliceds[i].CombineDiff(Sliceds);
                    allSliceds[i].Clear();
                    allSliceds[i].AddRange(total);
                    added = true;
                    break;
                }
            }
            if (Sliceds.Count > 0 && !added)
            {
                allSliceds.Add(Sliceds);
            }
        }
        Debug.Log("Total " + allSliceds.Count + " group");
        foreach (var item in allSliceds)
        {
            List<GameItem> DestroyingItems = item.ConvertAll(x => this[x]);
            if (DestroyingItems.Any(x => x == -1)&&DestroyingItems.Count<3) continue;
            int ScorePiece = 0;
            DestroyingItems.ForEach(x => ScorePiece += x.HaveStar ? 4 : 1);
            GameManager.instance.OnSliced(ScorePiece, DestroyingItems.CenterPositionofItems());
            foreach (var item2 in DestroyingItems)
            {
                item2.Destruction();
                RemoveItem(item2);
            }
        }
        bool result = allSliceds.Count > 0;
        if (result)
        {
            FallController().InvokeIE();
        }
        return result;
    }
    /// <summary>
    /// Enumerator where new items are taken into missing parts after destruction.
    /// </summary>
    /// <returns></returns>
    public IEnumerator FallController()
    {
        for (int i = 0; i < column; i++)
        {
            List<GameItem> columnItems = ColumnItems(i);
            columnItems.Sort((x, y) => x.gridIndex.y.CompareTo(y.gridIndex.y));
            for (int j = 0; j < columnItems.Count; j++)
            {
                GameItem columnItemx = columnItems[j];
                int dis = (int)(columnItemx.gridIndex.y) - j;
                columnItemx.gridIndex.y = j;
                if (dis > 0)
                {
                    Vector3 newpos = columnItemx.transform.position - (Vector3.up * GameManager.instance.runtimeVars.movementMultipery * dis);
                    columnItemx.MoveNewPos(newpos, .25f);
                    yield return new WaitForSeconds(.025f);
                }
            }

        }
        yield return new WaitForSeconds(.5f);
        if (!DestructionControl())
        {
            for (int i = 0; i < column; i++)
            {
                List<GameItem> columnItems = ColumnItems(i);
                int requered = row - columnItems.Count;
                if (requered > 0)
                {
                    ColumnFillAction(i);
                    yield return new WaitForSeconds(.1f);
                }
            }
            yield return new WaitForSeconds(1f);
            if (!DestructionControl())
            {
                GameManager.instance.OnSliceEnd();
                if (BombExposionControl())
                {
                    Debug.Log("Bomba Patladı");
                    AudioManager.PlayOneShotAudio("explosion");
                    Finish();
                    GameManager.instance.GameOver("Bomb Explodes");
                }
                else if (!CanMoveControl())
                {
                    Debug.Log("Hamle kalmadı");
                    AudioManager.PlayOneShotAudio("nomove");
                    Finish();
                    GameManager.instance.GameOver("There is no Movement to Continue");
                }
                isBusy = false;
            }
        }

    }
    /// <summary>
    /// Finds the largest item number on the grid
    /// </summary>
    /// <returns></returns>
    public int MaxValue()
    {
        if (items == null) return -1;
        if (items.Count <= 0) return -1;

        return items[items.Count - 1];
    }
    /// <summary>
    /// Function that returns items in certain columns of the grid as a list
    /// </summary>
    /// <returns></returns>
    public List<GameItem> ColumnItems(int _column)
    {
        if (_column < 0 || _column > column) return null;
        return items.FindAll(x => x.gridIndex.x == _column);
    }
    /// <summary>
    /// Function that returns items in certain rows of the grid as a list
    /// </summary>
    /// <returns></returns>
    public List<GameItem> RowItems(int _row)
    {
        if (_row < 0 || _row > row) return null;
        return items.FindAll(x => x.gridIndex.y == _row);
    }
    /// <summary>
    /// When the user selects an item, a close group for that item is determined and marked 
    /// </summary>
    /// <param name="item"></param>
    public void ItemSelected(GameItem item)
    {
        if (isBusy) return;

        if (selectData.isSelected)
        {
            Deselect();
        }

        var selectionRule = GameManager.instance.runtimeVars.selectedPattern.NeighborRule();
        Vector3 ClickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float mindis = Mathf.Sqrt(GameManager.instance.runtimeVars.movementMultiperx * GameManager.instance.runtimeVars.movementMultipery);
        List<GameItem> Group = selectionRule(item, this, ClickPos, mindis);
        Group.Sort((x, y) => ((int)x).CompareTo(y));
        selectData.isSelected = true;
        selectData.LastSelected = Group;
        selectData.LastSelected.ForEach(x => x.FocusUnFocus(true));
        GameManager.instance.OnSelectGroup(Group.CenterPositionofItems());
    }
    /// <summary>
    /// Cancels last selection
    /// </summary>
    public void Deselect()
    {
        if (!selectData.isSelected) return;
        selectData.LastSelected.ForEach(x =>
        {
            x.transform.SetParent(null);
            x.FocusUnFocus(false);
        });
        selectData.isSelected = false;
        selectData.LastSelected = new List<GameItem>();
        GameManager.instance.OnGroupSelectCancel();
    }
    /// <summary>
    /// Turning the last selected group to the right or left is performed
    /// </summary>
    public void Swipe(SwipeDirection _swipdir)
    {
        if (isBusy || !selectData.isSelected || selectData.LastSelected.Count <= 0) return;
        isBusy = true;

        float rotationfactor = (_swipdir == SwipeDirection.Left ? -1 : 1) * (360 / selectData.LastSelected.Count);
        Transform rotcenter = GameManager.instance.runtimeVars.selectionCenter;
        selectData.LastSelected.ForEach(ro => ro.transform.SetParent(rotcenter.transform));

        GameManager.instance.runtimeVars.lastMovesTemp = this;

        IEnumerator rotate()
        {
            bool issliced = false;
            for (int i = 0; i < selectData.LastSelected.Count; i++)
            {
                AudioManager.PlayOneShotAudio("swipe");

                List<System.Tuple<Vector2, Vector3>> ObjectPositions = selectData.LastSelected.ConvertAll(x => new System.Tuple<Vector2, Vector3>(x.gridIndex, x.transform.position));
                selectData.LastSelected.SlideListOneStep(_swipdir == SwipeDirection.Left);
                Vector3 rot = rotcenter.transform.eulerAngles + new Vector3(0, 0, rotationfactor);
                rotcenter.transform.DORotate(rot, 0.2f);
                yield return new WaitForSeconds(0.2f);
                for (int k = 0; k < ObjectPositions.Count; k++)
                {
                    selectData.LastSelected[k].gridIndex = ObjectPositions.Find(x => Vector3.Distance(x.Item2, selectData.LastSelected[k].transform.position) < 0.1f).Item1;
                }
                if (DestructionControl())
                {
                    issliced = true;
                    Deselect();
                    break;
                }
            }
            if (!issliced)
            {
                isBusy = false;
            }
            else
            {
                GameManager.instance.SetMoves(GameManager.instance.runtimeVars.movesCount + 1);
            }
        }
        rotate().InvokeIE();

    }
    /// <summary>
    /// Recolor all grid with new color group
    /// </summary>
    public void RestoreGrid(GridItemTemp[,] newcolorgroup)
    {
        Deselect();
        for (int i = 0; i < column; i++)
        {
            for (int j = 0; j < row; j++)
            {
                items.Find(x => x.gridIndex == new Vector2(i, j)).Restore(newcolorgroup[i, j]);
            }
        }
    }
    /// <summary>
    /// Control bomb game ites exposion status
    /// </summary>
    /// <returns></returns>
    private bool BombExposionControl()
    {
        bool result = false;
        foreach (var item in items)
        {
            if (item.bombCounter <= -1) continue;

            result = item.MinusBomb();
            if (result) break;
        }
        return result;
    }
    /// <summary>
    /// Check if the game is over 
    /// </summary>
    /// <returns></returns>
    private bool CanMoveControl()
    {
        bool result = false;

        var allNeighborsRule = GameManager.instance.runtimeVars.selectedPattern.AllNeighborsRule();
        var SlicedRule = GameManager.instance.runtimeVars.selectedPattern.PatternRule();

        List<GameItem> LastGroup = null;
        int slidestep = 0;

        GameObject centerobject = new GameObject();
       
        foreach (var item in items)
        {
            List<List<GameItem>> AllGroups = allNeighborsRule(item,this);
            for (int i = 0; i < AllGroups.Count; i++)
            {
                LastGroup = AllGroups[i];
                slidestep = 0;
                float rotationfactor =-(360 / LastGroup.Count);
                centerobject.transform.position = LastGroup.CenterPositionofItems();
                LastGroup.ForEach(x => x.transform.SetParent(centerobject.transform));
                for (int k = 0; k < LastGroup.Count; k++)
                {
                    List<System.Tuple<Vector2, Vector3>> ObjectPositions = LastGroup.ConvertAll(x => new System.Tuple<Vector2, Vector3>(x.gridIndex, x.transform.position));
                    LastGroup.SlideListOneStep(true);
                    centerobject.transform.Rotate(new Vector3(0, 0, rotationfactor));
                    for (int j = 0; j < ObjectPositions.Count; j++)
                    {
                        LastGroup[j].gridIndex = ObjectPositions.Find(x => Vector3.Distance(x.Item2, LastGroup[j].transform.position) < 0.1f).Item1;
                    }
                    slidestep++;
                    List<int> sliceds = SlicedRule(item, this);
                   
                    if (sliceds.Count>=LastGroup.Count)
                    {
                        result = true;
                        Debug.Log("Finded"+(int)item);
                        break;
                    }
                }
                LastGroup.ForEach(x => x.transform.SetParent(null));
                if (result) break;
            }
            if (result) break;
        }

        if (slidestep>0)
        {
            centerobject.transform.position = LastGroup.CenterPositionofItems();
            LastGroup.ForEach(x => x.transform.SetParent(centerobject.transform));
            float rotationfactor = (360 / LastGroup.Count);
            for (int i = 0; i < slidestep; i++)
            {
                List<System.Tuple<Vector2, Vector3>> ObjectPositions = LastGroup.ConvertAll(x => new System.Tuple<Vector2, Vector3>(x.gridIndex, x.transform.position));
                LastGroup.SlideListOneStep(false);
                centerobject.transform.Rotate(new Vector3(0, 0, rotationfactor));
                for (int j = 0; j < ObjectPositions.Count; j++)
                {
                    LastGroup[j].gridIndex = ObjectPositions.Find(x => Vector3.Distance(x.Item2, LastGroup[j].transform.position) < 0.1f).Item1;
                }
            }
            LastGroup.ForEach(x => x.transform.SetParent(null));
        }
        GameManager.Destroy(centerobject);
        
        
        return result;
    }

    public void Finish()
    {
        Deselect();
        for (int i = 0; i < items.Count; i++)
        {
            items[i].Destruction();
        }
        items.Clear();
    }

    /// <summary>
    /// Quick access indexing of game items by sequence number
    /// </summary>
    public GameItem this[int index]
    {
        get { return items.Find(x => x == index); }
    }

    public static implicit operator GridItemTemp[,](GameGrid grid)
    {
        GridItemTemp[,] result = new GridItemTemp[grid.column,grid.row];
        for (int i = 0; i < grid.column; i++)
        {
            for (int j = 0; j < grid.row; j++)
            {
                GameItem itemx = grid.items.Find(x => x.gridIndex == new Vector2(i, j));
                result[i, j] = new GridItemTemp(itemx.ItemColor, itemx.bombCounter, itemx.HaveStar);
            }
        }
        return result;
    }

    /// <summary>
    /// Struct which stores information about group selection  
    /// </summary>
    public struct SelectData
    {
        public bool isSelected;
        public List<GameItem> LastSelected;
    }


}
public class GridItemTemp
{
    public GridItemTemp(int _color,int _bomb,bool _star)
    {
        color = _color;
        havestar = _star;
        bombcounter = _bomb;
    }
    public int color;
    public bool havestar;
    public int bombcounter;
}
