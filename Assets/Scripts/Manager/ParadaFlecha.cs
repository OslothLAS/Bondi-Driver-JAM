using UnityEngine;

public class ParadaFlecha : MonoBehaviour
{
    public GameObject flechaPrefab;
    public Transform bondi;
    public ParadaSpawner spawner;

    private GameObject flechaActual;
    private bool flechaActiva = false;

    void Start()
    {
        Debug.Log("=== ParadaFlecha Start ===");

        if (bondi == null)
        {
            GameObject bondiObj = GameObject.FindGameObjectWithTag("Player");
            if (bondiObj != null)
            {
                bondi = bondiObj.transform;
                Debug.Log($"Bondi encontrado: {bondi.name}");
            }
            else
            {
                Debug.LogError("NO se encontro Bondi con tag Player!");
            }
        }
        else
        {
            Debug.Log($"Bondi ya asignado: {bondi.name}");
        }

        if (flechaPrefab != null)
        {
            Debug.Log($"Instanciando prefab: {flechaPrefab.name}");
            flechaActual = Instantiate(flechaPrefab, bondi.position, Quaternion.identity);
            Debug.Log($"flechaActual instanciado: {flechaActual.name}, activeSelf: {flechaActual.activeSelf}");
            AplicarMaterialHolografico();
        }
        else
        {
            Debug.LogError("flechaPrefab es NULL!");
        }

        Debug.Log($"spawner: {spawner != null}");
        Debug.Log("=== Fin Start ===");
    }

    void AplicarMaterialHolografico()
    {
        Renderer rend = flechaActual.GetComponent<Renderer>();
        if (rend != null)
        {
            Material mat = new Material(rend.material);
            mat.SetFloat("_Mode", 3);
            mat.SetColor("_Color", new Color(0f, 1f, 1f, 0.5f));
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            rend.material = mat;
        }
    }

    void LateUpdate()
    {
        Debug.Log($"=== LateUpdate === flechaActual:{flechaActual != null} bondi:{bondi != null} spawner:{spawner != null}");

        if (flechaActual == null || bondi == null || spawner == null)
        {
            Debug.LogWarning("Saliendo por null check");
            return;
        }

        ParadaController paradaActiva = GetParadaActiva();
        Debug.Log($"paradaActiva: {paradaActiva?.name ?? "null"}");

        if (paradaActiva != null)
        {
            Debug.Log("Mostrando hacia parada activa");
            MostrarFlechaHacia(paradaActiva.transform.position);
        }
        else
        {
            PassengerController pc = bondi.GetComponent<PassengerController>();
            int capacidad = pc != null ? pc.GetRemainingCapacity() : -1;
            Debug.Log($"capacidad: {capacidad}");

            ParadaController destino = spawner.ObtenerParadaDestino();
            bool destinoActivo = destino != null && destino.gameObject.activeSelf;
            Debug.Log($"destino: {destino?.name ?? "null"}, activo: {destinoActivo}");

            if (capacidad <= 0 && destinoActivo)
            {
                Debug.Log("Caso 1: capacidad 0 y destino activo");
                MostrarFlechaHacia(destino.transform.position);
            }
            else if (destinoActivo)
            {
                Debug.Log("Caso 2: destino activo (sin estar lleno)");
                MostrarFlechaHacia(destino.transform.position);
            }
            else
            {
                Debug.Log("Caso 3: ocultando flecha");
                OcultarFlecha();
            }
        }
    }

    void MostrarFlechaHacia(Vector3 targetPos)
    {
        Debug.Log($"=== MostrarFlechaHacia === target: {targetPos}");
        Debug.Log($"Bondi pos: {bondi.position}");

        Vector3 dir = targetPos - bondi.position;
        dir.y = 0;
        dir.Normalize();
        Debug.Log($"Dir normalizada: {dir}");

        Vector3 posBonda = bondi.position + bondi.forward * 500f;
        posBonda.y += 3f;
        Debug.Log($"Nueva pos: {posBonda}");

        flechaActual.transform.position = posBonda;
        flechaActual.transform.rotation = Quaternion.LookRotation(dir);
        flechaActual.SetActive(true);
        flechaActiva = true;

        Debug.Log($"Flecha activada. activeSelf: {flechaActual.activeSelf}");
    }

    void OcultarFlecha()
    {
        if (flechaActual != null)
            flechaActual.SetActive(false);
        flechaActiva = false;
    }

    ParadaController GetParadaActiva()
    {
        if (spawner.ParadasActivas.Count > 0)
            return spawner.ParadasActivas[0];
        return null;
    }

    void OnDestroy()
    {
        if (flechaActual != null)
            Destroy(flechaActual);
    }
}