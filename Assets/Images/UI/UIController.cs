using UnityEngine;

public class UIController : MonoBehaviour
{
    [Header("Referencia del Objeto")]
    public GameObject objetoObjetivo;

    // Se ejecuta al iniciar el juego
    void Start()
    {
        if (objetoObjetivo != null)
        {
            objetoObjetivo.SetActive(false); // Desactivación inicial
        }
    }

    // --- MÉTODOS PARA LOS BOTONES ---

    // Alternar: si está prendido lo apaga, si está apagado lo prende
    public void AlternarEstado()
    {
        if (objetoObjetivo != null)
        {
            objetoObjetivo.SetActive(!objetoObjetivo.activeSelf);
        }
    }

    // Fuerza el encendido
    public void ActivarObjeto()
    {
        if (objetoObjetivo != null) objetoObjetivo.SetActive(true);
    }

    // Fuerza el apagado
    public void DesactivarObjeto()
    {
        if (objetoObjetivo != null) objetoObjetivo.SetActive(false);
    }
}