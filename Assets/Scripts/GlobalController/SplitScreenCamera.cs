using UnityEngine;
using UnityEngine.InputSystem;

public class SplitScreenCamera : MonoBehaviour
{


    void Start()
    {
        // 1. Buscamos el componente PlayerInput que Unity agrega al spawnear
        PlayerInput pi = GetComponentInParent<PlayerInput>();
        Camera cam = GetComponent<Camera>();

        if (pi != null && cam != null)
        {
            // 2. Configuramos la cámara según el índice del jugador
            // Player Index 0 = Jugador 1 | Player Index 1 = Jugador 2
            if (pi.playerIndex == 0)
            {
                // Mitad superior
                cam.rect = new Rect(0.5f, 0, 0.5f, 1);
            }
            else
            {
                cam.rect = new Rect(0.5f, 0, 1, 0.5f);
            }
        }
    }
}