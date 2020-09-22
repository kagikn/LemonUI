using LemonUI.Elements;
using System;
using System.Collections.Generic;
using System.Drawing;
#if SHVDN2
using GTA;
#elif SHVDN3
using GTA;
using GTA.UI;
#elif FIVEM
using CitizenFX.Core;
using CitizenFX.Core.UI;
#endif

namespace LemonUI.Menus
{
    /// <summary>
    /// The Style of title for the Color Panel.
    /// </summary>
    public enum ColorTitleStyle
    {
        /// <summary>
        /// Does not shows any Title.
        /// The count will still be shown if <see cref="NativeColorPanel.ShowCount"/> is set to <see langword="true"/>.
        /// </summary>
        None = -1,
        /// <summary>
        /// Shows a Simple Title for all of the Colors.
        /// </summary>
        Simple = 0,
        /// <summary>
        /// Shows the Color Name as the Title.
        /// </summary>
        ColorName = 1
    }

    /// <summary>
    /// Represents the Color Information shown on the Panel.
    /// </summary>
    public class NativeColorData
    {
        #region Internal Fields

        internal readonly ScaledRectangle rectangle = new ScaledRectangle(PointF.Empty, SizeF.Empty);

        #endregion

        #region Public Properties

        /// <summary>
        /// The name of the color.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The RGBA values of the color.
        /// </summary>
        public Color Color
        {
            get => rectangle.Color;
            set => rectangle.Color = value;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new Color Panel information.
        /// </summary>
        /// <param name="name">The name of the color.</param>
        /// <param name="color">The RGBA values of the color.</param>
        public NativeColorData(string name, Color color)
        {
            Name = name;
            rectangle.Color = color;
        }

        #endregion
    }

    /// <summary>
    /// A Panel that allows you to select a Color.
    /// </summary>
    public class NativeColorPanel : NativePanel
    {
        #region Private Fields

        /// <summary>
        /// The position reported after the last Recalculation.
        /// </summary>
        private PointF lastPosition = PointF.Empty;
        /// <summary>
        /// The Width reported after the last Recalculation.
        /// </summary>
        private float lastWidth = 0;
        /// <summary>
        /// The title of the Color Panel.
        /// </summary>
        private ScaledText title = new ScaledText(PointF.Empty, "", 0.325f)
        {
            Alignment = Alignment.Center
        };
        /// <summary>
        /// The rectangle used for marking the item selection on the screen.
        /// </summary>
        private ScaledRectangle selectionRectangle = new ScaledRectangle(PointF.Empty, SizeF.Empty);
        /// <summary>
        /// The current index of the Colors.
        /// </summary>
        private int index = 0;
        /// <summary>
        /// The position of the first item.
        /// </summary>
        private int firstItem = 0;
        /// <summary>
        /// The maximum number of items shown at once.
        /// </summary>
        private int maxItems = 9;
        /// <summary>
        /// The generic title for this color.
        /// </summary>
        private string simpleTitle = "Color";
        /// <summary>
        /// The style of the title.
        /// </summary>
        private ColorTitleStyle titleStyle = ColorTitleStyle.Simple;
        /// <summary>
        /// If the number of colors should be shown.
        /// </summary>
        private bool showCount = true;
        /// <summary>
        /// The items that are currently visible on the screen.
        /// </summary>
        private List<NativeColorData> visibleItems = new List<NativeColorData>();

        #endregion

        #region Public Fields

        /// <summary>
        /// The default sound used for the Color Navigation.
        /// </summary>
        public static readonly Sound DefaultSound = new Sound("HUD_FRONTEND_DEFAULT_SOUNDSET", "NAV_LEFT_RIGHT");

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns the currently selected Color.
        /// </summary>
        public NativeColorData SelectedColor
        {
            get
            {
                if (Colors.Count == 0 || index >= Colors.Count)
                {
                    return null;
                }
                return Colors[SelectedIndex];
            }
        }
        /// <summary>
        /// The index of the currently selected Color.
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                if (Colors.Count == 0 || index >= Colors.Count)
                {
                    return -1;
                }
                return index;
            }
            set
            {
                // If the list of items is empty, don't allow the user to set the index
                if (Colors == null || Colors.Count == 0)
                {
                    throw new InvalidOperationException("There are no items in this menu.");
                }
                // If the value is over or equal than the number of items, raise an exception
                else if (value >= Colors.Count)
                {
                    throw new InvalidOperationException($"The index is over {Colors.Count - 1}");
                }

                // Calculate the bounds of the menu
                int lower = firstItem;
                int upper = firstItem + maxItems;

                // Time to set the first item based on the total number of items
                // If the item is between the allowed values, do nothing because we are on the correct first item
                if (value >= lower && value < upper - 1)
                {
                }
                // If the upper bound + 1 equals the new index, increase it by one
                else if (upper == value)
                {
                    firstItem += 1;
                }
                // If the first item minus one equals the value, decrease it by one
                else if (lower - 1 == value)
                {
                    firstItem -= 1;
                }
                // Otherwise, set it somewhere
                else
                {
                    // If the value is under the max items, set it to zero
                    if (value < maxItems)
                    {
                        firstItem = 0;
                    }
                    // Otherwise, set it at the bottom
                    else
                    {
                        firstItem = value - maxItems + 1;
                    }
                }

                // Save the index and update the items
                index = value;
                UpdateItems();
                // Finally, play the switch change sound
                Sound?.PlayFrontend();
            }
        }
        /// <summary>
        /// The Title used for the Panel when <see cref="TitleStyle"/> is set to <see cref="ColorTitleStyle.Simple"/>.
        /// </summary>
        public string Title
        {
            get => simpleTitle;
            set
            {
                simpleTitle = value;
                UpdateTitle();
            }
        }
        /// <summary>
        /// The style of the Panel Title.
        /// </summary>
        public ColorTitleStyle TitleStyle
        {
            get => titleStyle;
            set
            {
                titleStyle = value;
                UpdateTitle();
            }
        }
        /// <summary>
        /// If the count of items should be shown as part of the title.
        /// </summary>
        public bool ShowCount
        {
            get => showCount;
            set
            {
                showCount = value;
                UpdateTitle();
            }
        }
        /// <summary>
        /// THe maximum number of items shown on the screen.
        /// </summary>
        public int MaxItems
        {
            get => maxItems;
            set
            {
                if (value == maxItems)
                {
                    return;
                }
                maxItems = value;
                UpdateItems();
                UpdateTitle();
            }
        }
        /// <summary>
        /// The colors shown on this Panel.
        /// </summary>
        public List<NativeColorData> Colors { get; } = new List<NativeColorData>();
        /// <summary>
        /// The sound played when the item is changed.
        /// </summary>
        public Sound Sound { get; set; } = DefaultSound;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a color panel with no Items or Title.
        /// </summary>
        public NativeColorPanel() : this("")
        {
        }
        /// <summary>
        /// Creates a Panel with a specific Title and set of Colors.
        /// </summary>
        /// <param name="title">The title of the panel.</param>
        /// <param name="colors">The colors of the panel.</param>
        public NativeColorPanel(string title, params NativeColorData[] colors)
        {
            // Set the title of the Panel
            Title = title;
            // Add the colors that we got
            Colors.AddRange(colors);
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Updates the Text of the Title.
        /// </summary>
        private void UpdateTitle()
        {
            string newTitle = "";

            // Add the title based on the correct style
            switch (titleStyle)
            {
                case ColorTitleStyle.Simple:
                    newTitle = Title;
                    break;
                case ColorTitleStyle.ColorName:
                    newTitle = SelectedColor == null ? "" : SelectedColor.Name;
                    break;
            }

            // If we need to add the count of colors
            if (ShowCount)
            {
                // Add a space at the end if required
                if (!newTitle.EndsWith(" "))
                {
                    newTitle += " ";
                }

                // And add the item count
                newTitle += $"({SelectedIndex + 1} of {Colors.Count})";
            }

            // And finally set the new title
            title.Text = newTitle;
        }
        /// <summary>
        /// Updates the position of the Items.
        /// </summary>
        private void UpdateItems()
        {
            // See UpdateItemList() on LemonUI.Menus.NativeMenu to understand this section
            List<NativeColorData> list = new List<NativeColorData>();

            for (int i = 0; i < MaxItems; i++)
            {
                int start = firstItem + i;

                if (start >= Colors.Count)
                {
                    break;
                }

                list.Add(Colors[start]);
            }

            visibleItems = list;

            // This is the difference of size in both sides (cuz symetric)
            const float leftDifference = 16;
            const float rightDifference = 12;
            // Get the width based on the maximum number of items
            float width = (lastWidth - leftDifference - rightDifference) / maxItems;
            // And store the number of items already completed
            int count = 0;

            // Then, start iterating over the colors visible on the screen
            foreach (NativeColorData color in visibleItems)
            {
                // Set the position based on the number of items completed
                color.rectangle.Position = new PointF(lastPosition.X + leftDifference + (width * count), lastPosition.Y + 54);
                // And set the size of it based on the number of items
                color.rectangle.Size = new SizeF(width, 45);
                // Finally, increase the count by one
                count++;
            }

            // If there is a selected color item
            if (SelectedColor != null)
            {
                // Set the position and size of the selection rectangle based on the currently selected color
                ScaledRectangle colorRect = SelectedColor.rectangle;
                const float height = 8;
                selectionRectangle.Position = new PointF(colorRect.Position.X, colorRect.Position.Y - height);
                selectionRectangle.Size = new SizeF(colorRect.Size.Width, height);
            }

            // Finally, update the text of the title
            UpdateTitle();
        }
        /// <summary>
        /// Recalculates the Color panel with the last known Position and Width.
        /// </summary>
        private void Recalculate() => Recalculate(lastPosition, lastWidth);

        #endregion

        #region Public Functions

        /// <summary>
        /// Moves to the Previous Color.
        /// </summary>
        public void Previous()
        {
            // If there are no items, return
            if (Colors.Count == 0)
            {
                return;
            }

            // If we are on the first item, go back to the last one
            if (SelectedIndex == 0)
            {
                SelectedIndex = Colors.Count - 1;
            }
            // Otherwise, reduce it by one
            else
            {
                SelectedIndex -= 1;
            }
        }
        /// <summary>
        /// Moves to the Next Color.
        /// </summary>
        public void Next()
        {
            // If there are no items, return
            if (Colors.Count == 0)
            {
                return;
            }

            // If we are on the last item, go back to the first one
            if (Colors.Count - 1 == SelectedIndex)
            {
                SelectedIndex = 0;
            }
            // Otherwise, increase it by one
            else
            {
                SelectedIndex += 1;
            }
        }
        /// <summary>
        /// Adds a color to the Panel.
        /// </summary>
        /// <param name="color">The color to add.</param>
        public void Add(NativeColorData color)
        {
            if (Colors.Contains(color))
            {
                throw new ArgumentException("Color is already part of the Panel.", nameof(color));
            }
            Colors.Add(color);
            Recalculate();
        }
        /// <summary>
        /// Removes a color from the panel.
        /// </summary>
        /// <param name="color">The color to remove.</param>
        public void Remove(NativeColorData color)
        {
            // Remove it if there
            // If not, ignore it
            Colors.Remove(color);
            // If the index is higher or equal than the max number of items
            // Set the max - 1
            if (SelectedIndex >= Colors.Count)
            {
                SelectedIndex = Colors.Count - 1;
            }
            else
            {
                UpdateItems();
            }
        }
        /// <summary>
        /// Removes all of the 
        /// </summary>
        /// <param name="func"></param>
        public void Remove(Func<NativeColorData, bool> func)
        {
            foreach (NativeColorData color in new List<NativeColorData>(Colors))
            {
                if (func(color))
                {
                    Colors.Remove(color);
                }
            }
            Recalculate();
        }
        /// <summary>
        /// Removes all of the colors from the Panel.
        /// </summary>
        public void Clear()
        {
            Colors.Clear();
            Recalculate();
        }
        /// <summary>
        /// Checks if the Color Data is present on this Panel.
        /// </summary>
        /// <param name="color">The Color Data to check.</param>
        public void Contains(NativeColorData color) => Colors.Contains(color);
        /// <summary>
        /// Recalculates the position of the Color Panel.
        /// </summary>
        /// <param name="position">The position of the panel.</param>
        /// <param name="width">The width of the menu.</param>
        public override void Recalculate(PointF position, float width)
        {
            // Save the last position and width
            lastPosition = position;
            lastWidth = width;

            // Set the position and size of the Background
            Background.Position = position;
            Background.Size = new SizeF(width, 111);
            // And then set the position of the text
            title.Position = new PointF(position.X + 218, position.Y + 10);

            // Finally, update the list of items
            UpdateItems();
        }
        /// <summary>
        /// Draws the Color Panel.
        /// </summary>
        public override void Process()
        {
            // If the user pressed one of the keys, move to the left or right
            if (Controls.IsJustPressed(Control.FrontendLt))
            {
                Previous();
            }
            else if (Controls.IsJustPressed(Control.FrontendRt))
            {
                Next();
            }

            // Draw the items
            base.Process();
            title.Draw();
            foreach (NativeColorData color in visibleItems)
            {
                color.rectangle.Draw();
            }
            if (Colors.Count != 0)
            {
                selectionRectangle.Draw();
            }
        }

        #endregion
    }
}
