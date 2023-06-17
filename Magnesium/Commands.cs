using System.ComponentModel;
using Magnesium.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Remora.Commands.Attributes;
using Remora.Commands.Groups;
using Remora.Discord.API.Abstractions.Objects;
using Remora.Discord.API.Abstractions.Rest;
using Remora.Discord.Commands.Attributes;
using Remora.Discord.Commands.Conditions;
using Remora.Discord.Commands.Contexts;
using Remora.Results;

namespace Magnesium;

[Ephemeral]
[RequireContext(ChannelContext.Guild)]
public class Commands : CommandGroup 
{
    private readonly IMemoryCache _cache;
    private readonly MagnesiumContext _db;
    private readonly IInteractionContext _context;
    private readonly IDiscordRestInteractionAPI _interactions;

    public Commands
    (
        IMemoryCache cache,
        MagnesiumContext db,
        IInteractionContext context,
        IDiscordRestInteractionAPI interactions
    )
    {
        this._cache = cache;
        this._db = db;
        this._context = context;
        this._interactions = interactions;
    }

    [Command("add-global-channel")]
    [Description("Track this channel's mentions globally.")]
    [DiscordDefaultMemberPermissions(DiscordPermission.ManageChannels)]
    public async Task<Result> AddTrackedChannelAsync()
    {
        var exists = await _db.Channels.AnyAsync(c => c.GuildID == _context.Interaction.GuildID.Value);
        
        if (exists)
        {
            return (Result) await _interactions.EditOriginalInteractionResponseAsync
            (
                _context.Interaction.ApplicationID,
                _context.Interaction.Token,
                "<:icons_warning:908958943466893323> This channel is already being tracked globally!"
            );
        }

        _db.Channels.Add
        (
            new TrackedChannel 
            {
                Global = true,
                GuildID = _context.Interaction.GuildID.Value.Value,
                ChannelID = _context.Interaction.Channel.Value.ID.Value.Value,
                TrackingUserID = null,
            }
        );

        await _db.SaveChangesAsync();
        
        return (Result) await _interactions.EditOriginalInteractionResponseAsync
        (
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            "<:icons_Correct:859388130411282442> I'll track this channel for mentions!"
        );
    }

    [Command("remove-global-channel")]
    [Description("Stops tracking for the channel globally.")]
    [DiscordDefaultMemberPermissions(DiscordPermission.ManageChannels)]
    public async Task<Result> RemoveGlobalChannelAsync()
    {
        var tracker = await _db.Channels.FirstOrDefaultAsync(c => c.GuildID == _context.Interaction.GuildID.Value);
        
        if (tracker is null)
        {
            return (Result) await _interactions.EditOriginalInteractionResponseAsync
            (
                _context.Interaction.ApplicationID,
                _context.Interaction.Token,
                "<:icons_warning:908958943466893323> This channel isn't being tracked!"
            );
        }

        _db.Channels.Remove(tracker);

        await _db.SaveChangesAsync();
        
        return (Result) await _interactions.EditOriginalInteractionResponseAsync
        (
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            "<:icons_Correct:859388130411282442> I'll stop tracking this channel!"
        );
    }

    [Command("add-global-personal")]
    [Description("Track this channel's mentions personally.")]
    [RequireDiscordPermission(DiscordPermission.ManageChannels)]
    public async Task<Result> AddPersonalTrackedChannelAsync()
    {
        var globallyTracked = await _db.Channels.AnyAsync(c => c.GuildID == _context.Interaction.GuildID.Value && c.Global && c.ChannelID == _context.Interaction.Channel.Value.ID.Value);
        var personallyTracked = await _db.Channels.AnyAsync(c => c.GuildID == _context.Interaction.GuildID.Value && c.ChannelID == _context.Interaction.Channel.Value.ID.Value && c.TrackingUserID == _context.Interaction.Member.Value.User.Value.ID.Value);
        
        if (globallyTracked)
        {
            return (Result) await _interactions.EditOriginalInteractionResponseAsync
            (
                _context.Interaction.ApplicationID,
                _context.Interaction.Token,
                "<:icons_warning:908958943466893323> This channel is being tracked globally; no need to track it personally."
            );
        }

        if (personallyTracked)
        {
            return (Result) await _interactions.EditOriginalInteractionResponseAsync
            (
                _context.Interaction.ApplicationID,
                _context.Interaction.Token,
                "<:icons_warning:908958943466893323> You've already set a tracker for this channel..."
            );
        }

        _db.Channels.Add
        (
            new TrackedChannel 
            {
                Global = false,
                GuildID = _context.Interaction.GuildID.Value.Value,
                ChannelID = _context.Interaction.Channel.Value.ID.Value.Value,
                TrackingUserID = _context.Interaction.Member.Value.User.Value.ID.Value,
            }
        );

        await _db.SaveChangesAsync();
        
        return (Result) await _interactions.EditOriginalInteractionResponseAsync
        (
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            "<:icons_Correct:859388130411282442> I'll track this channel for mentions!"
        );
    }

    [Command("remove-global-channel")]
    [Description("Stops tracking for the channel personally.")]
    public async Task<Result> RemovePersonalGlobalChannelAsync()
    {
        var tracker = await _db.Channels.FirstOrDefaultAsync(c => c.GuildID == _context.Interaction.GuildID.Value);
        
        if (tracker is null)
        {
            return (Result) await _interactions.EditOriginalInteractionResponseAsync
            (
                _context.Interaction.ApplicationID,
                _context.Interaction.Token,
                "<:icons_warning:908958943466893323> This channel isn't being tracked!"
            );
        }

        _db.Channels.Remove(tracker);

        await _db.SaveChangesAsync();
        
        return (Result) await _interactions.EditOriginalInteractionResponseAsync
        (
            _context.Interaction.ApplicationID,
            _context.Interaction.Token,
            "<:icons_Correct:859388130411282442> I'll stop tracking this channel!"
        );
    }
}