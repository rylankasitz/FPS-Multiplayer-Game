using Prototype.NetworkLobby;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static PlayerId;

public class Health : NetworkBehaviour
{
    public Slider HealthUI;

    public GameObject Player;

    public float MaxHealth = 100;

    public Text CharacterName;

    [HideInInspector]
    public Transform[] BlueTeamSpawns;

    [HideInInspector]
    public Transform[] RedTeamSpawns;

    [HideInInspector]
    [SyncVar]
    public float TotalHealth = 100;

    [HideInInspector]
    [SyncVar]
    public PLAYERID PlayerID;

    [HideInInspector]
    [SyncVar]
    public TEAM Team;

    [HideInInspector]
    [SyncVar]
    public float Amount;


    void Start()
    {
        TotalHealth = MaxHealth;
    }

    void Update()
    {
        if (!isLocalPlayer) return;
        if (TotalHealth <= 0 || Input.GetKeyDown(KeyCode.K))
        {
            CmdRespawn();
        }
    }

    public void Spawn()
    {
        BlueTeamSpawns = GameObject.Find("BlueSpawns").GetComponentsInChildren<Transform>();
        RedTeamSpawns = GameObject.Find("RedSpawns").GetComponentsInChildren<Transform>();
        
        if (Team == TEAM.ONE)
        {
            transform.position = BlueTeamSpawns[Random.Range(0, BlueTeamSpawns.Length)].position;
        }
        else
        {
            transform.position = RedTeamSpawns[Random.Range(0, RedTeamSpawns.Length)].position;
        }

        Debug.Log("Spawning player " + PlayerID + " on team " + Team + " at position " + transform.position);
    }

    public void TakeDamage(float amount)
    {
        Debug.Log(PlayerID + " took " + amount + " damage");
        Amount = amount;
        CmdTakeDamage();
    }

    [Command]
    private void CmdTakeDamage()
    {
        RpcTakeDamage();
        TotalHealth = TotalHealth;
    }

    [ClientRpc]
    private void RpcTakeDamage()
    {
        HealthUI.value = (TotalHealth - Amount) / MaxHealth;
        TotalHealth -= Amount;
    }

    [Command]
    private void CmdRespawn()
    {
        gameObject.GetComponent<Health>().Spawn();
        gameObject.GetComponent<Flag>().RespawnFlag();
        TotalHealth = MaxHealth;
        RpcRespawn();
    }

    [ClientRpc]
    private void RpcRespawn()
    {
        Debug.Log("Player " + PlayerID + " died");
        gameObject.GetComponent<Health>().Spawn();
        TotalHealth = MaxHealth;
    }
}
