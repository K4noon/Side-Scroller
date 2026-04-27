using UnityEngine;

public class BulletTrail : MonoBehaviour
{

    private Vector3 _startPosition;
    public Vector3 _targetPosition;
    private float _progress;

    [SerializeField] private float _speed = 110f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _startPosition = transform.position.WithAxis(VectorsExtension.Axis.Z, -1);
    }

    // Update is called once per frame
    void Update()
    {
        _progress += Time.deltaTime * _speed;
        transform.position += (transform.right * _speed) * Time.deltaTime;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if the bullet hit something, check if it has a IHittable component and call TakeHit
        if (collision != null)
        {
            var hittable = collision.GetComponent<IHittable>();
            if (hittable != null)
            {
                hittable.TakeHit();
            }
            else if (collision.CompareTag("Ground"))
            {
                //destroy the bullet trail if it hits the ground
                Destroy(gameObject);
            }
            else if (collision.CompareTag("WALL"))
            {
                //destroy the bullet trail if it hits a wall
                Destroy(gameObject);
            }
            
        }
    }
}
