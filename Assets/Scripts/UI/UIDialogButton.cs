using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIDialogButton : MonoBehaviour
{
    public Text buttonLabel;
    public Button button;

    public void Setup(UIManager.DWButtonData dWButtonData)
    {
        buttonLabel.text = dWButtonData.title;
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(dWButtonData.onClick);
        button.onClick.AddListener(UIManager.instance.HideDialogWindow);
        
    }
}
