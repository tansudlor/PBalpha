#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro namespace

public class CreateEventPanelEditor : EditorWindow
{
    private Sprite iconSprite;
    private Sprite buttonSprite;
    private string eventNameText = "{Event name}";
    private string dateText = "2024-03-05 Expires in 7D";
    private string fromText = "From: {NPCNAME}";
    private Color fontColor = Color.white;

    [MenuItem("Tools/Create Event Panel")]
    public static void ShowWindow()
    {
        GetWindow<CreateEventPanelEditor>("Create Event Panel");
    }

    private void OnGUI()
    {
        GUILayout.Label("Event Panel Settings", EditorStyles.boldLabel);

        iconSprite = (Sprite)EditorGUILayout.ObjectField("Icon Sprite", iconSprite, typeof(Sprite), false);
        buttonSprite = (Sprite)EditorGUILayout.ObjectField("Button Sprite", buttonSprite, typeof(Sprite), false);

        eventNameText = EditorGUILayout.TextField("Event Name Text", eventNameText);
        dateText = EditorGUILayout.TextField("Date Text", dateText);
        fromText = EditorGUILayout.TextField("From Text", fromText);
        fontColor = EditorGUILayout.ColorField("Font Color", fontColor);

        if (GUILayout.Button("Create Event Panel"))
        {
            CreateEventPanel();
        }
    }

    private void CreateEventPanel()
    {
        // Create Canvas if none exists
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        // Create Panel
        GameObject panel = new GameObject("EventPanel");
        panel.transform.SetParent(canvas.transform);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(600, 150);
        panelRect.anchoredPosition = new Vector2(0, 0);
        panel.AddComponent<CanvasRenderer>();
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0.2f, 0.2f, 0.2f); // Dark gray background
        panelImage.rectTransform.localPosition = new Vector2(0, 0);

        // Create Icon
        GameObject icon = new GameObject("Icon");
        icon.transform.SetParent(panel.transform);
        RectTransform iconRect = icon.AddComponent<RectTransform>();
        iconRect.sizeDelta = new Vector2(100, 100);
        iconRect.anchoredPosition = new Vector2(-250, 0); // Adjusted position to align with the left side of the panel
        icon.AddComponent<CanvasRenderer>();
        Image iconImage = icon.AddComponent<Image>();
        iconImage.sprite = iconSprite;

        // Create Event Name Text
        GameObject eventName = new GameObject("EventName");
        eventName.transform.SetParent(panel.transform);
        RectTransform eventNameRect = eventName.AddComponent<RectTransform>();
        eventNameRect.sizeDelta = new Vector2(200, 50);
        eventNameRect.anchoredPosition = new Vector2(-50, 25); // Adjusted position to align above the center of the panel
        eventName.AddComponent<CanvasRenderer>();
        TextMeshProUGUI eventNameTextComponent = eventName.AddComponent<TextMeshProUGUI>();
        eventNameTextComponent.text = eventNameText;
        eventNameTextComponent.color = fontColor;
        eventNameTextComponent.fontSize = 24;
        eventNameTextComponent.alignment = TextAlignmentOptions.Center;

        // Create Date Text
        GameObject date = new GameObject("Date");
        date.transform.SetParent(panel.transform);
        RectTransform dateRect = date.AddComponent<RectTransform>();
        dateRect.sizeDelta = new Vector2(300, 30);
        dateRect.anchoredPosition = new Vector2(-50, 0); // Adjusted position to align with the center of the panel
        date.AddComponent<CanvasRenderer>();
        TextMeshProUGUI dateTextComponent = date.AddComponent<TextMeshProUGUI>();
        dateTextComponent.text = dateText;
        dateTextComponent.color = fontColor;
        dateTextComponent.fontSize = 14;
        dateTextComponent.alignment = TextAlignmentOptions.Center;

        // Create From Text
        GameObject from = new GameObject("From");
        from.transform.SetParent(panel.transform);
        RectTransform fromRect = from.AddComponent<RectTransform>();
        fromRect.sizeDelta = new Vector2(200, 30);
        fromRect.anchoredPosition = new Vector2(-50, -25); // Adjusted position to align below the center of the panel
        from.AddComponent<CanvasRenderer>();
        TextMeshProUGUI fromTextComponent = from.AddComponent<TextMeshProUGUI>();
        fromTextComponent.text = fromText;
        fromTextComponent.color = fontColor;
        fromTextComponent.fontSize = 14;
        fromTextComponent.alignment = TextAlignmentOptions.Center;

        // Create Gift Icon
        GameObject giftIcon = new GameObject("GiftIcon");
        giftIcon.transform.SetParent(panel.transform);
        RectTransform giftIconRect = giftIcon.AddComponent<RectTransform>();
        giftIconRect.sizeDelta = new Vector2(30, 30);
        giftIconRect.anchoredPosition = new Vector2(100, -25); // Position to the right of the "From" text
        giftIcon.AddComponent<CanvasRenderer>();
        Image giftIconImage = giftIcon.AddComponent<Image>();
        giftIconImage.sprite = iconSprite; // Use the same icon sprite or change it to another sprite if needed

        // Create Claim Button
        GameObject button = new GameObject("ClaimButton");
        button.transform.SetParent(panel.transform);
        RectTransform buttonRect = button.AddComponent<RectTransform>();
        buttonRect.sizeDelta = new Vector2(100, 50);
        buttonRect.anchoredPosition = new Vector2(250, 0); // Adjusted position to align with the right side of the panel
        button.AddComponent<CanvasRenderer>();
        Image buttonImage = button.AddComponent<Image>();
        buttonImage.sprite = buttonSprite;
        Button claimButton = button.AddComponent<Button>();

        // Create Button Text
        GameObject buttonText = new GameObject("ButtonText");
        buttonText.transform.SetParent(button.transform);
        RectTransform buttonTextRect = buttonText.AddComponent<RectTransform>();
        buttonTextRect.sizeDelta = new Vector2(100, 50);
        buttonTextRect.anchoredPosition = new Vector2(0, 0);
        buttonText.AddComponent<CanvasRenderer>();
        TextMeshProUGUI buttonTextComponent = buttonText.AddComponent<TextMeshProUGUI>();
        buttonTextComponent.text = "Claim";
        buttonTextComponent.color = Color.black;
        buttonTextComponent.fontSize = 18;
        buttonTextComponent.alignment = TextAlignmentOptions.Center;
    }
}
#endif