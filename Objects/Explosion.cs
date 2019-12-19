using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Explosion : NetworkBehaviour
{
    public float Radius = 5f;
    public float Power = 10f;
    public float MaxDamage = 50f;

    public GameObject ExplosionParticleSystem;


    void OnDestroy()
    {
        PlayExplosion();
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, Radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            CharacterController cc = hit.GetComponent<CharacterController>();

            if (rb != null)
            {
                rb.AddExplosionForce(Power, explosionPos, Radius, 3.0f);
                if (rb.GetComponent<ObjectHealth>() != null)
                {
                    Vector3 posDiff = rb.transform.position - explosionPos;
                    rb.GetComponent<ObjectHealth>().TakeDamage(MaxDamage * (posDiff.magnitude / Radius));
                }
            }     
            
            if (cc != null)
            {
                float force = Mathf.Clamp(Power / 3, 0, 15);
                Vector3 posDiff = cc.transform.position - explosionPos;
                cc.GetComponent<CharacterMovement>().AddImpact(posDiff, force);
                cc.GetComponent<Health>().TakeDamage(MaxDamage * (posDiff.magnitude / Radius));
            }
        }
    }

    private void PlayExplosion()
    {
        GameObject obj = Instantiate(ExplosionParticleSystem, transform.position, transform.rotation);
        Destroy(obj, 5);
    }
}
