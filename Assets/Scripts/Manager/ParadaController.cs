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

    private bool sonidoReproducido = false;

    private MeshRenderer meshRenderer;
    private PassengerController busActual;
    private Coroutine boardingCoroutine;

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
        // 1. Buscamos el componente PassengerController primero
        PassengerController pc = obj.GetComponentInParent<PassengerController>() ?? obj.GetComponent<PassengerController>();

        if (pc != null)
        {
            // 2. Filtramos por nombre del GameObject que tiene el script (el padre usualmente)
            string nombreBondi = pc.gameObject.name;

            if (nombreBondi == "Bondi_J1" || nombreBondi == "Bondi_J2")
            {
                if (busActual != null) return;

                Debug.Log($"Entró el bondi permitido: {nombreBondi}");
                busActual = pc;
                CambiarMaterial(materialVerde);

                if (!sonidoReproducido && AudioManager.instance != null)
                {
                    AudioManager.instance.PlaySuspensionSound();
                    sonidoReproducido = true;
                }

                if (esDestino)
                {
                    boardingCoroutine = StartCoroutine(BajadaPasajerosProcedimiento());
                }
                else if (pasajerosEnParada > 0 && busActual.GetRemainingCapacity() > 0)
                {
                    boardingCoroutine = StartCoroutine(SubidaPasajerosProcedimiento());
                }
            }
            else
            {
                Debug.Log($"Objeto con PassengerController detectado ({nombreBondi}), pero NO es un bondi permitido.");
            }
        }
    }

    private void AlSalir(GameObject obj)
    {
        if (busActual != null)
        {
            PassengerController pc = obj.GetComponentInParent<PassengerController>() ?? obj.GetComponent<PassengerController>();
            if (pc != null && pc == busActual)
            {
                if (boardingCoroutine != null)
                {
                    StopCoroutine(boardingCoroutine);
                    boardingCoroutine = null;

                    // CAMBIO CLAVE: En lugar de borrar (Clear), confirmamos (Commit) 
                    // lo que haya llegado a subir hasta este frame.
                    busActual.CommitPending();
                }

                busActual.SetStatusText("");
                CambiarMaterial(materialCeleste);

                // Ya no llamamos al spawner acá para que la parada no desaparezca 
                // si todavía quedan pasajeros esperando.
                busActual = null;
            }
        }
    }
    private IEnumerator SubidaPasajerosProcedimiento()
    {
        int capacidadDisponible = busActual.GetRemainingCapacity();
        int aSubir = Mathf.Min(pasajerosEnParada, capacidadDisponible);
        int subidosActualmente = 0;

        while (subidosActualmente < aSubir)
        {
            if (busActual == null) break;

            busActual.SetStatusText($"Subiendo... {aSubir - subidosActualmente} restantes");

            yield return new WaitForSeconds(tiempoPorPasajero);

            if (busActual == null) break;

            // --- PROCESAMIENTO ATÓMICO ---
            subidosActualmente++;
            pasajerosEnParada--; // Restamos de la parada inmediatamente
            busActual.AddPending(1); // Sumamos al bondi (como pendiente)
        }

        if (busActual != null)
        {
            busActual.SetStatusText("Listo");
            busActual.CommitPending();

            // Si la parada se quedó sin gente (pasajerosEnParada == 0), 
            // recién ahí le avisamos al spawner que cambie de parada.
            if (pasajerosEnParada <= 0 && spawner != null)
            {
                spawner.OnParadaDesocupada(this, busActual.GetRemainingCapacity(), true);
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
        pasajerosEnParada = UnityEngine.Random.Range(2, 9);
        sonidoReproducido = false;
    }
}
