using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;
using NetMQ;
using NetMQ.Sockets;
using System.Threading;
using Task = System.Threading.Tasks.Task;
//using CancellationToken = System.Threading;
using Application = UnityEngine.Application;


#nullable enable


//**--
// can also do this for better clear, sometimes it gets locked
// https://answers.unity.com/questions/704066/callback-before-unity-reloads-editor-assemblies.html#

namespace Meryel.UnityCodeAssist.Editor
{
    public class NetMQPublisher : Synchronizer.Model.IProcessor
    {
        PublisherSocket? pubSocket;
        readonly string pubConnString;

        Task? pullTask;
        CancellationTokenSource? pullTaskCancellationTokenSource;

        readonly Synchronizer.Model.Manager syncMngr;

        public List<Synchronizer.Model.Connect> clients;

        Synchronizer.Model.Connect? _self;

        Synchronizer.Model.Connect Self => _self!;

        void InitializeSelf()
        {
            var projectPath = CommonTools.GetProjectPath();
            _self = new Synchronizer.Model.Connect()
            {
                ModelVersion = Synchronizer.Model.Utilities.Version,
                ProjectPath = projectPath,
                ProjectName = getProjectName(),
                ContactInfo = $"Unity {Application.unityVersion}",
                AssemblyVersion = Assister.Version,
            };

            string getProjectName()
            {
                string[] s = projectPath.Split('/');
                string projectName = s[s.Length - 2];
                //Logg("project = " + projectName);
                return projectName;
            }
        }


        public static void LogContext()
        {
            Serilog.Log.Debug("LogginContext begin");

            //var context = typeof(NetMQConfig).GetProperty("Context", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);
            var context = typeof(NetMQConfig).GetField("s_ctx", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).GetValue(null);
            Serilog.Log.Debug("context: {Context}", context);

            if (context == null)
                return;

            var starting = context.GetType().GetField("m_starting", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(context);
            Serilog.Log.Debug("starting: {Starting}", starting);

            var terminating = context.GetType().GetField("m_terminating", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(context);
            Serilog.Log.Debug("terminating: {Terminating}", terminating);

            var sockets = context.GetType().GetField("m_sockets", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(context);
            //Logg("sockets:" + sockets);
            //var socketList = sockets as System.Collections.IList;
            if (sockets is System.Collections.IList socketList)
            {
                Serilog.Log.Debug("socketList: {SocketList} [{Count}]", socketList, socketList.Count);

                foreach (var socketItem in socketList)
                {
                    Serilog.Log.Debug("socketItem: {SocketItem}", socketItem);
                }
            }

            var endPoints = context.GetType().GetField("m_endpoints", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(context);
            //Logg("endPoints:" + endPoints);
            //var endPointDict = endPoints as System.Collections.IDictionary;
            if (endPoints is System.Collections.IDictionary endPointDict)
            {
                Serilog.Log.Debug("endPointDict: {EndPointDict} ,{Count}", endPointDict, endPointDict.Count);

                foreach (var endPointDictKey in endPointDict.Keys)
                {
                    Serilog.Log.Debug("endPointDictKey: {EndPointDictKey} => {EndPointDictValue}", endPointDictKey, endPointDict[endPointDictKey]);
                }
            }

            Serilog.Log.Debug("LogginContext end");
        }

        bool isBind = false;
        public NetMQPublisher()
        {
            // LogContext();

            Serilog.Log.Debug("NetMQ server initializing, begin");

            InitializeSelf();

            clients = new List<Synchronizer.Model.Connect>();
            syncMngr = new Synchronizer.Model.Manager(this);

            var (pubSub, pushPull) = Synchronizer.Model.Utilities.GetConnectionString(Self!.ProjectPath);
            pubConnString = pubSub;

            //NetMQConfig.Linger = new TimeSpan(0);

            //pub = new Publisher();
            pubSocket = new PublisherSocket();

            pubSocket.Options.SendHighWatermark = 1000;
            Serilog.Log.Debug("NetMQ server initializing, Publisher socket binding... {PubConnString}", pubConnString);
            //pubSocket.Bind("tcp://127.0.0.1:12349");


            try
            {
                pubSocket.Bind(pubConnString);
                isBind = true;
                Serilog.Log.Debug("NetMQ server initializing, Publisher socket bound");
            }
            catch (AddressAlreadyInUseException ex)
            {
                Serilog.Log.Warning(ex, "NetMQ.AddressAlreadyInUseException");
                LogContext();
                Serilog.Log.Warning("NetMQ.AddressAlreadyInUseException disposing pubSocket");
                pubSocket.Dispose();
                pubSocket = null;
                return;
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                Serilog.Log.Warning(ex, "Socket exception");
                LogContext();
                Serilog.Log.Warning("Socket exception disposing pubSocket");
                pubSocket.Dispose();
                pubSocket = null;
                return;
            }


            //pubSocket.SendReady += PubSocket_SendReady;
            //SendConnect();

            pullTaskCancellationTokenSource = new CancellationTokenSource();
            //pullThread = new System.Threading.Thread(async () => await PullAsync(conn.pushPull, pullThreadCancellationTokenSource.Token));
            //pullThread = new System.Threading.Thread(() => InitPull(conn.pushPull, pullTaskCancellationTokenSource.Token));
            //pullThread.Start();
            //Task.Run(() => InitPullAsync());

            /*
            pullTask = Task.Factory.StartNew(
                () => InitPull(conn.pushPull, pullTaskCancellationTokenSource.Token), pullTaskCancellationTokenSource.Token,
                System.Threading.Tasks.TaskCreationOptions.LongRunning, System.Threading.Tasks.TaskScheduler.Current);
            */


            pullTask = Task.Factory.StartNew(
                () => InitPull(pushPull, pullTaskCancellationTokenSource.Token),
                System.Threading.Tasks.TaskCreationOptions.LongRunning);

            //InitPull(conn.pushPull);

            Serilog.Log.Debug("NetMQ server initializing, initialized");

            // need to sleep here, clients will take some time to start subscribing
            // https://github.com/zeromq/netmq/issues/482#issuecomment-182200323
            Thread.Sleep(1000);
            SendConnect();
        }


        private void InitPull(string connectionString, CancellationToken cancellationToken)
        {
            using (var runtime = new NetMQRuntime())
            {
                runtime.Run(//cancellationToken,
                    PullAsync(connectionString, cancellationToken)
                    );
                Serilog.Log.Debug("Puller runtime ended");
            }
            Serilog.Log.Debug("Puller runtime disposed");
        }

        async Task PullAsync(string connectionString, CancellationToken cancellationToken)
        {
            Serilog.Log.Debug("Puller begin");
            using (var pullSocket = new PullSocket(connectionString))
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    string header, content;
                    try
                    {
                        var headerTuple = await pullSocket.ReceiveFrameStringAsync(cancellationToken);
                        var contentTuple = await pullSocket.ReceiveFrameStringAsync(cancellationToken);
                        header = headerTuple.Item1;
                        content = contentTuple.Item1;
                    }
                    catch (System.Threading.Tasks.TaskCanceledException)
                    {
                        // Cancellation (token) requested
                        break;
                    }
                    Serilog.Log.Debug("Pulled: {Header}, {Content}", header, content);

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    //**--optimize here, pass only params
                    MainThreadDispatcher.Add(() => syncMngr.ProcessMessage(header, content));
                    //syncMngr.ProcessMessage(header.Item1, content.Item1);
                }

                Serilog.Log.Debug("Puller closing");

                pullSocket.Unbind(connectionString);
                pullSocket.Close();

                Serilog.Log.Debug("Puller closed");
            }

            Serilog.Log.Debug("Puller disposed");
        }

        public void Clear()
        {
            // LogContext();

            Serilog.Log.Debug("NetMQ clearing, begin 1, pullTaskCancellationTokenSource: {PullTaskCancellationTokenSource}", pullTaskCancellationTokenSource);
            pullTaskCancellationTokenSource?.Cancel();

            Serilog.Log.Verbose("NetMQ clearing, begin 2, pubSocket: {PubSocket}", pubSocket);
            var pubSocketDebugStr = pubSocket?.ToString();
            Serilog.Log.Debug("NetMQ clearing, begin 3, isBind: {IsBind}", isBind);
            if (isBind)
            {
                pubSocket?.Unbind(pubConnString);
                isBind = false;
            }
            Serilog.Log.Verbose("NetMQ clearing, begin 4");
            pubSocket?.Close();
            Serilog.Log.Verbose("NetMQ clearing, begin 5");
            pubSocket?.Dispose();
            Serilog.Log.Verbose("NetMQ clearing, begin 6");
            pubSocket = null;
            Serilog.Log.Debug("NetMQ clearing, publisher closed pubSocketDebugStr: {PubSocketDebugStr}", pubSocketDebugStr);

            try
            {
                Serilog.Log.Debug("NetMQ clearing, Task 1 begin");

                if (pullTask != null && !pullTask.Wait(1000))
                    Serilog.Log.Warning("NetMQ clearing, pull task termination failed");

                Serilog.Log.Verbose("NetMQ clearing, Task 2 waited");

                pullTask?.Dispose();
                pullTask = null;

                Serilog.Log.Debug("NetMQ clearing, Task 3 disposed");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"\n{nameof(OperationCanceledException)} thrown\n");
                Serilog.Log.Error(ex, "NetMQ clearing, pull task");
            }
            finally
            {
                pullTaskCancellationTokenSource?.Dispose();
                pullTaskCancellationTokenSource = null;
                Serilog.Log.Debug("NetMQ clearing, task cancelled");
            }

            Serilog.Log.Debug("NetMQ clearing, cleaning up");
            //pullSocket?.Close();
            NetMQConfig.Cleanup(false);  // Must be here to work more than once. Also argument false is important, otherwise might freeze Unity upon exit or domain reload
            //pullThread?.Abort();
            Serilog.Log.Debug("NetMQ clearing, cleared");
        }

        string SerializeObject<T>(T obj)
            where T : class
        {
            // Odin cant serialize string arrays, https://github.com/TeamSirenix/odin-serializer/issues/26
            //var buffer = OdinSerializer.SerializationUtility.SerializeValue<T>(obj, OdinSerializer.DataFormat.JSON);
            //var str = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);

            // Newtonsoft works fine, but needs package reference
            //var str = Newtonsoft.Json.JsonConvert.SerializeObject(obj);

            // not working
            //var str = EditorJsonUtility.ToJson(obj);

            // needs nuget
            //System.Text.Json.JsonSerializer;

            var str = TinyJson.JsonWriter.ToJson(obj);

            return str;
        }

        void SendAux(Synchronizer.Model.IMessage message, bool logContent = true)
        {
            if (message == null)
                return;

            SendAux(message.GetType().Name, message, logContent);
        }

        void SendAux(string messageType, object content, bool logContent = true)
        {
            if (logContent)
                Serilog.Log.Debug("Publishing {MessageType} {@Content}", messageType, content);
            else
                Serilog.Log.Debug("Publishing {MessageType}", messageType);

            var publisher = pubSocket;
            if (publisher != null)
                publisher.SendMoreFrame(messageType).SendFrame(SerializeObject(content));
            else
                Serilog.Log.Error("Publisher socket is null");
        }

        public void SendConnect()
        {
            var connect = Self;

            SendAux(connect);
        }

        public void SendDisconnect()
        {
            var disconnect = new Synchronizer.Model.Disconnect()
            {
                ModelVersion = Self.ModelVersion,
                ProjectPath = Self.ProjectPath,
                ProjectName = Self.ProjectName,
                ContactInfo = Self.ContactInfo,
                AssemblyVersion = Self.AssemblyVersion,
            };

            SendAux(disconnect);
        }

        public void SendConnectionInfo()
        {
            var connectionInfo = new Synchronizer.Model.ConnectionInfo()
            {
                ModelVersion = Self.ModelVersion,
                ProjectPath = Self.ProjectPath,
                ProjectName = Self.ProjectName,
                ContactInfo = Self.ContactInfo,
                AssemblyVersion = Self.AssemblyVersion,
            };

            SendAux(connectionInfo);
        }

        public void SendHandshake()
        {
            var handshake = new Synchronizer.Model.Handshake();

            SendAux(handshake);
        }

        public void SendRequestInternalLog()
        {
            var requestInternalLog = new Synchronizer.Model.RequestInternalLog();

            SendAux(requestInternalLog);
        }

        public void SendInternalLog()
        {
            var internalLog = new Synchronizer.Model.InternalLog()
            {
                LogContent = Logger.ELogger.GetInternalLogContent(),
            };

            SendAux(internalLog, logContent: false);
        }


        void SendStringArrayAux(string id, string[] array)
        {
            var stringArray = new Synchronizer.Model.StringArray()
            {
                Id = id,
                Array = array,
            };

            SendAux(stringArray);
        }

        void SendStringArrayContainerAux(params (string id, string[] array)[] container)
        {
            var stringArrayContainer = new Synchronizer.Model.StringArrayContainer()
            {
                Container = new Synchronizer.Model.StringArray[container.Length],
            };

            for (int i = 0; i < container.Length; i++)
            {
                stringArrayContainer.Container[i] = new Synchronizer.Model.StringArray
                {
                    Id = container[i].id,
                    Array = container[i].array
                };
            }

            SendAux(stringArrayContainer);
        }

        public void SendTags(string[] tags) =>
            SendStringArrayAux(Synchronizer.Model.Ids.Tags, tags);

        /*
        {
            
            var tags = new Synchronizer.Model.Tags()
            {
                TagArray = tagArray,
            };

            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(tags);

            pubSocket.SendMoreFrame(nameof(Synchronizer.Model.Tags)).SendFrame(serialized);
            

        }*/

        public void SendLayers(string[] layerIndices, string[] layerNames)
        {
            /*
            var layers = new Synchronizer.Model.Layers()
            {
                LayerIndices = layerIndices,
                LayerNames = layerNames,
            };

            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(layers);

            pubSocket.SendMoreFrame(nameof(Synchronizer.Model.Layers)).SendFrame(serialized);
            */

            //SendStringArrayAux(Synchronizer.Model.Ids.Layers, layerNames);
            //SendStringArrayAux(Synchronizer.Model.Ids.LayerIndices, layerIndices);
            SendStringArrayContainerAux(
                (Synchronizer.Model.Ids.Layers, layerNames),
                (Synchronizer.Model.Ids.LayerIndices, layerIndices)
                );
        }

        public void SendSortingLayers(string[] sortingLayers, string[] sortingLayerIds, string[] sortingLayerValues)
        {
            //SendStringArrayAux(Synchronizer.Model.Ids.SortingLayers, sortingLayers);
            //SendStringArrayAux(Synchronizer.Model.Ids.SortingLayerIds, sortingLayerIds);
            //SendStringArrayAux(Synchronizer.Model.Ids.SortingLayerValues, sortingLayerValues);

            SendStringArrayContainerAux(
                (Synchronizer.Model.Ids.SortingLayers, sortingLayers),
                (Synchronizer.Model.Ids.SortingLayerIds, sortingLayerIds),
                (Synchronizer.Model.Ids.SortingLayerValues, sortingLayerValues)
                );
        }

        public void SendPlayerPrefs(string[] playerPrefKeys, string[] playerPrefValues,
            string[] playerPrefStringKeys, string[] playerPrefIntegerKeys, string[] playerPrefFloatKeys)
        {
            SendStringArrayContainerAux(
                (Synchronizer.Model.Ids.PlayerPrefKeys, playerPrefKeys),
                (Synchronizer.Model.Ids.PlayerPrefValues, playerPrefValues),
                (Synchronizer.Model.Ids.PlayerPrefStringKeys, playerPrefStringKeys),
                (Synchronizer.Model.Ids.PlayerPrefIntegerKeys, playerPrefIntegerKeys),
                (Synchronizer.Model.Ids.PlayerPrefFloatKeys, playerPrefFloatKeys)
                );
        }

        public void SendEditorPrefs(string[] editorPrefKeys, string[] editorPrefValues,
            string[] editorPrefStringKeys, string[] editorPrefIntegerKeys, string[] editorPrefFloatKeys,
            string[] editorPrefBooleanKeys)
        {
            SendStringArrayContainerAux(
                (Synchronizer.Model.Ids.EditorPrefKeys, editorPrefKeys),
                (Synchronizer.Model.Ids.EditorPrefValues, editorPrefValues),
                (Synchronizer.Model.Ids.EditorPrefStringKeys, editorPrefStringKeys),
                (Synchronizer.Model.Ids.EditorPrefIntegerKeys, editorPrefIntegerKeys),
                (Synchronizer.Model.Ids.EditorPrefFloatKeys, editorPrefFloatKeys),
                (Synchronizer.Model.Ids.EditorPrefBooleanKeys, editorPrefBooleanKeys)
                );
        }

        public void SendInputManager(string[] axisNames, string[] axisInfos, string[] buttonKeys, string[] buttonAxis, string[] joystickNames)
        {
            SendStringArrayContainerAux(
                (Synchronizer.Model.Ids.InputManagerAxes, axisNames),
                (Synchronizer.Model.Ids.InputManagerAxisInfos, axisInfos),
                (Synchronizer.Model.Ids.InputManagerButtonKeys, buttonKeys),
                (Synchronizer.Model.Ids.InputManagerButtonAxis, buttonAxis),
                (Synchronizer.Model.Ids.InputManagerJoystickNames, joystickNames)
                );
        }

        public void SendScriptMissing(string component)
        {
            var scriptMissing = new Synchronizer.Model.ScriptMissing()
            {
                Component = component,
            };

            SendAux(scriptMissing);
        }

        public void SendGameObject(GameObject go)
        {
            if (!go)
                return;

            Serilog.Log.Debug("SendGO: {GoName}", go.name);

            var dataOfSelf = go.ToSyncModel(priority:10000);
            if (dataOfSelf != null)
                SendAux(dataOfSelf);

            var dataOfHierarchy = go.ToSyncModelOfHierarchy();
            if (dataOfHierarchy != null)
            {
                foreach (var doh in dataOfHierarchy)
                    SendAux(doh);
            }

            var dataOfComponents = go.ToSyncModelOfComponents();
            if (dataOfComponents != null)
            {
                foreach (var doc in dataOfComponents)
                    SendAux(doc);
            }
            
        }

        public void SendScriptableObject(ScriptableObject so)
        {
            Serilog.Log.Debug("SendSO: {SoName}", so.name);

            var dataOfSo = so.ToSyncModel();
            if (dataOfSo != null)
                SendAux(dataOfSo);
        }

        public void SendAnalyticsEvent(string type, string content)
        {
            var dataOfAe = new Synchronizer.Model.AnalyticsEvent()
            {
                EventType = type,
                EventContent = content
            };
            SendAux(dataOfAe);
        }

        public void SendErrorReport(string errorMessage, string stack, string type)
        {
            var dataOfER = new Synchronizer.Model.ErrorReport()
            {
                ErrorMessage = errorMessage,
                ErrorStack = stack,
                ErrorType = type,
            };
            SendAux(dataOfER);
        }

        public void SendRequestVerboseType(string type, string docPath)
        {
            var dataOfRVT = new Synchronizer.Model.RequestVerboseType()
            {
                Type = type,
                DocPath = docPath,
            };
            SendAux(dataOfRVT);
        }



        string Synchronizer.Model.IProcessor.Serialize<T>(T value)
        {
            //return System.Text.Json.JsonSerializer.Serialize<T>(value);
            //return Newtonsoft.Json.JsonConvert.SerializeObject(value);
            return SerializeObject(value);
        }
        T Synchronizer.Model.IProcessor.Deserialize<T>(string data)
        {
            //return System.Text.Json.JsonSerializer.Deserialize<T>(data)!;
            //return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(data)!;
            return TinyJson.JsonParser.FromJson<T>(data)!;

            //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
            //T val = OdinSerializer.SerializationUtility.DeserializeValue<T>(buffer, OdinSerializer.DataFormat.JSON);
            //return val;
        }

        //**--make sure all Synchronizer.Model.IProcessor.Process methods are thread-safe

        // a new client has connected
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Connect connect)
        {
            if (connect.ModelVersion != Self.ModelVersion)
            {
                Serilog.Log.Error("Version mismatch with {ContactInfo}. Please update your asset and reinstall the Visual Studio extension. {ContactModel} != {SelfModel}", connect.ContactInfo, connect.ModelVersion, Self.ModelVersion);
                return;
            }

            if (connect.ProjectPath != Self.ProjectPath)
            {
                Serilog.Log.Error("Project mismatch with {ProjectName}. '{ConnectPath}' != '{SelfPath}'", connect.ProjectName, connect.ProjectPath, Self.ProjectPath);
                return;
            }

            if (!clients.Any(c => c.ContactInfo == connect.ContactInfo))
            {
                clients.Add(connect);
            }

            SendHandshake();
            if (ScriptFinder.GetActiveGameObject(out var activeGO))
                SendGameObject(activeGO);
            Assister.SendTagsAndLayers();
        }

        // a new client is online and requesting connection
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.RequestConnect requestConnect)
        {
            SendConnect();
        }
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Disconnect disconnect)
        {
            var client = clients.FirstOrDefault(c => c.ContactInfo == disconnect.ContactInfo);
            if (client == null)
                return;

            clients.Remove(client);
        }
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ConnectionInfo connectionInfo)
        {
            if (connectionInfo.ModelVersion != Self.ModelVersion)
            {
                Serilog.Log.Error("Version mismatch with {ContactInfo}. Please update your asset and reinstall the Visual Studio extension. {ContactModel} != {SelfModel}", connectionInfo.ContactInfo, connectionInfo.ModelVersion, Self.ModelVersion);
                return;
            }

            if (connectionInfo.ProjectPath != Self.ProjectPath)
            {
                Serilog.Log.Error("Project mismatch with {ProjectName}. '{ConnectPath}' != '{SelfPath}'", connectionInfo.ProjectName, connectionInfo.ProjectPath, Self.ProjectPath);
                return;
            }

            if (!clients.Any(c => c.ContactInfo == connectionInfo.ContactInfo))
            {
                SendConnect();
            }
            else
            {
                SendHandshake();
                if (ScriptFinder.GetActiveGameObject(out var activeGO))
                    SendGameObject(activeGO);
                Assister.SendTagsAndLayers();
            }
        }
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.RequestConnectionInfo requestConnectionInfo)
        {
            SendConnectionInfo();
        }
        /*
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Layers layers)
        {

        }
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Tags tags)
        {

        }
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.SortingLayers sortingLayers)
        {

        }*/
        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.StringArray stringArray)
        {
            Serilog.Log.Warning("Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.StringArray)");
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.StringArrayContainer stringArrayContainer)
        {
            Serilog.Log.Warning("Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.StringArrayContainer)");
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.GameObject gameObject)
        {
            Serilog.Log.Warning("Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.GameObject)");
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ComponentData component)
        {
            Serilog.Log.Warning("Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ComponentData)");
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.RequestScript requestScript)
        {
            if (requestScript.DeclaredTypes == null || requestScript.DeclaredTypes.Length == 0)
                return;

            var documentPath = requestScript.DocumentPath;

            foreach (var declaredType in requestScript.DeclaredTypes)
            {
                if (ScriptFinder.FindInstanceOfType(declaredType, documentPath, out var go, out var so))
                {
                    if (go != null)
                        SendGameObject(go);
                    else if (so != null)
                        SendScriptableObject(so);
                    else
                        Serilog.Log.Warning("Invalid instance of type");
                }
                else
                {
                    SendScriptMissing(declaredType);
                }
            }
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ScriptMissing scriptMissing)
        {
            Serilog.Log.Warning("Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ScriptMissing)");
        }


        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.Handshake handshake)
        {
            // Do nothing
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.RequestInternalLog requestInternalLog)
        {
            SendInternalLog();
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.InternalLog internalLog)
        {
            Logger.ELogger.VsInternalLog = internalLog.LogContent;
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.AnalyticsEvent analyticsEvent)
        {
            Serilog.Log.Warning("Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.AnalyticsEvent)");
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ErrorReport errorReport)
        {
            Serilog.Log.Warning("Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.ErrorReport)");
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.RequestVerboseType requestVerboseType)
        {
            Serilog.Log.Warning("Unity/Server shouldn't call Synchronizer.Model.IProcessor.Process(Synchronizer.Model.RequestVerboseType)");
        }

        void Synchronizer.Model.IProcessor.Process(Synchronizer.Model.RequestLazyLoad requestLazyLoad)
        {
            Monitor.LazyLoad(requestLazyLoad.Category);
        }



    }
}

