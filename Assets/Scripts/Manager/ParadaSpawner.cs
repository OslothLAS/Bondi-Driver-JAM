using UnityEngine;
using System.Collections.Generic;

public class ParadaSpawner : MonoBehaviour
{
    [Header("Configuracion de Distancia")]
    [Tooltip("Distancia minima desde el baricentro de los 4 bondis")]
    public float distanciaMinima = 400f; // Ajustá según el tamaño de tu mapa

    [Header("Referencias de Bondis")]
    // Usamos un array para manejar a los 4 jugadores de forma limpia
    public Transform[] bondis = new Transform[4];

    [Header("Base de Datos de Paradas")]
    [SerializeField] private List<ParadaController> todasLasParadas = new List<ParadaController>();
    private List<ParadaController> paradasActivas = new List<ParadaController>();

    public List<ParadaController> ParadasActivas => paradasActivas;

    void Start()
    {
        // 1. BUSQUEDA AUTOMATICA DE LOS 4 BONDIS
        for (int i = 0; i < 4; i++)
        {
            if (bondis[i] == null)
            {
                GameObject b = GameObject.Find("Bondi_J" + (i + 1));
                if (b != null) bondis[i] = b.transform;
            }
        }

        // 2. BUSQUEDA DE PARADAS Y DESTINOS
        todasLasParadas.Clear();
        // Buscamos todo lo que tenga el tag Parada
        GameObject[] paradasEncontradas = GameObject.FindGameObjectsWithTag("Parada");
        foreach (GameObject obj in paradasEncontradas)
        {
            ParadaController controller = obj.GetComponent<ParadaController>();
            if (controller != null)
            {
                todasLasParadas.Add(controller);
                controller.spawner = this;
            }
        }

        Debug.Log($"[Spawner] Sistemas listos. Bondis detectados: {GetCantidadBondisActivos()} | Paradas totales: {todasLasParadas.Count}");

        // 3. INICIO DE PARTIDA
        OcultarTodasLasParadas(true);

        // Activamos el destino (Obelisco)
        ParadaController destino = ObtenerParadaDestino();
        if (destino != null) ActivarParada(destino);

        // Spawneamos la primera parada de pasajeros
        ParadaController primera = ObtenerNuevaParada();
        if (primera != null) ActivarParada(primera);
    }

    // --- CÁLCULO DEL BARICENTRO (Punto medio de todos) ---
    public Vector3 GetBondisMidpoint()
    {
        Vector3 sumaPosiciones = Vector3.zero;
        int contador = 0;

        foreach (Transform t in bondis)
        {
            if (t != null)
            {
                sumaPosiciones += t.position;
                contador++;
            }
        }

        return contador > 0 ? sumaPosiciones / contador : Vector3.zero;
    }

    // Modificá este método dentro de tu ParadaSpawner.cs

    public void OnParadaDesocupada(ParadaController parada, int capacidadRestante, bool fueCompletada)
    {
        if (parada == null || !fueCompletada) return;

        bool eraDestino = parada.esDestino;

        // --- CAMBIO CLAVE: EL DESTINO NO SE DESACTIVA ---
        if (paradasActivas.Contains(parada))
        {
            // Solo desactivamos y removemos si ES una parada común
            if (!eraDestino)
            {
                parada.gameObject.SetActive(false);
                paradasActivas.Remove(parada);

                // Si el bondi todavía tiene lugar, spawneamos otra parada común
                if (capacidadRestante > 0)
                {
                    ParadaController nueva = ObtenerNuevaParada();
                    if (nueva != null) ActivarParada(nueva);
                }
            }
            else
            {
                // Si es destino, NO hacemos nada. Se queda ahí para el próximo bondi.
                Debug.Log("[Sistemas] Destino completado por un jugador, permanece activo para los demás.");
            }
        }

        // Si el bondi se llenó y por alguna razón el destino estaba apagado, lo prendemos
        if (capacidadRestante <= 0)
        {
            ParadaController destino = ObtenerParadaDestino();
            if (destino != null && !destino.gameObject.activeSelf)
            {
                ActivarParada(destino);
            }
        }
    }

    ParadaController ObtenerNuevaParada()
    {
        if (todasLasParadas.Count == 0) return null;

        Vector3 baricentro = GetBondisMidpoint();
        List<ParadaController> disponibles = new List<ParadaController>();

        foreach (var parada in todasLasParadas)
        {
            if (parada == null || paradasActivas.Contains(parada) || parada.esDestino)
                continue;

            float distancia = Vector3.Distance(baricentro, parada.transform.position);

            if (distancia >= distanciaMinima)
                disponibles.Add(parada);
        }

        // Fallback: si ninguna está lejos, agarramos una al azar para no romper el loop
        if (disponibles.Count == 0)
        {
            return todasLasParadas.Find(p => !p.esDestino && !paradasActivas.Contains(p));
        }

        return disponibles[Random.Range(0, disponibles.Count)];
    }

    // --- MÉTODOS DE CONTROL ---

    void ActivarParada(ParadaController parada)
    {
        if (parada == null || paradasActivas.Contains(parada)) return;
        parada.gameObject.SetActive(true);
        paradasActivas.Add(parada);
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
        return todasLasParadas.Find(p => p != null && p.esDestino);
    }

    private int GetCantidadBondisActivos()
    {
        int count = 0;
        foreach (var b in bondis) if (b != null) count++;
        return count;
    }

    private int GetCantidadParadasCargaActivas()
    {
        int count = 0;
        foreach (var p in paradasActivas) if (!p.esDestino) count++;
        return count;
    }
}