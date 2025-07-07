using System.Collections.Generic;
using System.Collections;
using PurrNet.Logging;
using PurrNet.Modules;
using UnityEngine;

namespace PurrNet
{
    public class PlayerSpawner : PurrMonoBehaviour
    {
        [SerializeField, HideInInspector] private NetworkIdentity playerPrefab;
        [SerializeField] private GameObject _playerPrefab;
        [Tooltip("Even if rules are to not despawn on disconnect, this will ignore that and always spawn a player.")]
        [SerializeField] private bool _ignoreNetworkRules;

        [SerializeField] private List<Transform> spawnPoints = new ();
        private int _currentSpawnPoint;

        private void Awake()
        {
            CleanupSpawnPoints();
        }

        private void CleanupSpawnPoints()
        {
            bool hadNullEntry = false;
            for (int i = 0; i < spawnPoints.Count; i++)
            {
                if (!spawnPoints[i])
                {
                    hadNullEntry = true;
                    spawnPoints.RemoveAt(i);
                    i--;
                }
            }

            if (hadNullEntry)
                PurrLogger.LogWarning($"Some spawn points were invalid and have been cleaned up.", this);
        }

        private void OnValidate()
        {
            if (playerPrefab)
            {
                _playerPrefab = playerPrefab.gameObject;
                playerPrefab = null;
            }
        }

        public override void Subscribe(NetworkManager manager, bool asServer)
        {
            if (asServer && manager.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
            {
                scenePlayersModule.onPlayerLoadedScene += OnPlayerLoadedScene;

                if (!manager.TryGetModule(out ScenesModule scenes, true))
                    return;

                if (!scenes.TryGetSceneID(gameObject.scene, out var sceneID))
                    return;

                if (scenePlayersModule.TryGetPlayersInScene(sceneID, out var players))
                {
                    foreach (var player in players)
                        OnPlayerLoadedScene(player, sceneID, true);
                }
            }
        }

        public override void Unsubscribe(NetworkManager manager, bool asServer)
        {
            if (asServer && manager.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
                scenePlayersModule.onPlayerLoadedScene -= OnPlayerLoadedScene;
        }

        private void OnDestroy()
        {
            if (NetworkManager.main &&
                NetworkManager.main.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
                scenePlayersModule.onPlayerLoadedScene -= OnPlayerLoadedScene;
        }

        private void OnPlayerLoadedScene(PlayerID player, SceneID scene, bool asServer)
        {
            StartCoroutine(OnPlayerLoadedScene_Co(player, scene, asServer));
        }
        private IEnumerator OnPlayerLoadedScene_Co(PlayerID player, SceneID scene, bool asServer)
        {
            while (spawnPoints.Count == 0)
            {
                yield return new WaitForEndOfFrame();
            }

            var main = NetworkManager.main;
            ScenesModule scenes = null;
            bool _continue = true;
            if (!main || !main.TryGetModule(out scenes, true))
                _continue = false;

            var unityScene = gameObject.scene;

            if (!scenes.TryGetSceneID(unityScene, out var sceneID))
                _continue = false;

            if (sceneID != scene)
                _continue = false;

            if (!asServer)
                _continue = false;

            bool isDestroyOnDisconnectEnabled = main.networkRules.ShouldDespawnOnOwnerDisconnect();
            if (!_ignoreNetworkRules && !isDestroyOnDisconnectEnabled && main.TryGetModule(out GlobalOwnershipModule ownership, true) &&
                ownership.PlayerOwnsSomething(player))
                _continue = false;

            if (_continue)
            {
                GameObject newPlayer;

                CleanupSpawnPoints();

                if (spawnPoints.Count > 0)
                {
                    var spawnPoint = spawnPoints[_currentSpawnPoint];
                    _currentSpawnPoint = (_currentSpawnPoint + 1) % spawnPoints.Count;
                    newPlayer = UnityProxy.Instantiate(_playerPrefab, spawnPoint.position, spawnPoint.rotation, unityScene);
                }
                else
                {
                    _playerPrefab.transform.GetPositionAndRotation(out var position, out var rotation);
                    newPlayer = UnityProxy.Instantiate(_playerPrefab, position, rotation, unityScene);
                }

                if (newPlayer.TryGetComponent(out NetworkIdentity identity))
                    identity.GiveOwnership(player);
            }
        }
        public void SetSpawns(List<Transform> _spawns)
        {
            spawnPoints = _spawns;
        }
        public Transform GetSpawn()
        {
            if (spawnPoints.Count == 0)
                return null;
            Transform t = spawnPoints[_currentSpawnPoint];
            _currentSpawnPoint = (_currentSpawnPoint + 1) % spawnPoints.Count;
            return t;
        }
    }
}
