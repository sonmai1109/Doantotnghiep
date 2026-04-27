using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Maison.Session
{
    public class ConstainBot
    {
        public static string BOT_HISTORY = "BOT_HISTORY";
    }

    // Tạo luôn một class nhỏ để định dạng cấu trúc tin nhắn
    public class ChatMsg
    {
        public string Type { get; set; } // "user" hoặc "bot"
        public string Content { get; set; }
    }
}
