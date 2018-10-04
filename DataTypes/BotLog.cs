using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tapi.Bot.SophiBot.DataTypes
{
    public class BotLog
    {
        
        public string ID { set; get; }
        public string UserID { set; get; }
        public string UserName { set; get; }
        public string Channel { set; get; }
        public DateTime created { set; get; }
        public string Message { set; get; }

        public string GetWriteSql()
        {
            return  $"INSERT UserLog (ID, UserID,UserName,Channel,created,Message) VALUES ({ID},{UserID},{UserName},{Channel},{created},{Trunc(Message,4000)}";
        }

        static string Trunc(string message, int len)
        {
            if (string.IsNullOrEmpty(message)) return message;
            return message.Length <= len ? message : message.Substring(0, len);
        }
    }
}