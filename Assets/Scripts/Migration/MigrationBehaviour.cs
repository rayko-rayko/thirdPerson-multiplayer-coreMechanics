using Fusion;
using UnityEngine;

namespace Asteroids.HostAdvanced
{
	public class MigrationBehaviour : NetworkBehaviour
	{
		[Networked] public PlayerRef LastKnownInputAuth { get; set; }
		[Networked(OnChanged = nameof(Migrate))] public NetworkBool IsPendingMigration { get; set; }

		private bool _migrated;

		public override void Spawned()
		{
			_migrated = false;
			if(IsPendingMigration)
				gameObject.SetActive(false);
			else
				Migrate();
		}

		internal void Migrate()
		{
			if (!_migrated)
			{
				Debug.Log($"{this}.Migrate()");
				IsPendingMigration = false; // ... and also get rid of this if player refs didn't change
				// We're storing this explicitly because it will otherwise be lost after the first migration
				// (in cases where the same session is migrated again, input auth for old player objects will be None, causing the object to spawn as a host object)
				LastKnownInputAuth = Object.InputAuthority;
				gameObject.SetActive(true);
				Migrated();
				_migrated = true;
			}
			else
			{
				Debug.Log($"Skipping {this}.Migrate() because we already migrated");
			}
		}

		public virtual void Migrated()
		{
		}

		public static void Migrate(Changed<MigrationBehaviour> changed)
		{
			if (!changed.Behaviour.IsPendingMigration)
			{
				changed.Behaviour.Migrate();
			}
		}
	}
}
