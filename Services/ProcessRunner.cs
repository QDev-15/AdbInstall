using System.Diagnostics;

namespace AdbInstaller.Services;

public sealed record ProcessResult(int ExitCode, string Output);

/// <summary>Runs an external executable (adb, java) off the UI thread, streaming each stdout/stderr line to
/// <paramref name="onLine"/> as it arrives (so the UI can show live progress) instead of only getting the
/// full output after the process exits.</summary>
public static class ProcessRunner
{
	public static async Task<ProcessResult> RunAsync(string fileName, string arguments, Action<string>? onLine = null, CancellationToken ct = default)
	{
		var psi = new ProcessStartInfo(fileName, arguments)
		{
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true,
			StandardOutputEncoding = System.Text.Encoding.UTF8,
			StandardErrorEncoding = System.Text.Encoding.UTF8,
		};

		using var process = new Process { StartInfo = psi, EnableRaisingEvents = true };
		var output = new System.Text.StringBuilder();
		var sync = new object();

		void Handle(string? line)
		{
			if (line == null) return;
			lock (sync) output.AppendLine(line);
			onLine?.Invoke(line);
		}

		process.OutputDataReceived += (_, e) => Handle(e.Data);
		process.ErrorDataReceived += (_, e) => Handle(e.Data);

		if (!process.Start()) throw new InvalidOperationException($"Could not start '{fileName}'.");
		process.BeginOutputReadLine();
		process.BeginErrorReadLine();

		// ConfigureAwait(false): AdbService.FindAdb() calls this synchronously via .GetAwaiter().GetResult() from
		// the Form's Load handler, before that handler's own first await -- so this runs on, and must complete
		// without hopping back onto, the blocked UI thread. Without this, resuming on the captured WinForms
		// SynchronizationContext deadlocks forever (the UI thread waits for a continuation that can only run on
		// the UI thread), and the main window is constructed but never shown.
		await using (ct.Register(() => { try { if (!process.HasExited) process.Kill(entireProcessTree: true); } catch { /* best-effort */ } }).ConfigureAwait(false))
		{
			await process.WaitForExitAsync(ct).ConfigureAwait(false);
		}

		return new ProcessResult(process.ExitCode, output.ToString());
	}
}
