using System;
using System.Diagnostics;

namespace App.LoadingFunction
{
    public class LoadStepWatch
    {
        private Stopwatch _watch;
        private Action<string, long, long> _stepCallback;

        public long ElapsedMilliseconds => _watch.ElapsedMilliseconds;

        public LoadStepWatch(Action<string, long, long> stepCallback)
        {
            _watch = Stopwatch.StartNew();
            _stepCallback = stepCallback;
        }

        public void Stop()
        {
            _watch.Stop();
        }

        public WatchStep NewStep(string name)
        {
            return new WatchStep(name, this);
        }

        public struct WatchStep : IDisposable
        {
            private LoadStepWatch _watch;

            private string _stepName;
            private long _startTime;

            public WatchStep(string name, LoadStepWatch watch)
            {
                _watch = watch;
                _stepName = name;
                _startTime = _watch.ElapsedMilliseconds;
            }

            public void Dispose()
            {
                var currentTime = _watch.ElapsedMilliseconds;
                _watch._stepCallback?.Invoke(
                    _stepName,
                    currentTime,
                    currentTime - _startTime);
            }
        }
    }
}
