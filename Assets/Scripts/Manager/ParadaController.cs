using UnityEngine;
using System.Collections;

public class ParadaController : MonoBehaviour
{
    [Header("Configuracion de Parada")]
    [Tooltip("Cantidad de personas esperando en esta parada")]
    public int pasajerosEnParada = 5;
    [Tooltip("Segundos que tarda en subir cada pasajero")]
    public float tiempoPorPasajero = 1.0f;

    [Header("Configuracion de Materiales")]
    public Material materialCeleste;
    public Material materialVerde;

    private MeshRenderer meshRenderer;
    private PassengerController busActual;
    private Coroutine boardingCoroutine;

    void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    // --- ENTRADA ---
    private void OnCollisionEnter(Collision collision) => AlEntrar(collision.gameObject);
    private void OnTriggerEnter(Collider other) => AlEntrar(other.gameObject);

    // --- SALIDA ---
    private void OnCollisionExit(Collision collision) => AlSalir(collision.gameObject);
    private void OnTriggerExit(Collider other) => AlSalir(other.gameObject);

    private void AlEntrar(GameObject obj)
    {
        // Si ya hay un bus en proceso de subida, ignoramos otros colisionadores del mismo bus
        if (busActual != null) return;

        PassengerController pc = obj.GetComponentInParent<PassengerController>() ?? obj.GetComponent<PassengerController>();
        if (pc != null)
        {
            busActual = pc;
            CambiarMaterial(materialVerde);
            
            // Iniciar la subida si hay gente y espacio
            if (pasajerosEnParada > 0 && busActual.GetRemainingCapacity() > 0)
            {
                boardingCoroutine = StartCoroutine(SubidaPasajerosProcedimiento());
            }
        }
    }

    private void AlSalir(GameObject obj)
    {
        if (busActual != null)
        {
            // Verificamos que sea el mismo bus el que sale
            PassengerController pc = obj.GetComponentInParent<PassengerController>() ?? obj.GetComponent<PassengerController>();
            if (pc == busActual)
            {
                // Interrumpir subida si estaba en curso
                if (boardingCoroutine != null)
                {
                    StopCoroutine(boardingCoroutine);
                    boardingCoroutine = null;
                    busActual.ClearPending();
                }

                busActual.SetStatusText(""); // Limpiamos el texto al salir
                CambiarMaterial(materialCeleste);
                busActual = null;
            }
        }
    }

    private IEnumerator SubidaPasajerosProcedimiento()
    {
        // Calculamos cuantos pueden subir antes de empezar
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
            Debug.Log($"Abordaje exitoso. Pasajeros restantes en parada: {pasajerosEnParada}");
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
}
