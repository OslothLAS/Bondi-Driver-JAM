using UnityEngine;
using TMPro;

public class DamageController : MonoBehaviour
{
    [Header("Referencias UI")]
    public TMP_Text collisionText;

    private int collisionCount = 0;

    void Start()
    {
        UpdateUI();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground")) return;

        collisionCount++;
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
