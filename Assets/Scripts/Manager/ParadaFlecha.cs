using UnityEngine;

public class ParadaFlecha : MonoBehaviour
{
    public GameObject flechaPrefab;
    public Transform bondi1;
    public Transform bondi2;
    public ParadaSpawner spawner;

    private GameObject flecha1;
    private GameObject flecha2;

    void Start()
    {
        if (bondi1 == null)
        {
            GameObject b1 = GameObject.Find("Bondi_J1");
            if (b1 != null) bondi1 = b1.transform;
        }
        if (bondi2 == null)
        {
            GameObject b2 = GameObject.Find("Bondi_J2");
            if (b2 != null) bondi2 = b2.transform;
        }

        if (flechaPrefab != null)
        {
            if (bondi1 != null) flecha1 = CrearFlecha(bondi1);
            if (bondi2 != null) flecha2 = CrearFlecha(bondi2);
        }
        else
        {
            Debug.LogError("flechaPrefab es NULL!");
        }
    }

    GameObject CrearFlecha(Transform target)
    {
        GameObject flecha = Instantiate(flechaPrefab, target.position, Quaternion.identity);
        Renderer rend = flecha.GetComponent<Renderer>();
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
        return flecha;
    }

    void LateUpdate()
    {
        ParadaController paradaActiva = GetParadaActiva();

        if (paradaActiva != null)
        {
            if (flecha1 != null && bondi1 != null)
                MostrarFlechaHacia(flecha1, bondi1, paradaActiva.transform.position);
            if (flecha2 != null && bondi2 != null)
                MostrarFlechaHacia(flecha2, bondi2, paradaActiva.transform.position);
        }
        else
        {
            if (flecha1 != null) flecha1.SetActive(false);
            if (flecha2 != null) flecha2.SetActive(false);
        }
    }

    void MostrarFlechaHacia(GameObject flecha, Transform bondi, Vector3 targetPos)
    {
        Vector3 dir = targetPos - bondi.position;
        dir.y = 0;
        dir.Normalize();

        Vector3 posBonda = bondi.position + bondi.forward * 48f;
        posBonda.y += 3f;

        flecha.transform.position = posBonda;
        flecha.transform.rotation = Quaternion.LookRotation(dir);
        flecha.SetActive(true);
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
        if (flecha1 != null) Destroy(flecha1);
        if (flecha2 != null) Destroy(flecha2);
    }
}