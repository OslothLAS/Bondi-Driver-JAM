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
        if (bondi == null)
        {
            GameObject bondiObj = GameObject.FindGameObjectWithTag("Player");
            if (bondiObj != null)
                bondi = bondiObj.transform;
        }

        if (flechaPrefab != null)
        {
            flechaActual = Instantiate(flechaPrefab, bondi.position, Quaternion.identity);
            AplicarMaterialHolografico();
            flechaActual.SetActive(false);
        }
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
        if (flechaActual == null || bondi == null || spawner == null)
            return;

        ParadaController paradaActiva = GetParadaActiva();

        if (paradaActiva != null)
        {
            MostrarFlechaHacia(paradaActiva.transform.position);
        }
        else
        {
            PassengerController pc = bondi.GetComponent<PassengerController>();
            int capacidad = pc != null ? pc.GetRemainingCapacity() : 0;

            if (capacidad <= 0)
            {
                ParadaController destino = spawner.ObtenerParadaDestino();
                if (destino != null && destino.gameObject.activeSelf)
                {
                    MostrarFlechaHacia(destino.transform.position);
                    return;
                }
            }
            OcultarFlecha();
        }
    }

    void MostrarFlechaHacia(Vector3 targetPos)
    {
        Vector3 dir = targetPos - bondi.position;
        float distancia = dir.magnitude;
        dir.y = 0;
        dir.Normalize();

        Vector3 posBonda = bondi.position;
        posBonda.y += 3f;
        flechaActual.transform.position = posBonda;

        flechaActual.transform.rotation = Quaternion.LookRotation(dir);

        float escala = Mathf.Lerp(1f, 2f, Mathf.Clamp01(distancia / 500f));
        flechaActual.transform.localScale = Vector3.one * escala;

        flechaActual.SetActive(true);
        flechaActiva = true;
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