using UnityEngine;
using TMPro;

public class PassengerController : MonoBehaviour
{
    [Header("Configuracion de Capacidad")]
    public int maxCapacity = 30;
    
    [Header("UI Reference")]
    public TMP_Text passengerUI;
    public TMP_Text statusUI;

    private int confirmedPassengers = 0;
    private int pendingPassengers = 0;

    void Start()
    {
        UpdateUI();
        SetStatusText(""); // Empezamos con el estado vacio
    }

    public void SetStatusText(string text)
    {
        if (statusUI != null)
        {
            statusUI.text = text;
        }
    }

    public void AddPending(int count)
    {
        // Solo sumamos si no excedemos la capacidad maxima
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

    public int GetRemainingCapacity()
    {
        return maxCapacity - confirmedPassengers;
    }

    public int GetCurrentPassengers()
    {
        return confirmedPassengers;
    }

    public void RemovePending(int count)
    {
        if (confirmedPassengers >= count)
        {
            confirmedPassengers -= count;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (passengerUI != null)
        {
            passengerUI.text = $"{confirmedPassengers + pendingPassengers}/{maxCapacity}";
        }
    }
}
