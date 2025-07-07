using UnityEngine;

namespace GSDev.UI
{
    public class ConversionTextContent : ConversionBase
    {
        // [SerializeField] private Text _text;
        // public string NewContent = "Off";
        // public string OldContent = "On";
        [SerializeField] private GameObject _objTextOn;
        [SerializeField] private GameObject _objTextOff;

        // protected override void OnAwake()
        // {
        //     if (!_text) _text = GetComponent<Text>();
        // }

        protected override void OnSwitch(bool conversion)
        {
            // if (_text != null)
            //     _text.text = conversion ? OldContent : NewContent;
            _objTextOn.SetActive(conversion);
            _objTextOff.SetActive(!conversion);
        }
    }
}
