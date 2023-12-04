using UnityEngine;

public class ControllableCube : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    private GameObject cube;

    void Start()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "ControllableCube";
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        cube.transform.Translate(movement);
    }
}