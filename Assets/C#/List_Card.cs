using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class List_Card : Card
{
    override public void OnClick(BaseEventData eventData)
    {
        var pointerEventData = eventData as PointerEventData;
        switch (pointerEventData.pointerId)
        {
            case -1:
                manager.AddSelectCardList(data);
                break;
            case -2:
                SideChange();
                break;
        }
    }
}
