using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    public float searchHeight = 5f;
    public float minJumpHeight = 5f;
    public float maxJumpHeight = 8f;

    private float spawnTimer = 0f;
    public float spawnInterval = 3f;

    private float waterLevel;
    private Bounds waterBounds;
    private Bounds searchBounds;

    //public Rigidbody2D fishPrefab;
    public CaveFishController fishPrefab;

    // Start is called before the first frame update
    void Start()
    {
        waterLevel = waterBounds.max.y;

        searchBounds = waterBounds;
        searchBounds.SetMinMax(waterBounds.min, new Vector3(waterBounds.max.x, waterBounds.max.y + searchHeight, waterBounds.max.z));

    }

    private void Awake()
    {
        waterBounds = gameObject.GetComponent<Collider2D>().bounds;
    }

    // Update is called once per frame
    void Update()
    {
        //Collider2D playerCollider;
        Transform playerFound;

        if (spawnTimer > 0)
        {
            spawnTimer -= Time.deltaTime;
        }
        else
        {
            //playerCollider = Physics2D.OverlapArea(waterBounds.min, waterBounds.max, LayerMask.GetMask("Player"));
            if (playerFound = PlayerInWater()) // player in water
            {
                SpawnSwimmingFish(playerFound);
            }
            else if (playerFound = PlayerAboveWater())
            {
               // playerCollider = Physics2D.OverlapArea(searchBounds.min, searchBounds.max, LayerMask.GetMask("Player"));
                SpawnJumpingFish2(playerFound.position, playerFound.GetComponent<Rigidbody2D>().velocity);
            }
        }
    }

    public Transform PlayerInWater()
    {
        Collider2D playerCollider;
        Bounds bounds = waterBounds;
        bounds.max = new Vector2 (bounds.max.x, bounds.max.y + 0.5f);

        playerCollider = Physics2D.OverlapArea(bounds.min, bounds.max, LayerMask.GetMask("Player"));
        if (playerCollider != null)
        {
            return playerCollider.transform;
        }
        else
        {
            return null;
        }
    }

    public Transform PlayerAboveWater()
    {
        Collider2D playerCollider;
        playerCollider = Physics2D.OverlapArea(searchBounds.min, searchBounds.max, LayerMask.GetMask("Player"));
        if (playerCollider != null)
        {
            return playerCollider.transform;
        }
        else
        {
            return null;
        }
    }


    private void SpawnSwimmingFish(Transform playerFound)
    {
        float spawnX, spawnY;
        //spawnX = Random.Range(waterBounds.min.x +0.5f, waterBounds.max.x -0.5f);
        spawnX = Random.Range(Mathf.Max(playerFound.position.x - 2f, waterBounds.min.x + 0.5f), Mathf.Min(playerFound.position.x + 2f, waterBounds.max.x - 0.5f));
        spawnY = waterBounds.min.y;

        CaveFishController fish = Instantiate(fishPrefab, new Vector2 (spawnX, spawnY), transform.rotation);
        fish.Chase(playerFound, waterBounds, this);

        spawnTimer = spawnInterval;

    }

    private void SpawnJumpingFish(Vector2 playerPosition, Vector2 playerVelocity)
    {
        float spawnX, spawnY;
        float speedX, speedY;
        float targetX;

        float lowSpeedX = 5f;
        float lowSpeedY = 15f;
        float lowOffsetX = 6f; // x offset to hit target with velocity 5x, 15y

        float highSpeedX = 5f;
        float highSpeedY = 20f;
        float highOffsetX = 8f; // x offset to hit target with velocity 5x, 20y

        bool okToShoot = false;

        // shoot where player will be in 1 second, clamp to water bounds, clamp velocity to deal with dash
        targetX = Mathf.Clamp(playerPosition.x + Mathf.Clamp(playerVelocity.x,-4f,4f), waterBounds.min.x + 0.5f, waterBounds.max.x - 0.5f); 

        //targetX = Random.Range(Mathf.Max(playerPosition.x - 2f, waterBounds.min.x + 0.5f), Mathf.Min(playerPosition.x + 2f, waterBounds.max.x - 0.5f));

        int rnum = Random.Range(0, 4);

        // thi is just to avoid compiler errors since variables are recognized as assigned in switch statement
        spawnX = waterBounds.center.x;
        speedX = 0f;
        speedY = 15f;

        switch (rnum)
        {
            case 0: // Shoot low left
                bool canShootLowFromLeft = (targetX - lowOffsetX) > (waterBounds.min.x + 0.5f);
                if (canShootLowFromLeft)
                {
                    spawnX = targetX - lowOffsetX;
                    speedX = lowSpeedX;
                    speedY = lowSpeedY;
                    okToShoot = true;
                }
                break;
            case 1:  // Shoot high left
                bool canShootHighFromLeft = (targetX - highOffsetX) > (waterBounds.min.x + 0.5f);
                if (canShootHighFromLeft)
                {
                    spawnX = targetX - highOffsetX;
                    speedX = highSpeedX;
                    speedY = highSpeedY;
                    okToShoot = true;
                }
                break;
            case 2:  // Shoot low right
                bool canShootLowFromRight = (targetX + lowOffsetX) < (waterBounds.max.x - 0.5f);
                if (canShootLowFromRight)
                {
                    spawnX = targetX + lowOffsetX;
                    speedX = -lowSpeedX;
                    speedY = lowSpeedY;
                    okToShoot = true;
                }
                break;
            case 3:  // Shoot high right
                bool canShootHighFromRight = (targetX + highOffsetX) < (waterBounds.max.x - 0.5f);
                if (canShootHighFromRight)
                {
                    spawnX = targetX + highOffsetX;
                    speedX = -highSpeedX;
                    speedY = highSpeedY;
                    okToShoot = true;
                }
                break;
        }

        spawnY = waterLevel;

        if (okToShoot)
        {
            CaveFishController fish = Instantiate(fishPrefab, new Vector2(spawnX, spawnY), transform.rotation);
            fish.Jump(new Vector2(speedX, speedY), waterBounds, this);

            spawnTimer = spawnInterval;
        }
    }

    private void SpawnJumpingFish2(Vector2 playerPosition, Vector2 playerVelocity)
    {
        float spawnX, spawnY;
        float speedX, speedY;
        float targetX, targetY;
        float jumpY;
        float horizontalTravel, verticalTravel;

        float g = -2.5f*Physics2D.gravity.y; //based on fish gravity scale of 2.5
        float timeToPeak, timeToFall;

        float spawnXmin = Mathf.Max(waterBounds.min.x + 0.5f, playerPosition.x - waterBounds.extents.x);
        float spawnXmax = Mathf.Min(waterBounds.max.x - 0.5f, playerPosition.x + waterBounds.extents.x);

        //spawnX = Random.Range( spawnXmin, spawnXmax);
        spawnX = PickRandomWithKeepout(spawnXmin, spawnXmax, playerPosition.x - 2f, playerPosition.x + 2f);
        spawnY = waterLevel;

        // shoot where player will be in 1 second, clamp to water bounds, clamp velocity to deal with dash
        targetX = Mathf.Clamp(playerPosition.x + Mathf.Clamp(playerVelocity.x, -4f, 4f), waterBounds.min.x + 0.5f, waterBounds.max.x - 0.5f);

        float limitJumpMax = waterBounds.max.y + maxJumpHeight;
        float limitJumpMin = Mathf.Max(waterBounds.max.y + minJumpHeight, Mathf.Min(playerPosition.y+2f, limitJumpMax));

        jumpY = Random.Range(limitJumpMin, limitJumpMax);
        targetY = Mathf.Min(playerPosition.y, jumpY);

        verticalTravel = jumpY - spawnY;
        speedY = Mathf.Sqrt(2 * g * verticalTravel);

        timeToPeak = speedY / g;
        timeToFall = Mathf.Sqrt(2 * (jumpY - targetY) / g);

        horizontalTravel = (targetX - spawnX);
        speedX = horizontalTravel / (timeToPeak + timeToFall);

        // make sure we don't overshoot the pool
        speedX = Mathf.Clamp(speedX , - 0.5f * (spawnX - waterBounds.min.x + 0.5f) / timeToPeak, 0.5f * (waterBounds.max.x - 0.5f - spawnX)/timeToPeak);

        Debug.Log("g: " + g + ", spawnX: " + spawnX + ", spawnY: " + spawnY);
        Debug.Log("speedX: " + speedX + ", speedY: " + speedY);
        Debug.Log("time to peak: " + timeToPeak + ", time to fall: " + timeToFall);

        CaveFishController fish = Instantiate(fishPrefab, new Vector2(spawnX, spawnY), transform.rotation);
        fish.Jump(new Vector2(speedX, speedY), waterBounds, this);

        spawnTimer = spawnInterval;
    }

    private float PickRandomWithKeepout(float rangemin, float rangemax, float keepoutmin, float keepoutmax)
    {
        float leftrange = Mathf.Max(keepoutmin,rangemin)-rangemin;
        float rightrange = rangemax - Mathf.Min(keepoutmax,rangemax);
        float totalrange = leftrange + rightrange;

        float x = Random.Range(0, totalrange);

        if (x > leftrange)
        {
            return (rangemax - totalrange + x);
        }
        else
        {
            return (rangemin + x);
        }


    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(searchBounds.center, searchBounds.size);
    }
}
