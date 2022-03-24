using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Manager;

public struct CardData
{
    public List<Sprite> spriteList;
    public Sprite original, back;
    public int id;
    public CardType cardType;
}

public enum CardType
{
    Normal,Other,GR
}

public class Card : MonoBehaviour
{
    bool isDrag;
    bool isRotate;
    int rotateCount;
    int right_click_count;

    int id;
    Image image;
    List<Sprite> spriteList = new List<Sprite>();

    public CardData data;

    [HideInInspector] public Manager manager;
    [SerializeField] InputField inputField;
    Image inputField_img;

    private void Start()
    {
        inputField_img = inputField.GetComponent<Image>();
        InputText();
    }

    public void OnOff_InputField(bool b)
    {
        inputField.gameObject.SetActive(b);
    }

    public void SetCard(CardData data)
    {
        image = GetComponent<Image>();
        manager = GameObject.Find("Manager").GetComponent<Manager>();

        this.data = data;
        this.id = data.id;
        this.spriteList = data.spriteList;
    }

    public void OnCard(Side side = Side.back)
    {
        if (side == Side.front)
        {
            right_click_count = 0;
        }
        else if (side == Side.back)
        {
            right_click_count = 1;
        }
        image.sprite = spriteList[right_click_count];
    }

    public void OnDrag(BaseEventData baseeventData)
    {
        var eventData = baseeventData as PointerEventData;
        Vector2 targetPos = eventData.position;
        transform.position = targetPos;
    }

    virtual public void OnClick(BaseEventData eventData)
    {
        if (!isDrag)
        {            
            var pointerEventData = eventData as PointerEventData;
            switch (pointerEventData.pointerId)
            {
                case -1:
                    rotateCount++;
                    int angle = 0;
                    if (rotateCount % 4 == 0) angle = 0;
                    else if (rotateCount % 4 == 1) angle = -90;
                    else if (rotateCount % 4 == 2) angle = 180;
                    else angle = 90;
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                    //isRotate = !isRotate;
                    //if (isRotate) transform.rotation = Quaternion.Euler(0, 0, -90);
                    //else transform.rotation = Quaternion.Euler(0, 0, 0);
                    break;
                case -2:
                    SideChange();
                    break;
            }
        }
        manager.SelectCard(data, image.sprite, image.rectTransform.sizeDelta);
    }

    public void PointerEnter()
    {
        //transform.SetAsLastSibling();
    }

    public void SideChange()
    {
        int list_length = spriteList.Count;
        right_click_count++;
        int count = right_click_count % list_length;
        Sprite sprite = spriteList[count];
        image.sprite = sprite;

        if(spriteList.Count == 3)
        {
            Vector2 size = new Vector2();
            if (count == 0) size = new Vector2(63, 88);
            else if (count == 1) size = new Vector2(126, 88);
            else if (count == 2) size = new Vector2(88, 189);
            image.rectTransform.sizeDelta = size;
        }
        else if (spriteList.Count == 4)
        {
            Vector2 size = new Vector2();
            if (count == 3) size = new Vector2(88, 189);
            else size = new Vector2(63, 88);
            image.rectTransform.sizeDelta = size;
        }
    }

    public void BeginDrag()
    {
        isDrag = true;
        transform.SetAsLastSibling();
    }

    public void EndDrag()
    {
        isDrag = false;
    }

    public void InputText()
    {
        if(inputField.text == "") inputField_img.color = new Color(1, 1, 1, 0f);
        else inputField_img.color = new Color(1, 1, 1, 0.9f);
    }
}

