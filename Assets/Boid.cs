using UnityEngine;

public struct BoidData
{
    public Vector3 position;
    public Vector3 direction;

    public Vector3 separationSum;

    public Vector3 cohesionSum;
    public int cohesionNum;

    public Vector3 alignmentSum;
    public int alignmentNum;
}

public class Boid : MonoBehaviour
{
    public BoidManager boidManager;
    public Collider2D boundingCollider;
    public float speed = 1f;
    public int index;

    private bool isWrappingX = false;
    private bool isWrappingY = false;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Vector3 direction = transform.up;
        direction = boidManager.separation ? Separation(direction) : direction;
        direction = boidManager.cohesion ? Cohesion(direction) : direction;
        direction = boidManager.alignment ? Alignment(direction) : direction;
        //direction = AvoidEdge(direction);

        Vector3 newPos = transform.position + (direction.normalized * boidManager.boidSpeed * Time.deltaTime);
        transform.up = direction;

        // if (!boundingCollider.bounds.Contains(newPos))
        // {
        //     transform.Rotate(new Vector3(0, 0, 180), Space.Self);
        // }
        // else
        // {
        transform.position = newPos;
        // }

        // TODO: compute check left out for run-time switching
        boidManager.computeBoidData[index].position = transform.position;
        boidManager.computeBoidData[index].direction = transform.up;

        ScreenWrap();
    }

    private void ScreenWrap()
    {
        if (spriteRenderer.isVisible)
        {
            isWrappingX = false;
            isWrappingY = false;
            return;
        }

        if (isWrappingX && isWrappingY)
        {
            return;
        }

        var cam = Camera.main;
        var viewportPosition = cam.WorldToViewportPoint(transform.position);
        var newPosition = transform.position;

        if (!isWrappingX && (viewportPosition.x > 1 || viewportPosition.x < 0))
        {
            newPosition.x = -newPosition.x;

            isWrappingX = true;
        }

        if (!isWrappingY && (viewportPosition.y > 1 || viewportPosition.y < 0))
        {
            newPosition.y = -newPosition.y;

            isWrappingY = true;
        }

        transform.position = newPosition;
    }

    private Vector3 AvoidEdge(Vector3 direction)
    {
        Vector3 leftPos = transform.position + Quaternion.AngleAxis(45, Vector3.forward) * transform.up * boidManager.viewLength;
        Vector3 rightPos = transform.position + Quaternion.AngleAxis(-45, Vector3.forward) * transform.up * boidManager.viewLength;

        if (!boundingCollider.bounds.Contains(leftPos))
        {
            direction += (rightPos - transform.position).normalized * boidManager.obstacleSpeed;
        }
        else if (!boundingCollider.bounds.Contains(rightPos))
        {
            direction += (leftPos - transform.position).normalized * boidManager.obstacleSpeed;
        }

        return direction;
    }

    private Vector3 Separation(Vector3 direction)
    {
        Vector3 separationVector = Vector3.zero;

        if (!boidManager.useCompute)
        {
            var neighbours = boidManager.GetNeighbours(this, boidManager.boidSeparationRadius);

            foreach (var neighbour in neighbours)
            {
                if (IsInFOV(neighbour.transform.position))
                {
                    Vector3 neighbourVector = neighbour.transform.position - transform.position;
                    float magnitude = neighbourVector.magnitude;

                    if (magnitude > 0)
                    {
                        separationVector += neighbourVector.normalized / magnitude;
                    }
                }
            }

        }
        else
        {
            separationVector = boidManager.computeBoidData[index].separationSum;
        }

        return direction - (separationVector.normalized * boidManager.separationSpeed);
    }

    private Vector3 Cohesion(Vector3 direction)
    {
        var averagePosition = Vector3.zero;
        var numNeighbours = 0;

        if (!boidManager.useCompute)
        {
            var neighbours = boidManager.GetNeighbours(this, boidManager.boidCohesionRadius);


            foreach (var neighbour in neighbours)
            {
                if (IsInFOV(neighbour.transform.position))
                {
                    averagePosition += neighbour.transform.position;
                    numNeighbours++;
                }
            }
        }
        else
        {
            averagePosition = boidManager.computeBoidData[index].cohesionSum;
            numNeighbours = boidManager.computeBoidData[index].cohesionNum;
        }

        if (numNeighbours == 0) return direction;

        averagePosition /= numNeighbours;

        return direction + (averagePosition - transform.position).normalized * boidManager.cohesionSpeed;
    }

    private Vector3 Alignment(Vector3 direction)
    {
        var averageAlignment = Vector3.zero;
        var numNeighbours = 0;
        if (!boidManager.useCompute)
        {
            var neighbours = boidManager.GetNeighbours(this, boidManager.boidAlignmentRadius);


            foreach (var neighbour in neighbours)
            {
                averageAlignment += neighbour.transform.up;
                numNeighbours++;
            }
        }
        else
        {
            averageAlignment = boidManager.computeBoidData[index].alignmentSum;
            numNeighbours = boidManager.computeBoidData[index].alignmentNum;
        }

        if (numNeighbours == 0) return direction;

        averageAlignment /= numNeighbours;

        return direction + (averageAlignment.normalized * boidManager.alignmentSpeed);
    }

    private bool IsInFOV(Vector3 pos)
    {
        return Vector3.Angle(transform.up, pos - transform.position) < boidManager.fieldOfView;
    }
}
