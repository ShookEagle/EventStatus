using System;
using System.Collections.Generic;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Logging;

namespace EventStatus;

public class EventStatus : BasePlugin
{
    public override string ModuleName => "EventStatus";
    public override string ModuleVersion => "1.2.0";
    public override string ModuleAuthor => "ShookEagle";

    private Dictionary<string, PlayerData> playersData = new Dictionary<string, PlayerData>();
    private DateTime? eventStartTime = null;
    private bool EventStarted;
    private int PeakPlayers = 0;

    public override void Load(bool hotReload)
    {
        Logger.LogInformation("Event Status Ready");
    }

    [ConsoleCommand("css_estart", "Starts Event Status Collection")]
    [ConsoleCommand("css_startevent", "Starts Event Status Collection")]
    [ConsoleCommand("css_eventstart", "Starts Event Status Collection")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    [RequiresPermissions("@css/rcon")]
    public void OnEStart(CCSPlayerController? caller, CommandInfo commandInfo)
    {
        if (!EventStarted)
        {
            List<CCSPlayerController> allPlayersBots = Utilities.GetPlayers();
            var allPlayers = allPlayersBots.Where(x => !x.IsBot);
            eventStartTime = DateTime.UtcNow;
            playersData.Clear();
            EventStarted = true;
            PeakPlayers = Utilities.GetPlayers().Count;
            foreach (var player in allPlayers)
            {
                var steamIdString = player.SteamID.ToString();
                playersData[steamIdString] = new PlayerData
                {
                    SteamId = steamIdString,
                    PlayerName = player.PlayerName,
                    JoinTime = DateTime.UtcNow
                };
            }
            Server.PrintToChatAll($"[{ChatColors.DarkBlue}EC{ChatColors.White}] Event started.");
            Logger.LogInformation("DateTime Collection Started");
        }
        else
        {
            Server.PrintToChatAll($"[{ChatColors.DarkBlue}EC{ChatColors.White}] An event is already started.");
        }
    }

    [ConsoleCommand("css_ecstatus", "Returns Current Event Status in Caller Console")]
    [ConsoleCommand("css_estatus", "Returns Current Event Status in Caller Console")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    [RequiresPermissions("@css/rcon")]
    public void OnEStatus(CCSPlayerController? caller, CommandInfo commandInfo)
    {
        if (caller != null)
        {
            if (eventStartTime == null)
            {
                Server.PrintToChatAll($"[{ChatColors.DarkBlue}EC{ChatColors.White}] No event is currently running.");
                return;
            }

            var eventDuration = DateTime.UtcNow - eventStartTime.Value;
            caller.PrintToConsole($"=== Begin Event Logs (Duration: {FormatTimeSpan(eventDuration)}) - (Peak: {PeakPlayers} players) ===\n");

            int rank = 1;
            var orderedPlayers = playersData.Values
                .OrderByDescending(player => player.TotalTimeSpent)
                .ToList();

            foreach (var player in orderedPlayers)
            {
                var timeSpent = player.TotalTimeSpent;

                if (player.JoinTime != DateTime.MinValue)
                {
                    timeSpent += DateTime.UtcNow - player.JoinTime;
                }

                caller.PrintToConsole($"#{rank}. {player.SteamId} {player.PlayerName} {FormatTimeSpan(timeSpent)}\n");
                rank++;
            }

            caller.PrintToConsole("=== End Event Logs ===");
            caller.PrintToChat($"[{ChatColors.DarkBlue}EC{ChatColors.White}] Status Output in Console.");
            Logger.LogInformation($"{caller.PlayerName} Called Status");
        }
    }

    [ConsoleCommand("css_estop", "Stops Event Status Collection")]
    [ConsoleCommand("css_stopevent", "Stops Event Status Collection")]
    [ConsoleCommand("css_endevent", "Stops Event Status Collection")]
    [ConsoleCommand("css_eventstop", "Stops Event Status Collection")]
    [CommandHelper(whoCanExecute: CommandUsage.CLIENT_ONLY)]
    [RequiresPermissions("@css/rcon")]
    public void OnEStop(CCSPlayerController? caller, CommandInfo commandInfo)
    {
        if (EventStarted)
        {
            eventStartTime = null;
            playersData.Clear();
            EventStarted = false;
            Server.PrintToChatAll($"[{ChatColors.DarkBlue}EC{ChatColors.White}] Event Stopped.");
            Logger.LogInformation("DateTime Collection Stopped");
            PeakPlayers = 0;
        }
        else
        {
            Server.PrintToChatAll($"[{ChatColors.DarkBlue}EC{ChatColors.White}] No event is currently running.");
        }
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player != null && !player.IsBot)
        {
            var steamIdString = player.SteamID.ToString();
            if (eventStartTime != null)
            {
                if (!playersData.ContainsKey(steamIdString))
                {
                    playersData[steamIdString] = new PlayerData
                    {
                        SteamId = steamIdString,
                        PlayerName = player.PlayerName
                    };
                }
                playersData[steamIdString].JoinTime = DateTime.UtcNow;
            }
            if (PeakPlayers < Utilities.GetPlayers().Count)
            {
                PeakPlayers = Utilities.GetPlayers().Count;
            }
        }
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player != null)
        {
            var steamIdString = player.SteamID.ToString();
            if (eventStartTime != null && playersData.ContainsKey(steamIdString))
            {
                var playerData = playersData[steamIdString];
                playerData.TotalTimeSpent += DateTime.UtcNow - playerData.JoinTime;
                playerData.JoinTime = DateTime.MinValue;
            }
        }
        return HookResult.Continue;
    }

    private string FormatTimeSpan(TimeSpan timeSpan)
    {
        if (timeSpan.TotalHours >= 1)
        {
            return $"{timeSpan.TotalHours:F2} hours";
        }
        if (timeSpan.TotalMinutes >= 1)
        {
            return $"{timeSpan.TotalMinutes:F2} minutes";
        }
        return $"{timeSpan.TotalSeconds:F2} seconds";
    }

    private class PlayerData
    {
        public string SteamId { get; set; } = "XXXXXXXXXXXXXXXXX";
        public string PlayerName { get; set; } = "Unknown";
        public TimeSpan TotalTimeSpent { get; set; } = TimeSpan.Zero;
        public DateTime JoinTime { get; set; } = DateTime.MinValue;
    }
}