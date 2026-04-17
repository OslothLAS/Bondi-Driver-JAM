using UnityEngine;
using TMPro;
using System.Collections;

public class DamageController : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Text collisionText;

    [Header("Configuración de Daño")]
    public int maxCollisions = 10;
    public float cooldownTime = 1f;
    private float nextCollisionTime = 0f;
    private int collisionCount = 0;

    [Header("Configuración de Respawn")]
    public Transform respawnPoint;
    public float flickerDuration = 2f;
    public float flickerSpeed = 0.1f;
    public GameObject efectoExplosion; // Opcional: Prefab de partículas de explosión

    private Rigidbody rb;
    private Renderer[] myRenderers;
    private PassengerController passengerController; // Referencia al otro script

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myRenderers = GetComponentsInChildren<Renderer>();

        // Buscamos el componente de pasajeros en este mismo objeto
        passengerController = GetComponent<PassengerController>();

        UpdateUI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;
        if (Time.time < nextCollisionTime) return;

        collisionCount++;
        nextCollisionTime = Time.time + cooldownTime;
        UpdateUI();

        if (collisionCount >= maxCollisions)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        // 1. Efecto visual de explosión (opcional)
        if (efectoExplosion != null)
        {
            Instantiate(efectoExplosion, transform.position, Quaternion.identity);
        }

        // 2. Lógica de Pasajeros: Vaciamos el bondi
        if (passengerController != null)
        {
            passengerController.PerderTodosLosPasajeros();
        }

        // 3. Resetear contador y teletransportar
        collisionCount = 0;
        UpdateUI();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.position = respawnPoint.position;
        transform.rotation = respawnPoint.rotation;

        StartCoroutine(FlickerEffect());
    }

    private IEnumerator FlickerEffect()
    {
        float timer = 0;
        while (timer < flickerDuration)
        {
            foreach (var r in myRenderers)
            {
                if (r != null) r.enabled = !r.enabled;
            }
            yield return new WaitForSeconds(flickerSpeed);
            timer += flickerSpeed;
        }

        foreach (var r in myRenderers)
        {
            if (r != null) r.enabled = true;
        }
    }

    private void UpdateUI()
    {
        if (collisionText != null)
        {
            collisionText.text = $"Choques: {collisionCount}/{maxCollisions}";
        }
    }
}