using UnityEngine;
using System.Collections;

public class ParadaController : MonoBehaviour
{
    [Header("Configuracion de Parada")]
    [Tooltip("Cantidad de personas esperando en esta parada")]
    public int pasajerosEnParada = 5;
    [Tooltip("Segundos que tarda en subir/bajar cada pasajero")]
    public float tiempoPorPasajero = 1.0f;

    [Header("Configuracion de Destino")]
    [Tooltip("Si esta activo, es una parada de destino donde bajan todos los pasajeros")]
    public bool esDestino = false;

    [Header("Configuracion de Materiales")]
    public Material materialCeleste;
    public Material materialVerde;

    [Header("Referencia Spawner")]
    [HideInInspector] public ParadaSpawner spawner;

    private MeshRenderer meshRenderer;
    private PassengerController busActual;
    private Coroutine boardingCoroutine;
    private bool spawnHizoEnSalida = false;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        spawner = GetComponentInParent<ParadaSpawner>();
        ResetParada();
    }

    // --- ENTRADA ---
    private void OnCollisionEnter(Collision collision) => AlEntrar(collision.gameObject);
    private void OnTriggerEnter(Collider other) => AlEntrar(other.gameObject);

    // --- SALIDA ---
    private void OnCollisionExit(Collision collision) => AlSalir(collision.gameObject);
    private void OnTriggerExit(Collider other) => AlSalir(other.gameObject);

    private void AlEntrar(GameObject obj)
    {
        Debug.Log("AlEntrar");
        if (busActual != null) return;

        PassengerController pc = obj.GetComponentInParent<PassengerController>() ?? obj.GetComponent<PassengerController>();
        if (pc != null)
        {
            busActual = pc;
            CambiarMaterial(materialVerde);

            if (esDestino)
            {
                boardingCoroutine = StartCoroutine(BajadaPasajerosProcedimiento());
            }
            else if (pasajerosEnParada > 0 && busActual.GetRemainingCapacity() > 0)
            {
                boardingCoroutine = StartCoroutine(SubidaPasajerosProcedimiento());
            }
        }
    }

    private void AlSalir(GameObject obj)
    {
        if (busActual != null)
        {
            PassengerController pc = obj.GetComponentInParent<PassengerController>() ?? obj.GetComponent<PassengerController>();
            if (pc == busActual)
            {
                if (boardingCoroutine != null)
                {
                    StopCoroutine(boardingCoroutine);
                    boardingCoroutine = null;
                    busActual.ClearPending();
                }

                busActual.SetStatusText("");
                CambiarMaterial(materialCeleste);

                if (spawner != null && !spawnHizoEnSalida)
                {
                    int capacidadRestante = busActual.GetRemainingCapacity();
                    bool fueCompletada = pasajerosEnParada == 0;
                    spawner.OnParadaDesocupada(this, capacidadRestante, fueCompletada);
                }

                spawnHizoEnSalida = false;
                busActual = null;
            }
        }
    }

    private IEnumerator SubidaPasajerosProcedimiento()
    {
        spawnHizoEnSalida = false;

        int capacidadDisponible = busActual.GetRemainingCapacity();
        int aSubir = Mathf.Min(pasajerosEnParada, capacidadDisponible);
        int subidosActualmente = 0;

        while (subidosActualmente < aSubir)
        {
            // Verificamos si el bus sigue ahi antes de actualizar el texto
            if (busActual == null) break;

            busActual.SetStatusText($"Subiendo... {aSubir - subidosActualmente} restantes");

            yield return new WaitForSeconds(tiempoPorPasajero);

            // Verificamos de nuevo despues de la espera de 1 segundo
            if (busActual == null) break;

            subidosActualmente++;
            busActual.AddPending(1);
        }

        // Si llegamos aqui y el bus sigue siendo valido
        if (busActual != null)
        {
            busActual.SetStatusText("Listo");
            busActual.CommitPending();
            pasajerosEnParada -= subidosActualmente;
            spawnHizoEnSalida = true;
            Debug.Log($"Abordaje completo. Pasajeros restantes en parada: {pasajerosEnParada}");

            if (spawner != null)
            {
                int capacidadRestante = busActual.GetRemainingCapacity();
                spawner.OnParadaDesocupada(this, capacidadRestante, subidosActualmente > 0);
            }
        }

        boardingCoroutine = null;
    }

    private IEnumerator BajadaPasajerosProcedimiento()
    {
        int pasajerosABajar = busActual.GetCurrentPassengers();

        while (pasajerosABajar > 0)
        {
            if (busActual == null) break;

            busActual.SetStatusText($"Bajando... {pasajerosABajar} restantes");

            yield return new WaitForSeconds(tiempoPorPasajero);

            if (busActual == null) break;

            pasajerosABajar--;
            busActual.RemovePending(1);
        }

        if (busActual != null)
        {
            busActual.SetStatusText("Listo");
            busActual.CommitPending();
            Debug.Log($"Bajada completa. Pasajeros restantes en bus: {busActual.GetCurrentPassengers()}");
        }

        boardingCoroutine = null;
    }

    private void CambiarMaterial(Material nuevoMaterial)
    {
        if (meshRenderer != null && nuevoMaterial != null)
        {
            meshRenderer.material = nuevoMaterial;
        }
    }

    public void ResetParada()
    {
        spawnHizoEnSalida = false;
        pasajerosEnParada = UnityEngine.Random.Range(2, 9);
    }
}
