using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Configuracion de Tiempo")]
    public float tiempoDePartidaMinutos = 1.5f;

    [Header("Referencias de Jugadores")]
    public PassengerController jugador1;
    public PassengerController jugador2;

    [Header("UI de Partida")]
    public TMP_Text timerUI;

    [Header("Carteles de Fin de Juego")]
    public GameObject cartelGanadorJ1; // Arrastrá el panel/objeto del J1 aquí
    public GameObject cartelGanadorJ2; // Arrastrá el panel/objeto del J2 aquí
    public GameObject cartelEmpate;    // Arrastrá el panel de empate aquí

    private float tiempoRestante;
    private bool juegoActivo = true;

    void Start()
    {
        tiempoRestante = tiempoDePartidaMinutos * 60f;

        // Aseguramos que todos los carteles empiecen apagados
        if (cartelGanadorJ1) cartelGanadorJ1.SetActive(false);
        if (cartelGanadorJ2) cartelGanadorJ2.SetActive(false);
        if (cartelEmpate) cartelEmpate.SetActive(false);

        UpdateTimerUI();
    }

    void Update()
    {
        if (!juegoActivo) return;

        if (tiempoRestante > 0)
        {
            tiempoRestante -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            FinalizarPartida();
        }
    }

    void FinalizarPartida()
    {
        juegoActivo = false;
        tiempoRestante = 0;
        UpdateTimerUI();

        // Calculamos los puntajes
        int puntosJ1 = (jugador1 != null) ? jugador1.GetScore() : 0;
        int puntosJ2 = (jugador2 != null) ? jugador2.GetScore() : 0;

        // Activamos ÚNICAMENTE el cartel que corresponde
        if (puntosJ1 > puntosJ2)
        {
            if (cartelGanadorJ1) cartelGanadorJ1.SetActive(true);
        }
        else if (puntosJ2 > puntosJ1)
        {
            if (cartelGanadorJ2) cartelGanadorJ2.SetActive(true);
        }
        else
        {
            if (cartelEmpate) cartelEmpate.SetActive(true);
        }

        // Pausamos el tiempo de juego
        Time.timeScale = 0f;
    }

    void UpdateTimerUI()
    {
        if (timerUI == null) return;
        int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);
        timerUI.text = string.Format("{0:00}:{1:00}", minutos, segundos);
    }
}