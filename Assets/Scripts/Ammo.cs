using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : MonoBehaviour
{    
    float duration = 3;
    public bool shooted;
    public Vector3 shootOrigin;
    public delegate void HitEventHandler();
    public event HitEventHandler OnAmmoHit;
    public delegate void HitTargetEventHandler(int id);
    public event HitTargetEventHandler OnAmmoHitTarget;
    public GameObject explosionPrefab;
    AudioSource ammoFallingSound;
    AudioSource ammoHittingGround;

    // Rebote
    public int maxBounces = 3;
    int bounceCount = 0;

    void Start()
    {
        ammoFallingSound = GameObject.Find("SFX").transform.Find("FallingDown").GetComponent<AudioSource>();
        ammoHittingGround = GameObject.Find("SFX").transform.Find("HittingGround").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shooted)
        {
            transform.Rotate(360 *Time.deltaTime , 0, 0);
            duration -= Time.deltaTime;
            if (duration <= 0)
            {
                ammoFallingSound.Play();
                AmmoMissed(); // Usar AmmoMissed cuando cae al vacío
                shooted = false;
            }
        }
    }

    void AmmoHit()
    {
        OnAmmoHit?.Invoke();
        if (explosionPrefab)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        OnAmmoHit = null;
        Destroy(gameObject);
    }
    
    // Método para cuando la esfera cae al vacío (pierde munición)
    void AmmoMissed()
    {
        // Solo perder munición cuando cae al vacío
        SlingShot slingShot = FindObjectOfType<SlingShot>();
        if (slingShot != null)
        {
            slingShot.NotifyAmmoLost();
        }
        
        OnAmmoHit?.Invoke();
        if (explosionPrefab)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        OnAmmoHit = null;
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!shooted)
            return;
            
        // Detectar colisión con alien malo
        BadAlien badAlien = other.GetComponent<BadAlien>();
        if (badAlien != null)
        {
            OnAmmoHitTarget?.Invoke(badAlien.ID);
            badAlien.ReceiveDamage(100, shootOrigin);            
            AmmoHit();
            return;
        }
        
        // Detectar colisión con alien bueno (por error)
        GoodAlien goodAlien = other.GetComponent<GoodAlien>();
        if (goodAlien != null)
        {
            Debug.Log("¡Has golpeado al alien bueno por error!");
            goodAlien.OnCapturedByPlayer();
            AmmoHit();
            return;
        }
        
        // Detectar colisión con Target (para compatibilidad con el sistema anterior)
        Target target = other.GetComponent<Target>();
        if (target != null)
        {
            OnAmmoHitTarget?.Invoke(target.ID);
            target.ReceiveDamage(100, shootOrigin);            
            AmmoHit();
            return;
        }
        
        // Si no es ningún alien, reproducir sonido de impacto
        ammoHittingGround.Play();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!shooted)
            return;
            
        // Rebote: solo cuenta si no es un alien o target
        if (collision.gameObject.GetComponent<BadAlien>() == null && 
            collision.gameObject.GetComponent<GoodAlien>() == null &&
            collision.gameObject.GetComponent<Target>() == null)
        {
            bounceCount++;
            if (bounceCount >= maxBounces)
            {
                AmmoMissed(); // Usar AmmoMissed cuando rebota demasiado
            }
        }
    }
}
