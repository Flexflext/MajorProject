using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private int levelSceneIndex;
    [SerializeField] private int mainSceneIndex;

    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject deathMenu;

    private ConfirmPanel confirmPanel;
    private PersonelAudioManager personelAudioManager;

    // Start is called before the first frame update
    void Awake()
    {
        personelAudioManager = GetComponent<PersonelAudioManager>();
        confirmPanel = GetComponentInChildren<ConfirmPanel>();
        confirmPanel.gameObject.SetActive(false);
    }


    public void OpenMainMenu(bool _tosetto)
    {
        confirmPanel.gameObject.SetActive(false);
        mainMenu.SetActive(_tosetto);
    }
    
    public void OpenSettings()
    {
        personelAudioManager.Play(EPossibleSounds.UI, ERandomSound.Static, true);
        mainMenu.gameObject.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        personelAudioManager.Play(EPossibleSounds.UI, ERandomSound.Static, true);
        mainMenu.gameObject.SetActive(true);
        settingsMenu.SetActive(false);
    }

    public void QuitButton()
    {
        personelAudioManager.Play(EPossibleSounds.UI, ERandomSound.Static, true);
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.SetPanel("Really Quit?", OnQuitConfirm);
    }

    public void PlayButton()
    {
        personelAudioManager.Play(EPossibleSounds.UI, ERandomSound.Static, true);
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.SetPanel("Play Level?", OnPlayConfirm);
    }

    public void MainMenuButton()
    {
        personelAudioManager.Play(EPossibleSounds.UI, ERandomSound.Static, true);
        confirmPanel.gameObject.SetActive(true);
        confirmPanel.SetPanel("Return to Menu?", OnMenuConfirm);
    }

    public void OpenDeathScreen()
    {
        mainMenu.SetActive(false);
        deathMenu.SetActive(true);
    }

    public void SetMasterVolumeAmount(float _sliderfloat)
    {
        AudioMixerManager.Instance.SetMasterVolume(_sliderfloat);
    }

    public void SetSFXVolumeAmount(float _sliderfloat)
    {
        AudioMixerManager.Instance.SetSFXVolume(_sliderfloat);
    }

    public void SetAmbientVolumeAmount(float _sliderfloat)
    {
        AudioMixerManager.Instance.SetAmbientVolume(_sliderfloat);
    }

    private void OnPlayConfirm(bool _confirm)
    {
        personelAudioManager.Play(EPossibleSounds.MagicSelection, ERandomSound.Static, true);
        if (!_confirm) return;
        loadingPanel.SetActive(true);
        SceneManager.LoadScene(levelSceneIndex);
    }

    private void OnMenuConfirm(bool _confirm)
    {
        personelAudioManager.Play(EPossibleSounds.hover, ERandomSound.Static, true);
        if (!_confirm) return;
        loadingPanel.SetActive(true);
        SceneManager.LoadScene(mainSceneIndex);
    }

    private void OnQuitConfirm(bool _confirm)
    {
        personelAudioManager.Play(EPossibleSounds.hover, ERandomSound.Static, true);
        if (!_confirm) return;
        Application.Quit();

        


#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
    }

}
