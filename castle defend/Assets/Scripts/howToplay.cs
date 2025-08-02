using UnityEngine;
using UnityEngine.UI;

public class howToplay : MonoBehaviour
{
    private GameObject panel;

    public void ShowInstructions()
    {
        if (panel != null) return;

        // Get or create Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // Create Panel
        panel = new GameObject("HowToPlayPanel");
        panel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(700, 500);
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;

        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.9f);

        // Create Instructions Text
        GameObject textGO = new GameObject("InstructionText");
        textGO.transform.SetParent(panel.transform, false);
        Text text = textGO.AddComponent<Text>();
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(660, 400);
        textRect.anchoredPosition = new Vector2(0, -20);
        text.alignment = TextAnchor.UpperLeft;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 18;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = @"Gameplay Basics
Enemies spawn from the left and move toward your castle.
Use your mouse to grab high and throw them before they arrive.
Letting enemies reach the castle decreases your health.

Grab Enemy: Left Mouse Button (Hold)
Throw Enemy: Drag Mouse Upward

Level 1: Basic Warrior only.
Level 2: Soldier Warrior only.
Level 3: Armored Warrior only.
Level 4: Basic + Soldier + Armored Warrior type.

Main Objective:
Survive each wave by defending your castle.

Secondary Goals:
Prevent any enemy from reaching your base.
Complete stages with full castle health.";

        // Create Close Button
        GameObject btnGO = new GameObject("CloseButton");
        btnGO.transform.SetParent(panel.transform, false);
        Button btn = btnGO.AddComponent<Button>();
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = new Color(0.7f, 0.2f, 0.2f);

        RectTransform btnRect = btnGO.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(120, 40);
        btnRect.anchorMin = new Vector2(1f, 1f);
        btnRect.anchorMax = new Vector2(1f, 1f);
        btnRect.pivot = new Vector2(1f, 1f);
        btnRect.anchoredPosition = new Vector2(-10, -10);

        // Button Text
        GameObject btnTextGO = new GameObject("ButtonText");
        btnTextGO.transform.SetParent(btnGO.transform, false);
        Text btnText = btnTextGO.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.text = "Close";
        btnText.fontSize = 18;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        RectTransform btnTextRect = btnText.GetComponent<RectTransform>();
        btnTextRect.sizeDelta = btnRect.sizeDelta;
        btnTextRect.anchoredPosition = Vector2.zero;

        // Close action
        btn.onClick.AddListener(CloseInstructions);
    }

    private void CloseInstructions()
    {
        if (panel != null)
        {
            Destroy(panel);
            panel = null;
        }
    }
}
