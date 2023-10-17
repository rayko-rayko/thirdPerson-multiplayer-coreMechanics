using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

namespace Asteroids.HostAdvanced
{
	public class MigrationManager : MonoBehaviour, INetworkRunnerCallbacks
	{
		private Dictionary<PlayerRef, List<NetworkObject>> _delayedMigration = new Dictionary<PlayerRef, List<NetworkObject>>();
		private byte[] _uglyLocalTokenHack;

		public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
		{
			if (runner.IsServer)
			{
				byte[] token = runner.LocalPlayer==player ? _uglyLocalTokenHack : runner.GetPlayerConnectionToken(player);
				Debug.Log($"Player {player} Joined with token {FormatByteArray(token)}");
				PlayerRef migrateFromPlayerRef = FromByteArray(token);
				if (migrateFromPlayerRef != PlayerRef.None)
				{
					Debug.Log($"Requesting Migration of Player {migrateFromPlayerRef} to {player}");
					MigratePlayerObjects(migrateFromPlayerRef,player);
				}
			}
		}

		public async void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
		{
			INetworkObjectPool originalPool = runner.ObjectPool;
			INetworkSceneManager originalSceneManager = runner.SceneManager;
			SceneRef originalScene = runner.CurrentScene;

			byte[] reconnectToken = ToByteArray(runner.LocalPlayer);

			Debug.Log($"Host Migration - Reconnecting player {runner.LocalPlayer} with token {FormatByteArray(reconnectToken)}");

			// Shutdown and destroy the old runner
			await runner.Shutdown(destroyGameObject:false, shutdownReason: ShutdownReason.HostMigration);
			Destroy(runner);

			// Give Unity time to remove the component (or AddComponent will fail)
			await Task.Yield();

			// Setup the new runner...
			runner = gameObject.AddComponent<NetworkRunner>();
			runner.name = "Migrated Runner";
			runner.ProvideInput = true;

			_uglyLocalTokenHack = reconnectToken;
			// Start the new Runner using the "HostMigrationToken" and pass a callback ref in "HostMigrationResume".
			StartGameResult result = await runner.StartGame(new StartGameArgs() {
				HostMigrationToken = hostMigrationToken,   // contains all necessary info to restart the Runner
				HostMigrationResume = HostMigrationResume, // this will be invoked to resume the simulation
				ObjectPool = originalPool,
				Scene = originalScene,
				SceneManager = originalSceneManager,
				ConnectionToken = reconnectToken
			});

			// Check StartGameResult as usual
			if (result.Ok) {
				Debug.Log("Reconnected after host migration");
			} else {
				Debug.LogWarning($"Reconnection after host migration failed with {result.ShutdownReason}");
			}
		}

		// Resume Simulation on the new Runner
		void HostMigrationResume(NetworkRunner runner) {
			Debug.Log("Migrating Host State for Scene Objects");
			foreach (var sceneNO in runner.GetResumeSnapshotNetworkSceneObjects())
			{
				sceneNO.Item1.CopyStateFromSceneObject( sceneNO.Item2 );
				ResumeMigration( sceneNO.Item1,  sceneNO.Item1);
			}

			Debug.Log("Migrating Host State for Dynamic Objects");
			foreach (NetworkObject resumeNO in runner.GetResumeSnapshotNetworkObjects())
			{
				MigrationBehaviour mb = resumeNO.GetComponent<MigrationBehaviour>();
				if (mb)
				{
					Debug.Log($"Re-spawning {resumeNO.name} with input authority {resumeNO.InputAuthority}");
					Vector3 p=Vector3.zero;
					Quaternion q=Quaternion.identity;
					if (resumeNO.TryGetBehaviour<NetworkPositionRotation>(out var posRot))
					{
						p = posRot.ReadPosition();
						q = posRot.ReadRotation();
					}
					var newNO = runner.Spawn(resumeNO, p,q, PlayerRef.None, (networkRunner, o) =>
					{
						o.CopyStateFrom(resumeNO);
						o.GetComponent<MigrationBehaviour>().IsPendingMigration = true; // Only need this because player refs change
					});
					ResumeMigration(resumeNO, newNO);
				}
			}
			Debug.Log("Migrated Host State");
		}

		private void ResumeMigration(NetworkObject resumeNO, NetworkObject newNO)
		{
			MigrationBehaviour oldMigrator = resumeNO.GetComponent<MigrationBehaviour>();
			if (oldMigrator != null)
			{
				MigrationBehaviour newMigrator = newNO.GetComponent<MigrationBehaviour>();

				if (oldMigrator.LastKnownInputAuth != PlayerRef.None)
				{
					// If player refs didn't change we could set this here instead:					newMigrator.IsPendingMigration = true;
					if (!_delayedMigration.TryGetValue(oldMigrator.LastKnownInputAuth, out List<NetworkObject> list))
					{
						list = new List<NetworkObject>();
						_delayedMigration[oldMigrator.LastKnownInputAuth] = list;
					}
					list.Add(newNO);
					Debug.Log($"Delayed migration of {newNO.name} for player {oldMigrator.LastKnownInputAuth}");
				}
				else
				{
					newMigrator.Migrate();
					Debug.Log($"Migrated {newMigrator.name}");
				}
			}
			else
			{
				Debug.Log($"Not re-spawning {resumeNO.name} - it is not a migration behaviour");
			}
		}

		public static unsafe byte[] ToByteArray(PlayerRef value)
		{
			int tv = value + 1;
			byte* pv = (byte*)&tv;
			byte[] token = new byte[4];
			token[0] = pv[0];
			token[1] = pv[1];
			token[2] = pv[2];
			token[3] = pv[3];
			return token;
		}

		public static unsafe PlayerRef FromByteArray(byte[] token)
		{
			if (token == null)
				return PlayerRef.None;
			int value;
			byte* pv = (byte*)&value;
			pv[0] = token[0];
			pv[1] = token[1];
			pv[2] = token[2];
			pv[3] = token[3];
			return value-1;
		}
		
		private string FormatByteArray(byte[] token)
		{
			if (token == null)
				return "<null>";
			StringBuilder sb = new StringBuilder();
			foreach (byte b in token)
			{
				sb.Append($"[{b}]");
			}
			return sb.ToString();
		}

		private void MigratePlayerObjects(PlayerRef oldref, PlayerRef newref)
		{
			Debug.Log($"Migrating Player {oldref} to {newref}");
			if (_delayedMigration.TryGetValue(oldref, out List<NetworkObject> migrated))
			{
				Debug.Log($"Migrating {migrated.Count} objects from Player {oldref} to {newref}");
				_delayedMigration.Remove(oldref);
				foreach (var resume in migrated)
				{
					Debug.Log($"Re-assigning input auth for [{resume?.Name}] from player {oldref} to {newref}");
					resume.AssignInputAuthority( newref );
					MigrationBehaviour m = resume.GetComponent<MigrationBehaviour>();
					m.Migrate(); // Call this instantly on the server because OnChanged won't trigger here (because it's migrated in the same frame as it is spawned). We only need this because player refs change
				}
			}
			else
				Debug.LogWarning($"Nothing to migrate? ({_delayedMigration.Count} lists in collection)");
		}

		public void OnInput(NetworkRunner runner, NetworkInput input) { }
		public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
		public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
		public void OnConnectedToServer(NetworkRunner runner) { }
		public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
		public void OnDisconnectedFromServer(NetworkRunner runner) { }
		public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
		public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
		public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
		public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
		public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
		public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
		public void OnSceneLoadDone(NetworkRunner runner) { } 
		public void OnSceneLoadStart(NetworkRunner runner) { }
	}
}
