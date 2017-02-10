using LiveSplit.Model;
using LiveSplit.UI;
using LiveSplit.UI.Components;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
namespace LiveSplit.Splasher {
	public class SplasherComponent : IComponent {
		public string ComponentName { get { return "Splasher Autosplitter"; } }
		public TimerModel Model { get; set; }
		public IDictionary<string, Action> ContextMenuControls { get { return null; } }
		internal static string[] keys = { "CurrentSplit", "State", "SceneName", "Checkpoints", "CurrentCP", "Paused", "GameMode", "ChronoState", "ControlLock" };
		private SplasherMemory mem;
		private int currentSplit = -1, state = 0, lastLogCheck = 0, lastCheckpoint = 0;
		private bool hasLog = false, lastFinished = false;
		private Dictionary<string, string> currentValues = new Dictionary<string, string>();
		private SplasherSettings settings;
		private static string LOGFILE = "_Splasher.log";

		public SplasherComponent() {
			mem = new SplasherMemory();
			settings = new SplasherSettings();
			foreach (string key in keys) {
				currentValues[key] = "";
			}
		}

		public void GetValues() {
			if (!mem.HookProcess()) { return; }

			if (Model != null) {
				HandleSplits();
			}

			LogValues();
		}
		private void HandleSplits() {
			bool shouldSplit = false;

			if (currentSplit == -1) {
				float elapsedTime = mem.ElapsedTime();
				shouldSplit = mem.SceneName() == "A1" && mem.GameMode() != GameMode.Standard && mem.ChronoState() == ChronometerState.Running && mem.CurrentCheckpoint() == 0 && elapsedTime > 0 && elapsedTime < 0.5;
			} else if (Model.CurrentState.CurrentPhase == TimerPhase.Running) {
				int checkpoint = mem.CurrentCheckpoint();
				if (settings.Checkpoints) {
					if (lastCheckpoint < checkpoint) {
						shouldSplit = true;
					}
				}

				bool finished = mem.ChronoState() == ChronometerState.Finished;
				if (finished && !lastFinished) {
					shouldSplit = true;
				}

				lastFinished = finished;
				lastCheckpoint = checkpoint;

				LockControlType controlLock = mem.ControlLock();
				Model.CurrentState.IsGameTimePaused = controlLock != LockControlType.None || mem.Paused();
			}

			GameMode gameMode = mem.GameMode();
			HandleSplit(shouldSplit, settings.AutoReset && gameMode == GameMode.Standard || (gameMode == GameMode.TimeAttack && string.IsNullOrEmpty(mem.SceneName())));
		}
		private void HandleGameTimes() {
			if (currentSplit >= 0 && currentSplit <= Model.CurrentState.Run.Count) {
				TimeSpan gameTime = TimeSpan.FromSeconds(mem.ElapsedTime());
				if (currentSplit == Model.CurrentState.Run.Count) {
					Time t = Model.CurrentState.Run[currentSplit - 1].SplitTime;
					Model.CurrentState.Run[currentSplit - 1].SplitTime = new Time(t.RealTime, gameTime);
				} else {
					Model.CurrentState.SetGameTime(gameTime);
				}
			}
		}
		private void HandleSplit(bool shouldSplit, bool shouldReset = false) {
			if (shouldReset) {
				if (currentSplit >= 0) {
					Model.Reset();
				}
			} else if (shouldSplit) {
				if (currentSplit < 0) {
					Model.Start();
				} else {
					Model.Split();
				}
			}
		}
		private void LogValues() {
			if (lastLogCheck == 0) {
				hasLog = File.Exists(LOGFILE);
				lastLogCheck = 300;
			}
			lastLogCheck--;

			if (hasLog || !Console.IsOutputRedirected) {
				string prev = "", curr = "";
				foreach (string key in keys) {
					prev = currentValues[key];

					switch (key) {
						case "CurrentSplit": curr = currentSplit.ToString(); break;
						case "State": curr = state.ToString(); break;
						case "SceneName": curr = mem.SceneName(); break;
						case "Checkpoints": curr = mem.Checkpoints().ToString(); break;
						case "CurrentCP": curr = mem.CurrentCheckpoint().ToString(); break;
						case "Paused": curr = mem.Paused().ToString(); break;
						case "GameMode": curr = mem.GameMode().ToString(); break;
						case "ChronoState": curr = mem.ChronoState().ToString(); break;
						case "ControlLock": curr = mem.ControlLock().ToString(); break;
						default: curr = ""; break;
					}

					if (!prev.Equals(curr)) {
						WriteLogWithTime(key + ": ".PadRight(16 - key.Length, ' ') + prev.PadLeft(25, ' ') + " -> " + curr);

						currentValues[key] = curr;
					}
				}
			}
		}

		public void Update(IInvalidator invalidator, LiveSplitState lvstate, float width, float height, LayoutMode mode) {
			if (Model == null) {
				Model = new TimerModel() { CurrentState = lvstate };
				Model.InitializeGameTime();
				Model.CurrentState.IsGameTimePaused = true;
				lvstate.OnReset += OnReset;
				lvstate.OnPause += OnPause;
				lvstate.OnResume += OnResume;
				lvstate.OnStart += OnStart;
				lvstate.OnSplit += OnSplit;
				lvstate.OnUndoSplit += OnUndoSplit;
				lvstate.OnSkipSplit += OnSkipSplit;
			}

			GetValues();
		}

		public void OnReset(object sender, TimerPhase e) {
			currentSplit = -1;
			state = 0;
			lastCheckpoint = 0;
			lastFinished = false;
			Model.CurrentState.IsGameTimePaused = true;
			WriteLog("---------Reset----------------------------------");
		}
		public void OnResume(object sender, EventArgs e) {
			WriteLog("---------Resumed--------------------------------");
		}
		public void OnPause(object sender, EventArgs e) {
			WriteLog("---------Paused---------------------------------");
		}
		public void OnStart(object sender, EventArgs e) {
			currentSplit = 0;
			state = 0;
			lastCheckpoint = 0;
			lastFinished = false;
			Model.CurrentState.IsGameTimePaused = false;
			WriteLog("---------New Game-------------------------------");
		}
		public void OnUndoSplit(object sender, EventArgs e) {
			currentSplit--;
			state = 0;
		}
		public void OnSkipSplit(object sender, EventArgs e) {
			currentSplit++;
			state = 0;
		}
		public void OnSplit(object sender, EventArgs e) {
			currentSplit++;
			state = 0;
			HandleGameTimes();
		}
		private void WriteLog(string data) {
			if (hasLog || !Console.IsOutputRedirected) {
				if (Console.IsOutputRedirected) {
					using (StreamWriter wr = new StreamWriter(LOGFILE, true)) {
						wr.WriteLine(data);
					}
				} else {
					Console.WriteLine(data);
				}
			}
		}
		private void WriteLogWithTime(string data) {
			WriteLog(DateTime.Now.ToString(@"HH\:mm\:ss.fff") + (Model != null && Model.CurrentState.CurrentTime.RealTime.HasValue ? " | " + Model.CurrentState.CurrentTime.RealTime.Value.ToString("G").Substring(3, 11) : "") + ": " + data);
		}

		public Control GetSettingsControl(LayoutMode mode) { return settings; }
		public void SetSettings(XmlNode document) { settings.SetSettings(document); }
		public XmlNode GetSettings(XmlDocument document) { return settings.UpdateSettings(document); }
		public void DrawHorizontal(Graphics g, LiveSplitState state, float height, Region clipRegion) { }
		public void DrawVertical(Graphics g, LiveSplitState state, float width, Region clipRegion) { }
		public float HorizontalWidth { get { return 0; } }
		public float MinimumHeight { get { return 0; } }
		public float MinimumWidth { get { return 0; } }
		public float PaddingBottom { get { return 0; } }
		public float PaddingLeft { get { return 0; } }
		public float PaddingRight { get { return 0; } }
		public float PaddingTop { get { return 0; } }
		public float VerticalHeight { get { return 0; } }
		public void Dispose() { }
	}
}