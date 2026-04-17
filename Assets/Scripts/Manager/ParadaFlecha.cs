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
            {
                bondi = bondiObj.transform;
            }
            else
            {
                Debug.LogError("NO se encontro Bondi con tag Player!");
            }
        }

        if (flechaPrefab != null)
        {
            flechaActual = Instantiate(flechaPrefab, bondi.position, Quaternion.identity);
            AplicarMaterialHolografico();
        }
        else
        {
            Debug.LogError("flechaPrefab es NULL!");
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
            float dist = Vector3.Distance(bondi.position, paradaActiva.transform.position);
            MostrarFlechaHacia(paradaActiva.transform.position);
        }
        else
        {
            OcultarFlecha();
        }
    }

    void MostrarFlechaHacia(Vector3 targetPos)
    {
        Vector3 dir = targetPos - bondi.position;
        dir.y = 0;
        dir.Normalize();

        Vector3 posBonda = bondi.position + bondi.forward * 48f;
        posBonda.y += 3f;

        flechaActual.transform.position = posBonda;
        flechaActual.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(-90f, 0f, 0f);
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
        foreach (var parada in spawner.ParadasActivas)
        {
            if (parada != null && !parada.esDestino)
                return parada;
        }
        return null;
    }

    void OnDestroy()
    {
        if (flechaActual != null)
            Destroy(flechaActual);
    }
}