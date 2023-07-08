using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;

namespace @RepositoryOwner.@RepositoryName.Commands
{
    public sealed class PingCommand : BaseCommand
    {
        [Command("ping"), Description("Checks the current latency of the bot.")]
        public static async Task ExecuteAsync(CommandContext context) => await context.ReplyAsync($"Pong! {context.Client.Ping}ms");
    }
}
