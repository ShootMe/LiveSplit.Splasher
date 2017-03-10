using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using LiveSplit.Memory;
namespace LiveSplit.Splasher {
	public partial class SplasherMemory {
		public Process Program { get; set; }
		public bool IsHooked { get; set; } = false;
		private DateTime lastHooked;
		private ProgramPointer gameData, gameManager, chronoState;

		public SplasherMemory() {
			lastHooked = DateTime.MinValue;
			gameData = new ProgramPointer(this, MemPointer.GameData);
			gameManager = new ProgramPointer(this, MemPointer.GameManager) { AutoDeref = false };
			chronoState = new ProgramPointer(this, MemPointer.ChronoHUD) { AutoDeref = false };
		}

		public string SceneName() {
			//GameData.Instance.CurrentLevelMetaData.SceneName
			return gameData.Read(0x208, 0x18);
		}
		public string LevelName() {
			//GameData.Instance.CurrentLevelMetaData.LevelName.Id
			return gameData.Read(0x208, 0x20, 0x10);
		}
		public int Checkpoints() {
			//GameData.Instance.CurrentLevelData.CheckpointCount
			return gameData.Read<int>(0x200, 0x38);
		}
		public int CurrentCheckpoint() {
			//GameManager.Instance.CheckpointIndex
			return gameManager.Read<int>(0x0, 0xd8);
		}
		public float PBTime(int checkpoint) {
			//GameData.Instance.CurrentLevelData.PersonalBestTimes[checkpoint]
			return gameData.Read<float>(0x200, 0x20, 0x20 + 0x4 * checkpoint);
		}
		public float CurrentTime(int checkpoint) {
			//GameData.Instance.CurrentLevelData.currentTimes[checkpoint]
			return gameData.Read<float>(0x200, 0x28, 0x20 + 0x4 * checkpoint);
		}
		public bool Paused() {
			//GameManager.Instance.paused
			return gameManager.Read<bool>(0x0, 0xa0);
		}
		public LockControlType ControlLock() {
			//GameManager._lockControl
			return (LockControlType)gameManager.Read<int>(0xc);
		}
		public GameMode GameMode() {
			//GameManager.Mode
			return (GameMode)gameManager.Read<int>(0x10);
		}
		public ChronometerState ChronoState() {
			//ChronoHUD.State
			return (ChronometerState)chronoState.Read<int>(0xc);
		}
		public float ElapsedTime() {
			//ChronoHUD.elapsedTime
			return (float)(int)(chronoState.Read<float>(0x10) * 100f) / 100f;
		}

		public bool HookProcess() {
			if ((Program == null || Program.HasExited) && DateTime.Now > lastHooked.AddSeconds(1)) {
				lastHooked = DateTime.Now;
				Process[] processes = Process.GetProcessesByName("Splasher");
				Program = processes.Length == 0 ? null : processes[0];
				IsHooked = true;
			}

			if (Program == null || Program.HasExited) {
				IsHooked = false;
			}

			return IsHooked;
		}
		public void Dispose() {
			if (Program != null) {
				Program.Dispose();
			}
		}
	}
	public enum MemVersion {
		None,
		V1
	}
	public enum MemPointer {
		GameData,
		GameManager,
		ChronoHUD
	}
	public class ProgramPointer {
		private static Dictionary<MemVersion, Dictionary<MemPointer, string>> funcPatterns = new Dictionary<MemVersion, Dictionary<MemPointer, string>>() {
			{MemVersion.V1, new Dictionary<MemPointer, string>() {
				{MemPointer.GameData, "55488BEC564883EC08488BF1488B0425????????488B4018488B4018F30F1005EC000000F30F5AC0F30F100DD0000000F30F5AC9F30F105610F30F5AD2F20F5CCAF20F59C1488BC8|-56"},
				{MemPointer.GameManager, "55488BEC564883EC08488BF1F30F10050C030000F30F5AC0F20F5AC04883EC2049BB????????????????41FFD34883C420B8" },
				{MemPointer.ChronoHUD, "55488BEC564883EC18488BF1B8????????488930488B4648488945E8488B0425????????488B4018488B80|-30" } //ChronoHUD.Awake
			}},
		};
		private IntPtr pointer;
		public SplasherMemory Memory { get; set; }
		public MemPointer Name { get; set; }
		public MemVersion Version { get; set; }
		public bool AutoDeref { get; set; }
		private int lastID;
		private DateTime lastTry;
		public ProgramPointer(SplasherMemory memory, MemPointer pointer) {
			this.Memory = memory;
			this.Name = pointer;
			this.AutoDeref = true;
			lastID = memory.Program == null ? -1 : memory.Program.Id;
			lastTry = DateTime.MinValue;
		}

		public IntPtr Value {
			get {
				GetPointer();
				return pointer;
			}
		}
		public T Read<T>(params int[] offsets) where T : struct {
			return Memory.Program.Read<T>(Value, offsets);
		}
		public string Read(params int[] offsets) {
			if (!Memory.IsHooked) { return string.Empty; }

			bool is64bit = Memory.Program.Is64Bit();
			IntPtr p = IntPtr.Zero;
			if (is64bit) {
				p = (IntPtr)Memory.Program.Read<ulong>(Value, offsets);
			} else {
				p = (IntPtr)Memory.Program.Read<uint>(Value, offsets);
			}
			return Memory.Program.Read(p, is64bit);
		}
		public void Write(int value, params int[] offsets) {
			Memory.Program.Write(Value, value, offsets);
		}
		public void Write(long value, params int[] offsets) {
			Memory.Program.Write(Value, value, offsets);
		}
		public void Write(double value, params int[] offsets) {
			Memory.Program.Write(Value, value, offsets);
		}
		public void Write(float value, params int[] offsets) {
			Memory.Program.Write(Value, value, offsets);
		}
		public void Write(short value, params int[] offsets) {
			Memory.Program.Write(Value, value, offsets);
		}
		public void Write(byte value, params int[] offsets) {
			Memory.Program.Write(Value, value, offsets);
		}
		public void Write(bool value, params int[] offsets) {
			Memory.Program.Write(Value, value, offsets);
		}
		private void GetPointer() {
			if (!Memory.IsHooked) {
				pointer = IntPtr.Zero;
				Version = MemVersion.None;
				return;
			}

			if (Memory.Program.Id != lastID) {
				pointer = IntPtr.Zero;
				Version = MemVersion.None;
				lastID = Memory.Program.Id;
			}
			if (pointer == IntPtr.Zero && DateTime.Now > lastTry.AddSeconds(1)) {
				lastTry = DateTime.Now;
				pointer = GetVersionedFunctionPointer();
				if (pointer != IntPtr.Zero) {
					bool is64bit = Memory.Program.Is64Bit();
					pointer = (IntPtr)Memory.Program.Read<uint>(pointer);
					if (AutoDeref) {
						if (is64bit) {
							pointer = (IntPtr)Memory.Program.Read<ulong>(pointer);
						} else {
							pointer = (IntPtr)Memory.Program.Read<uint>(pointer);
						}
					}
				}
			}
		}
		private IntPtr GetVersionedFunctionPointer() {
			foreach (MemVersion version in Enum.GetValues(typeof(MemVersion))) {
				Dictionary<MemPointer, string> patterns = null;
				if (!funcPatterns.TryGetValue(version, out patterns)) { continue; }

				string pattern = null;
				if (!patterns.TryGetValue(Name, out pattern)) { continue; }

				IntPtr ptr = Memory.Program.FindSignatures(pattern)[0];
				if (ptr != IntPtr.Zero) {
					Version = version;
					return ptr;
				}
			}
			Version = MemVersion.None;
			return IntPtr.Zero;
		}
	}
	public enum GameMode {
		Standard,
		TimeAttack,
		SpeedrunSelfish,
		SpeedrunFull,
		SpeedrunAny
	}
	public enum ChronometerState {
		Off,
		Running,
		Finished
	}
	public enum LockControlType {
		None,
		NoInputs,
		NoInputsNoCamOffset,
		FreezeAll,
		NoInputNoPhysics
	}
}