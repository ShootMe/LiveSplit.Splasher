using System.Threading;
namespace LiveSplit.Splasher {
	public class SplasherTest {
		private static SplasherComponent comp = new SplasherComponent();
		public static void Main(string[] args) {
			Thread t = new Thread(GetVals);
			t.IsBackground = true;
			t.Start();
			System.Windows.Forms.Application.Run();
		}
		private static void GetVals() {
			while (true) {
				try {
					comp.GetValues();

					Thread.Sleep(12);
				} catch { }
			}
		}
	}
}