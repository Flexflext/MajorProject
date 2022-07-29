using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaygroundManager : MonoBehaviour
{
    public static PlaygroundManager Instance;
    [SerializeField] private Menu pauseMenu;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera cam;
    [SerializeField] private List<SpiderPlayGroundManager> spiders = new List<SpiderPlayGroundManager>();
    private Cinemachine.CinemachineTransposer camTansposer;
    

    private int curSpiderIndex = 0;
    private bool paused;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(this.gameObject);
        }
        camTansposer = cam.GetCinemachineComponent<Cinemachine.CinemachineTransposer>();

        
        pauseMenu.OpenMainMenu(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SwitchToNextSpider();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void SwitchToNextSpider()
    {
        StopCurrentSpider();
        StartNewSpider();
    }

    private void StopCurrentSpider()
    {
        spiders[curSpiderIndex].StartStopSpider(false);
        curSpiderIndex++;
        if (curSpiderIndex == spiders.Count)
        {
            curSpiderIndex = 0;
        }
    }

    private void StartNewSpider()
    {
        cam.Follow = spiders[curSpiderIndex].followObj;
        cam.LookAt = spiders[curSpiderIndex].followObj;
        camTansposer.m_FollowOffset = spiders[curSpiderIndex].offset;

        spiders[curSpiderIndex].StartStopSpider(true);
    }

    private void TogglePauseMenu()
    {

        if (paused)
        {
            paused = false;
            pauseMenu.OpenMainMenu(false);
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            paused = true;
            pauseMenu.OpenMainMenu(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    }

    public void Subscribe(SpiderPlayGroundManager _spider)
    {
        spiders.Add(_spider);

        if (spiders.Count == 1)
        {
            StartNewSpider();
        }
        else
        {
            _spider.StartStopSpider(false);

        }
    }

    public void Unsubscribe(SpiderPlayGroundManager _spider)
    {
        spiders.Remove(_spider);
    }
}
