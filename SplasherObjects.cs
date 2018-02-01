namespace LiveSplit.Splasher {
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