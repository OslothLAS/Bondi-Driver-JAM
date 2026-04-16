using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Configuracion de Tiempo")]
    [Range(1f, 2.5f)]
    public float tiempoDePartidaMinutos = 1.5f;

    [Header("Referencias de Jugadores")]
    public PassengerController jugador1;
    public PassengerController jugador2;

    [Header("UI de Partida")]
    public TMP_Text timerUI;
    public TMP_Text winnerUI; // El nuevo texto para el ganador
    public GameObject panelFinDePartida;

    private float tiempoRestante;
    private bool juegoActivo = true;

    void Start()
    {
        tiempoRestante = tiempoDePartidaMinutos * 60f;
        if (panelFinDePartida != null) panelFinDePartida.SetActive(false);
        if (winnerUI != null) winnerUI.text = ""; // Empezamos sin ganador
        UpdateTimerUI();
    }

    void Update()
    {
        if (juegoActivo)
        {
            if (tiempoRestante > 0)
            {
                tiempoRestante -= Time.deltaTime;
                UpdateTimerUI();
            }
            else
            {
                tiempoRestante = 0;
                FinalizarPartida();
            }
        }
    }

    void FinalizarPartida()
    {
        juegoActivo = false;

        // --- LÓGICA DEL GANADOR ---
        if (winnerUI != null && jugador1 != null && jugador2 != null)
        {
            int puntosJ1 = jugador1.GetScore();
            int puntosJ2 = jugador2.GetScore();

            if (puntosJ1 > puntosJ2)
            {
                winnerUI.text = "GANADOR: JUGADOR 1";
                winnerUI.color = Color.cyan; // Color del Bondi_J1
            }
            else if (puntosJ2 > puntosJ1)
            {
                winnerUI.text = "GANADOR: JUGADOR 2";
                winnerUI.color = Color.green; // Color del Bondi_J2
            }
            else
            {
                winnerUI.text = "¡EMPATE TÉCNICO!";
                winnerUI.color = Color.white;
            }
        }

        if (panelFinDePartida != null) panelFinDePartida.SetActive(true);
        Time.timeScale = 0f;
    }

    void UpdateTimerUI()
    {
        if (timerUI != null)
        {
            int minutos = Mathf.FloorToInt(tiempoRestante / 60);
            int segundos = Mathf.FloorToInt(tiempoRestante % 60);
            timerUI.text = string.Format("{0:00}:{1:00}", minutos, segundos);
        }
    }
}