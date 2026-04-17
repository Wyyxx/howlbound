using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class BossWinScreenManager : MonoBehaviour
{
    public static BossWinScreenManager Instance;

    [Header("Referencias UI")]
    public GameObject bossWinPanel;
    public Button mainMenuButton;
    public Button continueButton;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (bossWinPanel != null)
            bossWinPanel.SetActive(false);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(OnMainMenuPressed);

        if (continueButton != null)
            continueButton.onClick.AddListener(OnContinuePressed);
    }

    public void ShowBossWinScreen()
    {
        if (bossWinPanel != null)
        {
            bossWinPanel.SetActive(true);
            Debug.Log("<color=yellow>¡Jefe Derrotado!</color>");
        }
    }

    void OnMainMenuPressed()
    {
        if (PlayerRunData.Instance != null)
            Destroy(PlayerRunData.Instance.gameObject);

        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    void OnContinuePressed()
    {
        // Regresa al mapa para continuar la aventura
        if (MapManager.Instance != null)
            MapManager.Instance.ReturnToMap("CombatScene");
        else
            SceneManager.LoadScene("Mapa", LoadSceneMode.Single);
    }
}