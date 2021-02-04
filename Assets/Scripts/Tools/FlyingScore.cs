using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class FlyingScore : MonoBehaviour
{
    public TextMeshPro textComponent;

    /// <summary>
    /// Timed, move up and opacity reduction function made to give an effective air to score effects
    /// </summary>
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

    /// <summary>
    /// Customized operator for easier text writing to Textmesh component 
    /// </summary>
    public static FlyingScore operator+(FlyingScore x, string y)
    {
        x.textComponent.text = y;
        x.Fly();
        return x;
    } 
}
