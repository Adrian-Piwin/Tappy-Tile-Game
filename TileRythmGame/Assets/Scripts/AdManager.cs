using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour
{
    private string gameId;

    private bool testMode = true;

    private string placementId = "tappytilead";

    // Start is called before the first frame update
    IEnumerator Start()
    {
        if (Application.platform==RuntimePlatform.IPhonePlayer)
        {
            gameId = "3746006";
        }else
        {
            gameId = "3746007";
        }

        Advertisement.Initialize(gameId, testMode);

        while (!Advertisement.IsReady(placementId))
        {
            yield return null;
        }

        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Show(placementId);
    }

}
