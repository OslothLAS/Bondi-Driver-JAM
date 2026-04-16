using UnityEngine;
using System.Collections.Generic;

public class ParadaSpawner : MonoBehaviour
{
    [Header("Configuracion de Distancia")]
    [Tooltip("Distancia minima desde el punto medio de los bondis (en metros)")]
    public float distanciaMinima = 600f;

    [Header("Referencias de Bondis")]
    public Transform bondi1;
    public Transform bondi2;

    [Header("Base de Datos de Paradas")]
    [SerializeField] private List<ParadaController> todasLasParadas = new List<ParadaController>();
    private List<ParadaController> paradasActivas = new List<ParadaController>();

    public List<ParadaController> ParadasActivas => paradasActivas;

    void Start()
    {
        // 1. BUSQUEDA AUTOMATICA DE BONDIS (Hardcoded por nombre como pediste)
        if (bondi1 == null)
        {
            GameObject b1 = GameObject.Find("Bondi_J1");
            if (b1 != null) bondi1 = b1.transform;
        }
        if (bondi2 == null)
        {
            GameObject b2 = GameObject.Find("Bondi_J2");
            if (b2 != null) bondi2 = b2.transform;
        }

        // 2. BUSQUEDA AUTOMATICA DE PARADAS POR TAG
        todasLasParadas.Clear();
        GameObject[] paradasEncontradas = GameObject.FindGameObjectsWithTag("Parada");

        foreach (GameObject obj in paradasEncontradas)
        {
            ParadaController controller = obj.GetComponent<ParadaController>();
            if (controller != null)
            {
                todasLasParadas.Add(controller);
                controller.spawner = this; // Vinculamos el spawner a la parada
            }
        }

        Debug.Log($"[Spawner] Inicializado. Bondis: {(bondi1 ? 1 : 0) + (bondi2 ? 1 : 0)} | Paradas: {todasLasParadas.Count}");

        // 3. LOGICA INICIAL
        OcultarTodasLasParadas(true);

        // Activamos el destino si existe
        ParadaController destino = ObtenerParadaDestino();
        if (destino != null) ActivarParada(destino);

        // Si no hay paradas activas, spawneamos la primera de pasajeros
        if (paradasActivas.Count == 0 || (paradasActivas.Count == 1 && destino != null))
        {
            ParadaController primera = ObtenerNuevaParada();
            if (primera != null) ActivarParada(primera);
        }
    }

    // --- CALCULO DEL PUNTO MEDIO ---
    public Vector3 GetBondisMidpoint()
    {
        if (bondi1 != null && bondi2 != null)
            return (bondi1.position + bondi2.position) / 2f;

        if (bondi1 != null) return bondi1.position;
        if (bondi2 != null) return bondi2.position;

        return Vector3.zero; // Fallback si no hay ningun bondi
    }

    public void OnParadaDesocupada(ParadaController parada, int capacidadRestante, bool fueCompletada)
    {
        if (parada == null || !fueCompletada) return;

        bool eraDestino = parada.esDestino;

        if (paradasActivas.Contains(parada))
        {
            parada.gameObject.SetActive(false);
            paradasActivas.Remove(parada);
        }

        // Si el bondi está lleno, priorizar destino (o mantenerlo activo)
        if (capacidadRestante <= 0)
        {
            ParadaController destino = ObtenerParadaDestino();
            if (destino != null) destino.gameObject.SetActive(true);
            return;
        }

        // Si terminó la parada y no era destino, buscamos la siguiente
        if (!eraDestino)
        {
            ParadaController nuevaParada = ObtenerNuevaParada();
            if (nuevaParada != null) ActivarParada(nuevaParada);
        }
    }

    ParadaController ObtenerNuevaParada()
    {
        if (todasLasParadas.Count == 0) return null;

        Vector3 puntoReferencia = GetBondisMidpoint();
        List<ParadaController> disponibles = new List<ParadaController>();

        foreach (var parada in todasLasParadas)
        {
            if (parada == null || paradasActivas.Contains(parada) || parada.esDestino)
                continue;

            float distancia = Vector3.Distance(puntoReferencia, parada.transform.position);

            if (distancia >= distanciaMinima)
                disponibles.Add(parada);
        }

        if (disponibles.Count == 0)
        {
            Debug.LogWarning("[Spawner] Ninguna parada a mas de " + distanciaMinima + "m del punto medio.");
            return null;
        }

        return disponibles[Random.Range(0, disponibles.Count)];
    }

    void ActivarParada(ParadaController parada)
    {
        if (paradasActivas.Contains(parada)) return;

        parada.gameObject.SetActive(true);
        paradasActivas.Add(parada);
        Debug.Log($"[Spawner] Activada: {parada.name}");
    }

    void OcultarTodasLasParadas(bool incluirDestino = true)
    {
        foreach (var parada in todasLasParadas)
        {
            if (parada == null) continue;
            if (parada.esDestino && !incluirDestino) continue;
            parada.gameObject.SetActive(false);
        }
        paradasActivas.Clear();
    }

    public ParadaController ObtenerParadaDestino()
    {
        foreach (var parada in todasLasParadas)
        {
            if (parada != null && parada.esDestino) return parada;
        }
        return null;
    }
}