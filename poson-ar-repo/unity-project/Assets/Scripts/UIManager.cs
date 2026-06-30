using UnityEngine;
using TMPro;

public enum AppLanguage { English, Sinhala }

// Requires TextMeshPro with a Sinhala-capable font asset
// (generate via Window > TextMeshPro > Font Asset Creator using a Noto Sans Sinhala TTF).
public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject arHud;
    [SerializeField] private InfoPanelUI infoPanel;
    [SerializeField] private TMP_Text languageToggleLabel;

    public static AppLanguage CurrentLanguage = AppLanguage.English;

    public void OnStartARButton()
    {
        startPanel.SetActive(false);
        arHud.SetActive(true);
    }

    public void ToggleLanguage()
    {
        CurrentLanguage = CurrentLanguage == AppLanguage.English
            ? AppLanguage.Sinhala
            : AppLanguage.English;
        languageToggleLabel.text = CurrentLanguage == AppLanguage.English ? "EN" : "සිං";
        BroadcastMessage("RefreshLanguage", SendMessageOptions.DontRequireReceiver);
    }
}

public class InfoPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text bodyText;

    public void ShowInfo(string english, string sinhala)
    {
        panelRoot.SetActive(true);
        bodyText.text = UIManager.CurrentLanguage == AppLanguage.English ? english : sinhala;
    }

    public void Hide() => panelRoot.SetActive(false);
}
