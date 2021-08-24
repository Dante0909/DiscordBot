﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Fuse.NET;

namespace DiscordBot
{
    class Program
    {
        static readonly string[] scopes = { SheetsService.Scope.Spreadsheets };
        static readonly string applicationName = "DiscordBot";
        static readonly string spreadsheetId = "1WgC53UV82mGiR1iE3hYGTRoHVmWfXsydAmO5LuvRDiM";
        static SheetsService service;

        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;
        private static List<Sheet> sheets = new List<Sheet>();
        
     
        static void Main(string[] args)
        {
            GoogleCredential credential;

            using (var stream = new FileStream("keys.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
            }
            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            GetSheets(service);


            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();

          
        }

        public async Task RunBotAsync()
        {
            Console.WriteLine("test");
            client = new DiscordSocketClient();
            commands = new CommandService();
            services = new ServiceCollection().AddSingleton(client).AddSingleton(commands).BuildServiceProvider();
            string token = "ODc4MTM3MjQ3NzA1MjEwODkw.YR8zCg.cYW81zllVw-fj7iiaEI8HHKsSLI";
            client.Log += Client_Log;

            await RegisterCommandsAsync();
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();
            
            GoogleCredential credential;

            using (var stream = new FileStream("keys.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream).CreateScoped(scopes);
            }
            service = new SheetsService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = applicationName,
            });

            //GetSheetNames(service);
            await Task.Delay(-1);
        }

        private Task Client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            client.MessageReceived += HandleCommandAsync;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly(),services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            var context = new SocketCommandContext(client, message);
            if (message.Author.IsBot) return;

            int argPos = 0;
            if(message.HasStringPrefix("%", ref argPos))
            {
                var result = await commands.ExecuteAsync(context, argPos, services);
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            }
        }

        
       
        static void ReadEntries()
        {
            var range = $"Nerofest!A1:F10";
            var request = service.Spreadsheets.Values.Get(spreadsheetId, range);
            var response = request.Execute();
            var values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach(var row in values)
                {
                    Console.WriteLine("{0} {1} | {2} | {3}", row[5], row[4], row[3], row[1]);
                }
            }
            else Console.WriteLine("No data found");
        }
        public static bool ContainsSheet(string eventName)
        {
            string[] names = sheets.Select(x => x.Name).ToArray();
            char[] upper = eventName.ToCharArray();
            upper[0] = Char.ToUpper(upper[0]);
            return names.Contains(eventName) || names.Contains(new string(upper));
        }
        public static void CreateEntry(string eventName, string dps, string link, string description)
        {
            if (link.StartsWith("<")) link = link.Remove(0,1);
            if (link.EndsWith(">")) link = link.Remove(link.Length - 1,1);
            var range = $"{eventName}!A1:B1";

            var valueRange = new ValueRange();
            var objectList = new List<object>() { dps, link, description };

            valueRange.Values = new List<IList<object>> { objectList };
            var appendRequest = service.Spreadsheets.Values.Append(valueRange, spreadsheetId, range);
            appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;
            appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var appendResponse = appendRequest.Execute();
        }

        static void UpdateEntry()
        {
            var range = $"Nerofest!D543";
            var valueRange = new ValueRange();

            var objectList = new List<object>() { "updated" };
            valueRange.Values = new List<IList<object>> { objectList };

            var updateRequest = service.Spreadsheets.Values.Update(valueRange, spreadsheetId, range);
            updateRequest.ValueInputOption = (SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum?)SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
            var updateResponse = updateRequest.Execute();
        }
        static void DeleteEntry()
        {
            var range = $"Nerofest!A543:F";
            var requestBody = new ClearValuesRequest();

            var deleteRequest = service.Spreadsheets.Values.Clear(requestBody, spreadsheetId, range);
            deleteRequest.Execute();
        }
        static List<Sheet> GetSheets(SheetsService api)
        {
            var getRequest = api.Spreadsheets.Get(spreadsheetId);
            var response = getRequest.Execute();

            List<string> sheetsName = response.Sheets.Select(x => x.Properties.Title).ToList();
            List<int> sheetsId = response.Sheets.Select(x => (int)x.Properties.SheetId).ToList();

            //var sheets = new List<Sheet>();
            for(int i = 0; i < sheetsId.Count; ++i)
            {
                sheets.Add(new Sheet(sheetsId[i], sheetsName[i]));
            }

            return sheets;
        }
        public static int GetSheetIdFromArgument(string name)
        {
            var fuseOptions = new FuseOptions
            {
                includeMatches = true,
                includeScore = true
            };

            var input = new List<FuseBs>();

            for(int i = 0; i < sheets.Count; ++i)
            {
                input.Add(new FuseBs { title = sheets[i].Name });
            }

            var fuse = new Fuse<FuseBs>(input, fuseOptions);
            fuse.AddKey("title");
            var output = fuse.Search(name);
            if (output.Count < 1 || output[0].score > 0.6f) return -1;
            

            return sheets.Where(x => x.Name == output[0].item.title).Select(x => x.Id).First();


        }
        public struct FuseBs
        {
            public string title;
        }
    }
}