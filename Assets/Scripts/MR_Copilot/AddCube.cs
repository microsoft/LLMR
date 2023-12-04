using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddCube : MonoBehaviour
{
    public Vector3 topLeft;
    public Vector3 bottomRight;
    private Camera cam;

    public float x_s;
    public float y_s;
    public float z_s;



    // Start is called before the first frame update
    void Start()
    {
        float x = 739;
        float y = 401;
        float w = 118;
        float h = 247;
        float W = 884;
        float H = 835;

        float x_hat = (x + 1 / 2 * w) / W;
        float y_hat = (y - 1 / 2 * h) / H;
        x_s = Screen.width * x_hat;
        y_s = Screen.height * y_hat;
        z_s = Camera.main.nearClipPlane + 1;

        Vector3 center = Camera.main.ScreenToWorldPoint(new Vector3(x_s, y_s, z_s));
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = center;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Vector2 image_to_screen_space(Vector2 p_img)
    {
        x_s = Screen.width * p_img.x;
        y_s = Screen.height * p_img.y;

        return new Vector2(x_s, y_s);
    }


    Vector3 image_to_world_space(Vector2 p_img, float z_s)
    {
        Vector2 p_s = image_to_screen_space(p_img);
        Vector3 p_w = Camera.main.ScreenToWorldPoint(new Vector3(p_s.x, p_s.y, z_s));

        return p_w;
    }
}
