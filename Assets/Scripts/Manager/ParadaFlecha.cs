using UnityEngine;

public class ParadaFlecha : MonoBehaviour
{
    public GameObject flechaPrefab;
    public Transform bondi;
    public ParadaSpawner spawner;

    private GameObject flechaActual;
    private bool mostrandoFlecha = false;

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
            flechaActual.SetActive(false);

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

        if (mostrandoFlecha)
        {
            float speed = 30f * Time.deltaTime;
            flechaActual.transform.RotateAround(bondi.position, Vector3.up, speed);

            Vector3 dir = flechaActual.transform.position - bondi.position;
            dir.y = 0;
            if (dir.magnitude > 0.1f)
            {
                flechaActual.transform.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    void MostrarFlechaHacia(Vector3 targetPos)
    {
        Vector3 dir = targetPos - bondi.position;
        float distancia = dir.magnitude;
        dir.Normalize();

        Vector3 pos = bondi.position + Vector3.up * 2f;
        flechaActual.transform.position = pos;
        flechaActual.transform.rotation = Quaternion.LookRotation(dir);

        float escala = Mathf.Lerp(0.5f, 1.5f, Mathf.Clamp01(distancia / 100f));
        flechaActual.transform.localScale = Vector3.one * escala;

        flechaActual.SetActive(true);
        mostrandoFlecha = true;
    }

    void OcultarFlecha()
    {
        flechaActual.SetActive(false);
        mostrandoFlecha = false;
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