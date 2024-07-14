namespace HealthSync.BackgroundServices
{
	/// <summary>
	/// Logger scopr to include Sync Context Id
	/// </summary>
	public class LoggerScope : IDisposable
	{
		private static readonly AsyncLocal<LoggerScope> _current = new();

		public string SyncContextId { get; }

		public static LoggerScope Current
		{
			get => _current.Value;
			private set => _current.Value = value;
		}

		public LoggerScope(string syncContextId)
		{
			SyncContextId = syncContextId;
			Current = this;
		}

		public void Dispose() => Current = null;
	}
}
