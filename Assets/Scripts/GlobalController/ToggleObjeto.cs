using UnityEngine;

public class ToggleObjeto : MonoBehaviour
{
    public GameObject objetoATogglear;

    public void SwitchEstado()
    {
        if (objetoATogglear != null)
        {
            bool nuevoEstado = !objetoATogglear.activeSelf;
            objetoATogglear.SetActive(nuevoEstado);

            if (nuevoEstado)
            {
                AudioSource audio = objetoATogglear.GetComponent<AudioSource>();
                if (audio != null)
                {
                    audio.Play();
                }
            }
        }
    }
}