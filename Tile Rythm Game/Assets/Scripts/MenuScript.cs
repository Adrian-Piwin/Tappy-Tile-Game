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
    private CanvasGroup canvasGroup;

    void Start()
    {
        colorsSelected = normalBtn.colors;
        colorsUnselected = normalBtn.colors;
        colorsSelected.normalColor = new Color32(255, 255, 255, 100);
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
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
        //gameObject.SetActive(isMenu);
        StartCoroutine(animateMenu(!isMenu, 0.3f));
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

    IEnumerator animateMenu(bool isMenu, float duration)
    {
        Vector3 currentPosition = transform.position;
        float offset = 2f;
        float offsetMultiply;

        // fade from opaque to transparent
        if (isMenu)
        {   
            offsetMultiply = 0;
            // loop over 1 second backwards
            for (float i = 1; i >= 0; i -= Time.deltaTime/duration)
            {
                // set color with i as alpha
                canvasGroup.alpha = i;

                // offset menu y
                offsetMultiply += Time.deltaTime/duration;
                transform.position = transform.position + new Vector3(0, -1 * offset * offsetMultiply, 0);

                yield return null;
            }
        }
        // fade from transparent to opaque
        else
        {

            transform.position = transform.position + new Vector3(0, -1 * offset, 0);

            // loop over 1 second
            for (float i = 0; i <= 1; i += Time.deltaTime/duration)
            {
                // set color with i as alpha
                canvasGroup.alpha = i;

                // offset menu y
                transform.position = transform.position + new Vector3(0, offset * i, 0);

                yield return null;
            }
        }
    }

    

}
