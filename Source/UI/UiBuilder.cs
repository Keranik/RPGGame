using RPGGame.Core;
using UnityEngine;
using UnityEngine.UIElements;
#nullable disable

namespace RPGGame.UI;
[Dependency(registrationType: RegistrationType.Singleton)]
public class UiBuilder
{
	public VisualElement MainGameOverlay { get; private set; }
    public VisualElement UiOverlay { get; private set; }
    public VisualElement PopupOverlay { get; private set; }

    public UiBuilder()
    {
        // Create the super canvas and set it as the root UI element
        NewSuperCanvas(id: "SuperCanvas");
		UnityEngine.Debug.Log("UI Builder Initialized");
	}

	public VisualElement NewSuperCanvas(string id)
    {
        VisualElement canvas = new VisualElement
        {
            name = id,
            style =
            {
                width = Length.Percent(value: 100),
                height = Length.Percent(value: 100),
                position = Position.Absolute
            }
        };

        canvas.style.position = Position.Absolute;
        return canvas;
    }

    public VisualElement CreateGameUI()
    {
        VisualElement root = new VisualElement
        {
            name = "root",
            style =
            {
                width = Length.Percent(value: 100),
                height = Length.Percent(value: 100),
                position = Position.Absolute,
				backgroundColor = new StyleColor(Color.clear)
			}
        };
        root.pickingMode = PickingMode.Ignore;

		MainGameOverlay = new VisualElement
        {
            name = "main-game-overlay",
            style =
            {
                width = Length.Percent(value: 100),
                height = Length.Percent(value: 100),
                position = Position.Absolute,
				backgroundColor = new StyleColor(Color.clear)
            }
        };
        MainGameOverlay.pickingMode = PickingMode.Ignore;
		root.Add(child: MainGameOverlay);

        UiOverlay = new VisualElement
        {
            name = "ui-overlay",
            style =
            {
                width = Length.Percent(value: 100),
                height = Length.Percent(value: 100),
                position = Position.Absolute,
				backgroundColor = new StyleColor(Color.clear)
            }
        };
        UiOverlay.pickingMode = PickingMode.Ignore;
		root.Add(child: UiOverlay);

        PopupOverlay = new VisualElement
        {
            name = "popup-overlay",
            style =
            {
                width = Length.Percent(value: 100),
                height = Length.Percent(value: 100),
                position = Position.Absolute,
				backgroundColor = new StyleColor(Color.clear)
            }
        };
		PopupOverlay.pickingMode = PickingMode.Ignore;
		root.Add(child: PopupOverlay);

        return root;
    }

    // Simplified methods to add elements to specific overlays
    public void AddToUIOverlay(VisualElement element)
    {
        UiOverlay?.Add(child: element);
    }

    public void AddToPopupOverlay(VisualElement element)
    {
        PopupOverlay?.Add(child: element);
    }

    public void AddToMainGameOverlay(VisualElement element)
    {
        MainGameOverlay?.Add(child: element);
    }
}


