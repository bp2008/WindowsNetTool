namespace WindowsNetTool
{
	/// <summary>
	/// Implemented by tool controls which contain user-editable state that can be lost,
	/// so the main form can warn before the application closes.
	/// </summary>
	public interface IHasUnsavedChanges
	{
		bool HasUnsavedChanges { get; }
	}
}
