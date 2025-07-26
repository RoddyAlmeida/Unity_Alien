using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class BadAlien : MonoBehaviour
{
    public int ID;
    public float moveSpeed = 0.005f; // Muy lento para acercarse gradualmente
    public float rotationSpeed = 25f;
    public float detectionRange = 5f; // Rango de detección más amplio
    public float startDelay = 1f; // Delay más corto
    public float approachSpeed = 0.003f; // Velocidad de acercamiento al alien bueno
    public float bounceDistance = 0.5f; // Distancia de rebote cuando golpea al alien bueno
    public float bounceSpeed = 0.01f; // Velocidad de rebote
    
    private Vector3 destination;
    private bool hasDestination = false;
    private ARPlane movePlane;
    private float planeRange;
    private float colliderHeight;
    private bool isMoving = false;
    private GoodAlien targetGoodAlien;
    private float startTime; // Nuevo: para el delay
    private bool isBouncing = false; // Estado de rebote
    private Vector3 bounceTarget; // Posición objetivo del rebote
    
    // Evento para cuando el alien malo es capturado por el jugador
    public delegate void BadAlienCapturedEventHandler(int id, int points);
    public event BadAlienCapturedEventHandler OnBadAlienCaptured;
    
    void Start()
    {
        // Configurar el alien malo
        colliderHeight = transform.localScale.y * GetComponent<CapsuleCollider>().height;
    }
    
    public void StartMoving(ARPlane plane, GoodAlien goodAlien)
    {
        movePlane = plane;
        targetGoodAlien = goodAlien;
        planeRange = Mathf.Max(plane.size.x, plane.size.y);
        
        // Posicionar el alien malo más lejos del centro (fuera del plano)
        float spawnDistance = planeRange * 1.5f; // 1.5 veces el tamaño del plano
        Vector3 randomDirection = Random.insideUnitSphere.normalized;
        randomDirection.y = 0; // Mantener en el plano horizontal
        Vector3 spawnPosition = plane.center + randomDirection * spawnDistance;
        spawnPosition.y = plane.center.y + colliderHeight / 2;
        
        transform.position = spawnPosition;
        hasDestination = RandomPoint(plane.center, 0.5f, planeRange, out destination);
        isMoving = true;
        isBouncing = false;
        startTime = Time.time; // Nuevo: registrar el tiempo de inicio
    }
    
    void Update()
    {
        if (!isMoving || targetGoodAlien == null) return;
        
        // Esperar el delay antes de empezar a acercarse
        if (Time.time - startTime < startDelay)
        {
            // Solo moverse aleatoriamente durante el delay
            if (hasDestination)
            {
                // Rotar hacia el destino
                Vector3 direction = (destination - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
                
                // Mover hacia el destino
                transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * 0.5f);
                
                if (Vector3.Distance(transform.position, destination) < 0.01f)
                {
                    hasDestination = false;
                }
            }
            else
            {
                // Buscar nuevo destino aleatorio
                hasDestination = RandomPoint(movePlane.center, 0.5f, planeRange, out destination);
            }
            return; // No acercarse durante el delay
        }
        
        // Si está rebotando, manejar el rebote
        if (isBouncing)
        {
            transform.position = Vector3.MoveTowards(transform.position, bounceTarget, bounceSpeed);
            
            if (Vector3.Distance(transform.position, bounceTarget) < 0.01f)
            {
                isBouncing = false;
            }
            return;
        }
        
        // Después del delay, acercarse lentamente al alien bueno
        float distanceToGoodAlien = Vector3.Distance(transform.position, targetGoodAlien.transform.position);
        
        // Rotar hacia el alien bueno
        Vector3 directionToGoodAlien = (targetGoodAlien.transform.position - transform.position).normalized;
        if (directionToGoodAlien != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToGoodAlien);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // Acercarse lentamente al alien bueno
        transform.position = Vector3.MoveTowards(transform.position, targetGoodAlien.transform.position, approachSpeed);
        
        // Si está muy cerca, golpear al alien bueno y rebotar
        if (distanceToGoodAlien < 0.3f)
        {
            // Golpear al alien bueno
            targetGoodAlien.OnCapturedByBadAlien();
            
            // Calcular posición de rebote (alejarse del alien bueno)
            Vector3 bounceDirection = (transform.position - targetGoodAlien.transform.position).normalized;
            bounceTarget = targetGoodAlien.transform.position + bounceDirection * bounceDistance;
            bounceTarget.y = transform.position.y; // Mantener la altura
            
            isBouncing = true;
            return;
        }
    }
    
    bool RandomPoint(Vector3 center, float rayYoffset, float range, out Vector3 result)
    {
        Vector3 next = center + Random.insideUnitSphere * range;
        RaycastHit hit;
        
        if (Physics.Raycast(next, Vector3.down, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject == movePlane.gameObject)
            {
                result = hit.point + Vector3.up * colliderHeight / 2;
                return true;
            }
        }
        
        result = Vector3.zero;
        return false;
    }
    
    public void StopMoving()
    {
        isMoving = false;
    }
    
    // Método para cuando el alien malo es capturado por el jugador
    public void OnCapturedByPlayer(int points)
    {
        Debug.Log("¡Alien malo capturado!");
        OnBadAlienCaptured?.Invoke(ID, points);
        Destroy(gameObject);
    }
    
    // Método para recibir daño (cuando la esfera lo golpea)
    public void ReceiveDamage(int damage, Vector3 shootOrigin)
    {
        Debug.Log("Alien malo recibe daño");
        float distance = Vector3.Distance(transform.position, shootOrigin);
        
        // Calcular puntos según distancia (igual que el sistema anterior)
        int points;
        if (distance < 1.0f)
        {
            points = 10;
        }
        else if (distance < 2.0f)
        {
            points = 15;
        }
        else if (distance < 3.5f)
        {
            points = 25;
        }
        else
        {
            points = 40;
        }
        
        OnCapturedByPlayer(points);
    }
} 