namespace WindowsNetTool.Tools.Export
{
	/// <summary>
	/// Implemented by tools whose current view can be exported via the main window's Export button.
	/// The button is disabled while the active tool does not implement this interface.
	/// </summary>
	public interface IExportableTool
	{
		/// <summary>
		/// Builds the tool's current content for export.  Called each time the user picks an item
		/// from the Export menu, so each export reflects exactly what the tool is showing at that
		/// moment.
		/// </summary>
		ExportableContent BuildExportContent();
	}
}
