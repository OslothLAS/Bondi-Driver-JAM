using UnityEngine;
using System.Collections.Generic;

public class ParadaSpawner : MonoBehaviour
{
    [Header("Configuracion de Distancia")]
    [Tooltip("Distancia minima desde el Bondi para que aparezca una parada (en metros)")]
    public float distanciaMinima = 600f;

    [Header("Referencias")]
    public Transform bondi;
    public List<ParadaController> todasLasParadas = new List<ParadaController>();

    private List<ParadaController> paradasActivas = new List<ParadaController>();

    public List<ParadaController> ParadasActivas => paradasActivas;

    void Start()
    {
        if (bondi == null)
        {
            GameObject bondiObj = GameObject.FindGameObjectWithTag("Player");
            if (bondiObj != null)
                bondi = bondiObj.transform;
        }

        Debug.Log($"ParadaSpawner Start - Paradas encontradas: {todasLasParadas.Count}");
        
        ParadaController destino = ObtenerParadaDestino();
        if (destino != null)
        {
            ActivarParada(destino);
        }
    }

    public void OnParadaDesocupada(ParadaController parada, int capacidadRestante, bool fueCompletada)
    {
        if (parada == null)
        {
            Debug.LogWarning("ParadaSpawner: parada es null");
            return;
        }

        Debug.Log($"OnParadaDesocupada: {parada.name}, capacidad: {capacidadRestante}, completada: {fueCompletada}");
        Debug.Log($"Paradas activas antes: {paradasActivas.Count}");

        bool eraDestino = parada.esDestino;

        bool estabaEnLista = paradasActivas.Contains(parada);

        if (estabaEnLista)
        {
            parada.gameObject.SetActive(false);
            paradasActivas.Remove(parada);
            Debug.Log($"Parada {parada.name} removida de activas");
        }
        else
        {
            Debug.LogWarning($"Parada {parada.name} NO estaba en lista de activas, forzando desactivacion");
            parada.gameObject.SetActive(false);
        }

        if (capacidadRestante <= 0)
        {
            Debug.Log("ParadaSpawner: Bus lleno, ocultando paradas y mostrando destino");
            ParadaController destino = ObtenerParadaDestino();
            if (destino != null)
            {
                destino.gameObject.SetActive(true);
            }
            return;
        }

        if (paradasActivas.Count > 0 && !fueCompletada)
        {
            Debug.Log("ParadaSpawner: Ya hay una parada activa, no se spawnea otra");
            return;
        }

        ParadaController destinoActual = ObtenerParadaDestino();
        if (destinoActual != null)
        {
            destinoActual.gameObject.SetActive(false);
        }

        if (!eraDestino)
        {
            ParadaController nuevaParada = ObtenerNuevaParada();
            if (nuevaParada != null)
            {
                ActivarParada(nuevaParada);
            }
        }
    }

    ParadaController ObtenerNuevaParada()
    {
        if (todasLasParadas.Count == 0 || bondi == null)
        {
            Debug.LogWarning("ParadaSpawner: No hay paradas o no hay bondi");
            return null;
        }

        List<ParadaController> disponibles = new List<ParadaController>();

        foreach (var parada in todasLasParadas)
        {
            if (parada == null || paradasActivas.Contains(parada))
                continue;

            float distancia = Vector3.Distance(bondi.position, parada.transform.position);
            Debug.Log($"Parada {parada.name}: distancia {distancia}m (min: {distanciaMinima}m)");
            if (distancia >= distanciaMinima)
            {
                disponibles.Add(parada);
            }
        }

        if (disponibles.Count == 0)
        {
            Debug.LogWarning("ParadaSpawner: Ninguna parada cumple la distancia minima");
            return null;
        }

        int indice = Random.Range(0, disponibles.Count);
        return disponibles[indice];
    }

    void ActivarParada(ParadaController parada)
    {
        Debug.Log($"Intentando activar: {parada.name} (hash: {parada.GetHashCode()})");
        Debug.Log($"Paradas activas actuales: {paradasActivas.Count}");
        foreach (var p in paradasActivas)
        {
            Debug.Log($"  - {p.name} (hash: {p.GetHashCode()})");
        }
        
        if (paradasActivas.Contains(parada))
        {
            Debug.Log($"  Ya estaba en lista, no se agrega");
            return;
        }
        parada.gameObject.SetActive(true);
        paradasActivas.Add(parada);
        Debug.Log($"Nueva parada spawneada: {parada.name} | Distancia: {Vector3.Distance(bondi.position, parada.transform.position):F1}m");
    }

    void OcultarTodasLasParadas(bool incluirDestino = true)
    {
        foreach (var parada in todasLasParadas)
        {
            if (parada != null)
            {
                if (parada.esDestino && !incluirDestino)
                    continue;
                parada.gameObject.SetActive(false);
            }
        }
        if (incluirDestino)
            paradasActivas.Clear();
        else
            paradasActivas.Clear();
    }

    public void SetDistanciaMinima(float distancia)
    {
        distanciaMinima = distancia;
    }

    public ParadaController ObtenerParadaDestino()
    {
        foreach (var parada in todasLasParadas)
        {
            if (parada != null && parada.esDestino)
                return parada;
        }
        return null;
    }
}