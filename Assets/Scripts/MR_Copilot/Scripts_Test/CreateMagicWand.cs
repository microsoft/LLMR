// To create a magic wand out of simple primitives, I will do the following:
// 1. Create a new C# script called CreateMagicWand that inherits from Widgets and has a summary variable that explains its function.
// 2. In the CreateMagicWand script, create a new GameObject called MagicWand and make it a child of the "---Widgets---" GameObject.
// 3. Create a cylinder GameObject called WandHandle and make it a child of the MagicWand GameObject.
// 4. Create a sphere GameObject called WandTip and make it a child of the MagicWand GameObject.
// 5. Position, scale, and rotate the WandHandle and WandTip GameObjects appropriately to resemble a magic wand.

using UnityEngine;

public class CreateMagicWand : Widgets
{
    private GameObject magicWand;
    private GameObject wandHandle;
    private GameObject wandTip;

    void Start()
    {
        summary = "This script creates a magic wand out of simple primitives";

        // Create a new GameObject called MagicWand and make it a child of the "---Widgets---" GameObject.
        magicWand = new GameObject("MagicWand");
        magicWand.transform.parent = GameObject.Find("---Widgets---").transform;

        // Create a cylinder GameObject called WandHandle and make it a child of the MagicWand GameObject.
        wandHandle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        wandHandle.name = "WandHandle";
        wandHandle.transform.parent = magicWand.transform;

        // Position, scale, and rotate the WandHandle GameObject appropriately to resemble a magic wand handle.
        wandHandle.transform.localPosition = new Vector3(0, 0, 0);
        wandHandle.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
        wandHandle.transform.localRotation = Quaternion.Euler(0, 0, 0);

        // Create a sphere GameObject called WandTip and make it a child of the MagicWand GameObject.
        wandTip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        wandTip.name = "WandTip";
        wandTip.transform.parent = magicWand.transform;

        // Position, scale, and rotate the WandTip GameObject appropriately to resemble a magic wand tip.
        wandTip.transform.localPosition = new Vector3(0, 0.5f, 0);
        wandTip.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        wandTip.transform.localRotation = Quaternion.Euler(0, 0, 0);
    }
}
