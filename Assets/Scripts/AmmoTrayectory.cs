using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoTrayectory : MonoBehaviour
{
    LineRenderer lineRenderer;
    int numPoints = 60;
    SlingShot slingShot;

    float timeBetweenPoints = 0.02f;
    public LayerMask CollidableLayers;

    Material ammoMaterial;
    public Material ammoHitMaterial;
    // Start is called before the first frame update
    void Start()
    {
        slingShot = GetComponent<SlingShot>();
        lineRenderer = GetComponent<LineRenderer>();
        ammoMaterial = lineRenderer.material;
    }

    // Update is called once per frame
    void Update()
    {
        var slingShot = GetComponent<SlingShot>();
        if (slingShot.currentAmmo && slingShot.isDrag)
            DrawTrayectory(slingShot);
        else if (slingShot.currentAmmo == null)
            lineRenderer.positionCount = 0;
    }

    void DrawTrayectory(SlingShot slingShot)
    {
        int linePoints = numPoints;
        lineRenderer.positionCount = numPoints;
        lineRenderer.SetPosition(0, slingShot.currentAmmo.transform.position);

        Vector3 force = slingShot.GetShootForce();
        Vector3 startingPosition = slingShot.currentAmmo.transform.position;
        Vector3 startingVelocity = force * (Time.fixedDeltaTime / slingShot.currentAmmo.GetComponent<Rigidbody>().mass);

        int i = 1;
        for (float t = Time.fixedDeltaTime; i < linePoints; t += timeBetweenPoints, i++)
        {
            Vector3 newPosition = startingPosition + t * startingVelocity;
            newPosition.y = startingPosition.y + startingVelocity.y * t + Physics.gravity.y / 2f  * t * t;
            lineRenderer.SetPosition(i, newPosition);

            if (Physics.OverlapSphere(newPosition, slingShot.currentAmmo.GetComponent<SphereCollider>().radius * slingShot.currentAmmo.transform.localScale.y, CollidableLayers).Length > 0)
            {
                lineRenderer.positionCount = i;
                break;
            }
        }
    }
}
