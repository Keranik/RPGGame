using RPGGame.Core;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGGame.UI.Templates
{
    public class BaseWindow<T> : VisualElement where T : BaseWindow<T>
    {
        protected VisualElement HeaderContainer;
        protected VisualElement ContentContainer;
        protected VisualElement FooterContainer;

		public bool IsVisible => style.display == DisplayStyle.Flex;

		public BaseWindow()
        {
            // Set up basic layout
            style.flexDirection = FlexDirection.Row;
            style.alignItems = Align.Stretch;
            style.justifyContent = Justify.SpaceEvenly;
            style.paddingTop = 10;
            style.paddingBottom = 10;
            style.paddingLeft = 15;
            style.paddingRight = 15;
            style.borderTopWidth = 2;
            style.borderBottomWidth = 2;
			style.alignSelf = Align.Stretch;

            // Create header, content, and footer containers
            HeaderContainer = new VisualElement
			{
				style = { alignSelf = Align.Stretch }
			};
            ContentContainer = new VisualElement
			{
				style = { alignSelf = Align.Stretch }
			};
            FooterContainer = new VisualElement
            {
                style = { alignSelf = Align.Stretch }
            };

            // Add containers to the window
            Add(child: HeaderContainer);
            Add(child: ContentContainer);
            Add(child: FooterContainer);
        }

        // Call this method to initialize the window content
        public T InitializeWindow()
        {
            AddHeaderContent();
            AddCustomContent();
            AddFooterContent();
            return (T)this;
        }

		public T SetPosition(float leftPercent, float topPercent, float widthPercent, float heightPercent)
		{
			// Set the width and height as a percentage of the screen
			style.width = new Length(value: widthPercent, unit: LengthUnit.Percent);
			style.height = new Length(value: heightPercent, unit: LengthUnit.Percent);

			// Position the window using percentages
			style.position = Position.Absolute;
			style.left = new Length(value: leftPercent, unit: LengthUnit.Percent);
			style.top = new Length(value: topPercent, unit: LengthUnit.Percent);

			return (T)this;
		}

		public T SetBorderColor(Color color, float width, BorderSide side = BorderSide.All)
		{
			switch (side)
			{
			case BorderSide.Top:
				style.borderTopColor = color;
				style.borderTopWidth = width;
				break;
			case BorderSide.Bottom:
				style.borderBottomColor = color;
				style.borderBottomWidth = width;
				break;
			case BorderSide.Left:
				style.borderLeftColor = color;
				style.borderLeftWidth = width;
				break;
			case BorderSide.Right:
				style.borderRightColor = color;
				style.borderRightWidth = width;
				break;
			case BorderSide.All:
				style.borderTopColor = color;
				style.borderBottomColor = color;
				style.borderLeftColor = color;
				style.borderRightColor = color;
				style.borderTopWidth = width;
				style.borderBottomWidth = width;
				style.borderLeftWidth = width;
				style.borderRightWidth = width;
				break;
			}

			return (T)this;
		}

		public T SetBorderThickness(float width, BorderSide side = BorderSide.All)
		{
			switch (side)
			{
			case BorderSide.Top:
				style.borderTopWidth = width;
				break;
			case BorderSide.Bottom:
				style.borderBottomWidth = width;
				break;
			case BorderSide.Left:
				style.borderLeftWidth = width;
				break;
			case BorderSide.Right:
				style.borderRightWidth = width;
				break;
			case BorderSide.All:
				style.borderTopWidth = width;
				style.borderBottomWidth = width;
				style.borderLeftWidth = width;
				style.borderRightWidth = width;
				break;
			}

			return (T)this;
		}

		public T SetBorderRadius(float radius, BorderSide side = BorderSide.All)
		{
			switch (side)
			{
			case BorderSide.Top:
				style.borderTopLeftRadius = radius;
				style.borderTopRightRadius = radius;
				break;
			case BorderSide.Bottom:
				style.borderBottomLeftRadius = radius;
				style.borderBottomRightRadius = radius;
				break;
			case BorderSide.Left:
				style.borderTopLeftRadius = radius;
				style.borderBottomLeftRadius = radius;
				break;
			case BorderSide.Right:
				style.borderTopRightRadius = radius;
				style.borderBottomRightRadius = radius;
				break;
			case BorderSide.All:
				style.borderTopLeftRadius = radius;
				style.borderTopRightRadius = radius;
				style.borderBottomLeftRadius = radius;
				style.borderBottomRightRadius = radius;
				break;
			}

			return (T)this;
		}

		public T SetStackDirection(FlexDirection direction) {
			style.flexDirection = direction;
			return (T)this;
		}

		// Apply a style to the window (can be overridden for custom styles)
		public virtual T ApplyStyle(StyleSheet styleSheet)
        {
            styleSheets.Add(styleSheet: styleSheet);
            return (T)this;
        }

        // Add header content to the window (e.g., title, close button) -- can be overridden
        protected virtual void AddHeaderContent()
        {
            Label headerLabel = new Label(text: "Default Header")
            {
                style =
                {
                    fontSize = 20,
                    color = new StyleColor(v: Color.white),
                    unityTextAlign = TextAnchor.UpperCenter
                }
            };

            HeaderContainer.Add(child: headerLabel);
        }

        // Add custom content to the window -- can be overridden in derived classes
        protected virtual void AddCustomContent()
        {
            Label contentLabel = new Label(text: "Default Content")
            {
                style =
                {
                    fontSize = 16,
                    color = new StyleColor(v: Color.white)
                }
            };

            ContentContainer.Add(child: contentLabel);
        }

        // Add footer content to the window -- can be overridden in derived classes
        protected virtual void AddFooterContent()
        {
            //Label footerLabel = new Label(text: "Default Footer")
            //{
            //    style =
            //    {
            //        fontSize = 12,
            //        color = new StyleColor(v: Color.gray)
            //    }
            //};

            //FooterContainer.Add(child: footerLabel);
        }

		// Method to show the window
		public void Show()
		{
			style.display = DisplayStyle.Flex;
		}

		// Method to hide the window
		public void Hide()
		{
			style.display = DisplayStyle.None;
		}

		// Method to close the Window
		public void Close() {
			RemoveFromHierarchy();
		}

		protected virtual void RenderUpdate() {

		}

		protected virtual void SyncUpdate(GameTime gameTime) {

		}
	}
}
