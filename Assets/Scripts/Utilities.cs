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
    

}
