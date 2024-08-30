using Mirror;
using System;
using Zenject;
using UnityEngine;
using com.playbux.map;
using com.playbux.api;
using Newtonsoft.Json;
using com.playbux.quest;
using com.playbux.enemy;
using System.Collections;
using com.playbux.avatar;
using com.playbux.events;
using com.playbux.entities;
using com.playbux.identity;
using Newtonsoft.Json.Linq;
using com.playbux.versioning;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using com.playbux.fakeplayer;
using Random = UnityEngine.Random;
using com.playbux.firebaseservice;
using System.Security.Cryptography;
using PlayMode = SETHD.Echo.PlayMode;
using com.playbux.gameeventcollection;
using com.playbux.networking.networkavatar;
using com.playbux.networking.mirror.message;

/*
Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

namespace com.playbux.networking.mirror.core
{
    public class PlaybuxNetworkManager : NetworkManager
    {
        // Overrides the base singleton so we don't
        // have to cast to this type everywhere.


        public static new PlaybuxNetworkManager singleton { get; private set; }

        [SerializeField]
        private NetworkIdentity playerIdentity;

#if SERVER
        private IQuestRunner questRunner;
        private IIdentitySystem identitySystem;
        private IMapController serverMapController;
        private IAvatarBoard<uint, NetworkIdentity> board;
        private GameEventCollection gameEventCollection;
#else
        private INetworkMessageReceiver<VersioningInfo> versioningInfoReceiver;
#endif

        private bool updateUserListNow = false;
        private SignalBus signalBus;
        private Versioning versioning;
        private EnemyAssetDatabase enemyAssetDatabase;
        private ICredentialProvider credentialProvider;
        private NetworkEnemyFactory enemyEntityFactory;
        private AuthenticationSignal authenticationSignal;
        private NetworkFakePlayerFactory fakePlayerFactory;
        private EntityFactory<NetworkIdentity> entityFactory;
        private FakePlayerAssetDatabase fakePlayerAssetDatabase;
        private Dictionary<GameObject, IEntity<NetworkIdentity>> playerEntities = new Dictionary<GameObject, IEntity<NetworkIdentity>>();     
       [Inject]
        private void Construct(
            SignalBus signalBus,
            Versioning versioning,
#if SERVER
            IQuestRunner questRunner,
            IIdentitySystem identitySystem,
            IMapController serverMapController,
            GameEventCollection gameEventCollection,
            IAvatarBoard<uint, NetworkIdentity> board,
#else
            INetworkMessageReceiver<VersioningInfo> versioningInfoReceiver,
#endif
            EnemyAssetDatabase enemyAssetDatabase,
            ICredentialProvider credentialProvider,
            NetworkEnemyFactory enemyEntityFactory,
            NetworkFakePlayerFactory fakePlayerFactory,
            EntityFactory<NetworkIdentity> entityFactory,
            FakePlayerAssetDatabase fakePlayerAssetDatabase
        )
        {
            this.signalBus = signalBus;
            this.entityFactory = entityFactory;
            this.fakePlayerFactory = fakePlayerFactory;
            this.enemyEntityFactory = enemyEntityFactory;
            this.enemyAssetDatabase = enemyAssetDatabase;
            this.credentialProvider = credentialProvider;
            authenticationSignal = new AuthenticationSignal();

#if SERVER
            this.board = board;
            this.questRunner = questRunner;
            this.identitySystem = identitySystem;
            this.gameEventCollection = gameEventCollection;
            this.serverMapController = serverMapController;
#else
            this.versioningInfoReceiver = versioningInfoReceiver;
            this.fakePlayerAssetDatabase = fakePlayerAssetDatabase;
            this.versioningInfoReceiver.OnEventCalled += OnVersioningClientResponse;
#endif
        }

        #region Unity Callbacks

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Awake()
        {
            base.Awake();
            Debug.unityLogger.logEnabled = true;
            Debug.Log("[version] : " + Application.version);
            singleton = this;
        }

        public override void OnValidate()
        {
            base.OnValidate();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// Networking is NOT initialized when this fires
        /// </summary>
        public override void Start()
        {
            base.Start();
            Debug.unityLogger.logEnabled = true;
            Debug.Log("[version] : " + Application.version);
            //StartCoroutine(WaitForStart());
        }

        
        /// <summary>
        /// Runs on both Server and Client
        /// </summary>
        public override void LateUpdate()
        {
            base.LateUpdate();
        }

        /// <summary>
        /// Runs on both Server and Client
        /// </summary>
        public override void OnDestroy()
        {
            base.OnDestroy();
        }

#endregion

        #region Start & Stop

        /// <summary>
        /// Set the frame rate for a headless server.
        /// <para>Override if you wish to disable the behavior or set your own tick rate.</para>
        /// </summary>
        public override void ConfigureHeadlessFrameRate()
        {
            base.ConfigureHeadlessFrameRate();
        }

        /// <summary>
        /// called when quitting the application by closing the window / pressing stop in the editor
        /// </summary>
        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// This causes the server to switch scenes and sets the networkSceneName.
        /// <para>Clients that connect to this server will automatically switch to this scene. This is called automatically if onlineScene or offlineScene are set, but it can be called from user code to switch scenes again while the game is in progress. This automatically sets clients to be not-ready. The clients must call NetworkClient.Ready() again to participate in the new scene.</para>
        /// </summary>
        /// <param name="newSceneName"></param>
        public override void ServerChangeScene(string newSceneName)
        {
            base.ServerChangeScene(newSceneName);
        }

        /// <summary>
        /// Called from ServerChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows server to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        public override void OnServerChangeScene(string newSceneName)
        {
        }

        /// <summary>
        /// Called on the server when a scene is completed loaded, when the scene load was initiated by the server with ServerChangeScene().
        /// </summary>
        /// <param name="sceneName">The name of the new scene.</param>
        public override void OnServerSceneChanged(string sceneName)
        {
        }

        /// <summary>
        /// Called from ClientChangeScene immediately before SceneManager.LoadSceneAsync is executed
        /// <para>This allows client to do work / cleanup / prep before the scene changes.</para>
        /// </summary>
        /// <param name="newSceneName">Name of the scene that's about to be loaded</param>
        /// <param name="sceneOperation">Scene operation that's about to happen</param>
        /// <param name="customHandling">true to indicate that scene loading will be handled through overrides</param>
        public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation,
            bool customHandling)
        {
        }

        /// <summary>
        /// Called on clients when a scene has completed loaded, when the scene load was initiated by the server.
        /// <para>Scene changes can cause player objects to be destroyed. The default implementation of OnClientSceneChanged in the NetworkManager is to add a player object for the connection if no player object exists.</para>
        /// </summary>
        public override void OnClientSceneChanged()
        {
            base.OnClientSceneChanged();
        }

        #endregion

        #region Server System Callbacks

        /// <summary>
        /// Called on the server when a new client connects.
        /// <para>Unity calls this on the Server when a Client connects to the Server. Use an override to tell the NetworkManager what to do when a client connects to the server.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerConnect(NetworkConnectionToClient conn)
        {
        }

        /// <summary>
        /// Called on the server when a client is ready.
        /// <para>The default implementation of this function calls NetworkServer.SetClientReady() to continue the network setup process.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerReady(NetworkConnectionToClient conn)
        {
            base.OnServerReady(conn);
        }

        /// <summary>
        /// Called on the server when a client adds a new player with ClientScene.AddPlayer.
        /// <para>The default implementation for this function creates a new player object from the playerPrefab.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
        }

        /// <summary>
        /// Called on the server when a client disconnects.
        /// <para>This is called on the Server when a Client disconnects from the Server. Use an override to decide what should happen when a disconnection is detected.</para>
        /// </summary>
        /// <param name="conn">Connection from client.</param>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            if (conn.identity != null)
            {
                GameObject spawned = conn.identity.gameObject;
                if (playerEntities.ContainsKey(spawned))
                {
                    playerEntities[spawned].Dispose();
                    playerEntities.Remove(spawned);
                }
            }

            base.OnServerDisconnect(conn);
            conn.Disconnect();
        }

        /// <summary>
        /// Called on server when transport raises an error.
        /// <para>NetworkConnection may be null.</para>
        /// </summary>
        /// <param name="conn">Connection of the client...may be null</param>
        /// <param name="transportError">TransportError enum</param>
        /// <param name="message">String message of the error.</param>
        public override void OnServerError(NetworkConnectionToClient conn, TransportError transportError,
            string message)
        {
        }

        #endregion

        #region Client System Callbacks

        /// <summary>
        /// Called on the client when connected to a server.
        /// <para>The default implementation of this function sets the client as ready and adds a player. Override the function to dictate what happens when the client connects.</para>
        /// </summary>
        public override void OnClientConnect()
        {
            base.OnClientConnect();
            signalBus.Fire(authenticationSignal);

            //var characterMessage = new CreateCharacterMessage("John" + Random.Range(1, 1000));
            //NetworkClient.Send(characterMessage);
        }

        /// <summary>
        /// Called on clients when disconnected from a server.
        /// <para>This is called on the client when it disconnects from the server. Override this function to decide what happens when the client disconnects.</para>
        /// </summary>
        public override void OnClientDisconnect()
        {
            if (!NetworkClient.isConnected)
                return;

            DisposePlayer(NetworkClient.localPlayer.gameObject);
        }

        /// <summary>
        /// Called on clients when a servers tells the client it is no longer ready.
        /// <para>This is commonly used when switching scenes.</para>
        /// </summary>
        public override void OnClientNotReady()
        {
        }

        /// <summary>
        /// Called on client when transport raises an error.</summary>
        /// </summary>
        /// <param name="transportError">TransportError enum.</param>
        /// <param name="message">String message of the error.</param>
        public override void OnClientError(TransportError transportError, string message)
        {
        }

        #endregion

        #region Start & Stop Callbacks

        // Since there are multiple versions of StartServer, StartClient and StartHost, to reliably customize
        // their functionality, users would need override all the versions. Instead these callbacks are invoked
        // from all versions, so users only need to implement this one case.

        /// <summary>
        /// This is invoked when a host is started.
        /// <para>StartHost has multiple signatures, but they all cause this hook to be called.</para>
        /// </summary>
        public override void OnStartHost()
        {
        }


        public override void OnStartServer()
        {
            base.OnStartServer();

            //TODO: activate after netID was sended

            APIServerConnector.GameFlagAPI().Forget();
            NetworkServer.RegisterHandler<AuthenticationMessage>(OnCreateCharacter);

            SendOnlineUser().Forget();


        }

        private async UniTask SendOnlineUser()
        {
            while (true)
            {
                try
                {
                    OnlineUser onlineUser = new OnlineUser();
                    onlineUser.Users = new List<UserDataToAPI>();
                    foreach (var connection in NetworkServer.connections)
                    {
                        try
                        {
                            IdentitySystem ids = (IdentitySystem)credentialProvider;
                            string _id = ids[connection.Value.identity.netId].UID;
                            UserDataToAPI userDataToAPI = new UserDataToAPI();
                            userDataToAPI.user_id = _id;
                            userDataToAPI.is_vip = false;
                            userDataToAPI.total_step = 0;
                            userDataToAPI.total_distance = 0;
                            userDataToAPI.online_duration = 0;
                            onlineUser.Users.Add(userDataToAPI);
                        }
                        catch
                        {

                        }
                    }

                    string jsonData = JsonConvert.SerializeObject(onlineUser);
                    Debug.Log(jsonData);
                    string data = await APIServerConnector.SendUserList(jsonData);
                    Debug.Log(data);


                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }

                try
                {
                    APIRecovery.GetInstante().ResendRequest();
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }


                await UniTask.WaitForSeconds(60f);
            }
        }


        /// <summary>
        /// This is invoked when the client is started.
        /// </summary>
        public override void OnStartClient()
        {
            NetworkClient.RegisterSpawnHandler(playerIdentity.assetId, SpawnPlayer, DisposePlayer);

            foreach (var id in enemyAssetDatabase.AssetIds)
            {
                NetworkClient.RegisterSpawnHandler(id, SpawnEnemy, DisposeEnemy);
            }

            NetworkClient.RegisterSpawnHandler(fakePlayerAssetDatabase.RandomAssetById.id, SpawnFakeRandomPlayer, DisposeFakePlayer);

            // foreach (var id in fakePlayerAssetDatabase.AssetIds)
            // {
            //     NetworkClient.RegisterSpawnHandler(id, SpawnFakePresetPlayer, DisposeFakePlayer);
            // }
        }

        /// <summary>
        /// This is called when a host is stopped.
        /// </summary>
        public override void OnStopHost()
        {
        }

        /// <summary>
        /// This is called when a server is stopped - including when a host is stopped.
        /// </summary>
        public override void OnStopServer()
        {
        }

        /// <summary>
        /// This is called when a client is stopped.
        /// </summary>
        public override void OnStopClient()
        {
            NetworkClient.DestroyAllClientObjects();
            NetworkClient.UnregisterSpawnHandler(playerIdentity.assetId);
#if SERVER
            serverMapController.Dispose();
#endif
        }

        #endregion

        //SERVER SIDE OnCreateCharacter
        private async void OnCreateCharacter(NetworkConnectionToClient connection, AuthenticationMessage message)
        {
            Debug.unityLogger.logEnabled = true;
            Debug.Log("[version] : " + Application.version);
#if DEVELOPMENT
            Debug.Log("[Login] token :" + message.Token);
#endif
            var syncResponse = await APIServerConnector.SyncAPI(message.Token);

            var syncQuest = await APIServerConnector.QuestList(message.Token);

            UserProfile userProfile = await APIServerConnector.GetMe(message.Token);

            await UniTask.WaitForSeconds(1);

            userProfile = await APIServerConnector.GetMe(message.Token);

            Debug.Log("[Login] token :" + JsonConvert.SerializeObject(userProfile));


            if (userProfile == null)
            {
                connection.Disconnect();

                Debug.Log("[Login] receive Identity from API fail.");

                return;
            }

            Debug.Log("[Login] receive Identity from API :" + JsonConvert.SerializeObject(userProfile));


            //TODO: if UID already in server reject previous player with this UID


            AvatarSet set = new AvatarSet(userProfile.equipments.JSONForAvatarSet());
            IEntity<NetworkIdentity> player = entityFactory.Create();
#if DEVELOPMENT
            Debug.Log("[Login] Create Player Onserver :" + player.Identity.netId);
#endif
            var spawnPosition = 1.5f * Random.insideUnitCircle + new Vector2(-17.3299999f, -47.0400009f);

            player.GameObject.transform.position = spawnPosition;
            var circle = player.GameObject.AddComponent<CircleCollider2D>();
            circle.radius = 1.0f;
            circle.isTrigger = true;
            var rigid = player.GameObject.AddComponent<Rigidbody2D>();
            rigid.gravityScale = 0f;
            player.GameObject.name = userProfile.display_name;



#if DEVELOPMENT
            Debug.Log("[Login] Add Player to player list  :");
#endif

            playerEntities.TryAdd(player.GameObject, player);


            ServerInit(player, userProfile, connection, message.Token).Forget();

#if DEVELOPMENT
            Debug.Log("[Login]Create PLayer on Client:");
#endif

            NetworkServer.AddPlayerForConnection(connection, player.GameObject); //NetID assign Here and send to client

#if DEVELOPMENT
            Debug.Log("[Login]Setup Avatar for player:");
#endif

#if SERVER
            board.UpdateAvatarSet(player.Identity.netId, set);
            StartCoroutine(SendEventToPlayer(connection));
#endif

#if DEVELOPMENT
            Debug.Log("Player name :" + message.Token + " spawned");
#endif
        }

        //CLIENT SIDE SpawnPlayer
        private GameObject SpawnPlayer(SpawnMessage msg)
        {
            if (enemyAssetDatabase.Contains(msg.assetId))
                return null;

            IEntity<NetworkIdentity> instance = entityFactory.Create();
            StartCoroutine(ClientInit(instance));
            signalBus.Fire(new BGMPlaySignal("BGM/Exterior", PlayMode.StartOver));
            return instance.GameObject;
        }

        private GameObject SpawnEnemy(SpawnMessage msg)
        {
            if (!enemyAssetDatabase.Contains(msg.assetId))
                return null;

            IEntity<EnemyIdentity> instance = enemyEntityFactory.Create(enemyAssetDatabase.Get(msg.assetId), msg.position);
            StartCoroutine(EnemyInit(instance));
            return instance.GameObject;
        }

        private GameObject SpawnFakePresetPlayer(SpawnMessage msg)
        {
            if (!fakePlayerAssetDatabase.Contains(msg.assetId))
                return null;

            IEntity<FakePlayerIdentity> instance = fakePlayerFactory.Create(fakePlayerAssetDatabase.Get(msg.assetId), msg.position);
            StartCoroutine(FakePlayerInit(instance));
            return instance.GameObject;
        }

        private GameObject SpawnFakeRandomPlayer(SpawnMessage msg)
        {
            IEntity<FakePlayerIdentity> instance = fakePlayerFactory.Create(fakePlayerAssetDatabase.RandomAssetById.asset, msg.position);
            StartCoroutine(FakePlayerInit(instance));
            return instance.GameObject;
        }

        private void DisposePlayer(GameObject spawned)
        {
            if (playerEntities.TryGetValue(spawned, out IEntity<NetworkIdentity> value))
            {
                var credential = credentialProvider.GetData(value.Identity);
                credentialProvider.OnPlayerDisconnected(credential);
                value.Dispose();
                playerEntities.Remove(spawned);
            }

#if SERVER
            NetworkServer.Destroy(spawned);
#else
            Destroy(spawned);
#endif
        }

        private void DisposeEnemy(GameObject spawned)
        {
#if SERVER
            NetworkServer.Destroy(spawned);
#else
            Destroy(spawned);
#endif
        }

        private void DisposeFakePlayer(GameObject spawned)
        {
#if SERVER
            NetworkServer.Destroy(spawned);
#else
            Destroy(spawned);
#endif
        }


#if !SERVER
        private void OnVersioningClientResponse(VersioningInfo message)
        {
            string serverVersion = message.version;

            //TODO: Show UI load new game
            //disconnect player

        }
#endif

#if SERVER
        private IEnumerator SendEventToPlayer(NetworkConnectionToClient connection)
        {
            yield return null;
            gameEventCollection.Clean();
            IGameEvent[] events = gameEventCollection.GetCollection();

            for (int i = 0; i < events.Length; i++)
            {
                //Debug.Log(events[i].ToString());
                events[i].OnClientConnected(connection);
                yield return new WaitForSeconds(1);
            }
        }
#endif



        //Server Side Initialize Player Information
        private async UniTask ServerInit(
            IEntity<NetworkIdentity> entity,
            UserProfile userProfile,
            NetworkConnectionToClient connectionToClient,
            string accessToken)
        {
            //Connect To API Server Recieve Player Data
            //yield return new WaitForSecondsRealtime(1 / sendRate);

            await UniTask.Delay(TimeSpan.FromSeconds(1 / sendRate), ignoreTimeScale: false);

#if BINARY_NETWORK
            Debug.LogWarning("Using binary data");
#else
            Debug.LogWarning("It TEXTJSON");
#endif
            credentialProvider.OnPlayerAuthenticated(userProfile.uid, connectionToClient.identity.netId);
            //NOTE: send flag here
#if SERVER

            await questRunner.AssignStartQuestFlag(connectionToClient, connectionToClient.identity.netId, userProfile.uid, "flagcollection,", "", accessToken, true);


            serverMapController.Initialize();
            entity.Initialize();
#endif
        }

        //Client Side Initialize Player Information
        private IEnumerator ClientInit(IEntity<NetworkIdentity> entity)
        {
            yield return new WaitForSecondsRealtime(1 / sendRate);
            entity.Initialize();
        }

        private IEnumerator EnemyInit(IEntity<EnemyIdentity> entity)
        {
            yield return new WaitForSecondsRealtime(1 / sendRate);
            entity.Initialize();
        }

        private IEnumerator FakePlayerInit(IEntity<FakePlayerIdentity> entity)
        {
            yield return new WaitForSecondsRealtime(1 / sendRate);
            entity.Initialize();
        }

        public class Factory : PlaceholderFactory<PlaybuxNetworkManager, NetworkSetting, PlaybuxNetworkManager>
        {
        }
    }
}