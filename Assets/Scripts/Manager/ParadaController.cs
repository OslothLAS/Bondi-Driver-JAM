using UnityEngine;
using System.Collections;

public class ParadaController : MonoBehaviour
{
    [Header("Configuracion de Parada")]
    public int pasajerosEnParada = 5;
    public float tiempoPorPasajero = 1.0f;

    [Header("Configuracion de Destino")]
    public bool esDestino = false;

    [Header("Configuracion de Materiales")]
    public Material materialCeleste;
    public Material materialVerde;

    [HideInInspector] public ParadaSpawner spawner;

    private bool sonidoReproducido = false;
    private MeshRenderer meshRenderer;
    private PassengerController busActual;
    private Coroutine boardingCoroutine;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        // Intentamos buscarlo, pero el Spawner también lo asigna al inicio
        if (spawner == null) spawner = GetComponentInParent<ParadaSpawner>();
        ResetParada();
    }

    private void OnCollisionEnter(Collision collision) => AlEntrar(collision.gameObject);
    private void OnTriggerEnter(Collider other) => AlEntrar(other.gameObject);

    private void OnCollisionExit(Collision collision) => AlSalir(collision.gameObject);
    private void OnTriggerExit(Collider other) => AlSalir(other.gameObject);

    private void AlEntrar(GameObject obj)
    {
        PassengerController pc = obj.GetComponentInParent<PassengerController>() ?? obj.GetComponent<PassengerController>();

        if (pc != null)
        {
            string nombreBondi = pc.gameObject.name;

            // --- CAMBIO CLAVE: AHORA DETECTA J1, J2, J3 Y J4 ---
            if (nombreBondi.Contains("Bondi_J"))
            {
                if (busActual != null) return; // Si ya hay un bondi cargando, ignoramos el resto

                Debug.Log($"[Logística] Entró el bondi permitido: {nombreBondi}");
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
                // Este es el mensaje que veías antes
                Debug.LogWarning($"[Seguridad] {nombreBondi} intentó cargar pero no es un bondi permitido.");
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
                    busActual.CommitPending(); // Confirmamos lo que subió hasta ahora
                }

                busActual.SetStatusText("");
                CambiarMaterial(materialCeleste);
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

            subidosActualmente++;
            pasajerosEnParada--;
            busActual.AddPending(1);
        }

        if (busActual != null)
        {
            busActual.SetStatusText("Listo");
            busActual.CommitPending();

            // --- REVISIÓN DEL RESETEO ---
            // Si la parada se vació, le avisamos al spawner.
            if (pasajerosEnParada <= 0 && spawner != null)
            {
                Debug.Log($"[Spawner] Parada {gameObject.name} vacía. Solicitando nueva parada.");
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

            // Si es destino, se resetea apenas el primer bondi baja a todos
            if (spawner != null)
            {
                spawner.OnParadaDesocupada(this, busActual.GetRemainingCapacity(), true);
            }
        }
        boardingCoroutine = null;
    }

    private void CambiarMaterial(Material nuevoMaterial)
    {
        if (meshRenderer != null && nuevoMaterial != null)
            meshRenderer.material = nuevoMaterial;
    }

    public void ResetParada()
    {
        pasajerosEnParada = UnityEngine.Random.Range(2, 9);
        sonidoReproducido = false;
        CambiarMaterial(materialCeleste);
    }
}