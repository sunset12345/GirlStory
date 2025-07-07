using System;

namespace App.LocalData
{
    public enum LocalDataEnum
    {
        None = 0,
        BaseSetting = 1,
        PlayerData = 2
    }

    [Serializable]
    public class PlayerData
    {
        public string Email;
        public string Password;
    }

    [Serializable] 
    public class BaseSetting
    {
        public bool IsMusicOn = true;
        public bool IsSoundOn = true;
    }   
}