using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KodingTask.Api.Hosting;

public sealed class ViteDevServer(
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<ViteDevServer> logger) : IHostedService, IDisposable
{
    private static readonly TimeSpan StartupTimeout = TimeSpan.FromSeconds(90);

    private readonly HttpClient _probe = new() { Timeout = TimeSpan.FromSeconds(2) };
    private Process? _process;

    public Uri ServerUrl { get; } = new(configuration["Development:ViteServerUrl"] ?? "http://localhost:5179");

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (await IsRespondingAsync(cancellationToken))
        {
            logger.LogInformation("[Vite] Using the dev server already running at {Url}.", ServerUrl);
            return;
        }

        var webClientPath = Path.GetFullPath(Path.Combine(
            environment.ContentRootPath,
            configuration["Development:WebClientPath"] ?? "../kodingtask-web"));
        logger.LogInformation("[Vite] Starting `npm run dev` in {Path}.", webClientPath);

        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        _process = Process.Start(new ProcessStartInfo
        {
            FileName = isWindows ? "cmd.exe" : "npm",
            Arguments = isWindows ? "/c npm run dev" : "run dev",
            WorkingDirectory = webClientPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        });
        if (_process is null)
            throw new InvalidOperationException("Could not start `npm run dev` for the web client.");

        _process.OutputDataReceived += (_, line) => Log(line.Data);
        _process.ErrorDataReceived += (_, line) => Log(line.Data);
        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();

        var deadline = DateTime.UtcNow + StartupTimeout;
        while (DateTime.UtcNow < deadline)
        {
            if (_process.HasExited)
                throw new InvalidOperationException($"`npm run dev` exited with code {_process.ExitCode}; see the [Vite] lines above.");
            if (await IsRespondingAsync(cancellationToken))
            {
                logger.LogInformation("[Vite] Dev server ready at {Url}.", ServerUrl);
                return;
            }
            await Task.Delay(500, cancellationToken);
        }

        throw new TimeoutException($"The Vite dev server did not answer at {ServerUrl} within {StartupTimeout.TotalSeconds} seconds.");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_process is { HasExited: false })
            _process.Kill(entireProcessTree: true);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _process?.Dispose();
        _probe.Dispose();
    }

    private async Task<bool> IsRespondingAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _probe.GetAsync(ServerUrl, cancellationToken);
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }

    private void Log(string? line)
    {
        if (!string.IsNullOrWhiteSpace(line))
            logger.LogInformation("[Vite] {Line}", line);
    }
}
