using UnityEngine;
using TMPro;

public class DamageController : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Text collisionText;

    [Header("Configuracion de Daño")]
    public float cooldownTime = 2f;
    private float nextCollisionTime = 0f;
    private int collisionCount = 0;

    void Start()
    {
        UpdateUI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;

        if (Time.time < nextCollisionTime) return;

        collisionCount++;
        nextCollisionTime = Time.time + cooldownTime;

        UpdateUI();

        Debug.Log($"Choque detectado con: {collision.gameObject.name}. Total: {collisionCount}");
    }

    private void UpdateUI()
    {
        if (collisionText != null)
        {
            collisionText.text = $"Choques: {collisionCount}";
        }
    }
}
