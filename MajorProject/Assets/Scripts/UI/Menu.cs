using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private int levelSceneIndex;
    [SerializeField] private int mainSceneIndex;

    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject deathMenu;

    private ConfirmPanel confirmPanel;

    // Start is called before the first frame update
    void Awake()
    {
        confirmPanel = GetComponentInChildren<ConfirmPanel>();
        confirmPanel.gameObject.SetActive(false);
    }


    public void OpenMainMenu(bool _tosetto)
    {
        mainMenu.SetActive(_tosetto);
    }
    
    public void OpenSettings()
    {
        mainMenu.gameObject.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        mainMenu.gameObject.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void QuitButton()
    {
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.SetPanel("Really Quit?", OnQuitConfirm);
    }

    public void PlayButton()
    {
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.SetPanel("Play Level?", OnPlayConfirm);
    }

    public void MainMenuButton()
    {
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.SetPanel("Return to Menu?", OnMenuConfirm);
    }

    public void OpenDeathScreen()
    {
        mainMenu.SetActive(false);
        deathMenu.SetActive(true);
    }

    private void OnPlayConfirm(bool _confirm)
    {
        if (!_confirm) return;
        SceneManager.LoadScene(levelSceneIndex);
    }

    private void OnMenuConfirm(bool _confirm)
    {
        if (!_confirm) return;
        SceneManager.LoadScene(mainSceneIndex);
    }

    private void OnQuitConfirm(bool _confirm)
    {
        if (!_confirm) return;
        Application.Quit();
    }

}
