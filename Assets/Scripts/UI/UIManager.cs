using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    private void Awake()
    {
        instance = this;
    }

    [Header("Hud")]
    public Text scoreLabel;
    public Text highScoreLabel;
    public Text movesCountLabel;
    public Text undoRightLabel;
    public GameObject undoButton;

    [Header("Menu")]
    public Animator menuAnimator;
    public GameObject menuContainer;

   
    [Header("Dialog Menu")]
    public GameObject dialogWindowContainer;
    public Text dialogWindowTitleLabel;
    public Text dialogWindowDescLabel;
    public Transform dialogWindowButtonContainer;
    public GameObject dialogWindowButtonPrefab;
   
    public void SyncScoreFields()
    {
        scoreLabel.text = GameManager.instance.runtimeVars.score.ToString();
        highScoreLabel.text ="High Score :"+ GameManager.instance.runtimeVars.highScore.ToString();
    }
    public void SyncMovesField()
    {
        movesCountLabel.text= GameManager.instance.runtimeVars.movesCount.ToString();
    }
    public void SyncUndoRigth()
    {
        undoRightLabel.text = GameManager.instance.runtimeVars.undoRight.ToString();
    }

    public void ShowHideMenu()
    {
        AudioManager.PlayOneShotAudio("click");
        bool status = menuContainer.activeInHierarchy;
        status = !status;
        GameManager.instance.runtimeVars.isPaused = status;
        menuContainer.SetActive(status);
        menuAnimator.SetTrigger(status?"open":"close");
    }

    public void ShowHideUndoButton(bool Status)
    {
        undoButton.SetActive(Status);
    }

    public void ShowDialogWindow(string title,string desc,params DWButtonData[] buttons)
    {
        dialogWindowContainer.SetActive(true);
        dialogWindowTitleLabel.text = title;
        dialogWindowDescLabel.text = desc;
        dialogWindowButtonContainer.ClearAllSubItems();
        foreach (var item in buttons)
        {
            UIDialogButton createdButton = Instantiate(dialogWindowButtonPrefab, dialogWindowButtonContainer)
                .GetComponent<UIDialogButton>();
            createdButton.Setup(item);
        }
    }
    public void HideDialogWindow()
    {
        dialogWindowContainer.SetActive(false);
    }

    public void MenuButtonClicks(int buttonID)
    {
        switch (buttonID)
        {
            case 0:
                //New Game
                ShowHideMenu();
                GameManager.instance.runtimeVars.isPaused = true;
                ShowDialogWindow("Are you sure?", "You will lose current game",
                    new DWButtonData("Yes", () =>
                    {
                        GameManager.instance.RestartGame();
                        
                    }),
                    new DWButtonData("No", () =>
                    {
                        GameManager.instance.runtimeVars.isPaused = false;
                       
                    }));
                break;
            case 1:
                //Leaderboard

                break;
            case 2:
                //Settings

                break;
            case 3:
                //Exit
                Application.Quit();
                break;
        }
    }

    /// <summary>
    /// Button profile for dialog window 
    /// </summary>
    public class DWButtonData
    {
        public DWButtonData(string _title,UnityAction _onClick)
        {
            title = _title;
            onClick = _onClick;
        }
        public string title;
        public UnityAction onClick;
    }
}
