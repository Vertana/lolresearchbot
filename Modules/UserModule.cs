using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;

namespace LolResearchBot.Modules
{
    // Modules must be public and inherit from an IModuleBase
    public class UserModule : ModuleBase<SocketCommandContext>
    {
        // private readonly SocketCommandContext _context;
        // public UserModule(SocketCommandContext context)
        // {
        //     _context = context;
        // }
        // Get info on a user, or the user who invoked the command if one is not specified
        [Command("userinfo")]
        public async Task UserInfoAsync(IUser user = null)
        {
            user = user ?? Context.User;
            var game = string.IsNullOrEmpty(user.Game.ToString()) ? "nothing at the moment" : user.Game.ToString();
            var status = string.IsNullOrEmpty(user.Status.ToString()) ? "no status set" : user.Status.ToString();
            var bot = user.IsBot ? "is a bot" : "is not a bot";
            await ReplyAsync(
                $@"{user} is playing {game}.{Environment.NewLine}{user} {bot}.{Environment.NewLine}Status: {status}");
        }

        // Ban a user
        [Command("ban")]
        [RequireContext(ContextType.Guild)]
        // make sure the user invoking the command can ban
        [RequireUserPermission(GuildPermission.BanMembers)]
        // make sure the bot itself can ban
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanUserAsync(IGuildUser user, [Remainder] string reason = null)
        {
            try
            {
                await user.Guild.AddBanAsync(user, reason: reason);
                await ReplyAsync($"Banned {user.Username}!");
            }
            catch (Exception ex)
            {
                if (ex is CommandException || ex is HttpException)
                {
                    var embed = new EmbedBuilder();
                    embed.WithImageUrl("https://media.giphy.com/media/5EDrPKdP1veIE/giphy.gif");
                    embed.Build();
                    await Context.Channel.SendMessageAsync($"Error banning {user.Username}.", false, embed);
                }
            }
        }

        [Command("kick")]
        [RequireContext(ContextType.Guild)]
        // make sure the user invoking the command can ban
        [RequireUserPermission(GuildPermission.KickMembers)]
        // make sure the bot itself can ban
        [RequireBotPermission(GuildPermission.KickMembers)]
        public async Task KickUserAsync(IGuildUser user, [Remainder] string reason = null)
        {
            try
            {
                await user.KickAsync(reason);
                await ReplyAsync($"Kicked {user.Username}!");
            }
            catch (Exception ex)
            {
                if (ex is CommandException || ex is HttpException)
                {
                    var embed = new EmbedBuilder();
                    embed.WithImageUrl(
                        "https://media1.tenor.com/images/c374c51f3cda56583cf23bac6f9a0230/tenor.gif?itemid=7859548");
                    embed.Build();

                    await Context.Channel.SendMessageAsync($"Error kicking {user.Username}.", false, embed);
                }

                throw;
            }
        }

        [Command("poke")]
        public async Task PokeUserAsync(IUser user = null)
        {
            var argpos = 0;
            var counter = 0;
            foreach (IUser mentioned in Context.Message.MentionedUsers)
                if (!mentioned.IsBot && mentioned.Id != Context.Client.CurrentUser.Id)
                {
                    if (Context.Guild.Name.ToLower().Contains("server")
                    ) // This if/else simply checks if server name contains the word server. It got weird seeing "user poked you on server A server."
                        await mentioned.SendMessageAsync(
                            $"{Context.Message.Author.Username} has poked you from the \"{Context.Message.Channel}\" channel on the \"{Context.Guild.Name}\".");
                    else
                        await mentioned.SendMessageAsync(
                            $"{Context.Message.Author.Username} has poked you from the \"{Context.Message.Channel}\" channel on the \"{Context.Guild.Name}\" server.");
                    await ReplyAsync($"{mentioned.Username} has been poked!");
                }
                else if (mentioned.IsBot && Context.Client.CurrentUser.Id != mentioned.Id
                ) // We don't poke bots or ourself.
                {
                    await ReplyAsync("We can't poke bots, sorry.");
                }
                else if (mentioned.IsBot && Context.Client.CurrentUser.Id == mentioned.Id &&
                         Context.Message.HasMentionPrefix(Context.Client.CurrentUser, ref argpos))
                {
                    counter++;
                    if (counter >= 2)
                        await ReplyAsync(
                            "We can't poke ourselves!."); // This is another check to make sure our bot wasn't targetted twice.
                }
        }
    }
}