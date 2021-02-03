using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class GameItem : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    public GameObject focusObject;
    public Vector2 gridIndex;
    private int itemColor;
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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }


    private void Start()
    {
        FallDown();
    }
    public void FallDown()
    {
        Vector3 pos = transform.position;
        transform.position += (Vector3.up * 10);
        MoveNewPos(pos, 0.55f);
    }

    private void OnMouseUpAsButton()
    {
        GameManager.instance.runtimeVars.grid.ItemSelected(this);
    }

    public void FocusUnFocus(bool status)
    {
        focusObject.SetActive(status);
    }

    public static implicit operator int(GameItem itemx)
    {
        if (!itemx) return -1;
        return (int)((itemx.gridIndex.x * GameManager.instance.gameSettings.row) + itemx.gridIndex.y);
    }

    public void Slice()
    {
        Debug.Log("sliced");
        ParticleManager.instance.ShowBreakParticle(transform.position,spriteRenderer.color);
        this.PushToCamp();
        
    }
     
    public void MoveNewPos(Vector3 _pos,float _time)
    {
        transform.DOMove(_pos, _time).OnComplete(() =>
        {
            AudioManager.PlayOneShotAudio("itemhit");
        }).SetEase(Ease.Flash);
    }
}
