using Magnesium.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Remora.Discord.API.Abstractions.Gateway.Events;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Gateway.Responders;
using Remora.Rest.Core;
using Remora.Results;

namespace Magnesium.Services;

public record ReadState(Snowflake GuildID, Snowflake ChannelID, Snowflake UserID, Snowflake LastMessage);

public class ReadStateService : IResponder<IMessageCreate>
{
    private readonly IMemoryCache _cache;
    private readonly MagnesiumContext _context;
    private readonly IDiscordRestUserAPI _users;
    private readonly IDiscordRestChannelAPI _messages;

    public ReadStateService
    (
        IMemoryCache cache,
        MagnesiumContext context,
        IDiscordRestUserAPI users,
        IDiscordRestChannelAPI messages
    )
    {
        this._cache = cache;
        this._context = context;
        this._users = users;
        this._messages = messages;
    }

    public async Task<Result> RespondAsync(IMessageCreate gatewayEvent, CancellationToken ct = default)
    {
        if (gatewayEvent.Author.IsBot.OrDefault(false) || !gatewayEvent.GuildID.IsDefined(out var guildID))
        {
            return Result.FromSuccess();
        }

        var trackers = (await _context.Channels.Where(t => t.ChannelID == gatewayEvent.ChannelID).ToArrayAsync()).ToDictionary(k => k.TrackingUserID ?? k.GuildID, k => k);

        _cache.GetOrCreate<ReadState>
        (
            $"{guildID}:{gatewayEvent.ChannelID}:{gatewayEvent.Author.ID}", 
            (_) => new ReadState(guildID, gatewayEvent.ChannelID, gatewayEvent.Author.ID, gatewayEvent.ID)
        );

        var skipUserCheck = trackers.ContainsKey(guildID.Value); // DM everyone regardless.

        foreach (var mentioned in gatewayEvent.Mentions)
        {
            if (!skipUserCheck)
            {
                var shouldDMUser = trackers.ContainsKey(mentioned.ID.Value);

                if (!shouldDMUser)
                {
                    continue;
                }
            }

            var channelResult = await _users.CreateDMAsync(mentioned.ID);

            if (!channelResult.IsDefined(out var dmChannel))
            {
                continue;
            }

            var exists = _cache.TryGetValue<ReadState>($"{guildID}:{gatewayEvent.ChannelID}:{mentioned.ID}", out var state);

            await _messages.CreateMessageAsync
            (
                dmChannel.ID,
                $"You've got mail! ( https://discord.com/channels/{guildID}/{gatewayEvent.ChannelID}/{gatewayEvent.ID} )\n" +
                (exists ? "" : $"Your last message is here, for context: https://discord.com/channels/{guildID}/{gatewayEvent.ChannelID}/{state!.LastMessage}")
            );
        }

        return Result.FromSuccess();
    }
}
