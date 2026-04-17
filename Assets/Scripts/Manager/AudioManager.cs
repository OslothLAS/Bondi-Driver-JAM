using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource background;
    public AudioSource suspensionSound;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (background != null)
        {
            background.loop = true;
            background.Play();
        }
    }

    public void PlaySuspensionSound()
    {
        if (suspensionSound != null)
        {
            suspensionSound.Play();
        }
    }
}