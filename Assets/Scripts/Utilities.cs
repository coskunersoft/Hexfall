using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Utilities
{
    public static void InvokeIE(this IEnumerator enumerator)
    {
        if (!GameManager.instance) return;
        GameManager.instance.StartCoroutine(enumerator);
    }

    public static Vector3 CenterPositionofItems(this List<GameItem> gameItems)
    {
        Vector3 total = Vector3.zero;
        foreach (var item in gameItems)
        {
            total += item.transform.position;
        }
        return (total / gameItems.Count);
    }

    public static void SlideListOneStep<T>(this IList<T> list,bool down)
    {
        List<T> fake = list.ToList();
        if (down)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (i< list.Count-1)
                {
                    list[i + 1] = fake[i];
                }
                else
                {
                    list[0] = fake[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    list[i-1] = fake[i];
                }
                else
                {
                    list[list.Count-1] = fake[i];
                }
            }
        }
    }

    public static System.Func<GameItem, GameGrid, List<int>> PatternRule(this Pattern pattern) 
    {
        System.Func<GameItem, GameGrid, List<int>> mainresult = null;
        switch (pattern)
        {
            case Pattern.Diamond:
                mainresult = (GameItem selected, GameGrid grid) =>
                {
                    GameItem y = grid[selected + 1];
                    GameItem z = grid[selected + ((selected.gridIndex.x+1)%2==0?grid.row:grid.row+1)];
                    List<int> result = new List<int>() { selected, y, z };

                   
                    if (result.Any(ro => ro == -1)) result = new List<int>();
                    else if (selected.gridIndex.x != y.gridIndex.x) result = new List<int>();
                    else if (!(selected.ItemColor == y.ItemColor && y.ItemColor == z.ItemColor)) result = new List<int>();

                    if (result.Count<=0)
                    {
                        y = grid[selected + ((selected.gridIndex.x + 1) % 2 == 0 ? grid.row-1 : grid.row)];
                        z = grid[y + 1];
                        result = new List<int>() { selected, y, z };
                      
                        if (result.Any(ro => ro == -1)) result = new List<int>();
                        else if (y.gridIndex.x != z.gridIndex.x) result = new List<int>();
                        else if (!(selected.ItemColor == y.ItemColor && y.ItemColor == z.ItemColor)) result = new List<int>();

                    }

                    return result;
                };
                break;
            default:
                throw new System.Exception("Invalid pattern");
        }
        return mainresult;
    }
    
    public static System.Func<GameItem, GameGrid,Vector3,float, List<GameItem>> neighborRule(this Pattern pattern)
    {
        System.Func<GameItem, GameGrid,Vector3,float, List<GameItem>> mainresult = null;
        switch (pattern)
        {
            case Pattern.Diamond:
                mainresult = (GameItem selected, GameGrid grid,Vector3 clickpos,float mindis) =>
                {
                    List<List<GameItem>> Groups =new List<List<GameItem>>();
                    //right down
                    GameItem down=grid[selected-1];
                    GameItem right_1 = grid[selected + ((selected.gridIndex.x + 1) % 2 == 1 ? grid.row : grid.row - 1)];
                    //right up
                    GameItem up=grid[selected + 1];
                    GameItem right_2=grid[selected + ((selected.gridIndex.x + 1) % 2 == 1 ? grid.row+1 : grid.row)];
                    GameItem left_1=grid[selected - ((selected.gridIndex.x + 1) % 2 == 1 ? grid.row - 1 : grid.row)];
                    GameItem left_2 = grid[selected - ((selected.gridIndex.x + 1) % 2 == 1 ? grid.row : grid.row + 1)];
                    Groups.Add(new List<GameItem>() {selected,down,right_1});
                    Groups.Add(new List<GameItem>() { selected, down, left_2 });
                    Groups.Add(new List<GameItem>() { selected, up, right_2 });
                    Groups.Add(new List<GameItem>() { selected, up, left_1 });
                    Groups.Add(new List<GameItem>() { selected, left_1, left_2 });
                    Groups.Add(new List<GameItem>() { selected, right_2, right_1 });
                    Groups = Groups.FindAll(x => !x.Any(ro => ro == -1));
                    Groups.Sort((x, y) => Vector2.Distance(clickpos, x.CenterPositionofItems()).CompareTo(Vector2.Distance(clickpos, y.CenterPositionofItems())));
                    return Groups[0];
                };
                break;
            default:
                throw new System.Exception("Invalid pattern");
        }
        return mainresult;
    }

  
}
