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
    public override string ModuleVersion => "1.0.0";
    public override string ModuleAuthor => "ShookEagle";

    private Dictionary<string, TimeSpan> playerTimes = new Dictionary<string, TimeSpan>();
    private Dictionary<string, DateTime> playerJoinTimes = new Dictionary<string, DateTime>();
    private DateTime? eventStartTime = null;
    private HashSet<string> allPlayerIds = new HashSet<string>();
    private bool EventStarted;

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
        if (!EventStarted) { 
            List<CCSPlayerController> allPlayersBots = Utilities.GetPlayers();
            var allPlayers = allPlayersBots.Where(x => !x.IsBot);
            eventStartTime = DateTime.UtcNow;
            playerTimes.Clear();
            playerJoinTimes.Clear();
            allPlayerIds.Clear();
            EventStarted = true;
            foreach (var player in allPlayers)
            {
                var steamIdString = player.SteamID.ToString();
                playerJoinTimes[steamIdString] = DateTime.UtcNow;
                allPlayerIds.Add(steamIdString);
            }
            Server.PrintToChatAll($"[{ChatColors.DarkBlue}EC{ChatColors.White}] Event started.");
            Logger.LogInformation("DateTime Collection Started");
        } else
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
            caller.PrintToConsole($"=== Begin Event Logs (Duration: {FormatTimeSpan(eventDuration)}) ===\n");

            int rank = 1;
            foreach (var steamIdString in allPlayerIds)
            {
                var player = Utilities.GetPlayerFromSteamId(ulong.Parse(steamIdString));
                var playerName = player != null ? player.PlayerName : "Unknown";

                var timeSpent = playerTimes.ContainsKey(steamIdString)
                    ? playerTimes[steamIdString]
                    : TimeSpan.Zero;

                if (playerJoinTimes.ContainsKey(steamIdString) && playerJoinTimes[steamIdString] != DateTime.MinValue)
                {
                    timeSpent += DateTime.UtcNow - playerJoinTimes[steamIdString];
                }

                caller.PrintToConsole($"#{rank}. {steamIdString} {playerName} {FormatTimeSpan(timeSpent)}\n");
                rank++;
            }

            caller.PrintToConsole("=== End Event Logs ===");
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
            playerTimes.Clear();
            playerJoinTimes.Clear();
            allPlayerIds.Clear();
            EventStarted = false;
            Server.PrintToChatAll($"[{ChatColors.DarkBlue}EC{ChatColors.White}] Event Stopped.");
            Logger.LogInformation("DateTime Collection Stopped");
        } else
        {
            Server.PrintToChatAll($"[{ChatColors.DarkBlue}EC{ChatColors.White}] No event is currently running.");
        }
    }

    [GameEventHandler]
    public HookResult OnPlayerConnectFull(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player != null && !player.IsBot)
        {
            var steamIdString = player.SteamID.ToString();
            if (eventStartTime != null && player != null)
            {
                playerJoinTimes[player.SteamID.ToString()] = DateTime.UtcNow;
                allPlayerIds.Add(steamIdString);
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
            if (eventStartTime != null && playerJoinTimes.ContainsKey(player.SteamID.ToString()))
            {
                var steamIdString = player.SteamID.ToString();
                if (!playerTimes.ContainsKey(steamIdString))
                {
                    playerTimes[steamIdString] = TimeSpan.Zero;
                }

                playerTimes[steamIdString] += DateTime.UtcNow - playerJoinTimes[steamIdString];
                playerJoinTimes[steamIdString] = DateTime.MinValue;
                return HookResult.Continue;
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
}
