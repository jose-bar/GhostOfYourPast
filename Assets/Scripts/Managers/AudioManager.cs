using UnityEngine;

public class SimpleAudioManager : MonoBehaviour
{
    public static SimpleAudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource sfxSource;

    [Header("Sound Effects")]
    public AudioClip buttonClickSound;
    public AudioClip itemPickupSound;
    public AudioClip doorOpenSound;
    public AudioClip puzzleCompleteSound;
    public AudioClip dayAdvanceSound;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayButtonClick()
    {
        PlaySound(buttonClickSound);
    }

    public void PlayItemPickup()
    {
        PlaySound(itemPickupSound);
    }

    public void PlayDoorOpen()
    {
        PlaySound(doorOpenSound);
    }

    public void PlayPuzzleComplete()
    {
        PlaySound(puzzleCompleteSound);
    }

    public void PlayDayAdvance()
    {
        PlaySound(dayAdvanceSound);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}