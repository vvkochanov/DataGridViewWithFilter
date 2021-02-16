using System;
using System.Drawing;

namespace DataGirdViewWithFilterDemo
{
    public class ColumnFilterableClickedEventArg : EventArgs
    {
        public int ColumnIndex { get; private set; }
        public Rectangle ButtonRectangle { get; private set; }
        public ColumnFilterableClickedEventArg(int colIndex, Rectangle btnRect)
        {
            this.ColumnIndex = colIndex;
            this.ButtonRectangle = btnRect;
        }
    }
}