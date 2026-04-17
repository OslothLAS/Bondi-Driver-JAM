using UnityEngine;

public class ParadaFlecha : MonoBehaviour
{
    [Header("Prefabs de Flechas")]
    public GameObject prefabParada;
    public GameObject prefabDestino;

    [Header("Referencias de Jugadores")]
    public Transform bondi1;
    public Transform bondi2;
    public ParadaSpawner spawner;

    [Header("Configuracion de Orbita")]
    public float orbitRadius = 5f;
    public Vector3 ejeRotacion = Vector3.up;
    public float alturaExtra = 3f;

    [Header("Personalización de Colores")]
    public Color colorParada = new Color(0f, 1f, 1f, 0.5f);
    public Color colorDestino = new Color(1f, 0.8f, 0f, 0.5f);

    // Referencias a los scripts de pasajeros (NUEVO)
    private PassengerController pc1, pc2;

    // Referencias internas de GameObjects
    private GameObject fActual1, fActual2;
    private GameObject fDestino1, fDestino2;

    void Start()
    {
        // 1. Buscamos bondis y sus PassengerControllers (NUEVO)
        if (bondi1 == null) bondi1 = GameObject.Find("Bondi_J1")?.transform;
        if (bondi2 == null) bondi2 = GameObject.Find("Bondi_J2")?.transform;

        if (bondi1 != null) pc1 = bondi1.GetComponent<PassengerController>();
        if (bondi2 != null) pc2 = bondi2.GetComponent<PassengerController>();

        // 2. Instanciamos los prefabs
        if (bondi1 != null)
        {
            if (prefabParada) fActual1 = CrearFlecha(bondi1, prefabParada, colorParada);
            if (prefabDestino) fDestino1 = CrearFlecha(bondi1, prefabDestino, colorDestino);
        }

        if (bondi2 != null)
        {
            if (prefabParada) fActual2 = CrearFlecha(bondi2, prefabParada, colorParada);
            if (prefabDestino) fDestino2 = CrearFlecha(bondi2, prefabDestino, colorDestino);
        }
    }

    GameObject CrearFlecha(Transform target, GameObject prefab, Color color)
    {
        GameObject obj = Instantiate(prefab, target.position, Quaternion.identity);
        Renderer rend = obj.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(rend.material);
            mat.SetColor("_Color", color);
            rend.material = mat;
        }
        return obj;
    }

    void LateUpdate()
    {
        if (spawner == null) return;

        // Buscamos las paradas en el spawner
        ParadaController pActual = GetParadaPorTipo(false);
        ParadaController pDestino = GetParadaPorTipo(true);

        // --- LÓGICA DE VISIBILIDAD BASADA EN PASAJEROS (NUEVO) ---

        // Para el J1: La flecha de destino solo aparece si tiene pasajeros > 0
        bool mostrarDestinoJ1 = (pc1 != null && pc1.GetCurrentPassengers() > 0);

        // Para el J2: Lo mismo
        bool mostrarDestinoJ2 = (pc2 != null && pc2.GetCurrentPassengers() > 0);

        // --- ACTUALIZACIÓN DE FLECHAS ---

        // Flechas de paradas comunes (Siempre aparecen si hay parada)
        ActualizarPosicion(fActual1, bondi1, pActual);
        ActualizarPosicion(fActual2, bondi2, pActual);

        // Flechas de destino (Solo aparecen si tienen pasajeros)
        ActualizarPosicion(fDestino1, bondi1, mostrarDestinoJ1 ? pDestino : null);
        ActualizarPosicion(fDestino2, bondi2, mostrarDestinoJ2 ? pDestino : null);
    }

    void ActualizarPosicion(GameObject flecha, Transform bondi, ParadaController parada)
    {
        if (flecha == null || bondi == null) return;

        // Si mandamos 'null' como parada, la flecha se oculta
        if (parada != null)
        {
            Vector3 dir = (parada.transform.position - bondi.position);
            Vector3 dirP = Vector3.ProjectOnPlane(dir, ejeRotacion).normalized;

            Vector3 finalPos = bondi.position + (dirP * orbitRadius);
            finalPos += ejeRotacion.normalized * alturaExtra;

            flecha.transform.position = finalPos;
            flecha.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(-90f, 0f, 0f);

            flecha.SetActive(true);
        }
        else
        {
            flecha.SetActive(false);
        }
    }

    ParadaController GetParadaPorTipo(bool esDestino)
    {
        foreach (var p in spawner.ParadasActivas)
        {
            if (p != null && p.esDestino == esDestino) return p;
        }
        return null;
    }

    void OnDestroy()
    {
        if (fActual1) Destroy(fActual1);
        if (fActual2) Destroy(fActual2);
        if (fDestino1) Destroy(fDestino1);
        if (fDestino2) Destroy(fDestino2);
    }
}