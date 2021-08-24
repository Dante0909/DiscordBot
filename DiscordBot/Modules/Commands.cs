using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Modules
{
    public class Commands : BaseCommandModule
    {
        //[Command("ping")]
        //[Description("Returns pong")]
        //public async Task Ping(CommandContext ctx)
        //{
        //    await ctx.Channel.SendMessageAsync("Pong").ConfigureAwait(false);

        //}
        [Command("run")]
        [Description("Add a run to a sheet. Use quotations to input arguments with spaces. Ex: \"dual core\"")]
        public async Task Add(CommandContext ctx, 
            [Description("Name of the event")] string eventName, 
            [Description("Main damage dealer")] string dps, 
            [Description("Link of the run")] string link,
            [Description("Description")] string description = "")
            
        {
            if (Program.ContainsSheet(eventName))
            {
                Program.CreateEntry(eventName, dps, link, description);
                await ctx.Channel.SendMessageAsync($"Successfully added in { eventName }").ConfigureAwait(false);
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"Could not add in { eventName }, is the spelling right?").ConfigureAwait(false);
            }
            
            
            //int sheetId;
            //if(int.TryParse(eventName, out int result))
            //{
            //    sheetId = result;   
            //}
            //else sheetId = Program.GetSheetIdFromArgument(eventName);
            
            //if (sheetId == -1)
            //{
            //    await ctx.Channel.SendMessageAsync("Could not find matching event.");
            //}
            //else
            //{
            //    Program.CreateEntry(eventName, dps, link, description);
            //    await ctx.Channel.SendMessageAsync($"Successfully added in { eventName }").ConfigureAwait(false);
            //}

        }
    }
}
