using LiveSplit.Model;
using LiveSplit.UI.Components;
using System;
using System.Reflection;
namespace LiveSplit.Splasher {
	public class SplasherFactory : IComponentFactory {
		public string ComponentName { get { return "Splasher Autosplitter v" + this.Version.ToString(); } }
		public string Description { get { return "Autosplitter for Splasher"; } }
		public ComponentCategory Category { get { return ComponentCategory.Control; } }
		public IComponent Create(LiveSplitState state) { return new SplasherComponent(state); }
		public string UpdateName { get { return this.ComponentName; } }
		public string UpdateURL { get { return "https://raw.githubusercontent.com/ShootMe/LiveSplit.Splasher/master/"; } }
		public string XMLURL { get { return this.UpdateURL + "Components/LiveSplit.Splasher.Updates.xml"; } }
		public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
	}
}