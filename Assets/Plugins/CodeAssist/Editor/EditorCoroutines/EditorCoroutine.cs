/*
 * Derived from Unity package
 * https://docs.unity3d.com/Packages/com.unity.editorcoroutines@0.0/api/Unity.EditorCoroutines.Editor.html
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//namespace Unity.EditorCoroutines.Editor
namespace Meryel.UnityCodeAssist.Editor.EditorCoroutines
{
    /// <summary>
    /// A handle to an EditorCoroutine, can be passed to <see cref="EditorCoroutineUtility">EditorCoroutineUtility</see> methods to control lifetime.
    /// </summary>
    public class EditorCoroutine
    {
        private struct YieldProcessor
        {
            enum DataType : byte
            {
                None = 0,
                WaitForSeconds = 1,
                EditorCoroutine = 2,
                AsyncOP = 3,
            }
            struct ProcessorData
            {
                public DataType type;
                public double targetTime;
                public object current;
            }

            ProcessorData data;

            public void Set(object yield)
            {
                if (yield == data.current)
                    return;

                var type = yield.GetType();
                var dataType = DataType.None;
                double targetTime = -1;

                if(type == typeof(EditorWaitForSeconds))
                {
                    targetTime = EditorApplication.timeSinceStartup + (yield as EditorWaitForSeconds).WaitTime;
                    dataType = DataType.WaitForSeconds;
                }
                else if(type == typeof(EditorCoroutine))
                {
                    dataType = DataType.EditorCoroutine;
                }
                else if(type == typeof(AsyncOperation) || type.IsSubclassOf(typeof(AsyncOperation)))
                {
                    dataType = DataType.AsyncOP;
                }

                data = new ProcessorData { current = yield, targetTime = targetTime, type = dataType };
            }

            public bool MoveNext(IEnumerator enumerator)
            {
                var advance = data.type switch
                {
                    DataType.WaitForSeconds => data.targetTime <= EditorApplication.timeSinceStartup,
                    DataType.EditorCoroutine => (data.current as EditorCoroutine).m_IsDone,
                    DataType.AsyncOP => (data.current as AsyncOperation).isDone,
                    _ => data.current == enumerator.Current,//a IEnumerator or a plain object was passed to the implementation
                };
                if (advance)
                {
                    data = default;// (ProcessorData);
                    return enumerator.MoveNext();
                }
                return true;
            }
        }

        internal WeakReference m_Owner;
        IEnumerator m_Routine;
        YieldProcessor m_Processor;

        bool m_IsDone;

        internal EditorCoroutine(IEnumerator routine)
        {
            m_Owner = null;
            m_Routine = routine;
            EditorApplication.update += MoveNext;
        }

        internal EditorCoroutine(IEnumerator routine, object owner)
        {
            m_Processor = new YieldProcessor();
            m_Owner = new WeakReference(owner);
            m_Routine = routine;
            EditorApplication.update += MoveNext;
        }

        internal void MoveNext()
        {
            if (m_Owner != null && !m_Owner.IsAlive)
            {
                EditorApplication.update -= MoveNext;
                return;
            }

            bool done = ProcessIEnumeratorRecursive(m_Routine);
            m_IsDone = !done;

            if (m_IsDone)
                EditorApplication.update -= MoveNext;
        }

        static readonly Stack<IEnumerator> kIEnumeratorProcessingStack = new Stack<IEnumerator>(32);
        private bool ProcessIEnumeratorRecursive(IEnumerator enumerator)
        {
            var root = enumerator;
            while(enumerator.Current as IEnumerator != null)
            {
                kIEnumeratorProcessingStack.Push(enumerator);
                enumerator = enumerator.Current as IEnumerator;
            }

            //process leaf
            m_Processor.Set(enumerator.Current);
            var result = m_Processor.MoveNext(enumerator);

            while (kIEnumeratorProcessingStack.Count > 1)
            {
                if (!result)
                {
                    result = kIEnumeratorProcessingStack.Pop().MoveNext();
                }
                else
                    kIEnumeratorProcessingStack.Clear();
            }

            if (kIEnumeratorProcessingStack.Count > 0 && !result && root == kIEnumeratorProcessingStack.Pop())
            {
                result = root.MoveNext();
            }

            return result;
        }

        internal void Stop()
        {
            m_Owner = null;
            m_Routine = null;
            EditorApplication.update -= MoveNext;
        }
    }
}
