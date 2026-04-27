using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerAimAndShoot : MonoBehaviour
{
    [SerializeField] private GameObject gun;
    [SerializeField] private float _speed;
    [SerializeField] private Transform _gunPoint;
    [SerializeField] private GameObject _gun;
    [SerializeField] private GameObject Bullet_Trail;

    [SerializeField] private float ShootDelay = 0.2f; // cadence modifiable dans l'inspector
    private float currentShootDelay;

    private Vector2 worldPosition;
    private Vector2 direction;
    private float angle;

    private void Awake()
    {
        // s'assure que le timer commence à 0 (prêt à tirer)
        currentShootDelay = 0f;
    }

    private void Update()
    {
        // décrémente le timer en premier pour que Shoot() lise la valeur à jour
        if (currentShootDelay > 0f)
            currentShootDelay -= Time.deltaTime;

        HandleGunRotation();
        Shoot();
    }

    private void Shoot()
    {
        // ne tirer que si on maintient le bouton et si le timer est écoulé
        if (Input.GetMouseButton(0) && currentShootDelay <= 0f)
        {
            // réinitialise le timer de cadence après un tir
            currentShootDelay = ShootDelay;

            // instantiate the bullet trail
            GameObject bulletTrail = Instantiate(Bullet_Trail, _gunPoint.position, Quaternion.identity);
            bulletTrail.transform.right = direction;
            // destroy the bullet trail after a short time
            Destroy(bulletTrail, 1f);
            // raycast to check if the bullet hit something
            var hit = Physics2D.Raycast(_gunPoint.position, direction, 40f);

            var trailScript = bulletTrail.GetComponent<BulletTrail>();
            if (hit.collider != null)
            {
                // try to find IHittable on hit collider or its parents
                var hittable = hit.collider.GetComponent<IHittable>() ?? hit.collider.GetComponentInParent<IHittable>();
                hittable?.TakeHit();
            }
            else
            {
                // no hit: set target far (si nécessaire)
                var endPosition = _gunPoint.position + (Vector3)(direction.normalized * 40f);
            }
        }
    }

    private void HandleGunRotation()
    {
        // rotate the gun to point towards the mouse position
        worldPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        direction = worldPosition - (Vector2)transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        gun.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // flip the gun when it reaching a 90 degree angle
        angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Vector3 localScale = new Vector3(0.7375f, 0.1570312f, 1f);
        if (angle > 90f || angle < -90f)
        {
            localScale.y = -0.1570312f;
        }
        else
        {
            localScale.y = 0.1570312f;
        }

        gun.transform.localScale = localScale;
    }
}