//Create a simple simulation of predators hunting prey, and use a model of a sheep and a wolf to represent them. Let's make the prey reproduce every 10 seconds and move randomly (maybe switch direction every few seconds?),
//and the predators follow nearest prey. Let's make this bounded within a square of 15x15. Also create a single sheep for me to control myself with the arrow keys, and make the camera follow it from above.

// Step by step plan:
// 1. Load the sheep and wolf models from Sketchfab.
// 2. Create a script for the prey behavior (reproduce and move randomly).
// 3. Create a script for the predator behavior (follow nearest prey).
// 4. Create a script for the player-controlled sheep (arrow keys and camera follow).
// 5. Instantiate the initial sheep and wolves in the scene.
// 6. Attach the appropriate scripts to the sheep and wolves.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PredatorPreySimulation : MonoBehaviour
{
    public class PreyBehavior : MonoBehaviour
    {
        public float reproductionTime = 10f;
        public float moveSpeed = 2f;
        public float changeDirectionTime = 2f;

        private Vector3 randomDirection;
        private float timeSinceLastDirectionChange;

        void Start()
        {
            StartCoroutine(Reproduce());
            randomDirection = GetRandomDirection();
            timeSinceLastDirectionChange = 0f;
        }

        void Update()
        {
            timeSinceLastDirectionChange += Time.deltaTime;
            if (timeSinceLastDirectionChange >= changeDirectionTime)
            {
                randomDirection = GetRandomDirection();
                timeSinceLastDirectionChange = 0f;
            }

            transform.position += randomDirection * moveSpeed * Time.deltaTime;
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -7.5f, 7.5f), transform.position.y, Mathf.Clamp(transform.position.z, -7.5f, 7.5f));
        }

        IEnumerator Reproduce()
        {
            while (true)
            {
                yield return new WaitForSeconds(reproductionTime);
                GameObject newSheep = Instantiate(gameObject, transform.position + new Vector3(1, 0, 1), Quaternion.identity);
                newSheep.name = "Sheep";
            }
        }

        Vector3 GetRandomDirection()
        {
            return new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;
        }
    }

    public class PredatorBehavior : MonoBehaviour
    {
        public float moveSpeed = 3f;

        void Update()
        {
            GameObject nearestPrey = FindNearestPrey();
            if (nearestPrey != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, nearestPrey.transform.position, moveSpeed * Time.deltaTime);
            }
        }

        GameObject FindNearestPrey()
        {
            GameObject[] preyList = GameObject.FindGameObjectsWithTag("Prey");
            GameObject nearestPrey = null;
            float minDistance = Mathf.Infinity;

            foreach (GameObject prey in preyList)
            {
                float distance = Vector3.Distance(transform.position, prey.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPrey = prey;
                }
            }

            return nearestPrey;
        }
    }

    public class PlayerSheep : MonoBehaviour
    {
        public float moveSpeed = 2f;
        public Camera mainCamera;

        void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 movement = new Vector3(horizontal, 0, vertical).normalized * moveSpeed * Time.deltaTime;
            transform.position += movement;
            transform.position = new Vector3(Mathf.Clamp(transform.position.x, -7.5f, 7.5f), transform.position.y, Mathf.Clamp(transform.position.z, -7.5f, 7.5f));

            mainCamera.transform.position = new Vector3(transform.position.x, mainCamera.transform.position.y, transform.position.z - 5f);
        }
    }

    void Start()
    {
        SketchfabLoader loader = gameObject.GetComponent<SketchfabLoader>();
        GameObject sheepModel = loader.Load("Sheep");
        GameObject wolfModel = loader.Load("Wolf");

        loader.Scale(sheepModel, 0.5f);
        loader.Scale(wolfModel, 0.5f);

        GameObject playerSheep = Instantiate(sheepModel, new Vector3(0, 0, 0), Quaternion.identity);
        playerSheep.name = "PlayerSheep";
        playerSheep.AddComponent<PlayerSheep>();
        playerSheep.GetComponent<PlayerSheep>().mainCamera = Camera.main;

        for (int i = 0; i < 5; i++)
        {
            GameObject sheep = Instantiate(sheepModel, new Vector3(Random.Range(-7.5f, 7.5f), 0, Random.Range(-7.5f, 7.5f)), Quaternion.identity);
            sheep.name = "Sheep";
            sheep.tag = "Prey";
            sheep.AddComponent<PreyBehavior>();
        }

        for (int i = 0; i < 3; i++)
        {
            GameObject wolf = Instantiate(wolfModel, new Vector3(Random.Range(-7.5f, 7.5f), 0, Random.Range(-7.5f, 7.5f)), Quaternion.identity);
            wolf.name = "Wolf";
            wolf.AddComponent<PredatorBehavior>();
        }
    }
}