using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public enum PlayerID { Player1, Player2 }
    public PlayerID playerID;

    private void Awake()
    {
        BondiController controller = GetComponent<BondiController>();
        Camera cam = GetComponentInChildren<Camera>();

        if (playerID == PlayerID.Player1)
        {
            // Configura teclas para J1
            controller.upKey = "<Keyboard>/w";
            controller.downKey = "<Keyboard>/s";
            controller.leftKey = "<Keyboard>/a";
            controller.rightKey = "<Keyboard>/d";

            // Pantalla arriba
            if (cam != null) cam.rect = new Rect(0, 0.5f, 1, 0.5f);
        }
        else
        {
            // Configura teclas para J2
            controller.upKey = "<Keyboard>/upArrow";
            controller.downKey = "<Keyboard>/downArrow";
            controller.leftKey = "<Keyboard>/leftArrow";
            controller.rightKey = "<Keyboard>/rightArrow";

            // Pantalla abajo
            if (cam != null) cam.rect = new Rect(0, 0, 1, 0.5f);
        }
    }
}