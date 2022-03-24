using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

public class Manager : MonoBehaviour
{
    string path_deck, deck_text_path;
    Sprite back;

    List<CardData> deck_original = new List<CardData>();
    List<CardData> deck_current = new List<CardData>();
    List<CardData> hand = new List<CardData>();
    List<CardData> other = new List<CardData>();
    List<CardData> gr = new List<CardData>();

    bool isSelected;
    int back_index;
    CardData selectedCard;
    Sprite select_sprite;
    Vector2 selectImageSize;

    [SerializeField] int select_id_card; //デッキ外から選ぶ用
    List<int> select_id_list = new List<int>(); //デッキの中から複数選ぶ用

    [SerializeField]
    GameObject mainCanvas,otherCanvas;

    int count_HomeMode;
    [SerializeField]
    GameObject panel_exe, panel_webGL;

    RectTransform mainCanvas_rect;

    [SerializeField]
    InputField inputField, inputField_txtFilePath;

    [SerializeField]
    GameObject folderImage;
    [SerializeField]
    Transform folderPanel, txt_filePanel;
    [SerializeField]
    Text select_deck_text, select_deck_text2;

    [SerializeField]
    Transform parent_hand;
    [SerializeField]
    GameObject card;

    bool isOpen_list;
    [SerializeField]
    GameObject showListPanel;
    [SerializeField]
    Transform parent_list;
    [SerializeField]
    GameObject list_card;

    [SerializeField]
    Text deck_count_text;
    [SerializeField]
    RectTransform gr_deck_pos;
    [SerializeField]
    Text gr_count_text;

    bool isZoom;
    [SerializeField]
    GameObject zoomPanel;
    [SerializeField]
    Image zoomCard;

    int draw_pos_x = -300;
    int draw_pos_type;
    [SerializeField]
    Text draw_pos_buttonText;
    [SerializeField]
    InputField inputField_index;

    [SerializeField]
    RectTransform handPos;
    [SerializeField]
    RectTransform deckPos;

    [SerializeField]
    InputField inputField_URL;

    void Start()
    {
        mainCanvas.SetActive(false);
        otherCanvas.SetActive(true);
        mainCanvas_rect = mainCanvas.GetComponent<RectTransform>();

        count_HomeMode = 0;
        Change_HomePanel();
        bool isPath = PlayerPrefs.HasKey("path");
        if (isPath)
        {
            string savePath = PlayerPrefs.GetString("path");
            if (string.IsNullOrEmpty(savePath)) return;
            inputField.text = PlayerPrefs.GetString("path");
            LoadFolder();
        }

        bool isPath_txt = PlayerPrefs.HasKey("path_txt");
        if (isPath_txt)
        {
            string savePath = PlayerPrefs.GetString("path_txt");
            if (string.IsNullOrEmpty(savePath)) return;
            inputField_txtFilePath.text = PlayerPrefs.GetString("path_txt");
            LoadTextFile();
        }
    }

    public void Change_HomePanel()
    {
        if(count_HomeMode == 0)
        {
            panel_exe.SetActive(true);
            panel_webGL.SetActive(false);
        }
        else
        {
            panel_exe.SetActive(false);
            panel_webGL.SetActive(true);
        }
        count_HomeMode++;
        count_HomeMode = count_HomeMode % 2;
    }

    public void LoadFolder()
    {
        string filePath = inputField.text;
        string path_decks = filePath + "/Decks";
        string path_back = filePath + "/Back";
        string path_deck = path_decks + "/Deck";
        string path_main = path_deck + "/Main";

        back = ReadBackSprite(path_back);
        try
        {
            string[] folders = Directory.GetDirectories(path_decks, "*", SearchOption.TopDirectoryOnly);
            OnFolders(folders);
        }
        catch (DirectoryNotFoundException)
        {
            Debug.Log("Error");
        }
    }

    public void LoadTextFile()
    {
        string filePath = inputField_txtFilePath.text;

        try
        {
            string[] folders = Directory.GetFiles(filePath, "*.txt", SearchOption.AllDirectories);
            OnTextFiles(folders);
        }
        catch (FileNotFoundException)
        {
            Debug.Log("Error");
        }
    }

    public void OnFolders(string[] paths)
    {
        foreach (Transform tr in folderPanel) Destroy(tr.gameObject);
        //int n = 0;
        foreach (string path in paths)
        {
            GameObject folder = Instantiate(folderImage);
            folder.transform.SetParent(folderPanel, false);
            DeckSelect deck = folder.GetComponent<DeckSelect>();
            deck.type = 0;
            deck.Set(path);
        }
    }

    public void OnTextFiles(string[] paths)
    {
        foreach (Transform tr in txt_filePanel) Destroy(tr.gameObject);
        //int n = 0;
        foreach (string path in paths)
        {
            GameObject folder = Instantiate(folderImage);
            folder.transform.SetParent(txt_filePanel, false);
            DeckSelect deck = folder.GetComponent<DeckSelect>();
            deck.type = 1;
            deck.Set(path);
        }
    }

    public void SetPath(string path)
    {
        path_deck = path;
        select_deck_text.text = Path.GetFileName(path);
    }

    public void SetPath_Txt(string path)
    {
        deck_text_path = path;
        select_deck_text2.text = Path.GetFileName(path);     
    }

    public void GameStart()
    {
        if (string.IsNullOrEmpty(path_deck)) return;
        KeyManager key = GetComponent<KeyManager>();
        key.on = true;
        string path_main = path_deck + "/Main";
        string path_other = path_deck + "/Other";
        string path_GR = path_deck + "/GR";
        int n = 0;
        if (Directory.Exists(path_main))
        {
            string[] files_main = Directory.GetFiles(path_main, "*.jpg", SearchOption.TopDirectoryOnly);
            string[] folders_main = Directory.GetDirectories(path_main, "*", SearchOption.TopDirectoryOnly);

            foreach (string file in files_main)
            {
                CardData data = SetCardData(file);
                data.cardType = CardType.Normal;
                data.id = n;
                deck_original.Add(data);
                deck_current.Add(data);
                n++;
            }

            foreach (string folder in folders_main)
            {
                string folderName = Path.GetFileName(folder);
                if (int.TryParse(folderName, out int result))
                {
                    string[] files = Directory.GetFiles(folder, "*.jpg", SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        for (int i = 0; i < result; i++)
                        {
                            CardData data = SetCardData(file);
                            data.cardType = CardType.Normal;
                            data.id = n;
                            deck_original.Add(data);
                            deck_current.Add(data);
                            n++;
                        }
                    }
                }
            }
        }

        if (Directory.Exists(path_other))
        {
            string[] files_other = Directory.GetFiles(path_other, "*.jpg", SearchOption.TopDirectoryOnly);
            string[] path_inOther_Folder = Directory.GetDirectories(path_other, "*", SearchOption.TopDirectoryOnly);
            foreach (string folder in path_inOther_Folder)
            {
                CardData data = SetCardData_Other(folder);
                data.cardType = CardType.Other;
                data.id = n;
                hand.Add(data);
                //other.Add(data);
                n++;
            }

            foreach (string file in files_other)
            {
                CardData data = SetCardData(file);
                data.cardType = CardType.Other;
                data.id = n;
                hand.Add(data);
                //other.Add(data);
                n++;
            }
            SetOtherCard(hand);
            //SetOtherCard(other);
        }

        
        //.Where(s => !s.EndsWith(".meta", System.StringComparison.OrdinalIgnoreCase)).ToArray();

        if (Directory.Exists(path_GR))
        {
            string[] files_GR = Directory.GetFiles(path_GR, "*.jpg", SearchOption.TopDirectoryOnly);
            string[] folders_GR = Directory.GetDirectories(path_GR, "*", SearchOption.TopDirectoryOnly);
            foreach (string file in files_GR)
            {
                CardData data = SetCardData(file);
                data.cardType = CardType.GR;
                data.id = n;
                gr.Add(data);
                n++;
            }

            foreach (string folder in folders_GR)
            {
                string folderName = Path.GetFileName(folder);
                if (int.TryParse(folderName, out int result))
                {
                    string[] files = Directory.GetFiles(folder, "*.jpg", SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        for (int i = 0; i < result; i++)
                        {
                            CardData data = SetCardData(file);
                            data.cardType = CardType.GR;
                            data.id = n;
                            gr.Add(data);
                            n++;
                        }
                    }
                }
            }
        }

        deck_count_text.text = $"残り\n{deck_current.Count()}枚";
        if (gr.Count == 0) gr_deck_pos.gameObject.SetActive(false);
        gr_count_text.text = $"残り\n{gr.Count()}枚";
        Shuffle_Deck();
        Shuffle_GR();
        mainCanvas.SetActive(true);
        otherCanvas.SetActive(false);
    }


    public async void GameStart_Web()
    {
        if (string.IsNullOrEmpty(deck_text_path)) return;
        KeyManager key = GetComponent<KeyManager>();
        key.on = true;

        string url_back_default = "https://i.gzn.jp/img/2014/07/02/why-internet-blue/twitterb_m.png";
        Texture2D tex_back_def = await DownloadTexture(url_back_default);
        back = Sprite.Create(tex_back_def, new Rect(0, 0, tex_back_def.width, tex_back_def.height), Vector2.zero);

        int type = 0;
        int n = 0;
        string all = File.ReadAllText(deck_text_path);
        //Debug.Log(all);

        var lines = all.Replace("\r\n", "\n").Split(new[] { '\n', '\r' });
        foreach (string line in lines)
        {
            string[] texts = line.Split(',', '、');

            int count;
            if (texts.Length > 1)
            {
                bool success = int.TryParse(texts[1], out int result);
                count = (success) ? result : 1;
            }
            else count = 1;

            if (string.IsNullOrEmpty(texts[0])) Debug.Log("empty");
            else if (texts[0] == "Back") type = -1;
            else if (texts[0] == "Main") type = 0;
            else if (texts[0] == "Other") type = 1;
            else if (texts[0] == "GR") type = 2;
            else
            {
                if (type == -1)
                {
                    string url = texts[0];
                    Texture2D tex_back = await DownloadTexture(url);
                    back = Sprite.Create(tex_back, new Rect(0, 0, tex_back.width, tex_back.height), Vector2.zero);
                }
                else if (type == 0)
                {
                    string url = texts[0];
                    Texture2D tex = await DownloadTexture(url);
                    for (int i = 0; i < count; i++)
                    {
                        CardData data = SetCardData_Web(tex);
                        data.id = n;
                        data.cardType = CardType.Normal;
                        deck_original.Add(data);
                        deck_current.Add(data);
                        n++;
                    }
                }
                else if (type == 1)
                {
                    List<Texture2D> texs = new List<Texture2D>();
                    foreach (string text in texts)
                    {
                        string url = text;
                        Texture2D tex = await DownloadTexture(url);
                        texs.Add(tex);
                    }
                    CardData data = SetCardData_Other_Web(texs);
                    if (data.spriteList.Count == 1) data.spriteList.Add(back);
                    data.id = n;
                    data.cardType = CardType.Other;
                    hand.Add(data);
                    //other.Add(data);
                    n++;
                }
                else if (type == 2)
                {
                    string url = texts[0];
                    Texture2D tex = await DownloadTexture(url);
                    for (int i = 0; i < count; i++)
                    {
                        CardData data = SetCardData_Web(tex);
                        data.id = n;
                        data.cardType = CardType.GR;
                        gr.Add(data);
                        n++;
                    }
                }
            } 
        }
        SetOtherCard(hand);
        //SetOtherCard(other);
        deck_count_text.text = $"残り\n{deck_current.Count()}枚";
        if (gr.Count == 0) gr_deck_pos.gameObject.SetActive(false);
        gr_count_text.text = $"残り\n{gr.Count()}枚";
        Shuffle_Deck();
        Shuffle_GR();
        mainCanvas.SetActive(true);
        otherCanvas.SetActive(false);
    }

    async UniTask<Texture2D> DownloadTexture(string uri)
    {
        var r = UnityWebRequestTexture.GetTexture(uri);
        await r.SendWebRequest();
        return DownloadHandlerTexture.GetContent(r);
    }

    public void SetOtherCard(List<CardData> list)
    {
        int x = -350;
        int y = 100;
        foreach (CardData data in list)
        {
            SetCard(data: data, x:x, y:y, side: Side.front, onAnima:false);
            if (x == -350) x = -275;
            else
            {
                x = -350;
                y -= 20;
            }
        }
    }

    public void Draw_GR()
    {
        if (gr.Count <= 0) return;
        CardData data = gr[0];
        gr.Remove(data);
        hand.Add(data);

        Vector2 pos = GetPos_Canvas(mainCanvas, draw_pos_type);
        int x = (int)pos.x;
        int y = (int)pos.y;
        SetCard(data: data, x: x, y: y, side: Side.front, onAnima: true, gr:true);
        gr_count_text.text = $"残り\n{gr.Count()}枚";
    }

    //デッキの一番上を引く
    public void Draw_Top(int type) 
    {
        if (deck_current.Count() <= 0) return;
        CardData data = deck_current[0];

        Side side = (Side)System.Enum.ToObject(typeof(Side), type);
        Draw_Deck(data, side);
    }

    //デッキの中から引数のdataのカードをhandに持ってくる
    public void Draw_Deck(CardData data, Side side=Side.back)
    {
        deck_current.Remove(data);
        hand.Add(data);

        Vector2 pos = GetPos_Canvas(mainCanvas, draw_pos_type);
        int x = (int)pos.x;
        int y = (int)pos.y;
        SetCard(data:data, x:x, y:y, side:side, onAnima:true);
        deck_count_text.text = $"残り\n{deck_current.Count()}枚";
        
        if (isOpen_list) CloseShowList(clear:false);
        //ShowList(deck_current);
    }

    Vector2 GetPos_Canvas(GameObject canvas, int type)
    {
        Vector2 size = canvas.GetComponent<RectTransform>().rect.size;
        int canvas_h = (int)size.y;
        int card_h = 88;
        int margin = 5;
        draw_pos_x += 40;
        if (draw_pos_x > 240) draw_pos_x = -300;
        int x = draw_pos_x;

        if (type == 0)
        {
            int y = 30;
            return new Vector2(x, y);
        }
        else if(type == 1)
        {
            int y = -canvas_h / 2 + (card_h / 2) + margin;
            return new Vector2(x, y);
        }
        else
        {
            int y = 50 + card_h + card_h/2;
            return new Vector2(x, y);
        }
    }

    //選んだカードをデッキの一番下に戻す
    public void SelectCard_BackToDeck(int type)
    {
        if (!isSelected) return;
        isSelected = false;
        CardData data = SearchCardData(hand, select_id_card);
        Position pos = (Position)System.Enum.ToObject(typeof(Position), type);
        int index;

        if(data.cardType == CardType.Normal || data.cardType == CardType.Other)
        {
            if (pos == Position.top) index = 0;
            else if (pos == Position.bottom) index = deck_current.Count();
            else
            {
                if (int.TryParse(inputField_index.text, out int result))
                {
                    if (deck_current.Count < result) result = deck_current.Count;
                    index = result;
                }
                else index = 0;
            }
            //else index = back_index;
            SendCardToDeck(index, data);
        }
        else if(data.cardType == CardType.GR)
        {
            index = gr.Count;
            SendCardToGR(index, data);
        }    
    }

    public void SendCardToDeck(int index, CardData data)
    {
        deck_current.Insert(index, data);
        hand.Remove(data);
        DestroyCard(parent_hand, data.id);
        deck_count_text.text = $"残り\n{deck_current.Count()}枚";
        if (isOpen_list) CloseShowList();
    }

    public void SendCardToGR(int index, CardData data)
    {
        gr.Insert(index, data);
        hand.Remove(data);
        DestroyCard(parent_hand, data.id);
        gr_count_text.text = $"残り\n{gr.Count()}枚";
        if (isOpen_list) CloseShowList();
    }

    public void Show_Deck(int type)
    {
        Side side = (Side)System.Enum.ToObject(typeof(Side), type);
        isOpen_list = !isOpen_list;
        if (isOpen_list)
        {
            ShowList(deck_current, side);
            showListPanel.SetActive(true);
            select_id_list.Clear();
        }
        else CloseShowList();
        
    }

    public void AddSelectCardList(CardData data)
    {
        int id = data.id;

        bool dup = CheckDuplicate(id, select_id_list);
        if(dup)
        {
            select_id_list.Remove(id);
        }
        else
        {
            select_id_list.Add(id);
        }

        ChangeColor(select_id_list);
    }

    public void SelectCard(CardData data, Sprite selectSprite, Vector2 imageSize)
    {
        this.select_sprite = selectSprite;
        this.selectImageSize = imageSize;
        isSelected = true;
        select_id_card = data.id;

        foreach (Transform tr in parent_hand)
        {
            Card card = tr.GetComponent<Card>();
            int card_id = card.data.id;

            if (select_id_card == card_id) card.GetComponent<Image>().color = new Color(1f, 1f, 0.5f);
            else card.GetComponent<Image>().color = new Color(1f, 1f, 1f);
        }
    }

    public void Draw_SelectCard(int type)
    {
        Side side = (Side)System.Enum.ToObject(typeof(Side), type);
        foreach (int id in select_id_list)
        {
            CardData data = SearchCardData(deck_current, id);
            Draw_Deck(data, side);
            DestroyCard(parent_list, id);
        }
        select_id_list.Clear();
    }

    public void ChangeColor(List<int> list)
    {
        foreach (Transform tr in parent_list)
        {
            List_Card card = tr.GetComponent<List_Card>();
            int card_id = card.data.id;

            bool dup = CheckDuplicate(card_id, list);
            if (dup) card.GetComponent<Image>().color = new Color(1f, 1f, 0.5f);
            else card.GetComponent<Image>().color = new Color(1f, 1f, 1f);
        }
    }


    bool CheckDuplicate(int target, List<int> list)
    {
        foreach (int n in list)
        {
            if (n == target)
            {
                return true;
            }
        }
        return false;
    }

    public void ShowList(List<CardData> list, Side side)
    {
        foreach (Transform tr in parent_list) Destroy(tr.gameObject);

        foreach (CardData data in list)
        {
            GameObject obj = Instantiate(this.list_card);
            obj.transform.SetParent(parent_list, false);
            List_Card card = obj.GetComponent<List_Card>();
            card.OnOff_InputField(false);
            card.SetCard(data);
            card.OnCard(side:side);
        }
    }

    public void SetCard(CardData data,int x=0, int y=0, Side side=Side.back, bool onAnima=true, bool gr = false)
    {
        GameObject obj = Instantiate(this.card);
        obj.transform.SetParent(parent_hand, false);

        RectTransform card_rect = obj.GetComponent<RectTransform>();
        if(onAnima)
        {
            card_rect.anchoredPosition = (gr)?
                gr_deck_pos.anchoredPosition: deckPos.anchoredPosition;
            card_rect.DOAnchorPos(new Vector2(x, y), 0.3f).SetEase(Ease.Linear);
        }
        else
        {
            card_rect.anchoredPosition = new Vector2(x, y);
        }

        
        Card card = obj.GetComponent<Card>();
        card.SetCard(data);
        card.OnCard(side);
    }

    public void DestroyCard(Transform parent, int target)
    {
        foreach (Transform tr in parent)
        {
            CardData data = tr.GetComponent<Card>().data;
            int id = data.id;
            if (target == id)
            {
                Destroy(tr.gameObject);
                break;
            }
        }
    }

    public void Zoom_Card()
    {
        if (!isSelected) return;
        isZoom = !isZoom;
        if(isZoom)
        {
            Vector2 canvasSize = mainCanvas_rect.rect.size;
            if (selectImageSize.y >= selectImageSize.x)
            {
                float size_w = canvasSize.x * (5f / 6);
                float aspect_ratio = selectImageSize.y / selectImageSize.x; //選択した画像の高さを幅で割った値
                float size_h = size_w * aspect_ratio;
                zoomCard.rectTransform.sizeDelta = new Vector2(size_w, size_h);
            }
            else
            {
                float size_h = canvasSize.y * (5f / 6);
                float aspect_ratio = selectImageSize.x / selectImageSize.y; //選択した画像の幅を高さで割った値
                float size_w = size_h * aspect_ratio;
                zoomCard.rectTransform.sizeDelta = new Vector2(size_w, size_h);
            }

            zoomPanel.SetActive(true);
            Sprite sprite = select_sprite;
            zoomCard.sprite = sprite;
        }
        else
        {
            zoomPanel.SetActive(false);
        }
    }

    public void Shuffle_Deck()
    {
        deck_current = ShuffleList(deck_current);
        CloseShowList();
    }

    public void Shuffle_GR()
    {
        gr = ShuffleList(gr);
    }

    public void CloseShowList(bool clear=true)
    {
        isOpen_list = false;
        showListPanel.SetActive(false);
        if(clear) select_id_list.Clear();
    }

    public void ChangeDrawPos(int n)
    {
        int type = (draw_pos_type + n) % 3;
        draw_pos_type = type;
        string text;
        if (draw_pos_type == 0) text = "真ん中";
        else if(draw_pos_type == 1) text = "下";
        else text = "上";
        draw_pos_buttonText.text = text;
    }

    List<CardData> ShuffleList(List<CardData> original)
    {
        int count = original.Count();
        List<CardData> result = new List<CardData>();
        for (int i = 0; i < count; i++) result.Add(new CardData());

        List<int> indexList = new List<int>();
        for (int i = 0; i < count; i++) indexList.Add(i);

        foreach (CardData data in original)
        {
            int r = Random.Range(0, indexList.Count());
            int index = indexList[r];
            indexList.Remove(index);
            result[index] = data;
        }

        return result;
    }

    CardData SetCardData(string path)
    {
        Texture2D tex = ReadFileImage(path);
        Sprite original = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        // Debug.Log($"{tex.width} : {tex.height}");
        CardData data = new CardData();
        data.spriteList = new List<Sprite>();
        data.spriteList.Add(original);
        data.spriteList.Add(back);
        return data;
    }

    CardData SetCardData_Web(Texture2D tex)
    {
        Sprite original = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        CardData data = new CardData();
        data.spriteList = new List<Sprite>();
        data.spriteList.Add(original);
        data.spriteList.Add(back);
        return data;
    }

    CardData SetCardData_Other(string path_folder)
    {
        CardData data = new CardData();
        data.spriteList = new List<Sprite>();
        string[] files = Directory.GetFiles(path_folder, "*.jpg", SearchOption.TopDirectoryOnly);

        foreach (string file in files)
        {
            Texture2D tex = ReadFileImage(file);
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            data.spriteList.Add(sprite);
        }
        return data;
    }

    CardData SetCardData_Other_Web(List<Texture2D> texs)
    {
        CardData data = new CardData();
        data.spriteList = new List<Sprite>();

        foreach (Texture2D tex in texs)
        {
            Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
            data.spriteList.Add(sprite);
        }
        return data;
    }

    CardData SearchCardData(List<CardData> list, int targetID)
    {
        foreach (CardData data in list)
        {
            int id = data.id;
            if (id == targetID)
            {
                return data;
            }
        }
        return new CardData();
    }

    Sprite ReadBackSprite(string path)
    {
        try
        {
            string[] files_back = Directory.GetFiles(@path, "*", SearchOption.TopDirectoryOnly);
            int backFile_length = files_back.Length;
            int random = Random.Range(0, backFile_length);
            string file_back = files_back[random];
            Texture2D tex_back = ReadFileImage(file_back);
            Sprite sprite = Sprite.Create(tex_back, new Rect(0, 0, tex_back.width, tex_back.height), Vector2.zero);
            return sprite;
        }
        catch(DirectoryNotFoundException)
        {
            Debug.Log("Error");
            return null;
        }
    }

    Texture2D ReadFileImage(string path)
    {
        byte[] byteData = File.ReadAllBytes(path);
        Texture2D texture2D = new Texture2D(0, 0, TextureFormat.RGBA32, false);
        texture2D.LoadImage(byteData);
        return texture2D;
    }

    public void SceneChange(string scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void InputText(string text)
    {
        PlayerPrefs.SetString("path", text);
    }

    public void InputText_Txt(string text)
    {
        PlayerPrefs.SetString("path_txt", text);
    }

    public enum Side
    {
        front,back
    }

    public enum Position
    {
        top,bottom,other
    }
}