using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    public AudioSource popSound;
    public AudioSource backgroundSound;
    private GameObject lastButton;

    void Start()
    {
        if (backgroundSound != null)
        {
            backgroundSound.loop = true;
            backgroundSound.Play();
        }
    }

    void Update()
    {
        if (EventSystem.current == null) return;

        GameObject currentObject = EventSystem.current.currentSelectedGameObject;
        bool isOverButton = currentObject != null && currentObject.GetComponent<UnityEngine.UI.Button>() != null;

        if (isOverButton && currentObject != lastButton && popSound != null)
        {
            popSound.Play();
        }

        lastButton = currentObject;
    }

    public void Jugar()
    {
        SceneManager.LoadScene("SampleScene");
    }

    public void Creditos()
    {
        SceneManager.LoadScene("CreditsScene");
    }

    public void Volver()
    {
        SceneManager.LoadScene("MainMenuScene");
    }
}