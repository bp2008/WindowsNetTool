using System.Windows.Forms;

namespace WindowsNetTool
{
	/// <summary>
	/// A ListView with double-buffered painting.  The stock ListView flickers when its items are
	/// updated frequently; DoubleBuffered is protected, so a subclass is needed to enable it.
	/// </summary>
	public class DoubleBufferedListView : ListView
	{
		public DoubleBufferedListView()
		{
			DoubleBuffered = true;
		}
	}
}
