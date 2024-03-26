/*
 * Derived from Unity package
 * https://docs.unity3d.com/Packages/com.unity.editorcoroutines@0.0/api/Unity.EditorCoroutines.Editor.html
 */

//namespace Unity.EditorCoroutines.Editor
namespace Meryel.UnityCodeAssist.Editor.EditorCoroutines
{
    /// <summary>
    /// Suspends the <see cref="EditorCoroutine">EditorCoroutine</see> execution for the given amount of seconds, using unscaled time. 
    /// The coroutine execution continues after the specified time has elapsed.
    /// <code>
    /// using System.Collections;
    /// using UnityEngine;
    /// using Unity.EditorCoroutines.Editor;
    /// using UnityEditor;
    ///
    /// public class MyEditorWindow : EditorWindow
    /// {
    ///     IEnumerator PrintEachSecond()
    ///     {
    ///         var waitForOneSecond = new EditorWaitForSeconds(1.0f);
    ///
    ///         while (true)
    ///         {
    ///             yield return waitForOneSecond;
    ///             Debug.Log("Printing each second");
    ///         }
    ///     }
    /// }
    /// </code>
    /// </summary>
    public class EditorWaitForSeconds
    {
        /// <summary>
        /// The time to wait in seconds.
        /// </summary>
        public float WaitTime { get; }

        /// <summary>
        /// Creates a instruction object for yielding inside a generator function.
        /// </summary>
        /// <param name="time">The amount of time to wait in seconds.</param>
        public EditorWaitForSeconds(float time)
        {
            WaitTime = time;
        }
    }
}