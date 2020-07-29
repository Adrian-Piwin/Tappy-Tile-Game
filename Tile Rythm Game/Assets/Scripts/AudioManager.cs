using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private float volumeLvl = 0.2f;

    public static AudioClip tileClickedSound;
    static AudioSource audioSource;

    void Start()
    {
        tileClickedSound = Resources.Load<AudioClip> ("tileclick");
        audioSource = GetComponent<AudioSource> ();
        audioSource.volume = volumeLvl;
    }

    public void playTileClickedSound()
    {
        audioSource.PlayOneShot(tileClickedSound);
    }
}
