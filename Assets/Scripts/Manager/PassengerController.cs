using UnityEngine;
using TMPro;

public class PassengerController : MonoBehaviour
{
    [Header("Configuracion de Capacidad")]
    public int maxCapacity = 30;

    [Header("Configuracion de Puntos")]
    [Tooltip("Puntos otorgados por cada pasajero que llega a destino")]
    public int pointsPerPassenger = 100;
    private int currentScore = 0;

    [Header("UI Reference")]
    public TMP_Text passengerUI;
    public TMP_Text statusUI;
    public TMP_Text scoreUI; // Nuevo: Arrastrá acá el texto del puntaje

    private int confirmedPassengers = 0;
    private int pendingPassengers = 0;

    void Start()
    {
        UpdateUI();
        UpdateScoreUI();
        SetStatusText("");
    }

    public void SetStatusText(string text)
    {
        if (statusUI != null) statusUI.text = text;
    }

    public void AddPending(int count)
    {
        if (confirmedPassengers + pendingPassengers + count <= maxCapacity)
        {
            pendingPassengers += count;
            UpdateUI();
        }
    }

    public void CommitPending()
    {
        confirmedPassengers += pendingPassengers;
        pendingPassengers = 0;
        UpdateUI();
        Debug.Log($"Pasajeros confirmados. Total: {confirmedPassengers}/{maxCapacity}");
    }

    public void ClearPending()
    {
        if (pendingPassengers > 0)
        {
            Debug.Log($"Subida interrumpida. Se perdieron {pendingPassengers} pasajeros.");
            pendingPassengers = 0;
            UpdateUI();
        }
    }

    // --- LOGICA DE PUNTOS ---

    public void RemovePending(int count)
    {
        if (confirmedPassengers >= count)
        {
            confirmedPassengers -= count;

            // Sumamos los puntos: pasajeros x valor
            AddScore(count * pointsPerPassenger);

            UpdateUI();
        }
    }

    private void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
        Debug.Log($"<color=yellow>Puntaje: {currentScore} (+{amount})</color>");
    }

    private void UpdateScoreUI()
    {
        if (scoreUI != null)
        {
            // Formateamos con ceros a la izquierda para que parezca arcade (opcional)
            scoreUI.text = $"PUNTOS: {currentScore:D6}";
        }
    }

    // --- GETTERS ---

    public int GetRemainingCapacity() => maxCapacity - confirmedPassengers;
    public int GetCurrentPassengers() => confirmedPassengers;

    private void UpdateUI()
    {
        if (passengerUI != null)
        {
            passengerUI.text = $"{confirmedPassengers + pendingPassengers}/{maxCapacity}";
        }
    }

    // Agregá esto en PassengerController.cs para que el GameManager pueda leer los puntos
    public int GetScore() => currentScore;
}