using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private float volumeLvl = 0.2f;

    public static AudioClip tileClickedSound, lostSound, createSound;
    static AudioSource audioSource;

    void Start()
    {
        tileClickedSound = Resources.Load<AudioClip> ("tileclick");
        lostSound = Resources.Load<AudioClip> ("losssound");
        createSound = Resources.Load<AudioClip> ("creationsound");
        
        audioSource = GetComponent<AudioSource> ();
        audioSource.volume = volumeLvl;
    }

    public void playSound(string sound)
    {
        switch (sound)
        {
            case "click":
                audioSource.PlayOneShot(tileClickedSound);
                break;
            case "loss":
                audioSource.PlayOneShot(lostSound);
                break;
            case "creation":
                audioSource.PlayOneShot(createSound);
                break;
            default:
                break;
        }
        
    }


}
