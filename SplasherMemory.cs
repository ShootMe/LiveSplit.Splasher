using LiveSplit.Memory;
using System;
using System.Diagnostics;
namespace LiveSplit.Splasher {
	public partial class SplasherMemory {
		//GameData.SetInstance
		private static ProgramPointer gameData = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "41FFD34883C420488B45C848898610020000B8????????488930488B8668", 19));
		//GameManager.Awake
		private static ProgramPointer gameManager = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "55488BEC564883EC08488BF1F30F1005CC050000F30F5AC0F20F5AC04883EC2049BB", 50));
		//ChronoHUD.Awake
		private static ProgramPointer chronoState = new ProgramPointer(AutoDeref.Single, new ProgramSignature(PointerVersion.V1, "55488BEC564883EC18488BF1B8????????488930488B4648488945E8488B0425", 13));
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;

		public SplasherMemory() {
			lastHooked = DateTime.MinValue;
		}

		public string SceneName() {
			//GameData.Instance.CurrentLevelMetaData.SceneName
			return gameData.Read(Program, 0x0, 0x208, 0x18, 0x0);
		}
		public string LevelName() {
			//GameData.Instance.CurrentLevelMetaData.LevelName.Id
			return gameData.Read(Program, 0x0, 0x208, 0x20, 0x10, 0x0);
		}
		public int Checkpoints() {
			//GameData.Instance.CurrentLevelData.CheckpointCount
			return gameData.Read<int>(Program, 0x0, 0x200, 0x38);
		}
		public int CurrentCheckpoint() {
			//GameManager.Instance.CheckpointIndex
			return gameManager.Read<int>(Program, 0x0, 0x118);
		}
		public float PBTime(int checkpoint) {
			//GameData.Instance.CurrentLevelData.PersonalBestTimes[checkpoint]
			return gameData.Read<float>(Program, 0x0, 0x200, 0x20, 0x20 + 0x4 * checkpoint);
		}
		public float CurrentTime(int checkpoint) {
			//GameData.Instance.CurrentLevelData.currentTimes[checkpoint]
			return gameData.Read<float>(Program, 0x0, 0x200, 0x28, 0x20 + 0x4 * checkpoint);
		}
		public bool Paused() {
			//GameManager.Instance.paused
			return gameManager.Read<bool>(Program, 0x0, 0x11c);
		}
		public LockControlType ControlLock() {
			//GameManager._lockControl
			return (LockControlType)gameManager.Read<int>(Program, 0xc);
		}
		public GameMode GameMode() {
			//GameManager.Mode
			return (GameMode)gameManager.Read<int>(Program, 0x10);
		}
		public ChronometerState ChronoState() {
			//ChronoHUD.State
			return (ChronometerState)chronoState.Read<int>(Program, 0xc);
		}
		public float ElapsedTime() {
			//ChronoHUD.elapsedTime
			return (float)(int)(chronoState.Read<float>(Program, 0x10) * 100f) / 100f;
		}

		public bool HookProcess() {
			IsHooked = Program != null && !Program.HasExited;
			if (!IsHooked && DateTime.Now > lastHooked.AddSeconds(1)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("Splasher");
				Program = processes != null && processes.Length > 0 ? processes[0] : null;

				if (Program != null && !Program.HasExited) {
					MemoryReader.Update64Bit(Program);
					IsHooked = true;
				}
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) {
				Program.Dispose();
			}
		}
	}
}