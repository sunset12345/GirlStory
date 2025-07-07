using System.Globalization;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Debugger = DG.Tweening.Core.Debugger;

// ReSharper disable once CheckNamespace
namespace DG.Tweening
{
    public static class DOTweenModuleExtend
    {

        #region DOCounter
        
        #region Text

        public static TweenerCore<int, int, NoOptions> DOCounter(this Text target, int start, int end, float duration)
        {
            int? _value = null;
            return DOTween.To(() => start, v =>
                {
                    if (_value == v) return;
                    _value = v;
                    target.text = v.ToString();
                }, end, duration)
                .SetEase(Ease.Linear)
                .SetTarget(target);
        }
        public static TweenerCore<int, int, NoOptions> DOCounter(this Text target, int start, int end, float duration, System.Func<int, string> callback)
        {
            int? _value = null;
            return DOTween.To(() => start, v =>
                {
                    if (_value == v) return;
                    _value = v;
                    target.text = callback(v);
                }, end, duration)
                .SetEase(Ease.Linear)
                .SetTarget(target);
        }
        public static Tweener DOCounter(this Text target, float start, float end, float duration)
        {
            return DOTween.To(setter: v => { target.text = v.ToString(CultureInfo.InvariantCulture); },
                    start, end, duration)
                .SetEase(Ease.Linear)
                .SetTarget(target);
        }
        public static Tweener DOCounter(this Text target, float start, float end, float duration,
            System.Func<float, string> callback)
        {
            return DOTween.To(v => { target.text = callback(v); }, start, end, duration)
                .SetEase(Ease.Linear)
                .SetTarget(target);
        }

        #endregion

        #region TMP_Text
        
        public static TweenerCore<int, int, NoOptions> DOCounter(this TMP_Text target, int start, int end, float duration)
        {
            int? _value = null;
            return DOTween.To(() => start, v =>
                {
                    if (_value == v) return;
                    _value = v;
                    target.text = v.ToString();
                }, end, duration)
                .SetEase(Ease.Linear)
                .SetTarget(target);
        }
        public static TweenerCore<int, int, NoOptions> DOCounter(this TMP_Text target, int start, int end, float duration, System.Func<int, string> callback)
        {
            int? _value = null;
            return DOTween.To(() => start, v =>
                {
                    if (_value == v) return;
                    _value = v;
                    target.text = callback(v);
                }, end, duration)
                .SetEase(Ease.Linear)
                .SetTarget(target);
        }
        public static Tweener DOCounter(this TMP_Text target, float start, float end, float duration)
        {
            return DOTween.To(setter: v => { target.text = v.ToString(CultureInfo.InvariantCulture); },
                    start, end, duration)
                .SetEase(Ease.Linear)
                .SetTarget(target);
        }
        public static Tweener DOCounter(this TMP_Text target, float start, float end, float duration,
            System.Func<float, string> callback)
        {
            return DOTween.To(v => { target.text = callback(v); }, start, end, duration)
                .SetEase(Ease.Linear)
                .SetTarget(target);
        }

        #endregion

        public static TweenerCore<int, int, NoOptions> DOCounter(this Component target, int start, int end, float duration, System.Action<int> callback = null)
        {
            int? _value = null;
            return DOTween.To(() => start, v =>
            {
                if (_value == v) return;
                _value = v;
                callback?.Invoke(v);
            }, end, duration).SetEase(Ease.Linear).SetTarget(target);
        }
        public static Tweener DOCounter(this Component target, float start, float end, float duration, System.Action<float> callback = null)
        {
            return DOTween.To(v => { callback?.Invoke(v); }, start, end, duration).SetEase(Ease.Linear).SetTarget(target);
        }
        
        public static TweenerCore<int, int, NoOptions> DOCounter(int start, int end, float duration, System.Action<int> callback = null)
        {
            int? _value = null;
            return DOTween.To(() => start, v =>
            {
                if (_value == v) return;
                _value = v;
                callback?.Invoke(v);
            }, end, duration).SetEase(Ease.Linear);
        }
        public static Tweener DOCounter(float start, float end, float duration, System.Action<float> callback = null)
        {
            return DOTween.To(v => { callback?.Invoke(v); }, start, end, duration).SetEase(Ease.Linear);
        }
        #endregion

        #region DelayCall
        private static int __delay_call_get() => 0;
        private static void __delay_call_set(int v) { }
        public static TweenerCore<int, int, NoOptions> DelayCall(this Component target, float delay, TweenCallback callback)
        {
            return DOTween.To(__delay_call_get, __delay_call_set, 0, 0).SetDelay(delay).SetTarget(target).OnComplete(callback);
        }
        public static TweenerCore<int, int, NoOptions> Delay(this Component target, float delay)
        {
            return DOTween.To(__delay_call_get, __delay_call_set, 0, 0).SetDelay(delay).SetTarget(target);
        }
        public static TweenerCore<int, int, NoOptions> DelayCall(float delay, TweenCallback callback)
        {
            return DOTween.To(__delay_call_get, __delay_call_set, 0, 0).SetDelay(delay).OnComplete(callback);
        }
        public static TweenerCore<int, int, NoOptions> Delay(float delay)
        {
            return DOTween.To(__delay_call_get, __delay_call_set, 0, 0).SetDelay(delay);
        }
        #endregion
        
        public static TweenerCore<Color, Color, ColorOptions> DOFade(this Graphic target, float startValue, float endValue, float duration)
        {
            var color = target.color;
            color.a = startValue;
            target.color = color;
            return DOTween.ToAlpha(() => target.color, x => target.color = x, endValue, duration)
                .SetTarget(target);
        }
        
        public static TweenerCore<Color, Color, ColorOptions> DOColor(this Component target, Color startValue, Color endValue, float duration, System.Action<Color> callback = null)
        {
            var t = DOTween.To(() => startValue, x => callback?.Invoke(x), endValue, duration);
            t.SetTarget(target);
            return t;
        }
        
        public static TweenerCore<string, string, StringOptions> DOText(this TMP_Text target, string endValue, float duration, bool richTextEnabled = true, ScrambleMode scrambleMode = ScrambleMode.None, string scrambleChars = null)
        {
            if (endValue == null) {
                if (Debugger.logPriority > 0) Debugger.LogWarning("You can't pass a NULL string to DOText: an empty string will be used instead to avoid errors");
                endValue = "";
            }
            TweenerCore<string, string, StringOptions> t = DOTween.To(() => target.text, x => target.text = x, endValue, duration);
            t.SetOptions(richTextEnabled, scrambleMode, scrambleChars)
                .SetEase(Ease.Linear)
                .SetTarget(target);
            return t;
        }
    }
}