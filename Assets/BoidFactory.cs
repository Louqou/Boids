using UnityEngine;

public class BoidFactory
{
    private BoidManager boidManager;
    private GameObject boidPrefab;
    private Collider2D boundingCollider;

    public BoidFactory(BoidManager boidManager, GameObject boidPrefab, Collider2D boundingCollider)
    {
        this.boidPrefab = boidPrefab;
        this.boundingCollider = boundingCollider;
        this.boidManager = boidManager;
    }

    public Boid CreateBoid()
    {
        Boid boid = GameObject.Instantiate(boidPrefab, RandomPointInBounds(boundingCollider.bounds), Quaternion.Euler(0, 0, Random.Range(0, 360))).GetComponent<Boid>();
        boid.boundingCollider = boundingCollider;
        boid.boidManager = boidManager;
        return boid;
    }

    private Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}
