using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static PlayerId;

public class ScoreFlag : NetworkBehaviour
{
    public GameObject BlueFlag;
    public GameObject RedFlag;

    public int RedPoints = 0;
    public int BluePoints = 0;

    public TEAM Team;

    private bool _scored;
    private float _preTime;
    private List<Flag> _flagPlayers;
    private LobbyManager _networkManager;

    [Server]
    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "BlueFlag" && Team == TEAM.TWO && !_scored)
        {
            Score(collider);
            _scored = true;
            _preTime = Time.time;
        }
        else if(collider.tag == "RedFlag" && Team == TEAM.ONE && !_scored)
        {
            Score(collider);
            _scored = true;
            _preTime = Time.time;
        }
    }

    void Start()
    {
        _scored = false;
        _preTime = Time.time;
        _networkManager = GameObject.Find("LobbyManager").GetComponent<LobbyManager>();

        GetAllPlayers();

        CmdRespawnFlag();
    }

    void Update()
    {
        GetAllPlayers();

        if ((Time.time - _preTime) >= 1)
        {
            _scored = false;
        }
    }

    private void Score(Collider collider)
    {
        Destroy(collider.gameObject);
        NetworkServer.Destroy(collider.gameObject);
        CmdRespawnFlag();

        UpdatePoints();
    }

    private void UpdatePoints()
    {
        if (Team == TEAM.TWO)
        {
            RedPoints++;
            foreach (Flag f in _flagPlayers)
            {
                f.CmdUpdateRedPoints();        
            }               
        }
        else
        {
            BluePoints++;
            foreach (Flag f in _flagPlayers)
            {
                f.CmdUpdateBluePoints();
            }             
        }
    }

    [Command]
    private void CmdRespawnFlag()
    {
        if (Team == TEAM.ONE)
        {
            Transform t = GameObject.Find("RedFlagPos").transform;
            GameObject obj = Instantiate(RedFlag, t.position, t.rotation);
            if (isServer)
            {
                NetworkServer.Spawn(obj);
            }
            //else if (isClient)
            //{
            //    NetworkServer.SpawnWithClientAuthority(obj, connectionToClient);
            //}
        }
        else
        {
            Transform t = GameObject.Find("BlueFlagPos").transform;
            GameObject obj = Instantiate(BlueFlag, t.position, t.rotation);
            if (isServer)
            {
                NetworkServer.Spawn(obj);
            }
            //else if (isClient)
            //{
            //    NetworkServer.SpawnWithClientAuthority(obj, connectionToClient);
            //}
        }
    }

    private void GetAllPlayers()
    {
        _flagPlayers = new List<Flag>();
        foreach (GameObject p in _networkManager.Players)
        {
            if (p != null)
                _flagPlayers.Add(p.GetComponent<Flag>());
        }
    }
}
