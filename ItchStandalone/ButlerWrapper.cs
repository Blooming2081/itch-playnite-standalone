using CliWrap;
using CliWrap.EventStream;
using Newtonsoft.Json.Linq;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ItchStandalone
{
    public class ButlerWrapper
    {
        private readonly ILogger logger = LogManager.GetLogger();
        private readonly ItchStandaloneSettingsViewModel settings;

        public ButlerWrapper(ItchStandaloneSettingsViewModel settings)
        {
            this.settings = settings;
        }

        private string GetButlerPath()
        {
            var path = settings.GetAbsoluteButlerPath();
            if (string.IsNullOrEmpty(path))
            {
                // Fallback or error handling
                return "butler"; 
            }
            return path;
        }

        public async Task<List<string>> FetchChannelsAsync(string gameUrl)
        {
            var channels = new List<string>();
            var butlerExe = GetButlerPath();

            // butler fetch <url> --json
            // Output example: {"type":"entry","value":{"channel":"windows-64", ...}}
            
            try 
            {
                var cmd = Cli.Wrap(butlerExe)
                    .WithArguments(new[] { "fetch", gameUrl, "--json" })
                    .WithValidation(CommandResultValidation.None);

                await foreach (var cmdEvent in cmd.ListenAsync())
                {
                    if (cmdEvent is StandardOutputCommandEvent stdOut)
                    {
                        try
                        {
                            var json = JObject.Parse(stdOut.Text);
                            if (json["type"]?.ToString() == "entry")
                            {
                                var channel = json["value"]?["channel"]?.ToString();
                                if (!string.IsNullOrEmpty(channel))
                                {
                                    channels.Add(channel);
                                }
                            }
                        }
                        catch {/* Warning: Ignore parse errors for non-json lines */}
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to fetch channels.");
            }

            return channels.Distinct().ToList();
        }

        public async Task InstallAsync(string gameUrl, string installDir, string channel, CancellationToken cancelToken, Action<double> progressCallback)
        {
            var butlerExe = GetButlerPath();
            // butler install <url> <dest> --channel <channel> --json

            var args = new List<string> { "install", gameUrl, installDir, "--json" };
            if (!string.IsNullOrEmpty(channel))
            {
                args.Add("--channel");
                args.Add(channel);
            }

            try
            {
                var cmd = Cli.Wrap(butlerExe)
                    .WithArguments(args)
                    .WithValidation(CommandResultValidation.None);

                await foreach (var cmdEvent in cmd.ListenAsync(cancelToken))
                {
                    if (cmdEvent is StandardOutputCommandEvent stdOut)
                    {
                        try
                        {
                            var json = JObject.Parse(stdOut.Text);
                            if (json["type"]?.ToString() == "progress")
                            {
                                var percentage = json["percentage"]?.ToObject<double>() ?? 0;
                                progressCallback?.Invoke(percentage * 100); 
                            }
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error during installation.");
                throw; 
            }
        }

        public async Task UninstallAsync(string installDir)
        {
            var butlerExe = GetButlerPath();
            // butler clean <path>
            
            try
            {
                await Cli.Wrap(butlerExe)
                    .WithArguments(new[] { "clean", installDir })
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error during uninstallation.");
                throw;
            }
        }

        public async Task<string> CheckVersionAsync()
        {
            var butlerExe = GetButlerPath();
            try
            {
                var result = await Cli.Wrap(butlerExe)
                    .WithArguments("version")
                    .ExecuteBufferedAsync();
                return result.StandardOutput;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to get butler version.");
                return "Error";
            }
        }
    }
}
