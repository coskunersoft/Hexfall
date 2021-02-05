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
        //Make an integer list inside list
        List<List<int>> allDestructionGroups = new List<List<int>>();
        //Find the elimination rule based on the selected pattern 
        var sliceRule = GameManager.instance.runtimeVars.selectedPattern.PatternRule();
        //Loops as long as all items
        foreach (var item in items)
        {
            //Exacute function and find the sequence number of objects to be deleted 
            List<int> Sliceds = sliceRule.Invoke(item, this);
            //make a control boolen for process control 
            bool added = false;

            //loop as many groups as previously found 
            for (int i = 0; i < allDestructionGroups.Count; i++)
            {
                //If the last group found matches the current group 
                if (allDestructionGroups[i].MatchCount(Sliceds) > 0)
                {
                    Debug.Log("MatchCountFinded");
                    //Merge the last found group with the previous one. (i)
                    allDestructionGroups[i] = allDestructionGroups[i].CombineDiff(Sliceds);
                    //set the process bool to true
                    added = true;
                    //No need to control others 
                    break;
                }
            }
            //f the current group is complete and not merged with one of the previous groups 
            if (Sliceds.Count > 0 && !added)
            {
                //Add to found groups 
                allDestructionGroups.Add(Sliceds);
            }
        }
        Debug.Log("Total " + allDestructionGroups.Count + " group");
        // loop to process all found groups
        foreach (var item in allDestructionGroups)
        {
            //convert groups to item list with sequence numbers 
            List<GameItem> DestroyingItems = item.ConvertAll(x => this[x]);
            //skip the step if the group is not complete and contains empty objects 
            if (DestroyingItems.Any(x => x == -1)&&DestroyingItems.Count<3) continue;

            //center of objects to be destroyed
            Vector3 CenterofDestruction = DestroyingItems.CenterPositionofItems();
            //Points multiplier to be awarded 
            int ScorePiece = 0;
            // We looped as much as all items of current group and 
            foreach (var item2 in DestroyingItems)
            {
                //increased the score multiplier according to the status of the items
                ScorePiece += item2.HaveStar ? 4 : 1;
                //push the object into the pool 
                item2.Destruction();
               // delete from grid list
                RemoveItem(item2);
            }
            //We notified the game manager that there was a Destruction 
            GameManager.instance.OnSliced(ScorePiece,CenterofDestruction);
        }
        //
        if (allDestructionGroups.Count > 0)
        {
            //ReCreate controls if at least 1 item group is destroyed 
            FallController().InvokeIE();
        }
        //Return result based on the number of destructions 
        return allDestructionGroups.Count > 0;
    }
    /// <summary>
    /// Enumerator where new items are taken into missing parts after destruction.
    /// </summary>
    /// <returns></returns>
    public IEnumerator FallController()
    {
        //loop as much as the grid's column number (column) 
        for (int i = 0; i < column; i++)
        {
            //finds current column(i) items
            List<GameItem> columnItems = ColumnItems(i);
            //sort them by row ascending 
            columnItems.Sort((x, y) => x.gridIndex.y.CompareTo(y.gridIndex.y));
            //loop as much as all column items count
            for (int j = 0; j < columnItems.Count; j++)
            {
                //keep the item variable
                GameItem columnItemx = columnItems[j];
                //how many units away from where it should be 
                int dis = (int)(columnItemx.gridIndex.y) - j;
                //reassign the sequence number (y/row)
                columnItemx.gridIndex.y = j;
                //If it is far from where it should be
                if (dis > 0)
                {
                    //determine your new position 
                    Vector3 newpos = columnItemx.transform.position - (Vector3.up * GameManager.instance.runtimeVars.movementMultipery * dis);
                    //move to position smoothly
                    columnItemx.MoveNewPos(newpos, .25f);
                    //wait very little time between each item 
                    yield return new WaitForSeconds(.025f);
                }
            }

        }
        //wait until all items drop 
        yield return new WaitForSeconds(.5f);
        //After dropping down, control of destruction again and if there is no destruction 
        if (!DestructionControl())
        {
            //loop as much as the grids column number again(column) 
            for (int i = 0; i < column; i++)
            {
                //finds current column(i) items 
                List<GameItem> columnItems = ColumnItems(i);
                //number of objects required for the column
                int requered = row - columnItems.Count;
                
                if (requered > 0)
                {
                    //invoke the action that is installed in the game manager
                    ColumnFillAction(i);
                    yield return new WaitForSeconds(.1f);
                }
            }
            //wait until all the columns are filled 
            yield return new WaitForSeconds(1f);
            //When all the columns are full again, check to destroy again and if there is no destruction
            if (!DestructionControl())
            {
                //notify game manager that the destruction is finished 
                GameManager.instance.OnSliceEnd();
                //do bomb explosion check  
                if (BombExposionControl())// if exploded 
                {
                    Debug.Log("Bomba Patladı");
                    //plays explosion sound
                    AudioManager.PlayOneShotAudio("explosion");
                    //finish grid
                    Finish();
                    //notify the game manager that the game is over with description
                    GameManager.instance.GameOver("Bomb Explodes");
                }
                //check for ability to move 
                else if (!CanMoveControl())//if there are no more moves to be made 
                {
                    Debug.Log("Hamle kalmadı");
                    //plays (no move) sound
                    AudioManager.PlayOneShotAudio("nomove");
                    //finish grid
                    Finish();
                    //notify the game manager that the game is over with description
                    GameManager.instance.GameOver("There is no Movement to Continue");
                }
                else //continue game
                {
                    isBusy = false;
                }
               
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

        //check if there is already a choice 
        if (selectData.isSelected)
        {
            Deselect();
        }

        //take the selection rule according to the pattern 
        var selectionRule = GameManager.instance.runtimeVars.selectedPattern.NeighborRule();
        //Find the click point 
        Vector3 ClickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Multiply the horizontal and vertical length of the element and get the square root 
        float mindis = Mathf.Sqrt(GameManager.instance.runtimeVars.movementMultiperx * GameManager.instance.runtimeVars.movementMultipery);
        //Selected Objects were found by running the action found according to the pattern 
        List<GameItem> Group = selectionRule(item, this, ClickPos, mindis);
        //Sort these items by grid ocation
        Group.Sort((x, y) => ((int)x).CompareTo(y));

        selectData.isSelected = true;
        //Push to selected list
        selectData.LastSelected = Group;
        //Focus Selected Objects
        selectData.LastSelected.ForEach(x => x.FocusUnFocus(true));
        //Report this action to the game manager 
        GameManager.instance.OnSelectGroup(Group.CenterPositionofItems());
    }
    /// <summary>
    /// Cancels last selection
    /// </summary>
    public void Deselect()
    {
        //Check if there is any group selected 
        if (!selectData.isSelected) return;
        //Remove the selection icon of all entities and subtract from parent
        selectData.LastSelected.ForEach(x =>
        {
            x.transform.SetParent(null);
            x.FocusUnFocus(false);
        });
        //Set selection status false
        selectData.isSelected = false;
        //Set Selected group empity
        selectData.LastSelected = new List<GameItem>();
        //Report this action to the game manager 
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
    /// <summary>
    /// Finish grid and destroy objects
    /// </summary>
    public void Finish()
    {
        //Cancel last selection
        Deselect();
        //loop all items
        for (int i = 0; i < items.Count; i++)
        {
            //destroy item
            items[i].Destruction();
        }
        //clear item list
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
    public static implicit operator List<GridItemTemp>(GameGrid grid)
    {
        List<GridItemTemp> result = new List<GridItemTemp>();
        for (int i = 0; i < grid.column; i++)
        {
            for (int j = 0; j < grid.row; j++)
            {
                GameItem itemx = grid.items.Find(x => x.gridIndex == new Vector2(i, j));
                result.Add(new GridItemTemp(itemx.ItemColor, itemx.bombCounter, itemx.HaveStar));
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

[System.Serializable]
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
