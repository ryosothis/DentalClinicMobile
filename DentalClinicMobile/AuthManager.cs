using System;

namespace DentalClinicMobile
{
    public static class AuthManager
    {
        public static User CurrentUser { get; set; }
        public static string CurrentUserEmail { get; set; }

        public static bool IsAuthenticated => CurrentUser != null;

        public static int? GetCurrentUserId()
        {
            return CurrentUser?.Id;
        }

        public static bool IsAdmin()
        {
            return CurrentUser?.RoleId == 1;
        }

        public static bool IsDoctor()
        {
            return CurrentUser?.RoleId == 3;
        }

        public static bool IsPatient()
        {
            return CurrentUser?.RoleId == 2;
        }

        public static string GetRoleName()
        {
            if (CurrentUser == null) return "Гость";
            return CurrentUser.RoleId switch
            {
                1 => "Администратор",
                2 => "Пользователь",
                3 => "Врач",
                _ => "Пользователь"
            };
        }

        public static void Logout()
        {
            CurrentUser = null;
            CurrentUserEmail = null;
        }
    }
}