using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
public class CreditsRoller : MonoBehaviour
{
    [Header("Configuración")]
    public float speed = 50f;
    public string mainMenuSceneName = "Main Menu";
    public float endPositionY = 1500f;

    [Header("Referencias")]
    public RectTransform creditsContainer;

    void Update()
    {
        creditsContainer.anchoredPosition += Vector2.up * speed * Time.deltaTime;

        bool escapePressed = Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        if (creditsContainer.anchoredPosition.y > endPositionY || escapePressed || mouseClicked)
        {
            ReturnToMenu();
        }
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}