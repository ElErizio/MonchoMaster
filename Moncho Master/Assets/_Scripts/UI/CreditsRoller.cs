using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CreditsRoller : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 60f;
    public string mainMenuSceneName = "Main Menu";
    public float endPositionY = 1500f;
    public float startPositionY = -1004;

    [Header("Referencias")]
    public RectTransform creditsContainer;

    void Start()
    {
        if (creditsContainer != null)
        {
            Vector2 startPos = creditsContainer.anchoredPosition;
            startPos.y = startPositionY;
            creditsContainer.anchoredPosition = startPos;
            Debug.Log("Créditos inicializados en posición: " + startPos.y);
        }
        else
        {
            Debug.LogError("creditsContainer no está asignado!");
        }
    }

    void Update()
    {
        if (creditsContainer == null) return;

        creditsContainer.anchoredPosition += Vector2.up * speed * Time.deltaTime;

        bool escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool anyKeyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;

        if (creditsContainer.anchoredPosition.y > endPositionY || escapePressed || mouseClicked || anyKeyPressed)
        {
            ReturnToMenu();
        }
    }

    void ReturnToMenu()
    {
        Debug.Log("Volviendo al menú principal");
        SceneManager.LoadScene(mainMenuSceneName);
    }
}