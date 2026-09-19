using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Components;
[Obsolete]
public class StatDisplay : VisualElement
{
	public StatDisplay(string statNameText, string statValueText)
	{
		// Create labels for the stat name and value
		Label statName = new Label(text: statNameText)
		{
			style =
			{
				fontSize = 36, color // Use the same font size as before
					= new StyleColor(v: Color.white) // White txt for stat name
			}
		};

		Label statValue = new Label(text: statValueText)
		{
			style =
			{
				fontSize = 36, color // Use the same font size as before
					= new StyleColor(v: Color.yellow) // Yellow txt for stat value
			}
		};

		// Set up the layout for the stat container (Row layout, spaced between items)
		style.flexDirection = FlexDirection.Row;
		style.justifyContent = Justify.SpaceBetween;
		style.marginBottom = 5;  // Same margin as in StatsWindow

		// Add the labels to the stat display container
		Add(child: statName);
		Add(child: statValue);
	}

	public StatDisplay(string statNameText, string statValueText, VisualElement column1, VisualElement column2)
	{
		// Create labels for the stat name and value
		Label statName = new Label(statNameText)
		{
			style =
			{
				fontSize = 36,
				color = new StyleColor(Color.white),
				unityTextAlign = TextAnchor.MiddleLeft
			}
		};

		Label statValue = new Label(statValueText)
		{
			style =
			{
				fontSize = 36,
				color = new StyleColor(Color.yellow),
				unityTextAlign = TextAnchor.MiddleRight
			}
		};

		// Add the labels to the provided columns
		column1.Add(statName);  // Add stat name to column1
		column2.Add(statValue); // Add stat value to column2
	}
}

