using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static PlayerId;


[NetworkSettings(channel = 1, sendInterval = 0.0f)]
public class CustomNetworkManager : NetworkManager
{
    public Transform Team1Spawn;
    public Transform Team2Spawn;

    public List<GameObject> Players = new List<GameObject>();

    //public GameObject BlueFlag;
    //public GameObject RedFlag;

    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

        Players.Add(player);

        if (CurrentPlayer < MaxPlayers)
        {
            player.GetComponent<Health>().PlayerID = (PLAYERID) CurrentPlayer;
            player.GetComponent<Health>().Team = (TEAM) (CurrentPlayer % 2);
            CurrentPlayer++;
        }

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId); 
    }

    /*public override void OnServerReady(NetworkConnection conn)
    {
        NetworkServer.SetClientReady(conn);

        GameObject bflag = Instantiate(BlueFlag, BlueFlag.transform.position, BlueFlag.transform.rotation);
        NetworkServer.Spawn(bflag);

        GameObject rflag = Instantiate(RedFlag, RedFlag.transform.position, RedFlag.transform.rotation);
        NetworkServer.Spawn(rflag);
    }*/
}
