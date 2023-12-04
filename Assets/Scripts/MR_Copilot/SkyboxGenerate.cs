// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor;
// using UnityEngine.UI;
// public class SkyboxGenerate : MonoBehaviour
// {
//     private string imageFileName = "cows.png"; // the name of the image file in the StreamingAssets folder

//     void Start()
//     {
//         // Load the image as a Texture2D asset from the Assets/Images folder
//         string assetPath = "Assets/Images/" + imageFileName;
//         Texture2D imageTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);

//         // Check if the image texture is valid
//         if (imageTexture == null)
//         {
//             Debug.LogError("Failed to load image texture from " + assetPath);
//             return;
//         }

//         // Create a new canvas and set it to Screen Space - Overlay mode
//         Canvas canvas = new GameObject("Canvas").AddComponent<Canvas>();
//         canvas.renderMode = RenderMode.ScreenSpaceOverlay;

//         // Create a new image and set it as a child of the canvas
//         Image image = new GameObject("Image").AddComponent<Image>();
//         image.transform.SetParent(canvas.transform, false);

//         // Set the image source to the image texture
//         image.sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0.5f, 0.5f));

//         // Set the image size and position to fill the canvas
//         image.rectTransform.anchorMin = Vector2.zero;
//         image.rectTransform.anchorMax = Vector2.one;
//         image.rectTransform.offsetMin = Vector2.zero;
//         image.rectTransform.offsetMax = Vector2.zero;

//         // Start a coroutine to render the cubemap after the image is updated
//         StartCoroutine(RenderCubemap(imageTexture));
//     }

//     IEnumerator RenderCubemap(Texture2D imageTexture)
//     {
//         // Wait for the next frame
//         yield return null;

//         // declare as a local variable
//         string shaderSource = @"
//         Shader ""Unlit/Texture""
//                 {
//                     Properties
//             {
//                 _MainTex (""Texture"", 2D) = ""white"" {}
//             }
//             SubShader
//             {
//                 Tags { ""RenderType"" = ""Opaque"" }
//                 LOD 100

//                 Pass
//                 {
//                     CGPROGRAM
//                     #pragma vertex vert
//                     #pragma fragment frag

//                     #include ""UnityCG.cginc""

//                     struct appdata
//                     {
//                         float4 vertex : POSITION;
//                         float2 uv : TEXCOORD0;
//                     };

//                     struct v2f
//                     {
//                         float2 uv : TEXCOORD0;
//                         float4 vertex : SV_POSITION;
//                     };

//                     sampler2D _MainTex;

//                     v2f vert (appdata v)
//                     {
//                         v2f o;
//                         o.vertex = UnityObjectToClipPos(v.vertex);
//                         o.uv = v.uv;
//                         return o;
//                     }

//                     fixed4 frag (v2f i) : SV_Target
//                     {
//                         fixed4 col = tex2D(_MainTex, i.uv);
//                         return col;
//                     }
//                     ENDCG
//                 }
//             }
//         }";

//         // Create a new cubemap texture
//         Cubemap cubemap = new Cubemap(512, TextureFormat.RGBA32, false);

//         // Create a new render texture and a camera
//         RenderTexture renderTexture = new RenderTexture(512, 512, 24);
//         Camera camera = new GameObject("Camera").AddComponent<Camera>();
//         camera.transform.position = Vector3.zero;
//         camera.transform.rotation = Quaternion.identity;
//         camera.fieldOfView = 90f;
//         camera.aspect = 1f;
//         camera.nearClipPlane = 0.01f;
//         camera.farClipPlane = 100f;
//         camera.targetTexture = renderTexture;

//         // Create a new shader from the source code
//         Shader shader = ShaderUtil.CreateShaderAsset(shaderSource);

//         // Create a new material and assign the shader
//         Material imageMaterial = new Material(shader);
//         // Material imageMaterial = new Material(Shader.Find("Unlit/Texture"));
//         imageMaterial.mainTexture = imageTexture;

//         // Create a new plane and assign the image material
//         GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
//         plane.transform.position = Vector3.zero;
//         plane.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
//         plane.transform.localScale = new Vector3(100f, 100f, 100f);
//         plane.GetComponent<MeshRenderer>().material = imageMaterial;

//         // Create a new texture to store the pixels from the render texture
//         Texture2D tempTexture = new Texture2D(512, 512, TextureFormat.RGBA32, false);

//         // Capture the image from six different angles and store them as the cubemap faces
//         camera.transform.LookAt(Vector3.right);
//         camera.Render();
//         RenderTexture.active = renderTexture;
//         tempTexture.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
//         tempTexture.Apply();
//         cubemap.SetPixels(tempTexture.GetPixels(), CubemapFace.PositiveX);

//         camera.transform.LookAt(Vector3.left);
//         camera.Render();
//         yield return new WaitForEndOfFrame(); 
//         RenderTexture.active = renderTexture;
//         tempTexture.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
//         tempTexture.Apply();
//         cubemap.SetPixels(tempTexture.GetPixels(), CubemapFace.NegativeX);

//         camera.transform.LookAt(Vector3.up);
//         camera.Render();
//         yield return new WaitForEndOfFrame(); 
//         RenderTexture.active = renderTexture;
//         tempTexture.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
//         tempTexture.Apply();
//         cubemap.SetPixels(tempTexture.GetPixels(), CubemapFace.PositiveY);

//         camera.transform.LookAt(Vector3.down);
//         camera.Render();
//         yield return new WaitForEndOfFrame(); 
//         RenderTexture.active = renderTexture;
//         tempTexture.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
//         tempTexture.Apply();
//         cubemap.SetPixels(tempTexture.GetPixels(), CubemapFace.NegativeY);

//         camera.transform.LookAt(Vector3.forward);
//         camera.Render();
//         yield return new WaitForEndOfFrame(); 
//         RenderTexture.active = renderTexture;
//         tempTexture.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
//         tempTexture.Apply();
//         cubemap.SetPixels(tempTexture.GetPixels(), CubemapFace.PositiveZ);

//         camera.transform.LookAt(Vector3.back);
//         camera.Render();
//         yield return new WaitForEndOfFrame(); 
//         RenderTexture.active = renderTexture;
//         tempTexture.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
//         tempTexture.Apply();
//         cubemap.SetPixels(tempTexture.GetPixels(), CubemapFace.NegativeZ);

//         // Apply the changes to the cubemap
//         cubemap.Apply();

//         // Render the image directly to the cubemap without using a renderTexture
//         camera.RenderToCubemap(cubemap);

//         // Check if the cubemap is valid
//         if (cubemap == null)
//         {
//             Debug.LogError("Failed to create cubemap from camera");
//             yield break;
//         }

//         // Save the cubemap as an asset for inspection
//         AssetDatabase.CreateAsset(cubemap, "Assets/Cubemap.asset");

//         // Destroy the render texture, the camera, the plane, the image texture, and the temporary texture
//         Destroy(renderTexture);
//         Destroy(camera.gameObject);
//         Destroy(plane);
//         //Destroy(imageTexture);
//         Destroy(tempTexture);

//         // Create a new material asset and assign it a suitable shader
//         Material skyboxMaterial = new Material(Shader.Find("Skybox/Cubemap"));

//         // Assign the cubemap texture to the _Tex property of the shader
//         skyboxMaterial.SetTexture("_Tex", cubemap);

//         // Create a new game object and add a skybox component
//         GameObject skyboxObject = new GameObject("Skybox");
//         Skybox skybox = skyboxObject.AddComponent<Skybox>();
//         skybox.material = skyboxMaterial;

//         Debug.Log("DONE");

//         // Optionally, adjust the skybox parameters
//         // skyboxMaterial.SetFloat("_Rotation", 90f); // rotate the skybox by 90 degrees
//         // skyboxMaterial.SetFloat("_Exposure", 0.5f); // reduce the skybox exposure
//         // skyboxMaterial.SetColor("_Tint", Color.red); // tint the skybox red
//     }
// }