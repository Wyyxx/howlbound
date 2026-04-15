using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OptionsManager : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelGeneral;
    public GameObject panelControles;

    [Header("Botones Pestañas")]
    public Button btnGeneral;
    public Button btnControles;

    [Header("General")]
    public Button btnPantallaCompleta;
    public Slider sliderVolumen;

    // Colores pestañas
    private Color tabActivo = new Color(0.2f, 0.5f, 0.6f, 1f);
    private Color tabInactivo = new Color(0.15f, 0.15f, 0.15f, 1f);

    private bool isFullscreen = true;

    void Start()
    {
        // Pestañas
        btnGeneral.onClick.AddListener(() => MostrarPanel(0));
        btnControles.onClick.AddListener(() => MostrarPanel(1));

        // Pantalla completa
        btnPantallaCompleta.onClick.AddListener(TogglePantallaCompleta);

        // Volumen
        sliderVolumen.onValueChanged.AddListener(SetVolumen);

        // Cargar valores guardados
        CargarOpciones();

        // Mostrar General por defecto
        MostrarPanel(0);
    }

    void MostrarPanel(int index)
    {
        panelGeneral.SetActive(index == 0);
        panelControles.SetActive(index == 1);

        btnGeneral.GetComponent<Image>().color = index == 0 ? tabActivo : tabInactivo;
        btnControles.GetComponent<Image>().color = index == 1 ? tabActivo : tabInactivo;
    }

    void TogglePantallaCompleta()
    {
        isFullscreen = !isFullscreen;
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);

        var texto = btnPantallaCompleta.GetComponentInChildren<TextMeshProUGUI>();
        texto.text = isFullscreen ? "Pantalla Completa: ON" : "Pantalla Completa: OFF";
    }

    void SetVolumen(float valor)
    {
        AudioListener.volume = valor;
        PlayerPrefs.SetFloat("Volumen", valor);
    }

    void CargarOpciones()
    {
        isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;

        var texto = btnPantallaCompleta.GetComponentInChildren<TextMeshProUGUI>();
        texto.text = isFullscreen ? "Pantalla Completa: ON" : "Pantalla Completa: OFF";

        sliderVolumen.value = PlayerPrefs.GetFloat("Volumen", 1f);
        AudioListener.volume = sliderVolumen.value;
    }

    public void CerrarOpciones()
    {
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
}