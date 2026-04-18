using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Carteles/Paneles de Fin")]
    public GameObject cartelGanadorJ1;
    public GameObject cartelGanadorJ2;
    public GameObject cartelEmpate;

    [Header("Boton Reiniciar")]
    public GameObject botonReiniciar;

    private float tiempoRestante;
    private bool juegoActivo = true;

    void Start()
    {
        tiempoRestante = tiempoDePartidaMinutos * 60f;
        Time.timeScale = 1f;

        if (cartelGanadorJ1) cartelGanadorJ1.SetActive(false);
        if (cartelGanadorJ2) cartelGanadorJ2.SetActive(false);
        if (cartelEmpate) cartelEmpate.SetActive(false);

        if (puntosFinalesJ1 != null) puntosFinalesJ1.gameObject.SetActive(false);
        if (puntosFinalesJ2 != null) puntosFinalesJ2.gameObject.SetActive(false);
        if (botonReiniciar != null) botonReiniciar.SetActive(false);

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

        // --- UI DE PUNTOS FINALES ---
        if (puntosFinalesJ1 != null)
        {
            puntosFinalesJ1.gameObject.SetActive(true);
            puntosFinalesJ1.text = $"Puntos: {puntosJ1}\nChoques: {choquesJ1}";
        }

        if (puntosFinalesJ2 != null)
        {
            puntosFinalesJ2.gameObject.SetActive(true);
            puntosFinalesJ2.text = $"Puntos: {puntosJ2}\nChoques: {choquesJ2}";
        }

        if (botonReiniciar != null) botonReiniciar.SetActive(true);

        // --- L�GICA DE GANADOR CON DESEMPATE ---
        if (puntosJ1 > puntosJ2)
        {
            cartelGanadorJ1.SetActive(true);
        }
        else if (puntosJ2 > puntosJ1)
        {
            cartelGanadorJ2.SetActive(true);
        }
        else // EMPATE EN PUNTOS: Desempate por menos choques
        {
            if (choquesJ1 < choquesJ2) // J1 tiene menos choques
            {
                if (cartelGanadorJ1) cartelGanadorJ1.SetActive(true);
                Debug.Log("Victoria para J1 por menos choques.");
            }
            else if (choquesJ2 < choquesJ1) // J2 tiene menos choques
            {
                if (cartelGanadorJ2) cartelGanadorJ2.SetActive(true);
                Debug.Log("Victoria para J2 por menos choques.");
            }
            else // Empate exacto en puntos Y choques
            {
                if (cartelEmpate) cartelEmpate.SetActive(true);
                Debug.Log("Empate absoluto.");
            }
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

    public void ReiniciarJuego()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}