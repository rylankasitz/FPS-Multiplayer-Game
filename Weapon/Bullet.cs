using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static PlayerId;

public class Bullet : NetworkBehaviour
{
    public float Damage = 15.0f;
    public float DespawnTime = 15.0f;
    public ParticleSystem HitObjParticleSystem;

    [HideInInspector]
    public CharacterWeapon SentPlayer;

    private PLAYERID _playerId;
    private TEAM _team;
    private bool _hit;

    [Server]
    void OnTriggerEnter(Collider collider)
    {
        Health health = collider.GetComponentInParent<Health>();

        if (collider.tag == "PlayerCollision" && health.PlayerID != _playerId && !_hit)
        {
            _hit = true;

            float multiplier = 1f;

            if (collider.name == "head")
                multiplier = 2f;

            SentPlayer.HitDamage = Damage * multiplier;
            if (health.TotalHealth <= Damage * multiplier)
            {                
                SentPlayer.CmdUpdateFatalHitUI();
            }
            else
            {
                SentPlayer.CmdUpdateHitUI();
            }

            Destroy(gameObject);
            NetworkServer.Destroy(gameObject);

            health.TakeDamage(Damage * multiplier);
        }
        if (collider.tag == "DestroyableObj" && !_hit)
        {
            _hit = true;
            Destroy(gameObject);
            NetworkServer.Destroy(gameObject);

            collider.transform.GetComponent<ObjectHealth>().TakeDamage(Damage);
        }
    }

    void Start()
    {
        _hit = false;
    }

    public void SetID(PLAYERID id)
    {
        _playerId = id;
    }

    public void SetTeam(TEAM team)
    {
        _team = team;
    }
}
