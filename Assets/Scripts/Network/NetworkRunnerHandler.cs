using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;
using System.Linq;


public class NetworkRunnerHandler : MonoBehaviour
{
    public NetworkRunner networkRunnerPrefab;

    NetworkRunner _networkRunner;

    public static CharacterInputActions characterInputActions;
    private void Awake()
    {
        NetworkRunner networkRunnerInScene = FindObjectOfType<NetworkRunner>();
        
        // IF we already have a network runner in the scene then we should not create another one but rather use the existing one
        if (networkRunnerInScene != null)
        {
            _networkRunner = networkRunnerInScene;
        }
    }
    
    void Start()
    {
        if (_networkRunner == null)
        {
            // NetworkRunner örneği oluşturuluyor ve ismi "Network Runner" olarak ayarlanıyor.
            _networkRunner = Instantiate(networkRunnerPrefab);
            _networkRunner.name = "Network Runner";

            if (SceneManager.GetActiveScene().name != "MainMenu")
            {
                // Network Runner'ı başlatmak için gerekli parametrelerle bir InitializeNetworkRunner görevi oluşturuluyor.
                var clientTask = InitializeNetworkRunner(_networkRunner, GameMode.AutoHostOrClient, "TestSession", GameManager.instance.GetConnectionToken(), NetAddress.Any(), SceneManager.GetActiveScene().buildIndex, null);
            }
            
            // Konsola bilgi mesajı yazdırılıyor.
            Debug.Log($"Server NetworkRunner started");
        }
    }

    public void StartHostMigration(HostMigrationToken hostMigrationToken)
    {
        // Create a new Network runner, old one is being shut down
        _networkRunner = Instantiate(networkRunnerPrefab);
        _networkRunner.name = "Network runner - Migrated";

        var clientTask = InitializeNetworkRunnerHostMigration(_networkRunner, hostMigrationToken);
        
        Debug.Log($"Host migration started");
    }
    
    INetworkSceneManager GetSceneManager(NetworkRunner networkRunner)
    {
        // Network Runner'ın bir sahne yöneticisi (sceneManager) bileşenini alıyoruz, eğer yoksa varsayılan bir tane ekliyoruz.
        var sceneManager = networkRunner.GetComponents(typeof(MonoBehaviour)).OfType<INetworkSceneManager>().FirstOrDefault();

        if (sceneManager == null)
        {
            // Handle networked object that already exits in the scene
            sceneManager = networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        return sceneManager;
    }

    // Network Runner'ı başlatmak için kullanılan method.
    protected virtual Task InitializeNetworkRunner(NetworkRunner networkRunner, GameMode gameMode, string sessionName, byte[] connectionToken, NetAddress netAddress, SceneRef sceneRef, Action<NetworkRunner> initialized)
    {
        var sceneManager = GetSceneManager(networkRunner);

        // Network Runner'a giriş sağlayabilmesi için ProvideInput değeri true olarak ayarlanıyor.
        networkRunner.ProvideInput = true;

        // Network Runner'ı başlatmak için StartGameArgs nesnesi oluşturuluyor ve belirtilen parametrelerle dolduruluyor.
        return networkRunner.StartGame(new StartGameArgs
        {
            GameMode = gameMode,
            Address =  netAddress,
            Scene = sceneRef,
            SessionName = sessionName,
            CustomLobbyName = "OurLobbyID",
            Initialized = initialized,
            SceneManager = sceneManager,
            ConnectionToken = connectionToken
        });
    }
    
    // Network Runner'ı başlatmak için kullanılan method.
    protected virtual Task InitializeNetworkRunnerHostMigration(NetworkRunner networkRunner, HostMigrationToken hostMigrationToken)
    {
        var sceneManager = GetSceneManager(networkRunner);

        // Network Runner'a giriş sağlayabilmesi için ProvideInput değeri true olarak ayarlanıyor.
        networkRunner.ProvideInput = true;

        // Network Runner'ı başlatmak için StartGameArgs nesnesi oluşturuluyor ve belirtilen parametrelerle dolduruluyor.
        return networkRunner.StartGame(new StartGameArgs
        {
            SceneManager = sceneManager,
            HostMigrationToken = hostMigrationToken, // contains all necessary info to restart the Runner
            HostMigrationResume = HostMigrationResume, // this will be invoked to resume the simulation
            ConnectionToken = GameManager.instance.GetConnectionToken()
        });
    }

    void HostMigrationResume(NetworkRunner networkRunner)
    {
        Debug.Log($"HostMigrationResume started");
        
        // Get reference for each Network object from the old Host
        foreach (var resumeNetworkObject in networkRunner.GetResumeSnapshotNetworkObjects())
        {
            // Grab all the player objects, they have a NetworkCharacterControllerPrototypeCustom
            if (resumeNetworkObject.TryGetBehaviour<NetworkCharacterControllerPrototypeCustom>(out var characterController))
            {
                networkRunner.Spawn(resumeNetworkObject, position: characterController.ReadPosition(), rotation: characterController.ReadRotation(), onBeforeSpawned: (networkRunner, newNetworkObject) =>
                {
                    newNetworkObject.CopyStateFrom(resumeNetworkObject);
                    
                    // Copy info state from old Behaviour to new behaviour
                    if (resumeNetworkObject.TryGetBehaviour<HPHandler>(out HPHandler oldHPHandler))
                    {
                        HPHandler newHPHandler = newNetworkObject.GetComponent<HPHandler>();
                        newHPHandler.CopyStateFrom(oldHPHandler);

                        newHPHandler.skipSettingStartValues = true;
                    }
                    
                    // Map the connection token with the new Network player
                    if (resumeNetworkObject.TryGetBehaviour<NetworkPlayer>(out var oldNetworkPlayer))
                    {
                        // Store Player token for reconnection
                        FindObjectOfType<Spawner>().SetConnectionTokenMapping(oldNetworkPlayer.token, newNetworkObject.GetComponent<NetworkPlayer>());
                    }
                });
            }
        }
        
        Debug.Log($" {networkRunner} {transform.name}");
        
        StartCoroutine(CleanUpHostMigrationCO());
        
        networkRunner.SetActiveScene(SceneManager.GetActiveScene().buildIndex);
        
        Debug.Log($"HostMigrationResume completed");

        
    }

    IEnumerator CleanUpHostMigrationCO()
    {
        Debug.Log($"before enable{characterInputActions.asset.enabled}, {transform.name}");
        
        yield return new WaitForSeconds(1.5f);
        
        characterInputActions.Enable();
        Debug.Log($"after enable{characterInputActions.asset.enabled}, {transform.name}");
        
        FindObjectOfType<Spawner>().OnHostMigrationCleanUp();
    }

    public void OnJoinLobby()
    {
        var clientTask = JoinLobby();
    }

    private async Task JoinLobby()
    {
        Debug.Log("JoinLobby started");

        string lobbyID = "OurLobbyID";

        var result = await _networkRunner.JoinSessionLobby(SessionLobby.Custom, lobbyID);
        
        if (!result.Ok)
        {
            Debug.LogError($"Unable to join lobby {lobbyID}");
        }
        else
        {
            Debug.Log("JoinLobby ok");
        }
    }

    public void CreateGame(string sessionName, string sceneName)
    {
        Debug.Log($"Create session {sessionName} scene {sceneName} build index {SceneUtility.GetBuildIndexByScenePath($"scenes/{sceneName}")}");
        
        // Join existing game as a client
        var clientTask = InitializeNetworkRunner(_networkRunner, GameMode.Host, sessionName, GameManager.instance.GetConnectionToken(),
            NetAddress.Any(), SceneUtility.GetBuildIndexByScenePath(($"scenes/{sceneName}")), null);
    }

    public void JoinGame(SessionInfo sessionInfo)
    {
        Debug.Log($"Join Session {sessionInfo.Name}");
        
        // Join existing game as a client
        var clientTask = InitializeNetworkRunner(_networkRunner, GameMode.Client, sessionInfo.Name,
            GameManager.instance.GetConnectionToken(), NetAddress.Any(), SceneManager.GetActiveScene().buildIndex, null);
    }
    
    private void OnEnable()
    {
        characterInputActions = new CharacterInputActions();
        characterInputActions.Enable();
    }
    private void OnDisable()
    {
        characterInputActions.Disable();
    }
}