using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordBot
{
    public class Sheet
    {
        public int Id { get; private set; }
        public string Name { get; private set; }
        public Sheet(int id, string name)
        {
            this.Id = id;
            this.Name = name;
        }
    }
}
