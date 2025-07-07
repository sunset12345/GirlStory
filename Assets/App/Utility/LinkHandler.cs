using GSDev.UI.Layer;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LinkHandler : MonoBehaviour , IPointerClickHandler
{

    private TextMeshProUGUI _label;

    private Camera _uiCamera;
    
    private void Start()
    {
        //"By start, you agree to our <u><color=blue><link=https://sites.google.com/view/nabi-user/>Terms of Use</link></color></u> And <u><color=blue><link=https://sites.google.com/view/nabiprivacypolicy/>Privacy Policy</link></color></u>";
        _uiCamera = LayerManager.Instance.Find("UICamera").GetComponent<Camera>();
        _label = GetComponent<TextMeshProUGUI>();
        _label.richText = true;
        _label.raycastTarget = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_label, Input.mousePosition, _uiCamera);
        if (linkIndex != -1)
        {
            TMP_LinkInfo linkInfo = _label.textInfo.linkInfo[linkIndex];
            string url = linkInfo.GetLinkID();
            Application.OpenURL(url);
        }
    }
}
