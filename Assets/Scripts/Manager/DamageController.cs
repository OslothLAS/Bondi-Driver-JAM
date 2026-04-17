using UnityEngine;
using TMPro;

public class DamageController : MonoBehaviour
{
    [Header("Configuración de Choques")]
    public int maxChoquesParaRespawn = 5;
    public float cooldownTime = 1.5f;

    [Header("Efectos de Explosión")]
    public GameObject prefabExplosion;
    public float tiempoVidaExplosion = 2.5f;

    [Header("Sistemas de Humo (En Jerarquía)")]
    [Tooltip("El objeto del humo blanco/gris gradual")]
    public GameObject objetoHumoBlanco;
    [Tooltip("El objeto del humo crítico")]
    public GameObject objetoHumoCritico;

    [Header("Configuración Humo Blanco")]
    public float maximaEmisionBlanco = 60f;
    public Color colorHumoInicial = new Color(0.8f, 0.8f, 0.8f, 0.2f);
    public Color colorHumoFinal = new Color(0.3f, 0.3f, 0.3f, 0.9f);

    [Header("Referencias de UI y Respawn")]
    public TMP_Text contadorChoquesUI;
    public Transform respawnPoint;

    private int choquesActualesVida = 0;
    private int totalChoquesPartida = 0;
    private float nextCollisionTime = 0f;

    // Para controlar las partículas del humo blanco cuando esté activo
    private ParticleSystem psBlanco;
    private ParticleSystem.EmissionModule emissionBlanco;

    void Start()
    {
        // Setup inicial de componentes
        if (objetoHumoBlanco != null)
        {
            psBlanco = objetoHumoBlanco.GetComponent<ParticleSystem>();
            if (psBlanco != null) emissionBlanco = psBlanco.emission;

            objetoHumoBlanco.SetActive(true); // Empezamos con el blanco prendido
            if (psBlanco != null) emissionBlanco.rateOverTime = 0f;
        }

        if (objetoHumoCritico != null) objetoHumoCritico.SetActive(false);

        UpdateUI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;
        if (Time.time < nextCollisionTime) return;

        choquesActualesVida++;
        totalChoquesPartida++;
        nextCollisionTime = Time.time + cooldownTime;

        GestionarFasesDeHumo();
        UpdateUI();

        if (choquesActualesVida >= maxChoquesParaRespawn)
        {
            GenerarExplosion();
            Respawn();
        }
    }

    void GestionarFasesDeHumo()
    {
        float umbralCritico = maxChoquesParaRespawn * 0.9f;

        // --- ESTADO CRÍTICO (90% de daño) ---
        if (choquesActualesVida >= umbralCritico && choquesActualesVida < maxChoquesParaRespawn)
        {
            // Apagamos el blanco, prendemos el crítico
            if (objetoHumoBlanco != null) objetoHumoBlanco.SetActive(false);
            if (objetoHumoCritico != null) objetoHumoCritico.SetActive(true);
        }
        // --- ESTADO GRADUAL (Normal) ---
        else
        {
            // Aseguramos que el crítico esté apagado y el blanco prendido
            if (objetoHumoCritico != null) objetoHumoCritico.SetActive(false);
            if (objetoHumoBlanco != null)
            {
                objetoHumoBlanco.SetActive(true);
                ActualizarEmisionBlanca();
            }
        }
    }

    void ActualizarEmisionBlanca()
    {
        if (psBlanco == null) return;

        float damageRatio = Mathf.Clamp01((float)choquesActualesVida / maxChoquesParaRespawn);

        // Aplicamos la lógica de emisión y color que veníamos usando
        emissionBlanco.rateOverTime = damageRatio * maximaEmisionBlanco;

        var main = psBlanco.main;
        main.startColor = Color.Lerp(colorHumoInicial, colorHumoFinal, damageRatio);
    }

    void Respawn()
    {
        choquesActualesVida = 0;

        // Al reaparecer: Volvemos al estado inicial (Blanco ON / Crítico OFF)
        if (objetoHumoCritico != null) objetoHumoCritico.SetActive(false);
        if (objetoHumoBlanco != null)
        {
            objetoHumoBlanco.SetActive(true);
            ActualizarEmisionBlanca(); // Esto lo va a poner en 0 emisión por el reset de vida
        }

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        UpdateUI();
    }

    void GenerarExplosion()
    {
        if (prefabExplosion != null)
        {
            GameObject exp = Instantiate(prefabExplosion, transform.position, Quaternion.identity);
            Destroy(exp, tiempoVidaExplosion);
        }
    }

    void UpdateUI()
    {
        if (contadorChoquesUI != null)
        {
            contadorChoquesUI.text = $"CHOQUES: {choquesActualesVida}/{maxChoquesParaRespawn}";
        }
    }

    public int GetTotalChoques() => totalChoquesPartida;
}