using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Diagnostics;

namespace DataGirdViewWithFilterDemo
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    internal class DataGridViewFilterableHeaderCell : DataGridViewColumnHeaderCell
    {
        ComboBoxState currentState = ComboBoxState.Normal;
        Point cellLocation;
        Rectangle buttonRect;
        private Rectangle _dropDownButtonBounds = Rectangle.Empty;
        private int _dropDownButtonPaddingOffset = 0;
        private bool _filtered;

        public DataGridViewFilterableHeaderCell()
        {
        }

        public EventHandler<ColumnFilterableClickedEventArg> FilterButtonClicked { get; internal set; }
        protected Rectangle DropDownButtonBounds
        {
            get
            {
                if (!IsProgrammaticSorting)
                    return Rectangle.Empty;
                    
                SetDropDownButtonBounds();

                return _dropDownButtonBounds;
            }
        }
        public int DropDownButtonPaddingOffset { get => _dropDownButtonPaddingOffset; set => _dropDownButtonPaddingOffset = value; }
        private bool IsProgrammaticSorting
        {
            get { return (OwningColumn == null) || (OwningColumn.SortMode == DataGridViewColumnSortMode.Programmatic); }
        }


        protected override void OnDataGridViewChanged()
        {
            SetDropDownButtonBounds();

            base.OnDataGridViewChanged();
        }

        protected override void OnMouseDown(DataGridViewCellMouseEventArgs e)
        {
            if (IsMouseOverButton(e.Location))
                currentState = ComboBoxState.Pressed;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(DataGridViewCellMouseEventArgs e)
        {
            if (IsMouseOverButton(e.Location))
            {
                currentState = ComboBoxState.Normal;
                OnFilterButtonClicked();
            }
            else
            {
                base.OnMouseUp(e);
            }
        }

// /*
        protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates dataGridViewElementState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
        {
            base.Paint(graphics, clipBounds, cellBounds, rowIndex, dataGridViewElementState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
            //Debug.WriteLine($">---<Paint - rowIndex: {rowIndex}>---<");
            int width = DropDownButtonPaddingOffset;
            //int width = 20; // 20 px
            buttonRect = new Rectangle(cellBounds.X + cellBounds.Width - width, cellBounds.Y, width, cellBounds.Height);
            Rectangle buttonBounds = DropDownButtonBounds;
            cellLocation = cellBounds.Location;
            //ComboBoxRenderer.DrawDropDownButton(graphics, buttonRect, currentState);
            ComboBoxRenderer.DrawDropDownButton(graphics, buttonBounds, currentState);
        }
// */
        #region Painting
        /*        
                       /// <summary>
                       /// Paints the column header cell, including the drop-down button. 
                       /// </summary>
                       /// <param name="graphics">The Graphics used to paint the DataGridViewCell.</param>
                       /// <param name="clipBounds">A Rectangle that represents the area of the DataGridView that needs to be repainted.</param>
                       /// <param name="cellBounds">A Rectangle that contains the bounds of the DataGridViewCell that is being painted.</param>
                       /// <param name="rowIndex">The row index of the cell that is being painted.</param>
                       /// <param name="cellState">A bitwise combination of DataGridViewElementStates values that specifies the state of the cell.</param>
                       /// <param name="value">The data of the DataGridViewCell that is being painted.</param>
                       /// <param name="formattedValue">The formatted data of the DataGridViewCell that is being painted.</param>
                       /// <param name="errorText">An error message that is associated with the cell.</param>
                       /// <param name="cellStyle">A DataGridViewCellStyle that contains formatting and style information about the cell.</param>
                       /// <param name="advancedBorderStyle">A DataGridViewAdvancedBorderStyle that contains border styles for the cell that is being painted.</param>
                       /// <param name="paintParts">A bitwise combination of the DataGridViewPaintParts values that specifies which parts of the cell need to be painted.</param>
                       protected override void Paint(Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
                       {
                           base.Paint(graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);

                           // Continue only if the drop down is to be visible and ContentBackground is part of the paint request. 
                           if (!this.IsProgrammaticSorting || (paintParts & DataGridViewPaintParts.ContentBackground) == 0)
                           {
                               //return;
                           }

                           Rectangle buttonBounds = DropDownButtonBounds;
                           if (buttonBounds.Width > 0 && buttonBounds.Height > 0) // make sure there's something to draw...
                           {
                               bool DropDownEnabled = true;
                               bool IsDropDownShowing = true;
                               // Paint the button manually or using visual styles if visual styles 
                               // are enabled, using the correct state depending on whether the 
                               // filter list is showing and whether there is a filter in effect 
                               // for the current column. 
                               if (Application.RenderWithVisualStyles)
                               {
                                   ComboBoxState state = ComboBoxState.Normal;
                                   if (!DropDownEnabled)
                                       state = ComboBoxState.Disabled;
                                   else if (IsDropDownShowing)
                                       state = ComboBoxState.Pressed;
                                   ComboBoxRenderer.DrawDropDownButton(graphics, buttonBounds, state);
                               }
                               else
                               {
                                   int pressedOffset = 0;
                                   PushButtonState state = PushButtonState.Normal;
                                   if (!DropDownEnabled)
                                   {
                                       state = PushButtonState.Disabled;
                                   }
                                   else if (IsDropDownShowing)
                                   {
                                       state = PushButtonState.Pressed;
                                       pressedOffset = 1;
                                   }
                                   ButtonRenderer.DrawButton(graphics, buttonBounds, state);
                                   graphics.FillPolygon(DropDownEnabled ? SystemBrushes.ControlText : SystemBrushes.InactiveCaption,
                                                        new Point[]
                                                           {
                                                               new Point(
                                                                   buttonBounds.Width/2 +
                                                                   buttonBounds.Left - 1 + pressedOffset,
                                                                   buttonBounds.Height*3/4 +
                                                                   buttonBounds.Top - 1 + pressedOffset),
                                                               new Point(
                                                                   buttonBounds.Width/4 +
                                                                   buttonBounds.Left + pressedOffset,
                                                                   buttonBounds.Height/2 +
                                                                   buttonBounds.Top - 1 + pressedOffset),
                                                               new Point(
                                                                   buttonBounds.Width*3/4 +
                                                                   buttonBounds.Left - 1 + pressedOffset,
                                                                   buttonBounds.Height/2 +
                                                                   buttonBounds.Top - 1 + pressedOffset)
                                                           });
                               }

                               // and then paint a filtering and/or sorting glyph
                               if (_filtered)
                               {
                                   Bitmap glyph = Properties.Resources.FilterHeaderCellGlyph;
                                   Rectangle cbb = this.DataGridView.GetCellDisplayRectangle(this.ColumnIndex, -1, false);
                                   graphics.DrawImage(glyph, new Rectangle(buttonBounds.Left - glyph.Width - 3, (cbb.Height - glyph.Height) / 2, glyph.Width, glyph.Height));
                               }
                           }
               }
        */
        #endregion

        private bool IsMouseOverButton(Point e)
        {
            Point p = new Point(e.X + cellLocation.X, e.Y + cellLocation.Y);
            if (p.X >= buttonRect.X && p.X <= buttonRect.X + buttonRect.Width &&
                p.Y >= buttonRect.Y && p.Y <= buttonRect.Y + buttonRect.Height)
            {
                return true;
            }
            return false;
        }
        protected virtual void OnFilterButtonClicked()
        {
            FilterButtonClicked?.Invoke(this, new ColumnFilterableClickedEventArg(ColumnIndex, buttonRect));
        }

        public override object Clone()
        {
            return base.Clone();
        }

        private string GetDebuggerDisplay()
        {
            return $"Row {RowIndex}: {Value}";
            //return ToString();
        }

        /// <summary>
        /// Sets the position and size of _dropDownButtonBounds based on the current 
        /// cell bounds and the preferred cell height for a single line of header text. 
        /// </summary>
        private void SetDropDownButtonBounds()
        {
            //Debug.Write($">---<{nameof(SetDropDownButtonBounds)}");
            if (DataGridView == null)
            {
                _dropDownButtonBounds = Rectangle.Empty;
                _dropDownButtonPaddingOffset = 0;
                //Debug.WriteLine(">---<");
                return;
            }
            //Debug.WriteLine($" - CurrentCell: {DataGridView.CurrentCellAddress}>---<");

            // Retrieve the cell display rectangle, which is used to set the position of the drop-down button
            Rectangle cellBounds = DataGridView.GetCellDisplayRectangle(ColumnIndex, -1, false);
            if (cellBounds == Rectangle.Empty)
            {
                _dropDownButtonBounds = Rectangle.Empty;
                _dropDownButtonPaddingOffset = 0;
                return;
            }

            // Initialize a variable to store the button edge length, setting its initial value based on the font height
            int buttonEdgeLength = InheritedStyle.Font.Height + 5;

            // Calculate the height of the cell borders and padding
            Rectangle borderRect = BorderWidths(DataGridView.AdjustColumnHeaderBorderStyle(DataGridView.AdvancedColumnHeadersBorderStyle, new DataGridViewAdvancedBorderStyle(), false, false));
            int borderAndPaddingHeight = /*2 +*/ borderRect.Top + borderRect.Height + InheritedStyle.Padding.Vertical;
            //bool visualStylesEnabled = Application.RenderWithVisualStyles && DataGridView.EnableHeadersVisualStyles;
            //if (visualStylesEnabled)
            //{
            //borderAndPaddingHeight += 3;
            //}

            // Constrain the button edge length to the height of the column headers minus the border and padding height
            //if (buttonEdgeLength > DataGridView.ColumnHeadersHeight - borderAndPaddingHeight)
            //{
            //    buttonEdgeLength = DataGridView.ColumnHeadersHeight - borderAndPaddingHeight;
            //}

            // Constrain the button edge length to the width of the cell minus three
            if (buttonEdgeLength > cellBounds.Width/* - 3*/)
            {
                buttonEdgeLength = cellBounds.Width /*- 3*/;
            }

            // Calculate the location of the drop-down button, with adjustments based on whether visual styles are enabled
            int topOffset = 0;// visualStylesEnabled ? 4 : 1;
            int top = cellBounds.Bottom - buttonEdgeLength - topOffset;
            int leftOffset = 0;// visualStylesEnabled ? 3 : 1;
            int left = 0;
            if (DataGridView.RightToLeft == RightToLeft.No)
            {
                left = cellBounds.Right - buttonEdgeLength - leftOffset;
            }
            else
            {
                left = cellBounds.Left + leftOffset;
            }

            // Set the bounds using the calculated values, and adjust the cell padding accordingly
            _dropDownButtonBounds = new Rectangle(left, top, buttonEdgeLength, buttonEdgeLength);
            AdjustPadding(buttonEdgeLength + leftOffset);
        }

        /// <summary>
        /// Adjusts the cell padding to widen the header by the drop-down button width.
        /// </summary>
        /// <param name="newDropDownButtonPaddingOffset">The new drop-down button width.</param>
        private void AdjustPadding(int newDropDownButtonPaddingOffset)
        {
            // Determine the difference between the new and current padding adjustment.
            int widthChange = newDropDownButtonPaddingOffset - DropDownButtonPaddingOffset;

            // If the padding needs to change, store the new value and make the change.
            if (widthChange != 0)
            {
                // Store the offset for the drop-down button separately from the padding in case the client needs additional padding.
                _dropDownButtonPaddingOffset = newDropDownButtonPaddingOffset;

                // Create a new Padding using the adjustment amount, then add it to the cell's existing Style.Padding property value. 
                Padding dropDownPadding = new Padding(0, 0, widthChange, 0);
                Style.Padding = Padding.Add(InheritedStyle.Padding, dropDownPadding);
            }
        }

    }
}