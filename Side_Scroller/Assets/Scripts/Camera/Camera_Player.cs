using UnityEngine;

public class Camera_Player : MonoBehaviour
{
    
    public Transform target;
    public float smoothTime = 0.2f;
    public Vector2 offset = new Vector2(0f, 0f);
    public float minZ = -1f;
    public float maxZ = -2f;
    public float maxX = 6f;
    public float minX = 4f;
    public float minY = 0f;
    public float maxY = 1f;

    Vector3 velocity = Vector3.zero;

    

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;

        Vector3 currentPosition = transform.position; 
        Vector3 targetPosition = target.position; 
        targetPosition.x += offset.x; 
        targetPosition.y += offset.y; 

        // Clamp la cible sur l'axe Z entre minZ et maxZ
        targetPosition.z = Mathf.Clamp(targetPosition.z, minZ, maxZ);

        // SmoothDamp en 3D pour préserver et gérer correctement Z
        Vector3 newPosition = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime);

        // S'assurer qu'on reste strictement entre minZ et maxZ (évite tout dépassement)
        newPosition.z = Mathf.Clamp(newPosition.z, minZ, maxZ);

        transform.position = newPosition;
        if (transform.position.x > maxX)
        {
            transform.position = new Vector3(maxX, transform.position.y, transform.position.z);
            _ = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime);
            // Renommer les variables pour éviter le conflit
            Vector3 currentPositionX = transform.position;
            Vector3 targetPositionX = target.position;
        }
        if (transform.position.x < minX)
        {
            transform.position = new Vector3(minX, transform.position.y, transform.position.z);
            _ = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime);
        }
        if (transform.position.y > maxY)
        {
            transform.position = new Vector3(transform.position.x, maxY, transform.position.z);
            _ = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime);
        }
        if (transform.position.y < minY)
        {
            transform.position = new Vector3(transform.position.x, minY, transform.position.z);
            _ = Vector3.SmoothDamp(currentPosition, targetPosition, ref velocity, smoothTime); 
        }
    }

}
