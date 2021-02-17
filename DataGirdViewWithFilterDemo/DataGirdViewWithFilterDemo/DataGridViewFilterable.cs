using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DataGirdViewWithFilterDemo
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public partial class DataGridViewFilterable : DataGridView
    {
        List<FilterStatus> Filter = new List<FilterStatus>();
        TextBox textBoxCtrl = new TextBox();
        DateTimePicker DateTimeCtrl = new DateTimePicker();
        CheckedListBox CheckCtrl = new CheckedListBox();
        Button ApplyButtonCtrl = new Button();
        Button ClearFilterCtrl = new Button();
        ToolStripDropDown popup = new ToolStripDropDown();

        string StrFilter = "";
        string ButtonCtrlText = "Apply";
        string ClearFilterCtrlText = "Clear filters";
        string CheckCtrlAllText = "<All>";
        string SpaceText = "<Space>";

        // Текущий индекс ячейки
        private int ColumnIndex { get; set; }

        public DataGridViewFilterable()
        {
            InitializeComponent();
        }

        protected override void OnColumnAdded(DataGridViewColumnEventArgs e)
        {
            var header = new DataGridViewFilterableHeaderCell();
            header.FilterButtonClicked += new EventHandler<ColumnFilterableClickedEventArg>(header_FilterButtonClicked);
            e.Column.HeaderCell = header;
            e.Column.SortMode = DataGridViewColumnSortMode.Programmatic;

            base.OnColumnAdded(e);
        }

        public override void Sort(DataGridViewColumn dataGridViewColumn, ListSortDirection direction)
        {
            base.Sort(dataGridViewColumn, direction);
        }

        private void header_FilterButtonClicked(object sender, ColumnFilterableClickedEventArg e)
        {
            
            int widthTool = GetWhithColumn(e.ColumnIndex) + 50;
            if (widthTool < 130) widthTool = 130;

            ColumnIndex = e.ColumnIndex;

            textBoxCtrl.Clear();
            CheckCtrl.Items.Clear();

            textBoxCtrl.Size = new Size(widthTool, 30);
            textBoxCtrl.TextChanged -= textBoxCtrl_TextChanged;
            textBoxCtrl.TextChanged += textBoxCtrl_TextChanged;

            DateTimeCtrl.Size = new Size(widthTool, 30);
            DateTimeCtrl.Format = DateTimePickerFormat.Custom;
            DateTimeCtrl.CustomFormat = "dd.MM.yyyy";
            DateTimeCtrl.TextChanged -= DateTimeCtrl_TextChanged;
            DateTimeCtrl.TextChanged += DateTimeCtrl_TextChanged;

            CheckCtrl.ItemCheck -= CheckCtrl_ItemCheck;
            CheckCtrl.ItemCheck += CheckCtrl_ItemCheck;
            CheckCtrl.CheckOnClick = true;

            GetChkFilter();

            CheckCtrl.MaximumSize = new Size(widthTool, GetHeightTable() - 120);
            CheckCtrl.Size = new Size(widthTool, (CheckCtrl.Items.Count + 1) * 18);

            ApplyButtonCtrl.Text = ButtonCtrlText;
            ApplyButtonCtrl.Size = new Size(widthTool, 30);
            ApplyButtonCtrl.Click -= ApplyButtonCtrl_Click;
            ApplyButtonCtrl.Click += ApplyButtonCtrl_Click;

            ClearFilterCtrl.Text = ClearFilterCtrlText;
            ClearFilterCtrl.Size = new Size(widthTool, 30);
            ClearFilterCtrl.Click -= ClearFilterCtrl_Click;
            ClearFilterCtrl.Click += ClearFilterCtrl_Click;

            popup.Items.Clear();
            popup.AutoSize = true;
            popup.Margin = Padding.Empty;
            popup.Padding = Padding.Empty;

            ToolStripControlHost hostForTextBox = new ToolStripControlHost(textBoxCtrl)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false,
                Size = textBoxCtrl.Size
            };

            ToolStripControlHost hostForCheckCtrl = new ToolStripControlHost(CheckCtrl)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false,
                Size = CheckCtrl.Size
            };

            ToolStripControlHost hostForApplyButton = new ToolStripControlHost(ApplyButtonCtrl)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false,
                Size = ApplyButtonCtrl.Size
            };

            ToolStripControlHost hostForClearFilterButton = new ToolStripControlHost(ClearFilterCtrl)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false,
                Size = ClearFilterCtrl.Size
            };

            ToolStripControlHost hostForDateTimeCtrl = new ToolStripControlHost(DateTimeCtrl)
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                AutoSize = false,
                Size = DateTimeCtrl.Size
            };

            switch (Columns[ColumnIndex].ValueType.ToString())
            {
                case "System.DateTime":
                    popup.Items.Add(hostForDateTimeCtrl);
                    break;
                default:
                    popup.Items.Add(hostForTextBox);
                    break;
            }
            popup.Items.Add(hostForCheckCtrl);
            popup.Items.Add(hostForApplyButton);
            popup.Items.Add(hostForClearFilterButton);

            popup.Show(this, e.ButtonRectangle.X, e.ButtonRectangle.Bottom);
            hostForCheckCtrl.Focus();
        }

        // Выбор всех
        void CheckCtrl_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (e.Index == 0)
            {
                if (e.NewValue == CheckState.Checked)
                {
                    for (int i = 1; i < CheckCtrl.Items.Count; i++)
                        CheckCtrl.SetItemChecked(i, true);
                }
                else
                {
                    for (int i = 1; i < CheckCtrl.Items.Count; i++)
                        CheckCtrl.SetItemChecked(i, false);
                }
            }
        }

        // Очистить фильтры
        void ClearFilterCtrl_Click(object sender, EventArgs e)
        {
            Filter.Clear();
            StrFilter = "";
            ApllyFilter();
            popup.Close();
        }

        // Событие при изменении текста в TextBox
        void textBoxCtrl_TextChanged(object sender, EventArgs e)
        {
            string filterString = $"convert([{Columns[ColumnIndex].Name}], 'System.String') LIKE '%{textBoxCtrl.Text}%'";
            (DataSource as DataTable).DefaultView.RowFilter = filterString;
        }
        void DateTimeCtrl_TextChanged(object sender, EventArgs e)
        {
            (DataSource as DataTable).DefaultView.RowFilter = $"convert([{Columns[ColumnIndex].Name}], 'System.String') LIKE '%{DateTimeCtrl.Text}%'";
        }

        // Событие кнопки применить
        void ApplyButtonCtrl_Click(object sender, EventArgs e)
        {
            StrFilter = "";
            SaveChkFilter();
            ApllyFilter();
            popup.Close();
        }

        // Получаем данные из выбранной колонки 
        private List<string> GetDataColumns(int e)
        {
            List<string> ValueCellList = new List<string>();
            string Value;

            // Посик данных в столбце, исключая повторения
            foreach (DataGridViewRow row in Rows)
            {
                Value = row.Cells[e].Value.ToString();
                if (Value == "") Value = SpaceText;

                if (!ValueCellList.Contains(Value))
                    ValueCellList.Add(Value);
            }
            return ValueCellList;
        }

        // Получаем высоту таблицы
        private int GetHeightTable()
        {
            return Height;
        }

        // Получаем ширину выбранной колонки
        private int GetWhithColumn(int e)
        {
            return Columns[e].Width;
        }

        // Запомнить чекбоксы фильтра
        private void SaveChkFilter()
        {
            string col = Columns[ColumnIndex].Name;
            string itemChk;
            bool statChk;

            Filter.RemoveAll(x => x.columnName == col);

            for (int i = 1; i < CheckCtrl.Items.Count; i++)
            {
                itemChk = CheckCtrl.Items[i].ToString();
                statChk = CheckCtrl.GetItemChecked(i);
                Filter.Add(new FilterStatus() { columnName = col, valueString = itemChk, check = statChk });
            }
        }

        // Загрузить чекбоксы
        private void GetChkFilter()
        {
            List<FilterStatus> CheckList = new List<FilterStatus>();
            List<FilterStatus> CheckListSort = new List<FilterStatus>();

            // Посик сохранённых данных
            foreach (FilterStatus val in Filter)
            {
                if (Columns[ColumnIndex].Name == val.columnName)
                {
                    if (val.valueString == "") val.valueString = SpaceText;
                    CheckList.Add(new FilterStatus() { columnName = "", valueString = val.valueString, check = val.check });
                }
            }

            // Поиск данных в таблице
            foreach (string ValueCell in GetDataColumns(ColumnIndex))
            {
                int index = CheckList.FindIndex(item => item.valueString == ValueCell);
                if (index == -1)
                {
                    CheckList.Add(new FilterStatus { valueString = ValueCell, check = true });
                }
            }

            CheckCtrl.Items.Add(CheckCtrlAllText, CheckState.Indeterminate);
            // Сортировка
            switch (Columns[ColumnIndex].ValueType.ToString())
            {
                case "System.Int32":
                    CheckListSort = CheckList.OrderBy(x => Int32.Parse(x.valueString)).ToList();
                    foreach (FilterStatus val in CheckListSort)
                    {
                        if (val.check == true)
                            CheckCtrl.Items.Add(val.valueString, CheckState.Checked);
                        else
                            CheckCtrl.Items.Add(val.valueString, CheckState.Unchecked);
                    }
                    break;
                case "System.DateTime":
                    CheckListSort = CheckList.OrderBy(x => DateTime.Parse(x.valueString)).ToList();
                    foreach (FilterStatus val in CheckListSort)
                    {
                        if (val.check == true)
                            CheckCtrl.Items.Add(DateTime.Parse(val.valueString).ToString("dd.MM.yyyy"), CheckState.Checked);
                        else
                            CheckCtrl.Items.Add(DateTime.Parse(val.valueString).ToString("dd.MM.yyyy"), CheckState.Unchecked);
                    }
                    break;
                default:
                    CheckListSort = CheckList.OrderBy(x => x.valueString).ToList();
                    foreach (FilterStatus val in CheckListSort)
                    {
                        if (val.check == true)
                            CheckCtrl.Items.Add(val.valueString, CheckState.Checked);
                        else
                            CheckCtrl.Items.Add(val.valueString, CheckState.Unchecked);
                    }
                    break;
            }
        }


        // Применить фильтр
        private void ApllyFilter()
        {
            foreach (FilterStatus val in Filter)
            {
                if (val.valueString == SpaceText) val.valueString = "";
                if (val.check == false)
                {
                    // Исключение если bool              
                    string valueFilter = "'" + val.valueString + "' ";
                    if (valueFilter == "True")
                    {
                        valueFilter = "1";
                    }
                    if (valueFilter == "False")
                    {
                        valueFilter = "0";
                    }


                    if (StrFilter.Length == 0)
                    {
                        StrFilter = StrFilter + ("[" + val.columnName + "] <> " + valueFilter);
                    }
                    else
                    {
                        StrFilter = StrFilter + (" AND [" + val.columnName + "] <> " + valueFilter);
                    }
                }
            }
            (DataSource as DataTable).DefaultView.RowFilter = StrFilter;
        }

        protected override void OnColumnHeaderMouseClick(DataGridViewCellMouseEventArgs e)
        {
            //if (e.X < popup.Left)
            //{
            //}
            //Debug.WriteLine(">---<OnColumnHeaderMouseClick>---<");
            base.OnColumnHeaderMouseClick(e);
        }

        private string GetDebuggerDisplay()
        {
            return $"[{Name}: Columns - {{{Columns.Count}}}, Rows - {{{RowCount}}}]";
        }
    }
}
