using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShoeStore_AO
{
    public static class CurrentUser
    {
        public static int UserID { get; set; } = 0;
        public static string FullName { get; set; } = "Гость";
        public static string RoleName { get; set; } = "Гость";

        
        public static bool IsGuest => RoleName == "Гость";
        public static bool IsClient => RoleName == "Клиент";
        public static bool IsManager => RoleName == "Менеджер";
        public static bool IsAdmin => RoleName == "Администратор";

      
        public static bool CanSearch => IsManager || IsAdmin;
        public static bool CanSort => IsManager || IsAdmin;
        public static bool CanFilter => IsManager || IsAdmin;
    }
}