using UnityEngine;
using UnityEngine.UI;

public class MuteBtnController : MonoBehaviour
{
    public Sprite iconoSonido;
    public Sprite iconoMute;
    public AudioSource audioSource;

    private Image imagen;
    private bool estaMuteado = false;

    void Start()
    {
        imagen = GetComponent<Image>();
        if (imagen != null && iconoMute != null)
        {
            imagen.sprite = iconoMute;
            estaMuteado = true;
        }
    }

    public void ToggleSonido()
    {
        if (audioSource != null)
        {
            audioSource.mute = !audioSource.mute;
        }

        estaMuteado = audioSource != null ? audioSource.mute : !estaMuteado;

        if (imagen != null)
        {
            imagen.sprite = estaMuteado ? iconoMute : iconoSonido;
        }
    }
}
