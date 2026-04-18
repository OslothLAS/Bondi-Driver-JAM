using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [Header("Referencias 2 Jugadores")]
    public GameObject canvas2J;
    public GameObject camaras2J;
    public GameObject bondis2J;

    [Header("Referencias 4 Jugadores")]
    public GameObject canvas4J;
    public GameObject camaras4J;
    public GameObject bondis4J;

    void Awake()
    {
        // Leemos la elección que se hizo en el menú
        bool esModo4J = (MenuController.modoJugadores == 4);

        // Activamos/Desactivamos según corresponda
        if (canvas2J) canvas2J.SetActive(!esModo4J);
        if (camaras2J) camaras2J.SetActive(!esModo4J);
        if (bondis2J) bondis2J.SetActive(!esModo4J);

        if (canvas4J) canvas4J.SetActive(esModo4J);
        if (camaras4J) camaras4J.SetActive(esModo4J);
        if (bondis4J) bondis4J.SetActive(esModo4J);

        Debug.Log($"[Sistemas] Escena configurada para {MenuController.modoJugadores} jugadores.");
    }
}