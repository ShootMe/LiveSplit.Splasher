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
		internal static string[] keys = { "CurrentSplit", "State", "SceneName", "Checkpoints", "CurrentCP", "Paused", "GameMode", "ChronoState", "ControlLock", "FinTime" };
		private SplasherMemory mem;
		private int currentSplit = -1, state = 0, lastLogCheck = 0, lastCheckpoint = 0;
		private bool hasLog = false, lastFinished = false;
		private Dictionary<string, string> currentValues = new Dictionary<string, string>();
		private SplasherSettings settings;
		private static string LOGFILE = "_Splasher.log";

		public SplasherComponent(LiveSplitState state) {
			mem = new SplasherMemory();
			settings = new SplasherSettings();
			foreach (string key in keys) {
				currentValues[key] = "";
			}

			if (state != null) {
				Model = new TimerModel() { CurrentState = state };
				Model.InitializeGameTime();
				Model.CurrentState.IsGameTimePaused = true;
				state.OnReset += OnReset;
				state.OnPause += OnPause;
				state.OnResume += OnResume;
				state.OnStart += OnStart;
				state.OnSplit += OnSplit;
				state.OnUndoSplit += OnUndoSplit;
				state.OnSkipSplit += OnSkipSplit;
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
				shouldSplit = mem.GameMode() != GameMode.Standard && mem.ChronoState() == ChronometerState.Running && mem.CurrentCheckpoint() == 0 && elapsedTime > 0 && elapsedTime < 0.5;
			} else if (Model.CurrentState.CurrentPhase == TimerPhase.Running) {
				int checkpoint = mem.CurrentCheckpoint();
				if (settings.Checkpoints) {
					if (lastCheckpoint < checkpoint) {
						shouldSplit = true;
					}
				}

				ChronometerState chronoState = mem.ChronoState();
				bool finished = chronoState == ChronometerState.Finished;
				if (finished && !lastFinished) {
					shouldSplit = true;
				}

				lastFinished = finished;
				lastCheckpoint = checkpoint;

				LockControlType controlLock = mem.ControlLock();
				Model.CurrentState.IsGameTimePaused = controlLock != LockControlType.None || mem.Paused() || chronoState != ChronometerState.Running;
			}

			if (currentSplit >= 0 && Model.CurrentState.IsGameTimePaused) {
				Model.CurrentState.SetGameTime(TimeSpan.FromSeconds(mem.ElapsedTime()));
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
						case "FinTime":
							ChronometerState cs = mem.ChronoState();
							if (cs == ChronometerState.Finished) {
								float time = mem.ElapsedTime();
								int num = (int)(time / 60f);
								float num2 = time - (float)(60 * num);
								int num3 = (int)num2;
								int num4 = (int)((num2 - (float)num3) * 100f);
								string text = (num < 10) ? ("0" + num) : (string.Empty + num);
								string text2 = (num3 < 10) ? ("0" + num3) : (string.Empty + num3);
								string text3 = (num4 < 10) ? ("0" + num4) : (string.Empty + num4);
								curr = text + ":" + text2 + ":" + text3;
							} else {
								curr = prev;
							}
							break;
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
			IList<ILayoutComponent> components = lvstate.Layout.LayoutComponents;
			for (int i = components.Count - 1; i >= 0; i--) {
				ILayoutComponent component = components[i];
				if (component.Component is SplasherComponent && invalidator == null && width == 0 && height == 0) {
					components.Remove(component);
				}
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