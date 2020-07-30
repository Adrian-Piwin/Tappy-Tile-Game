using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{

    private bool newBestTimeBool = false;
    private bool normalSelected = false;

    [SerializeField]
    private GameObject newBestTitle;
    [SerializeField]
    private Text scoreTxt;
    [SerializeField]
    private Text highscoreTxt;
    [SerializeField]
    private Button normalBtn;
    [SerializeField]
    private Button hardBtn;

    private ColorBlock colorsSelected, colorsUnselected;

    void Start()
    {
        colorsSelected = normalBtn.colors;
        colorsUnselected = normalBtn.colors;
        colorsSelected.normalColor = new Color32(255, 255, 255, 100);
        swapBtns("normal");
    }

    public void updateScore(int newScore)
    {
        scoreTxt.text = "" + newScore;
    }

    public void updateBestTime(int newScore)
    {
        highscoreTxt.text = "" + newScore;
    }

    public void toggleNewBestTime(bool isNewBest)
    {
        newBestTitle.SetActive(isNewBest);
    }

    public IEnumerator toggleMenu(bool isMenu, float timer)
    {
        yield return new WaitForSeconds(timer);
        gameObject.SetActive(isMenu);
    }

    public void swapBtns(string btn)
    {
        if (btn == "normal"){
            if (!normalSelected)
            {
                normalBtn.colors = colorsSelected;
                hardBtn.colors = colorsUnselected;
                normalSelected = !normalSelected;
                toggleNewBestTime(false);
            }
        }else if (btn == "hard"){
            if (normalSelected)
            {
                hardBtn.colors = colorsSelected;
                normalBtn.colors = colorsUnselected;
                normalSelected = !normalSelected;
                toggleNewBestTime(false);
            }
        }
    }

    

}
