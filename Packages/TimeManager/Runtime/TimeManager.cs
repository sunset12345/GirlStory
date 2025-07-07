using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GSDev.Time
{
    public static class TimeManager
    {
        #region Time Behaviour

        private class TimeBehaviour : MonoBehaviour
        {
            private Stack<LinkedListNode<ActionEvent>> _pool;
            private LinkedList<DelayCallBack> _cache;
            private DelayCallBack _delayCallBack;

            private void Awake()
            {
                _pool = new Stack<LinkedListNode<ActionEvent>>();
                _cache = new LinkedList<DelayCallBack>();
                _delayCallBack = NewDelayCallbackInternal();
            }

            private void Update()
            {
                var crawler = _cache.First;
                while (crawler != null)
                {
                    crawler.Value.Update();
                    crawler = crawler.Next;
                }
            }

            internal DelayCallBack NewDelayCallbackInternal()
            {
                var delayCallBack = new DelayCallBack(_pool);
                _cache.AddLast(delayCallBack);
                return delayCallBack;
            }

            internal int SetupInvokeInternal(float time, Action callback)
            {
                return _delayCallBack.Add(time, callback);
            }

            internal void CancelInvokeInternal(int id)
            {
                _delayCallBack.Remove(id);
            }
        }
        private static readonly TimeBehaviour _timeBehaviour;
        static TimeManager()
        {
            var go = new GameObject("TimeManager");
            Object.DontDestroyOnLoad(go);
            _timeBehaviour = go.AddComponent<TimeBehaviour>();
        }

        #endregion

        #region Delay call back

        public static DelayCallBack NewDelayCallback()
        {
            return _timeBehaviour.NewDelayCallbackInternal();
        }

        public static int SetupInvoke(float time, Action callback)
        {
            return _timeBehaviour.SetupInvokeInternal(time, callback);
        }

        public static void CancelInvoke(int id)
        {
            _timeBehaviour.CancelInvokeInternal(id);
        }

        #endregion

        #region Server Time

        public static bool IsServerTimeSynced => _isServerTimeSynced;
        private static bool _isServerTimeSynced = false;
        private static long _serverTimeStartUTC;
        private static double _serverTimeStartTimestamp;
        public static long ServerTimeUTC => Convert.ToInt64(UnityEngine.Time.realtimeSinceStartupAsDouble - _serverTimeStartTimestamp) + _serverTimeStartUTC;

        public static void SetServerTime(long serverTime)
        {
            _serverTimeStartUTC = serverTime;
            _serverTimeStartTimestamp = UnityEngine.Time.realtimeSinceStartupAsDouble;
            _isServerTimeSynced = true;
        }

        public static void SetServerTime(string serverTime, string timeType = TimeUtil.TimeFormatFull)
        {
            var timeUtc = DateTime.UtcNow.ParseBy(serverTime, timeType, false);
            SetServerTime(timeUtc.TotalSeconds());
        }

        #endregion

    }
}