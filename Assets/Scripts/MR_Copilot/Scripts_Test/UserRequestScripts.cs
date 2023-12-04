using UnityEngine;
using UnityEngine.SceneManagement;

public class UserRequestScripts : Widgets
{
    public class LoadRandomSceneOnKeyPress : Widgets
    {
        void Start()
        {
            summary = "This script loads a random scene when the 'N' key is pressed";
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                int randomSceneIndex = UnityEngine.Random.Range(0, SceneManager.sceneCountInBuildSettings);
                SceneManager.LoadScene(randomSceneIndex);
            }
        }
    }

    public class CreateCylindricalButtonWidget : Widgets
    {
        void Start()
        {
            summary = "This script creates a cylindrical button widget named CylindricalButton if it has not been created";

            GameObject cylindricalButton = GameObject.Find("CylindricalButton");
            if (cylindricalButton == null)
            {
                cylindricalButton = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                cylindricalButton.name = "CylindricalButton";
                cylindricalButton.transform.parent = GameObject.Find("---Widgets---").transform;
                cylindricalButton.AddComponent<CapsuleCollider>();
            }
        }
    }

    public class CylindricalButtonKeyPress : Widgets
    {
        void Start()
        {
            summary = "This script makes the previously generated cylindrical button respond to keypresses W, A, S, D";
        }

        void Update()
        {
            GameObject cylindricalButton = GameObject.Find("CylindricalButton");
            if (cylindricalButton != null)
            {
                float moveSpeed = 5.0f;
                Vector3 moveDirection = Vector3.zero;

                if (Input.GetKey(KeyCode.W)) moveDirection += Vector3.forward;
                if (Input.GetKey(KeyCode.A)) moveDirection -= Vector3.right;
                if (Input.GetKey(KeyCode.S)) moveDirection -= Vector3.forward;
                if (Input.GetKey(KeyCode.D)) moveDirection += Vector3.right;

                cylindricalButton.transform.position += moveDirection * moveSpeed * Time.deltaTime;
            }
        }
    }

    public class DisableCylindricalButtonKeyPress : Widgets
    {
        void Start()
        {
            summary = "This script disables the response of the previously generated cylindrical button to keypresses";

            GameObject compiler = GameObject.Find("Compiler");
            if (compiler != null)
            {
                CylindricalButtonKeyPress keyPressScript = compiler.GetComponent<CylindricalButtonKeyPress>();
                if (keyPressScript != null)
                {
                    Destroy(keyPressScript);
                }
            }
        }
    }

    public class CylindricalButtonRaycastClick : Widgets
    {
        void Start()
        {
            summary = "This script makes the previously generated cylindrical button respond to mouse clicks through raycasting";
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.collider.gameObject.name == "CylindricalButton")
                    {
                        Debug.Log("CylindricalButton clicked");
                    }
                }
            }
        }
    }
}
