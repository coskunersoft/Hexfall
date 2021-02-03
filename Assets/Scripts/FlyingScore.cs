using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class FlyingScore : MonoBehaviour
{
    public TextMeshPro textComponent;

    private void Fly()
    {
        Color x = textComponent.color;
        x.a = 1;
        textComponent.color = x;
        x.a = 1;
        DOTween.To(() => x.a, ro => x.a = ro, 0, 3).OnUpdate(() =>
        {
            textComponent.color = x;
        });
        transform.DOMove(transform.position + (Vector3.up * GameManager.instance.runtimeVars.movementMultipery)*3, 3).OnComplete(() =>
        {
            this.PushToCamp();
        });
    }
    

    public static FlyingScore operator+(FlyingScore x, string y)
    {
        x.textComponent.text = y;
        x.Fly();
        return x;
    } 
}
