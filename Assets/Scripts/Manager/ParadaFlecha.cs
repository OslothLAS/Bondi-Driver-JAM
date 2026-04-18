using UnityEngine;

public class ParadaFlecha : MonoBehaviour
{
    [Header("Prefabs de Flechas")]
    public GameObject prefabParada;
    public GameObject prefabDestino;

    [Header("Configuracion de Orbita")]
    public float orbitRadius = 5f;
    public Vector3 ejeRotacion = Vector3.up;
    public float alturaExtra = 3f;

    [Header("Personalización de Colores")]
    public Color colorParada = new Color(0f, 1f, 1f, 0.5f);
    public Color colorDestino = new Color(1f, 0.8f, 0f, 0.5f);

    [Header("Referencias (Opcional manual)")]
    public ParadaSpawner spawner;

    // --- ARRAYS PARA 4 JUGADORES ---
    private Transform[] bondis = new Transform[4];
    private PassengerController[] pcs = new PassengerController[4];
    private GameObject[] fActuales = new GameObject[4];
    private GameObject[] fDestinos = new GameObject[4];

    void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            // 1. Buscamos bondis por nombre (J1, J2, J3, J4)
            string nombreBondi = "Bondi_J" + (i + 1);
            GameObject objBondi = GameObject.Find(nombreBondi);

            if (objBondi != null)
            {
                bondis[i] = objBondi.transform;
                pcs[i] = objBondi.GetComponent<PassengerController>();

                // 2. Instanciamos las flechas para este jugador
                if (prefabParada != null)
                    fActuales[i] = CrearFlecha(bondis[i], prefabParada, colorParada);

                if (prefabDestino != null)
                    fDestinos[i] = CrearFlecha(bondis[i], prefabDestino, colorDestino);
            }
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

        // Buscamos las paradas en el spawner una sola vez por frame
        ParadaController pActual = GetParadaPorTipo(false);
        ParadaController pDestino = GetParadaPorTipo(true);

        // Actualizamos cada jugador en un bucle
        for (int i = 0; i < 4; i++)
        {
            if (bondis[i] == null) continue;

            // Lógica de visibilidad: Destino solo si tiene pasajeros
            bool tienePasajeros = (pcs[i] != null && pcs[i].GetCurrentPassengers() > 0);

            // Flecha Parada Común
            ActualizarPosicion(fActuales[i], bondis[i], pActual);

            // Flecha Destino
            ActualizarPosicion(fDestinos[i], bondis[i], tienePasajeros ? pDestino : null);
        }
    }

    void ActualizarPosicion(GameObject flecha, Transform bondi, ParadaController parada)
    {
        if (flecha == null || bondi == null) return;

        if (parada != null)
        {
            Vector3 dir = (parada.transform.position - bondi.position);
            Vector3 dirP = Vector3.ProjectOnPlane(dir, ejeRotacion).normalized;

            Vector3 finalPos = bondi.position + (dirP * orbitRadius);
            finalPos += ejeRotacion.normalized * alturaExtra;

            flecha.transform.position = finalPos;
            // Apuntar hacia el objetivo, rotando 90 en X si el modelo está acostado
            flecha.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(-90f, 0f, 0f);

            if (!flecha.activeSelf) flecha.SetActive(true);
        }
        else
        {
            if (flecha.activeSelf) flecha.SetActive(false);
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
        // Limpieza de todos los objetos instanciados
        for (int i = 0; i < 4; i++)
        {
            if (fActuales[i]) Destroy(fActuales[i]);
            if (fDestinos[i]) Destroy(fDestinos[i]);
        }
    }
}