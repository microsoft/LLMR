using UnityEngine;

public class BirdFlyingAnimation : Widgets
{
    public GameObject bird;
    public GameObject leftWingRoot;
    public GameObject leftWingMid;
    public GameObject leftWingTip;
    public GameObject rightWingRoot;
    public GameObject rightWingMid;
    public GameObject rightWingTip;
    public float wingFlapSpeed = 2f;
    public float maxFlapAngle = 10f;

    void Start()
    {
        summary = "This script animates the wings of a bird to simulate flying.";

        bird = GameObject.Find("HummingBird");
        leftWingRoot = GameObject.Find("Shldr_L.9");
        leftWingMid = GameObject.Find("Elbow_L.10");
        leftWingTip = GameObject.Find("Wrist_L.11");
        rightWingRoot = GameObject.Find("Shldr_R.27");
        rightWingMid = GameObject.Find("Elbow_R.28");
        rightWingTip = GameObject.Find("Wrist_R.29");
    }

    void Update()
    {
        // Calculate the new rotation angle
        float angle = maxFlapAngle * Mathf.Sin(Time.time * wingFlapSpeed);

        // Apply the rotation to the wings
        leftWingRoot.transform.localRotation = Quaternion.Euler(0, -angle, -angle);
        // leftWingMid.transform.localRotation = Quaternion.Euler(0, angle, 0);
        // leftWingTip.transform.localRotation = Quaternion.Euler(0, angle, 0);
        rightWingRoot.transform.localRotation = Quaternion.Euler(0, 0, -angle);
        // rightWingMid.transform.localRotation = Quaternion.Euler(0, angle, 0);
        // rightWingTip.transform.localRotation = Quaternion.Euler(0, angle, 0);
    }
}