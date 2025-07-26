using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class Target : MonoBehaviour
{
    public int ID;
    public int points = 10;
    int health = 100;

    // Variables para el sistema de sonidos aleatorios
    private static float lastAlienSoundTime = 0f;
    private static int lastAlienSoundIndex = -1;
    private static float soundCooldown = 5f; // 5 segundos de espera
    private AudioSource[] alienSounds;

    public delegate void TargetDestroyedEventHandler (int id, int points);
    public event TargetDestroyedEventHandler OnTargetDestroy;
    
    void Start()
    {
        // Configurar sonidos de aliens
        SetupAlienSounds();
    }
    
    void SetupAlienSounds()
    {
        // Buscar los 4 sonidos de AlienSpeaking
        GameObject sfx = GameObject.Find("SFX");
        alienSounds = new AudioSource[4];
        
        // Configurar los 4 sonidos de AlienSpeaking
        for (int i = 1; i <= 4; i++)
        {
            Transform alienSound = sfx.transform.Find($"AlienSpeaking{i}");
            if (alienSound != null)
            {
                alienSounds[i-1] = alienSound.GetComponent<AudioSource>();
                if (alienSounds[i-1] != null)
                {
                    alienSounds[i-1].playOnAwake = false;
                }
            }
        }
    }
    
    public void ReceiveDamage(int damage, Vector3 shootOrigin)
    {
        Debug.Log("Alien recibe daño");
        health -= damage;
        if (health <= 0)
        {
            Debug.Log("Alien destruido");
            OnTargetDestroy?.Invoke(ID, GetPointsAccordingDistance(shootOrigin));
            OnTargetDestroy = null;
            PlayRandomAlienSound();
            Destroy(this.gameObject);
        }
    }
    
    void PlayRandomAlienSound()
    {
        // Verificar si han pasado 5 segundos desde el último sonido
        if (Time.time - lastAlienSoundTime < soundCooldown)
        {
            return; // No reproducir sonido si no han pasado 5 segundos
        }
        
        // Encontrar un sonido válido
        List<int> availableSounds = new List<int>();
        for (int i = 0; i < alienSounds.Length; i++)
        {
            if (alienSounds[i] != null && i != lastAlienSoundIndex)
            {
                availableSounds.Add(i);
            }
        }
        
        // Si no hay sonidos disponibles, no reproducir nada
        if (availableSounds.Count == 0)
        {
            return;
        }
        
        // Seleccionar un sonido aleatorio
        int randomIndex = availableSounds[Random.Range(0, availableSounds.Count)];
        alienSounds[randomIndex].Play();
        
        // Actualizar variables de control
        lastAlienSoundTime = Time.time;
        lastAlienSoundIndex = randomIndex;
    }

    int GetPointsAccordingDistance(Vector3 shootOrigin)
    {
        float distance = Vector3.Distance(transform.position, shootOrigin);
        
        // Sistema de bonos por distancia mejorado
        if (distance < 1.0f)
        {
            // Tiro cercano: 10 puntos base
            return 10;
        }
        else if (distance < 2.0f)
        {
            // Tiro medio: 15 puntos base
            return 15;
        }
        else if (distance < 3.5f)
        {
            // Tiro lejano: 25 puntos base
            return 25;
        }
        else
        {
            // Tiro muy lejano: 40 puntos base
            return 40;
        }
    }
}