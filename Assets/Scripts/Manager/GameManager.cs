using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("Configuracion de Tiempo")]
    public float tiempoDePartidaMinutos = 1.5f;

    [Header("Referencias MODO 2 JUGADORES")]
    public PassengerController j1_2J;
    public DamageController damageJ1_2J;
    public PassengerController j2_2J;
    public DamageController damageJ2_2J;

    [Header("Referencias MODO 4 JUGADORES")]
    public PassengerController j1_4J;
    public DamageController damageJ1_4J;
    public PassengerController j2_4J;
    public DamageController damageJ2_4J;
    public PassengerController j3_4J;
    public DamageController damageJ3_4J;
    public PassengerController j4_4J;
    public DamageController damageJ4_4J;

    [Header("UI de Partida (Relojes)")]
    public TMP_Text timerUI;
    public TMP_Text timerUI2;

    [Header("Marcadores Finales 2 JUGADORES")]
    public TMP_Text puntosFinalesJ1_2J;
    public TMP_Text puntosFinalesJ2_2J;

    [Header("Marcadores Finales 4 JUGADORES")]
    public TMP_Text puntosFinalesJ1_4J;
    public TMP_Text puntosFinalesJ2_4J;
    public TMP_Text puntosFinalesJ3_4J;
    public TMP_Text puntosFinalesJ4_4J;

    [Header("Carteles Victoria 2 JUGADORES")]
    public GameObject cartelGanadorJ1_2J;
    public GameObject cartelGanadorJ2_2J;

    [Header("Carteles Victoria 4 JUGADORES")]
    public GameObject cartelGanadorJ1_4J;
    public GameObject cartelGanadorJ2_4J;
    public GameObject cartelGanadorJ3_4J;
    public GameObject cartelGanadorJ4_4J;

    [Header("Otros")]
    public GameObject cartelEmpate;

    private float tiempoRestante;
    private bool juegoActivo = true;

    void Start()
    {
        // --- LÓGICA DE TIEMPO AUTOMÁTICA ---
        if (MenuController.modoJugadores == 4)
        {
            tiempoDePartidaMinutos = 5f;
            Debug.Log("[Sistemas] Modo 4 Jugadores: Partida de 5 minutos.");
        }
        else
        {
            tiempoDePartidaMinutos = 3f;
            Debug.Log("[Sistemas] Modo 2 Jugadores: Partida de 3 minutos.");
        }

        tiempoRestante = tiempoDePartidaMinutos * 60f;
        Time.timeScale = 1f;

        DesactivarTodoAlInicio();
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

        bool esModo4J = (MenuController.modoJugadores == 4);
        List<PlayerData> resultados = new List<PlayerData>();

        // --- CARGA DE DATOS SEGÚN EL MODO SELECCIONADO ---
        if (esModo4J)
        {
            AgregarDatosJugador(resultados, j1_4J, damageJ1_4J, 1, puntosFinalesJ1_4J);
            AgregarDatosJugador(resultados, j2_4J, damageJ2_4J, 2, puntosFinalesJ2_4J);
            AgregarDatosJugador(resultados, j3_4J, damageJ3_4J, 3, puntosFinalesJ3_4J);
            AgregarDatosJugador(resultados, j4_4J, damageJ4_4J, 4, puntosFinalesJ4_4J);
        }
        else
        {
            AgregarDatosJugador(resultados, j1_2J, damageJ1_2J, 1, puntosFinalesJ1_2J);
            AgregarDatosJugador(resultados, j2_2J, damageJ2_2J, 2, puntosFinalesJ2_2J);
        }

        if (resultados.Count == 0) return;

        // Ranking con triple criterio: Puntos -> Choques -> Pasajeros a bordo
        var ranking = resultados.OrderByDescending(r => r.puntos)
                                .ThenBy(r => r.choques)
                                .ThenByDescending(r => r.pasajerosActuales)
                                .ToList();

        bool empateAbsoluto = ranking.Count > 1 &&
                             ranking[0].puntos == ranking[1].puntos &&
                             ranking[0].choques == ranking[1].choques &&
                             ranking[0].pasajerosActuales == ranking[1].pasajerosActuales;

        if (empateAbsoluto)
        {
            if (cartelEmpate) cartelEmpate.SetActive(true);
        }
        else
        {
            ActivarCartelGanador(ranking[0].id);
        }

        Time.timeScale = 0f;
    }

    void ActivarCartelGanador(int idGanador)
    {
        bool esModo4J = (MenuController.modoJugadores == 4);

        if (esModo4J)
        {
            if (idGanador == 1) cartelGanadorJ1_4J.SetActive(true);
            else if (idGanador == 2) cartelGanadorJ2_4J.SetActive(true);
            else if (idGanador == 3) cartelGanadorJ3_4J.SetActive(true);
            else if (idGanador == 4) cartelGanadorJ4_4J.SetActive(true);
        }
        else
        {
            if (idGanador == 1) cartelGanadorJ1_2J.SetActive(true);
            else if (idGanador == 2) cartelGanadorJ2_2J.SetActive(true);
        }
    }

    void AgregarDatosJugador(List<PlayerData> lista, PassengerController pc, DamageController dc, int id, TMP_Text uiPuntos)
    {
        if (pc == null) return;

        int pts = pc.GetScore();
        int chq = (dc != null) ? dc.GetTotalChoques() : 0;
        int pasajerosArriba = pc.GetCurrentPassengers();

        if (uiPuntos != null)
        {
            uiPuntos.gameObject.SetActive(true);
            uiPuntos.text = $"Puntos: {pts}\nChoques: {chq}\nCarga: {pasajerosArriba}";
        }

        lista.Add(new PlayerData { id = id, puntos = pts, choques = chq, pasajerosActuales = pasajerosArriba });
    }

    void UpdateTimerUI()
    {
        int minutos = Mathf.FloorToInt(tiempoRestante / 60);
        int segundos = Mathf.FloorToInt(tiempoRestante % 60);
        string tiempoTexto = string.Format("{0:00}:{1:00}", minutos, segundos);

        if (timerUI != null) timerUI.text = tiempoTexto;
        if (timerUI2 != null) timerUI2.text = tiempoTexto;
    }

    void DesactivarTodoAlInicio()
    {
        GameObject[] carteles = {
            cartelGanadorJ1_2J, cartelGanadorJ2_2J,
            cartelGanadorJ1_4J, cartelGanadorJ2_4J, cartelGanadorJ3_4J, cartelGanadorJ4_4J,
            cartelEmpate
        };
        foreach (var c in carteles) if (c) c.SetActive(false);

        TMP_Text[] textos = {
            puntosFinalesJ1_2J, puntosFinalesJ2_2J,
            puntosFinalesJ1_4J, puntosFinalesJ2_4J, puntosFinalesJ3_4J, puntosFinalesJ4_4J
        };
        foreach (var t in textos) if (t) t.gameObject.SetActive(false);
    }

    private class PlayerData
    {
        public int id;
        public int puntos;
        public int choques;
        public int pasajerosActuales;
    }
}