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

    private PlayerController player;
    private SpiderController spider;
    private CinemachineBrain cineBrain;
    private bool setfirstContollingPlayer;
    private bool canchange;

    private Vector3 dir;

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
        cineBrain = cam.GetComponent<CinemachineBrain>();
    }

    private void Update()
    {
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
                ChangeInControllCharacter();
            }
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

    public void PlayerSubscribe(PlayerController _player)
    {
        player = _player;
    }

    public void SpiderSubscribe(SpiderController _spider)
    {
        spider = _spider;
    }
}
