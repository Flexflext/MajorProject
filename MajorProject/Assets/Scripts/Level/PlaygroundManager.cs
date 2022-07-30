using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaygroundManager : MonoBehaviour
{
    public static PlaygroundManager Instance;
    [SerializeField] private float deathTimer;
    [SerializeField] private Menu pauseMenu;
    [SerializeField] private Cinemachine.CinemachineVirtualCamera cam;
    [SerializeField] private List<SpiderPlayGroundManager> spiders = new List<SpiderPlayGroundManager>();
    private Cinemachine.CinemachineTransposer camTansposer;
    

    private int curSpiderIndex = 0;
    private bool paused;
    private bool isdead;

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

    }

    private void Start()
    {
        Time.timeScale = 1;
        pauseMenu.OpenMainMenu(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && !isdead && !paused)
        {
            SwitchToNextSpider();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && !isdead)
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

    public void SetDeath()
    {
        isdead = true;
        StartCoroutine(C_DeathTimer());
    }

    public void SetDeathNoTimer()
    {
        isdead = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenu.OpenDeathScreen();
    }

    private IEnumerator C_DeathTimer()
    {
        yield return new WaitForSeconds(deathTimer);

        float curTime = 1;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        pauseMenu.OpenDeathScreen();

        while (curTime > 0)
        {
            if (curTime <= 0)
            {
                curTime = 0;
            }

            Time.timeScale = curTime;
            yield return null;
            curTime -= Time.deltaTime;
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
