using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameGrid
{
    public GameGrid(int _row,int _column)
    {
        row = _row;
        column = _column;
        items = new List<GameItem>();
    }
    private List<GameItem> items;
    public int row;
    public int column;

    public System.Action<int> ColumnFillAction;

    public void RemoveItem(GameItem _item)
    {
        items.Remove(_item);
        items.Sort((x, y) => ((int)x).CompareTo(y));
    }

    public void AddItem(GameItem _item)
    {
        items.Add(_item);
        items.Sort((x, y) => ((int)x).CompareTo(y));
    }

    public void SliceControl(Pattern _pattern)
    {
        List<int> Result = new List<int>();

        System.Func<GameItem, GameGrid, List<int>> sliceRule
            = _pattern.PatternRule();
        foreach (var item in items)
        {
            List<int> Sliceds = sliceRule.Invoke(item, this);
            Result.AddRange(Sliceds);
        }
        foreach (var item in Result)
        {
            GameItem itemx = this[item];
            if (!itemx) continue;
            itemx.Slice();
            RemoveItem(itemx);
        }
        FallController().InvokeIE();
    }

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
                if (dis>0)
                {
                    Vector3 newpos = columnItemx.transform.position - (Vector3.up * GameManager.instance.runtimeVars.movementMultipery*dis);
                    columnItemx.MoveNewPos(newpos, .75f);
                }
            }
            
        }
        yield return new WaitForSeconds(.75f);
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

    }

    public int MaxValue()
    {
        if (items == null) return -1;
        if (items.Count <= 0) return -1;

        return items[items.Count - 1];
    }

    public List<GameItem> ColumnItems(int _column)
    {
        if (_column < 0 || _column > column) return null;
        return items.FindAll(x => x.gridIndex.x == _column);
    }
    public List<GameItem> RowItems(int _row)
    {
        if (_row < 0 || _row > row) return null;
        return items.FindAll(x => x.gridIndex.y == _row);
    }



    public static implicit operator List<GameItem>(GameGrid a)
    {
        return a.items;
    }

    public GameItem this[int index]
    {
        get { return items.Find(x => x == index); }
    }
}
