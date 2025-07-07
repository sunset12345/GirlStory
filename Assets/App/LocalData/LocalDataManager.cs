using GSDev.EventSystem;
using GSDev.Singleton;
using UnityEngine;

namespace App.LocalData
{
    /// <summary>
    /// 数据缓存管理器
    /// </summary>
    /// <remarks>
    /// 该类用于管理应用程序中的数据缓存。它允许设置、获取和清除缓存数据。
    /// </remarks>
    public class LocalDataManager : Singleton<LocalDataManager>, IEventSender
    {
        public EventDispatcher Dispatcher => EventDispatcher.Global;

        public void SetData(string key, object value)
        {
            LocalData.Save(key, value);
            // 触发数据更新事件
            this.DispatchEvent(Witness<LocalDataUpdateEvent>._, key);
        }

        public void ClearData()
        {
            PlayerPrefs.DeleteAll();
            // 触发数据清除事件
            this.DispatchEvent(Witness<LocalDataClearAllEvent>._);
        }

        #region baseSetting

        public void SetBaseSetting(bool musicOn, bool soundOn)
        {
            var baseSetting = LocalData.Load<BaseSetting>(LocalDataEnum.BaseSetting.ToString(), new BaseSetting());

            baseSetting.IsMusicOn = musicOn;
            baseSetting.IsSoundOn = soundOn;
            SetData(LocalDataEnum.BaseSetting.ToString(), baseSetting);
        }

        public BaseSetting GetBaseSetting() => LocalData.Load<BaseSetting>(LocalDataEnum.BaseSetting.ToString(), new BaseSetting());

        #endregion

        #region playerData
        
        public void SetPlayerData(string email, string password)
        {
            var playerData = LocalData.Load<PlayerData>(LocalDataEnum.PlayerData.ToString(), new PlayerData());

            playerData.Email = email;
            playerData.Password = password;
            SetData(LocalDataEnum.PlayerData.ToString(), playerData);
        }

        public PlayerData GetPlayerData() => LocalData.Load<PlayerData>(LocalDataEnum.PlayerData.ToString(), new PlayerData());

        #endregion
    }

    public class LocalDataClearAllEvent : EventBase { }
    public class LocalDataUpdateEvent : EventBase<string>
    {
        public string Key => Field1;
    }

}