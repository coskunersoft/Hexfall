using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class GameItem : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    public GameObject focusObject;
    public GameObject starObject;
    public Vector2 gridIndex;
    private int itemColor;
    /// <summary>
    /// Easy change colorindex and appearance color by assigning the sequence number of the colors in the color pool, 
    /// </summary>
    public int ItemColor
    {
        get { return itemColor; }
        set
        {
            if (value > GameManager.instance.gameSettings.colorScale.Length && value < 0)
                throw new System.Exception("Item color out of range from color scale");
            if (spriteRenderer) spriteRenderer.color = GameManager.instance.gameSettings.colorScale[value];
            itemColor = value;
        }
    }
    public bool HaveStar
    {
        get { return starObject.activeInHierarchy; }
    }

    public int bombCounter = -1;
    public GameObject bombContainer;
    public TMPro.TextMeshPro bombText;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
       
    }

    private void Start()
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    public void ShowHideStar(bool status)
    {
        starObject.SetActive(status);
    }
    /// <summary>
    /// Process of smooth dropping items from top to bottom 
    /// </summary>
    public void FallDown()
    {
        ShowHideStar(Random.Range(0, 100) <= GameManager.instance.gameSettings.starItemFrequency&&bombCounter==-1);
        Vector3 pos = transform.position;
        transform.position += (Vector3.up * 10);
        MoveNewPos(pos, 0.55f);
    }
    /// <summary>
    /// Detect physical click with the item 
    /// </summary>
    private void OnMouseUpAsButton()
    {
        if (GameManager.instance.runtimeVars.isPaused) return;
        GameManager.instance.runtimeVars.grid.ItemSelected(this);
    }
    /// <summary>
    /// Showing and hiding of white areas around the item during click, click cancellation
    /// </summary>
    /// <param name="status"></param>
    public void FocusUnFocus(bool status)
    {
        focusObject.SetActive(status);
    }
    /// <summary>
    /// Actions for object destruction 
    /// </summary>
    public void Destruction()
    {
        Debug.Log("Destroyed Game Item");
        ParticleManager.instance.ShowBreakParticle(transform.position,spriteRenderer.color);
        this.PushToCamp();
        
    }
    /// <summary>
    /// Item movement function with time adjustment 
    /// </summary>
    public void MoveNewPos(Vector3 _pos,float _time)
    {
        transform.DOMove(_pos, _time).OnComplete(() =>
        {
            AudioManager.PlayOneShotAudio("itemhit");
        }).SetEase(Ease.Flash);
    }
    /// <summary>
    /// Item will be reshaped when called
    /// </summary>
    public void Restore(GridItemTemp gridItemTemp)
    {
        transform.DOScale(transform.localScale / 10, Random.Range(0.1f, 0.3f)).OnComplete(() =>
        {
            transform.DOScale(transform.localScale * 10, Random.Range(0.1f, 0.3f));
            ShowHideStar(gridItemTemp.havestar);
            SetBombStyle(gridItemTemp.bombcounter >-1,gridItemTemp.bombcounter);
            this.ItemColor = gridItemTemp.color;
        });
    }
    /// <summary>
    /// Make it bomb or normal 
    /// </summary>
    public void SetBombStyle(bool status,int start=-1)
    {
        if (status)
        {
            ShowHideStar(false);
            bombContainer.SetActive(true);
            bombCounter = start;
            SyncBombText();
        }
        else
        {
            bombCounter = -1;
            bombContainer.SetActive(false);
        }
    }
    /// <summary>
    /// Updates bomb text
    /// </summary>
    public void SyncBombText()
    {
        bombText.text = bombCounter.ToString();
        bombText.transform.DOScale(bombText.transform.localScale * 1.5f, 0.2F).OnComplete(() =>
        {
            bombText.transform.DOScale(bombText.transform.localScale / 1.5f, 0.2F);
        });
    }
    /// <summary>
    /// Minus bomb exposion count
    /// </summary>
    public bool MinusBomb()
    {
        if (bombCounter <= -1) return false;
        bombCounter--;
        SyncBombText();
        return bombCounter <= 0;
    }


    /// <summary>
    /// Custom operator for easy access to order number of items in grid
    /// </summary>
    /// <param name="itemx"></param>
    public static implicit operator int(GameItem itemx)
    {
        if (!itemx) return -1;
        return (int)((itemx.gridIndex.x * GameManager.instance.gameSettings.row) + itemx.gridIndex.y);
    }

    
}
