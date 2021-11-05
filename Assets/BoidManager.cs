using UnityEngine;
using System.Collections.Generic;

// Make sure the Boids update after this Mono
public class BoidManager : MonoBehaviour
{
    [SerializeField]
    private int numberOfBoids = 10;

    public float boidSeparationRadius = 1.5f;
    public float boidCohesionRadius = 3f;
    public float boidAlignmentRadius = 3f;

    public int fieldOfView = 270;
    public float boidSpeed = 2f;
    public float viewLength = 1f;

    public float separationSpeed = 0.01f;
    public float cohesionSpeed = 0.01f;
    public float alignmentSpeed = 0.01f;
    public float obstacleSpeed = 0.03f;

    public float wobbleStrength = 1f;

    public bool separation = true;
    public bool cohesion = true;
    public bool alignment = true;

    [SerializeField]
    private Collider2D boundingCollider = null;
    [SerializeField]
    private GameObject boidPrefab = null;

    [SerializeField]
    private ComputeShader computeBoid = null;
    private ComputeBuffer boidBuffer = null;
    public bool useCompute = true;

    [HideInInspector]
    public List<Boid> boids;

    [HideInInspector]
    public BoidData[] computeBoidData;

    private void Start()
    {
        boids = new List<Boid>();
        BoidFactory boidFactory = new BoidFactory(this, boidPrefab, boundingCollider);
        numberOfBoids = (numberOfBoids / 64) * 64;
        computeBoidData = new BoidData[numberOfBoids];

        for (int i = 0; i < numberOfBoids; i++)
        {
            Boid boid = boidFactory.CreateBoid();
            boid.index = i;
            boids.Add(boid);
            computeBoidData[i] = new BoidData();
            computeBoidData[i].position = boid.transform.position;
            computeBoidData[i].direction = boid.transform.up;
        }

        SetUpShader();
    }

    private void Update()
    {
        if (useCompute)
        {
            SetShaderParameters();
            ResetBoidData();
            computeBoid.Dispatch(0, numberOfBoids / 64, 1, 1);
            boidBuffer.GetData(computeBoidData);
        }
    }

    private void LateUpdate()
    {
        boidBuffer.SetData(computeBoidData);
    }

    public List<Boid> GetNeighbours(Boid boid, float radius)
    {
        List<Boid> neighbours = new List<Boid>();

        foreach (Boid currentBoid in boids)
        {
            if (boid == currentBoid) continue;

            if (Vector3.Distance(boid.transform.position, currentBoid.transform.position) < radius)
            {
                neighbours.Add(currentBoid);
            }
        }

        return neighbours;
    }

    private void SetUpShader()
    {
        boidBuffer = new ComputeBuffer(numberOfBoids, System.Runtime.InteropServices.Marshal.SizeOf(typeof(BoidData)));
        boidBuffer.SetData(computeBoidData);
        computeBoid.SetBuffer(0, "boids", boidBuffer);
    }

    private void ResetBoidData()
    {
        for (int i = 0; i < numberOfBoids; i++)
        {
            computeBoidData[i].separationSum = Vector3.zero;

            computeBoidData[i].cohesionSum = Vector3.zero;
            computeBoidData[i].cohesionNum = 0;

            computeBoidData[i].alignmentSum = Vector3.zero;
            computeBoidData[i].alignmentNum = 0;
        }

        boidBuffer.SetData(computeBoidData);
    }

    private void SetShaderParameters()
    {
        computeBoid.SetInt("numberOfBoids", numberOfBoids);

        computeBoid.SetFloat("boidSeparationRadius", boidSeparationRadius);
        computeBoid.SetFloat("boidCohesionRadius", boidCohesionRadius);
        computeBoid.SetFloat("boidAlignmentRadius", boidAlignmentRadius);

        computeBoid.SetInt("fieldOfView", fieldOfView);
        computeBoid.SetFloat("boidSpeed", boidSpeed);
        computeBoid.SetFloat("viewLength", viewLength);

        computeBoid.SetFloat("separationSpeed", separationSpeed);
        computeBoid.SetFloat("cohesionSpeed", cohesionSpeed);
        computeBoid.SetFloat("alignmentSpeed", alignmentSpeed);
        computeBoid.SetFloat("obstacleSpeed", obstacleSpeed);
    }

    private void OnDestroy()
    {
        boidBuffer.Dispose();
    }
}
