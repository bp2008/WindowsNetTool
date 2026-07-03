namespace WindowsNetTool
{
	/// <summary>
	/// Implemented by tool controls which should reload system state each time the user switches
	/// to them, so they are not left showing stale data (e.g. old interface names after a rename
	/// performed in a different tool).
	/// </summary>
	public interface IRefreshOnActivate
	{
		void RefreshOnActivate();
	}
}
