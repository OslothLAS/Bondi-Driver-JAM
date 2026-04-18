using UnityEngine;

public class PlayerSetup : MonoBehaviour
{
    public enum PlayerID { Player1, Player2, Player3, Player4 }
    public PlayerID playerID;

    private void Awake()
    {
        BondiController controller = GetComponent<BondiController>();
        Camera cam = GetComponentInChildren<Camera>();

        // CONFIGURACIÓN DE TECLAS Y CÁMARA SEGÚN ID
        switch (playerID)
        {
            case PlayerID.Player1:
                // J1: WASD + E
                controller.upKey = "<Keyboard>/w";
                controller.downKey = "<Keyboard>/s";
                controller.leftKey = "<Keyboard>/a";
                controller.rightKey = "<Keyboard>/d";
                controller.hornKey = "<Keyboard>/e";

                // Cámara: Arriba-Izquierda
                if (cam != null) cam.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                break;

            case PlayerID.Player2:
                // J2: IJKL + O
                controller.upKey = "<Keyboard>/i";
                controller.downKey = "<Keyboard>/k";
                controller.leftKey = "<Keyboard>/j";
                controller.rightKey = "<Keyboard>/l";
                controller.hornKey = "<Keyboard>/o";

                // Cámara: Arriba-Derecha
                if (cam != null) cam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
                break;

            case PlayerID.Player3:
                // J3: Flechas + Ctrl Derecho
                controller.upKey = "<Keyboard>/upArrow";
                controller.downKey = "<Keyboard>/downArrow";
                controller.leftKey = "<Keyboard>/leftArrow";
                controller.rightKey = "<Keyboard>/rightArrow";
                controller.hornKey = "<Keyboard>/rightCtrl";

                // Cámara: Abajo-Izquierda
                if (cam != null) cam.rect = new Rect(0, 0, 0.5f, 0.5f);
                break;

            case PlayerID.Player4:
                // J4: Numpad 8462 + *
                controller.upKey = "<Keyboard>/numpad8";
                controller.downKey = "<Keyboard>/numpad2";
                controller.leftKey = "<Keyboard>/numpad4";
                controller.rightKey = "<Keyboard>/numpad6";
                controller.hornKey = "<Keyboard>/numpadMultiply";

                // Cámara: Abajo-Derecha
                if (cam != null) cam.rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                break;
        }
    }
}