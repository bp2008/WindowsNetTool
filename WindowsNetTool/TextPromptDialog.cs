using System;
using System.Windows.Forms;

namespace WindowsNetTool
{
	/// <summary>
	/// A simple modal dialog that prompts the user for one line of text.
	/// This form is constructed entirely in code; it has no designer file.
	/// </summary>
	public class TextPromptDialog : Form
	{
		private readonly TextBox txtInput;

		public string Value
		{
			get { return txtInput.Text; }
		}

		public TextPromptDialog(string title, string prompt, string initialValue)
		{
			Text = title;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			MinimizeBox = false;
			MaximizeBox = false;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.CenterParent;
			ClientSize = new System.Drawing.Size(420, 108);

			Label lblPrompt = new Label();
			lblPrompt.AutoSize = true;
			lblPrompt.Location = new System.Drawing.Point(12, 12);
			lblPrompt.MaximumSize = new System.Drawing.Size(396, 0);
			lblPrompt.Text = prompt;
			Controls.Add(lblPrompt);

			txtInput = new TextBox();
			txtInput.Location = new System.Drawing.Point(12, 40);
			txtInput.Size = new System.Drawing.Size(396, 20);
			txtInput.Text = initialValue ?? "";
			Controls.Add(txtInput);

			Button btnOk = new Button();
			btnOk.Text = "OK";
			btnOk.DialogResult = DialogResult.OK;
			btnOk.Location = new System.Drawing.Point(252, 72);
			btnOk.Size = new System.Drawing.Size(75, 25);
			Controls.Add(btnOk);

			Button btnCancel = new Button();
			btnCancel.Text = "Cancel";
			btnCancel.DialogResult = DialogResult.Cancel;
			btnCancel.Location = new System.Drawing.Point(333, 72);
			btnCancel.Size = new System.Drawing.Size(75, 25);
			Controls.Add(btnCancel);

			AcceptButton = btnOk;
			CancelButton = btnCancel;
			txtInput.SelectAll();
		}

		/// <summary>
		/// Shows the prompt and returns the entered text, or null if the user cancelled.
		/// </summary>
		public static string Show(IWin32Window owner, string title, string prompt, string initialValue)
		{
			using (TextPromptDialog dialog = new TextPromptDialog(title, prompt, initialValue))
			{
				if (dialog.ShowDialog(owner) == DialogResult.OK)
					return dialog.Value;
				return null;
			}
		}
	}
}
