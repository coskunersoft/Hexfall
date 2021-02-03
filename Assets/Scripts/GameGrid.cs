using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using DG.Tweening;

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

    public bool isBusy = false;

    public SelectData selectData;

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

    public bool SliceControl()
    {
        List<int> allSliceds = new List<int>();
        var sliceRule = GameManager.instance.runtimeVars.selectedPattern.PatternRule();
        foreach (var item in items)
        {
            List<int> Sliceds = sliceRule.Invoke(item, this);
            allSliceds.AddRange(Sliceds);
        }
        foreach (var item in allSliceds)
        {
            GameItem itemx = this[item];
            if (!itemx) continue;
            itemx.Slice();
            RemoveItem(itemx);
        }
        bool result = allSliceds.Count > 0;
        if (result)
        {
            FallController().InvokeIE();
        }
        return result;
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
                    columnItemx.MoveNewPos(newpos, .25f);
                    yield return new WaitForSeconds(.025f);
                }
            }
           
        }
        yield return new WaitForSeconds(.5f);
        if (!SliceControl())
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
            if (!SliceControl()) isBusy = false;
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

    public void ItemSelected(GameItem item)
    {
        if (isBusy) return;
        
        if (selectData.isSelected)
        {
            Deselect();
        }

        var selectionRule= GameManager.instance.runtimeVars.selectedPattern.neighborRule();
        Vector3 ClickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        float mindis = Mathf.Sqrt(GameManager.instance.runtimeVars.movementMultiperx * GameManager.instance.runtimeVars.movementMultipery);
        List<GameItem> Group = selectionRule(item, this,ClickPos,mindis);
        Group.Sort((x, y) => ((int)x).CompareTo(y));
        selectData.isSelected = true;
        selectData.LastSelected = Group;
        selectData.LastSelected.ForEach(x => x.FocusUnFocus(true));
        GameManager.instance.OnSelectGroup(Group.CenterPositionofItems());
    }

    public void Deselect()
    {
        selectData.LastSelected.ForEach(x => x.FocusUnFocus(false));
        selectData.isSelected = false;
        selectData.LastSelected = new List<GameItem>();
        GameManager.instance.OnGroupSelectCancel();
    }

    public void Swipe(SwipeDirection _swipdir)
    {
        if (isBusy||!selectData.isSelected||selectData.LastSelected.Count<=0) return;
        isBusy = true;
       
        float rotationfactor = 360 / selectData.LastSelected.Count;
        if (_swipdir==SwipeDirection.Left)
        {
            rotationfactor = -rotationfactor;
        }

      

        GameObject rotcenter = new GameObject();
        rotcenter.transform.position = selectData.LastSelected.CenterPositionofItems();
        selectData.LastSelected.ForEach(ro => ro.transform.SetParent(rotcenter.transform));

        IEnumerator rotate()
        {
            bool issliced = false;
            for (int i = 0; i < selectData.LastSelected.Count; i++)
            {
                List<System.Tuple<Vector2, Vector3>> ObjectPositions = selectData.LastSelected.ConvertAll(x => new System.Tuple<Vector2, Vector3>(x.gridIndex, x.transform.position));
                selectData.LastSelected.SlideListOneStep(_swipdir == SwipeDirection.Left);
                Vector3 rot = rotcenter.transform.eulerAngles + new Vector3(0, 0, rotationfactor);
                rotcenter.transform.DORotate(rot, 0.2f);
                yield return new WaitForSeconds(0.3f);
                for (int k = 0; k < ObjectPositions.Count; k++)
                {
                    selectData.LastSelected[k].gridIndex = ObjectPositions.Find(x => Vector3.Distance(x.Item2, selectData.LastSelected[k].transform.position) < 0.1f).Item1;
                }
                if (SliceControl())
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
        }
        rotate().InvokeIE();
        
        
       
    }

    public static implicit operator List<GameItem>(GameGrid a)
    {
        return a.items;
    }

    public GameItem this[int index]
    {
        get { return items.Find(x => x == index); }
    }

    public struct SelectData
    {
        public bool isSelected;
        public List<GameItem> LastSelected;
    }
}
