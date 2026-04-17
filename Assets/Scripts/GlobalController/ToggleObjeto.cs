using UnityEngine;

public class ToggleObjeto : MonoBehaviour
{
    public GameObject objetoATogglear;

    public void SwitchEstado()
    {
        if (objetoATogglear != null)
        {
            // La magia: ponemos el objeto en el estado OPUESTO al actual (!)
            objetoATogglear.SetActive(!objetoATogglear.activeSelf);
        }
    }
}