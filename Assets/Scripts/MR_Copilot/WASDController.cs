using UnityEngine;

public class WASDController : MonoBehaviour
{
    // Adjust the speed and sensitivity of the movement and rotation
    public float moveSpeed = 5f;
    public float rotateSpeed = 100f;
    public GameObject input_field;

    // Update is called once per frame
    void Update()
    {
        // enable and disable the input field with enter
        if (Input.GetKeyDown(KeyCode.Return))
        {
            bool flag = input_field.activeSelf;
            input_field.SetActive(!flag);
        }

        // disable movement when input field is enabled
        if (!input_field.activeSelf)
        {
            // Get the horizontal and vertical input axes
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // Move the object forward or backward based on the vertical input
            transform.Translate(Vector3.forward * vertical * moveSpeed * Time.deltaTime);

            // Rotate the object left or right based on the horizontal input
            transform.Rotate(Vector3.up * horizontal * rotateSpeed * Time.deltaTime);
        }

    }
}