using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    [SerializeField] private bool controllPlayer = true;
    [SerializeField] private float minRangeToPlayer = 3.0f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private Camera cam;
    [SerializeField] private Menu pauseMenu;
    [SerializeField] private float deathTimer = 3.0f;
     
    private PlayerController player;
    private SpiderController spider;
    private CinemachineBrain cineBrain;
    private bool setfirstContollingPlayer;
    private bool canchange;

    private Vector3 dir;

    private bool paused;
    private bool isdead;
    private bool switching;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != null)
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        pauseMenu.OpenMainMenu(false);
        Cursor.lockState = CursorLockMode.Locked;
        cineBrain = cam.GetComponent<CinemachineBrain>();
    }

    private void Update()
    {
        if (isdead) return;

        if (!setfirstContollingPlayer)
        {
            setfirstContollingPlayer = true;
            if (controllPlayer)
            {
                SetPlayerControlling();
            }
            else
            {
                SetSpiderControlling();
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            if (canchange)
            {
                if (!switching)
                {
                    StartCoroutine(C_SwitchAnim());
                }
                
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }

        canchange = CheckIfCanSwitchToPLayer();
        HUD.Instance.SetCanChange(canchange);
    }

    private void ChangeInControllCharacter()
    {
        controllPlayer = !controllPlayer;

        if (controllPlayer)
        {
            SetPlayerControlling();
        }
        else
        {
            SetSpiderControlling();
        }
    }

    private IEnumerator C_SwitchAnim()
    {
        switching = true;
        HUD.Instance.Switch();
        yield return new WaitForSecondsRealtime(0.2f);
        ChangeInControllCharacter();
        yield return new WaitForSecondsRealtime(0.2f);
        switching = false;
    }

    private void SetPlayerControlling()
    {
        player.SetPlayerControlling(true);
        cineBrain.enabled = false;
        cam.transform.parent = player.GetCamParent();
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localRotation = Quaternion.identity;
        //spider.transform.position = player.transform.position;
        spider.GiveUpControll();
    }

    private void SetSpiderControlling()
    {
        player.SetPlayerControlling(false);
        spider.SetControll();
        cam.transform.parent = null;
        cineBrain.enabled = true;
    }

    private bool CheckIfCanSwitchToPLayer()
    {
        dir = player.transform.position - spider.transform.position;

        if (dir.sqrMagnitude <= minRangeToPlayer * minRangeToPlayer)
        {
            if (Physics.Raycast(spider.transform.position, dir, minRangeToPlayer, playerLayer))
            {
                return true;
            }
            else return false;
        }
        else return false;
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

    public void PlayerSubscribe(PlayerController _player)
    {
        player = _player;
    }

    public void SpiderSubscribe(SpiderController _spider)
    {
        spider = _spider;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
