using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameGUIManager : Singleton<GameGUIManager>
{
    public GameObject homeGui;
    public GameObject gameGui;

    public Dialog pauseDialog;
    public Dialog gameoverDialog;

    Dialog m_curDialog;

    public Dialog CurDialog { get => m_curDialog; set => m_curDialog = value; }

    public override void Awake(){
        MakeSingleton(false);
    }

    public void ShowGameGui(bool isShow){
        if(gameGui)
            gameGui.SetActive(isShow);
        

        if(homeGui)
            homeGui.SetActive(!isShow);
        
    }

    public void PauseGame(){
        if(pauseDialog){
            pauseDialog.Show(true);
            m_curDialog=pauseDialog;
        }
    }

    public void GameoverDialog(){
        if(gameoverDialog){
            gameoverDialog.Show(true);
            m_curDialog=gameoverDialog;
        }
    }

    public void ResumeGame(){
        if(m_curDialog)
            m_curDialog.Show(false);
    }

    public void Replay(){
        if(m_curDialog)
            m_curDialog.Show(false);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame(){
        ResumeGame();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        Application.Quit();
    }
}
