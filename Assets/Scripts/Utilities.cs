using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Utilities
{
    /// <summary>
    /// Runs enumerator 
    /// </summary>
    public static void InvokeIE(this IEnumerator enumerator)
    {
        if (!GameManager.instance) return;
        GameManager.instance.StartCoroutine(enumerator);
    }
    /// <summary>
    /// Finds the center of the game object group 
    /// </summary>
    public static Vector3 CenterPositionofItems(this List<GameItem> gameItems)
    {
        Vector3 total = Vector3.zero;
        foreach (var item in gameItems)
        {
            total += item.transform.position;
        }
        return (total / gameItems.Count);
    }
    /// <summary>
    /// Shifts the elements of the list one row
    /// </summary>
    /// <param name="down">Scroll direction </param>
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

    /// <summary>
    /// Determines the number of common elements by comparing the elements of two lists 
    /// </summary>
    public static int MatchCount(this List<int> list,List<int> Other)
    {
        return list.Count(x => Other.Any(y => y==x));
    }
    /// <summary>
    /// Combines two lists without taking their common elements 
    /// </summary>
    public static List<int> CombineDiff(this List<int> list, List<int> Other)
    {
        List<int> result = new List<int>();
        foreach (var item in list)
        {
            result.Add(item);
        }
        foreach (var item in Other)
        {
            if (!result.Any(ro => ro==item)) result.Add(item);
        }
        return result;
    }
    /// <summary>
    /// Pulls an object back into the pool 
    /// </summary>
    public static void PushToCamp<T>(this T o)where T:Object
    {
        ObjectCamp.instance.TakeObjecy(o);
    }
    /// <summary>
    /// Determines random number  between two ranges with ignore list
    /// </summary>
    public static int RandomintegerWithIgnore(int start,int end,List<int> ignore)
    {
        List<int> numbers = new List<int>();
        for (int i = start; i < end; i++)
        {
            numbers.Add(i);
        }
        numbers.RemoveAll(x => ignore.Any(ro => ro == x));
        return numbers[Random.Range(0, numbers.Count)];
    }

    /// <summary>
    /// Clear allsub items
    /// </summary>
    public static void ClearAllSubItems(this Transform t)
    {
        foreach (Transform item in t)
        {
            GameObject.Destroy(item.gameObject);
        }
    }

    /// <summary>
    /// Returns grouping rule according to the pattern selected during the object destruction check
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
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

                    List<int> result2 = new List<int>();
                    GameItem w = grid[selected + ((selected.gridIndex.x + 1) % 2 == 0 ? grid.row-1 : grid.row)];
                    GameItem q = grid[w + 1];
                    result2 = new List<int>() { selected, w, q };
                      
                    if (result2.Any(ro => ro == -1)) result2 = new List<int>();
                    else if (w.gridIndex.x != q.gridIndex.x) result2 = new List<int>();
                    else if (!(selected.ItemColor == w.ItemColor && w.ItemColor == q.ItemColor)) result2 = new List<int>();

                    result=result.CombineDiff(result2);
                    return result;
                };
                break;
            default:
                throw new System.Exception("Invalid pattern");
        }
        return mainresult;
    }
    /// <summary>
    /// Returns the selection rule according to the selected pattern during object selection.
    /// </summary>
    public static System.Func<GameItem, GameGrid,Vector3,float, List<GameItem>> NeighborRule(this Pattern pattern)
    {
        System.Func<GameItem, GameGrid,Vector3,float, List<GameItem>> mainresult = null;
        switch (pattern)
        {
            case Pattern.Diamond:
                mainresult = (GameItem selected, GameGrid grid,Vector3 clickpos,float mindis) =>
                {
                    var allNeighborRule = AllNeighborsRule(pattern);
                    List<List<GameItem>> Groups = allNeighborRule(selected, grid);
                    Groups.Sort((x, y) => Vector2.Distance(clickpos, x.CenterPositionofItems()).CompareTo(Vector2.Distance(clickpos, y.CenterPositionofItems())));
                    return Groups[0];
                };
                break;
            default:
                throw new System.Exception("Invalid pattern");
        }
        return mainresult;
    }
    /// <summary>
    /// Returns the selection rule according to the selected pattern during object selection.
    /// </summary>
    public static System.Func<GameItem, GameGrid,List<List<GameItem>>> AllNeighborsRule(this Pattern pattern)
    {
        System.Func<GameItem, GameGrid, List<List<GameItem>>> mainresult = null;
        switch (pattern)
        {
            case Pattern.Diamond:
                mainresult = (GameItem selected, GameGrid grid) =>
                {
                    List<List<GameItem>> Groups = new List<List<GameItem>>();
                    //right down
                    GameItem down = grid[selected - 1];
                    GameItem right_1 = grid[selected + ((selected.gridIndex.x + 1) % 2 == 1 ? grid.row : grid.row - 1)];
                    //right up
                    GameItem up = grid[selected + 1];
                    GameItem right_2 = grid[selected + ((selected.gridIndex.x + 1) % 2 == 1 ? grid.row + 1 : grid.row)];
                    GameItem left_1 = grid[selected - ((selected.gridIndex.x + 1) % 2 == 1 ? grid.row - 1 : grid.row)];
                    GameItem left_2 = grid[selected - ((selected.gridIndex.x + 1) % 2 == 1 ? grid.row : grid.row + 1)];
                    Groups.Add(new List<GameItem>() { selected, down, right_1 });
                    Groups.Add(new List<GameItem>() { selected, down, left_2 });
                    Groups.Add(new List<GameItem>() { selected, up, right_2 });
                    Groups.Add(new List<GameItem>() { selected, up, left_1 });
                    Groups.Add(new List<GameItem>() { selected, left_1, left_2 });
                    Groups.Add(new List<GameItem>() { selected, right_2, right_1 });
                    Groups = Groups.FindAll(x => !x.Any(ro => ro == -1));
                    float idealdistance = Mathf.Sqrt(GameManager.instance.runtimeVars.movementMultiperx * GameManager.instance.runtimeVars.movementMultipery);
                    Groups = Groups.FindAll(x =>
                    {
                        Vector3 center = x.CenterPositionofItems();
                        return !x.Any(ro => Vector3.Distance(ro.transform.position,center)>idealdistance);
                    });
                    return Groups;
                };
                break;
            default:
                throw new System.Exception("Invalid pattern");
        }
        return mainresult;
    }

}
