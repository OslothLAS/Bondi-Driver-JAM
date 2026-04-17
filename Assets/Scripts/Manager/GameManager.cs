using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Configuracion de Tiempo")]
    public float tiempoDePartidaMinutos = 1.5f;

    [Header("Referencias de Jugadores (Scripts)")]
    public PassengerController jugador1;
    public DamageController damageJ1;
    public PassengerController jugador2;
    public DamageController damageJ2;

    [Header("UI de Partida (Reloj)")]
    public TMP_Text timerUI;

    [Header("Marcadores Finales (TextMeshPro)")]
    public TMP_Text puntosFinalesJ1;
    public TMP_Text puntosFinalesJ2;

    [Header("Carteles/Paneles de Fin (Contienen los botones)")]
    public GameObject cartelGanadorJ1;
    public GameObject cartelGanadorJ2;
    public GameObject cartelEmpate;

    private float tiempoRestante;
    private bool juegoActivo = true;

    void Start()
    {
        // 1. Configuramos el tiempo inicial
        tiempoRestante = tiempoDePartidaMinutos * 60f;
        Time.timeScale = 1f; // Nos aseguramos que el juego NO empiece pausado

        // 2. SEGURIDAD: Desactivamos todos los carteles y botones al iniciar
        if (cartelGanadorJ1) cartelGanadorJ1.SetActive(false);
        if (cartelGanadorJ2) cartelGanadorJ2.SetActive(false);
        if (cartelEmpate) cartelEmpate.SetActive(false);


        // --- NUEVO: Desactivamos los textos de puntos finales ---
        if (puntosFinalesJ1 != null) puntosFinalesJ1.gameObject.SetActive(false);
        if (puntosFinalesJ2 != null) puntosFinalesJ2.gameObject.SetActive(false);

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

        int puntosJ1 = (jugador1 != null) ? jugador1.GetScore() : 0;
        int puntosJ2 = (jugador2 != null) ? jugador2.GetScore() : 0;
        int choquesJ1 = (damageJ1 != null) ? damageJ1.GetTotalChoques() : 0;
        int choquesJ2 = (damageJ2 != null) ? damageJ2.GetTotalChoques() : 0;

        // --- NUEVO: Activamos los objetos antes de escribir en ellos ---
        if (puntosFinalesJ1 != null)
        {
            puntosFinalesJ1.gameObject.SetActive(true);
            puntosFinalesJ1.text = $"Bondi 1: \nPuntos: {puntosJ1}\nChoques: {choquesJ1}";
        }

        if (puntosFinalesJ2 != null)
        {
            puntosFinalesJ2.gameObject.SetActive(true);
            puntosFinalesJ2.text = $"Bondi 2: \nPuntos: {puntosJ2}\nChoques: {choquesJ2}";
        }

        // Activación de los carteles de ganador (se mantiene igual)
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