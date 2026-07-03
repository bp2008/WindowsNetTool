using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace WindowsNetTool.Tools
{
	/// <summary>
	/// Shared helper for running the `netsh` command line tool.
	/// </summary>
	internal static class Netsh
	{
		/// <summary>
		/// Runs `netsh` with the given arguments and returns standard output.
		/// Throws NetshException if the exit code is nonzero.
		/// </summary>
		public static string RunChecked(string arguments)
		{
			using (Process p = new Process())
			{
				p.StartInfo.FileName = "netsh";
				p.StartInfo.Arguments = arguments;
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.CreateNoWindow = true;
				p.StartInfo.RedirectStandardOutput = true;
				p.StartInfo.RedirectStandardError = true;
				p.Start();
				Task<string> errTask = p.StandardError.ReadToEndAsync();
				string std = p.StandardOutput.ReadToEnd();
				string err = errTask.Result;
				p.WaitForExit();
				if (p.ExitCode != 0)
				{
					string message = "`netsh " + arguments + "` exited with code " + p.ExitCode + ".";
					if (!string.IsNullOrWhiteSpace(std))
						message += Environment.NewLine + std.Trim();
					if (!string.IsNullOrWhiteSpace(err))
						message += Environment.NewLine + err.Trim();
					throw new NetshException(message);
				}
				return std;
			}
		}
	}

	public class NetshException : Exception
	{
		public NetshException(string message) : base(message) { }
	}
}
