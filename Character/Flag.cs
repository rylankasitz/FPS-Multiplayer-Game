using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static PlayerId;

public class Flag : NetworkBehaviour
{
    #region Public Variables

    #region Visible Variables

    public float MovementMultiplier;

    public GameObject FlagUI;
    public GameObject BlueFlag;
    public GameObject RedFlag;
    public GameObject NotificationUI;

    public Slider BlueTeamScoreUI;
    public Slider RedTeamScoreUI;

    public Image BlueTeamUI;
    public Image RedTeamUI;

    public Text PlayerText;

    public int MaxScore = 2;

    [SyncVar]
    public int RedPoints = 0;

    [SyncVar]
    public int BluePoints = 0;

    #endregion

    #region Hidden Variables

    [HideInInspector]
    public Transform BlueFlagPosition;
    [HideInInspector]
    public Transform RedFlagPosition;

    #endregion

    #endregion

    #region Public Variables

    private TEAM _team;

    private Collider _collider;

    private bool _hasFlag;
    private bool _inRange;
    private bool _toStartPos;

    private ScoreFlag _redScore;
    private ScoreFlag _blueScore;

    private CharacterMovement _characterMovement;

    #endregion

    #region Unity Methods

    void OnTriggerEnter(Collider collider)
    {
        if (((collider.tag == "BlueFlag" && _team == TEAM.TWO) || 
            (collider.tag == "RedFlag" && _team == TEAM.ONE)) && !_hasFlag)
        {     
            _collider = collider;
            _inRange = true;
            SetUI(true);
        }
    }

    void OnTriggerExit(Collider collider)
    {
        if ((collider.tag == "BlueFlag" || collider.tag == "RedFlag") && !_hasFlag)
        {
            _inRange = false;
            SetUI(false);
        }
    }

    public void RespawnFlag()
    {
        if (_hasFlag)
        {
            _toStartPos = true;
            CmdSpawnFlag();
        }
    }

    void Start()
    {
        _team = GetComponent<Health>().Team;

        if (!isLocalPlayer)
        {
            BlueFlagPosition = GameObject.Find("BlueFlagPos").transform;
            RedFlagPosition = GameObject.Find("RedFlagPos").transform;

            _redScore = GameObject.Find("FlagDropRed").GetComponent<ScoreFlag>();
            _blueScore = GameObject.Find("FlagDropBlue").GetComponent<ScoreFlag>();
        }
        else
        {
            _characterMovement = GetComponent<CharacterMovement>();
        }

        if (_team == TEAM.ONE)
        {
            BlueTeamUI.enabled = true;
        }
        else
        {
            RedTeamUI.enabled = true;
        }
    }


    void Update()
    {
        if (!isLocalPlayer) return;

        if (!_hasFlag && _inRange && Input.GetButtonDown(InputConstants.PICKUP))
        {
            TakeFlag(_collider);
        }

        if (Input.GetButtonDown(InputConstants.DROP_FLAG) && _hasFlag)
        {
            DropFlag();
        }

        UpdateScoreUI();
    }

    #endregion

    #region Public Methods

    [Command]
    public void CmdUpdateBluePoints()
    {
        RpcUpdateBluePoints();
    }

    [Command]
    public void CmdUpdateRedPoints()
    {
        RpcUpdateRedPoints();
    }

    #endregion

    #region Private Methods

    #region Flag Methods

    private void TakeFlag(Collider collider)
    {
        _hasFlag = true;
        SetFlagUI();
        CmdDestroyFlag();
        SetUI(false);
        _characterMovement.MoveSpeed *= MovementMultiplier;
        _characterMovement.LeftRightSpeed *= MovementMultiplier;
    }

    private void DropFlag()
    {
        _toStartPos = false;
        CmdSpawnFlag();
        _hasFlag = false;
        SetFlagUI();
        _characterMovement.MoveSpeed = _characterMovement.MoveSpeed / MovementMultiplier;
        _characterMovement.LeftRightSpeed = _characterMovement.LeftRightSpeed / MovementMultiplier;
    }

    [Command]
    private void CmdDestroyFlag()
    {
        Destroy(_collider.gameObject);
        NetworkServer.Destroy(_collider.gameObject);
    }

    [Command]
    private void CmdSpawnFlag()
    {
        Transform t = transform;
        if (_team == TEAM.TWO)
        {
            if (_toStartPos) t = BlueFlagPosition;
            GameObject flagObj = Instantiate(BlueFlag, t.position, t.rotation);
            flagObj.transform.localPosition += new Vector3(0, 1, 1);
            if (isServer)
            {
                NetworkServer.Spawn(flagObj);
            }
            else if (isClient)
            {
                NetworkServer.SpawnWithClientAuthority(flagObj, connectionToClient);
            }
            flagObj.GetComponents<BoxCollider>()[1].enabled = true;
            flagObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
        else
        {
            if (_toStartPos) t = RedFlagPosition;
            GameObject flagObj = Instantiate(RedFlag, t.position, t.rotation);
            flagObj.transform.localPosition += new Vector3(0, 1, 1);
            if (isServer)
            {
                NetworkServer.Spawn(flagObj);
            }
            else if (isClient)
            {
                NetworkServer.SpawnWithClientAuthority(flagObj, connectionToClient);
            }
            flagObj.GetComponents<BoxCollider>()[1].enabled = true;
            flagObj.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        }
    }

    [ClientRpc]
    public void RpcUpdateBluePoints()
    {
        Debug.Log("Blue team scored");

        BluePoints = GameObject.Find("FlagDropBlue").GetComponent<ScoreFlag>().BluePoints;
        SetUI(false);
        if (BluePoints == MaxScore) EndGame();
    }

    [ClientRpc]
    public void RpcUpdateRedPoints()
    {
        Debug.Log("Red team scored");

        RedPoints = GameObject.Find("FlagDropRed").GetComponent<ScoreFlag>().RedPoints;
        SetUI(false);
        if (RedPoints == MaxScore) EndGame();
    }

    private void EndGame()
    {
        LobbyManager lobby = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();
        RectTransform endscreen = lobby.SetEndScreen(true);
        string status = "DEFEAT";

        if(GetComponent<Health>().Team == TEAM.ONE)
        {
            if (BluePoints == MaxScore) status = "VICTORY";
        }
        else
        {
            if (RedPoints == MaxScore) status = "VICTORY";
        }

        endscreen.GetComponentsInChildren<Text>()[2].text = RedPoints + " points";
        endscreen.GetComponentsInChildren<Text>()[1].text = BluePoints + " points";
        endscreen.GetComponentsInChildren<Text>()[0].text = status;

        gameObject.SetActive(false);
    }


    #endregion

    #region UI Methods

    private void SetUI(bool state)
    {
        NotificationUI.SetActive(state);
        NotificationUI.GetComponentInChildren<Text>().text = "'C' TO PICK UP";
    }

    private void SetFlagUI()
    {
        FlagUI.GetComponentsInChildren<RawImage>()[Mathf.Abs(((int)_team) - 1)].enabled = _hasFlag;
    }

    private void UpdateScoreUI()
    {
        BlueTeamScoreUI.value = BluePoints;
        RedTeamScoreUI.value = RedPoints;
    }

    #endregion
    

    #endregion
}
