using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class UISoundEmitter : MonoBehaviour,IPointerClickHandler, IPointerEnterHandler
{
    [SerializeReference] string _clickSfxPath = "SFX/UI/Click";
    [SerializeReference] string _hoverSfxPath = "SFX/UI/Hover";
    [SerializeReference] bool _enableHove = false;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(_clickSfxPath) == false)
            Managers.Sound.Play(_clickSfxPath, Define.Sound.Effect);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_enableHove == false) return;

        if(string.IsNullOrEmpty(_hoverSfxPath) == false)
            Managers.Sound.Play(_hoverSfxPath, Define.Sound.Effect);
    }
}
