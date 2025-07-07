using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace App.LoadingFunction
{
    public class LoadingManager : GSDev.Singleton.MonoSingleton<LoadingManager>
    {
        public ReactiveProperty<float> Progress { get; } = new(0f);
        public ReactiveProperty<string> Info { get; } = new(string.Empty);

        public class LoadingProcessor : IEnumerator<float>
        {
            private readonly List<IEnumerator<float>> _loaders = new();
            private int _index;
            private float _currentProgress;
            public Action OnFinish { get; set; }

            public LoadingProcessor(IEnumerable<IEnumerator<float>> loaders)
            {
                _loaders.AddRange(loaders);
            }

            public LoadingProcessor(IEnumerator<float> loader)
            {
                _loaders.Add(loader);
            }

            public bool MoveNext()
            {
                if (_loaders.Count == 0 || _index >= _loaders.Count)
                    return false;
                var loader = _loaders[_index];
                var currentLoaderProgress = 0f;
                if (!loader.MoveNext())
                {
                    ++_index;
                }
                else
                {
                    currentLoaderProgress = loader.Current;
                }

                var percent = 1.0f / _loaders.Count;
                _currentProgress = (_index + currentLoaderProgress) * percent;

                if (_index >= _loaders.Count)
                    OnFinish?.Invoke();

                return _index < _loaders.Count;
            }

            public void Reset()
            {
                _currentProgress = 0f;
                _index = 0;
            }

            public float Current => _currentProgress;

            object IEnumerator.Current => Current;

            public void Dispose()
            {
                _loaders.Clear();
            }
        }

        private readonly Stack<LoadingProcessor> _processors = new Stack<LoadingProcessor>();

        public LoadingProcessor CreateLoadingProcess(IEnumerable<IEnumerator<float>> loaders)
        {
            var processor = new LoadingProcessor(loaders);
            _processors.Push(processor);
            return processor;
        }

        public LoadingProcessor CreateLoadingProcess(IEnumerator<float> loader)
        {
            var processor = new LoadingProcessor(loader);
            _processors.Push(processor);
            return processor;
        }

        public void SetInfo(string info)
        {
            Info.Value = info;
        }

        private void Update()
        {
            if (_processors.Count == 0)
                return;

            var processor = _processors.Peek();
            if (!processor.MoveNext())
            {
                _processors.Pop();
            }

            var currentProgress = processor.Current;
            Progress.Value = currentProgress;
        }
    }
}
