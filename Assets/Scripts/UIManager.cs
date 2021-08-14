using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public Animator inGameMenuAnimator, pauseMenuAnimator, playerWonMenuAnimator, newGameMenuAnimator, mainMenuAnimator;
    public GameObject skipPlayerWonSeqButton, ContinueButton;
    public TextMeshProUGUI inGameScore, playerWonScore;

    private void Start()
    {
        HideEverything();
        ShowMainMenu(true);
    }

    public void ShowMainMenu(bool show)
    {
        mainMenuAnimator.SetBool("Show", show);
    }

    public void Pause(bool paused)
    {
        GameManager.current.playing = !paused;
        pauseMenuAnimator.SetBool("Show", paused);
        ShowInGameScreen(!paused);
    }

    public void ShowInGameScreen(bool show)
    {
        inGameMenuAnimator.SetBool("Playing", show);
    }

    public void PlayerWon()
    {
        HideEverything();
        skipPlayerWonSeqButton.SetActive(true);
    }

    public void ShowPlayerWonMenu(bool show)
    {
        playerWonMenuAnimator.SetBool("Show", show);
    }

    public void HideEverything()
    {
        ShowInGameScreen(false);
        pauseMenuAnimator.SetBool("Show", false);
        ShowPlayerWonMenu(false);
        skipPlayerWonSeqButton.SetActive(false);
        ShowMainMenu(false);
        ShowNewGameMenu(false);
    }

    public void UpdateScore(int Score, int moves)
    {
        inGameScore.text = "Score: " + Score + "\nMoves: " + moves;
        playerWonScore.text = "Score: " + Score + "  Moves: " + moves;
    }

    public void ShowNewGameMenu(bool show)
    {
        newGameMenuAnimator.SetBool("Show", show);
    }

    public void ShowContinueButton(bool Show)
    {
        ContinueButton.SetActive(Show);
    }
}
