//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using UnityEngine;

//public class Example : MonoBehaviour
//{
//    // A CancellationTokenSource to generate and cancel a CancellationToken
//    private CancellationTokenSource cts;

//    // A reference to the async function task
//    private Task asyncTask;

//    // A key code to trigger the cancellation
//    public KeyCode cancelKey = KeyCode.Escape;

//    private void Start()
//    {
//        // Create a new CancellationTokenSource
//        cts = new CancellationTokenSource();

//        // Get the CancellationToken from the source
//        CancellationToken ct = cts.Token;

//        // Start the async function and pass the CancellationToken
//        asyncTask = AsyncFunction(ct);
//    }

//    private void Update()
//    {
//        // Check if the cancel key is pressed
//        if (Input.GetKeyDown(cancelKey))
//        {
//            // Cancel the CancellationTokenSource
//            cts.Cancel();
//        }
//    }

//    private void OnDestroy()
//    {
//        // Dispose the CancellationTokenSource
//        cts.Dispose();
//    }

//    // An example async function that simulates some work
//    private async Task AsyncFunction(CancellationToken ct)
//    {
//        try
//        {
//            // Loop for 10 seconds
//            for (int i = 0; i < 10; i++)
//            {
//                // Check if the CancellationToken is canceled
//                ct.ThrowIfCancellationRequested();

//                // Simulate some work
//                await Task.Delay(1000);

//                // Print the current iteration
//                Debug.Log(i);
//            }

//            // Print the completion message
//            Debug.Log("Async function completed");
//        }
//        catch (OperationCanceledException)
//        {
//            // Handle the cancellation exception
//            Debug.Log("Async function canceled");
//        }
//    }
//}