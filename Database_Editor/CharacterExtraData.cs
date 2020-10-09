using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Database_Editor
{
    public partial class FormCharExtraData : Form
    {
        enum DataMode { Dialogue, Schedule };
        DataMode _eDataMode;
        int _iIndex = 0;
        Dictionary<string, string> _diStringData;
        public Dictionary<string, string> StringData => _diStringData;

        Dictionary<string, List<string>> _diListData;
        public Dictionary<string, List<string>> ListData => _diListData;
        public FormCharExtraData(string value, Dictionary<string, string> diCharacterData)
        {
            InitializeComponent();

            _diStringData = diCharacterData;
            this.Text = value;
            tbCharExtraDataInfo.Visible = true;
            dgvEditTags.Visible = false;
            LoadDataGridViewString();

            dgvCharExtraData.Focus();
            _eDataMode = DataMode.Dialogue;
        }

        public FormCharExtraData(string value, Dictionary<string, List<string>> diCharacterData)
        {
            InitializeComponent();

            _diListData = diCharacterData;
            this.Text = value;
            tbCharExtraDataInfo.Visible = false;
            dgvEditTags.Visible = true;
            LoadDataGridViewList();

            dgvCharExtraData.Focus();
            _eDataMode = DataMode.Schedule;
        }

        private void LoadDataGridViewString()
        {
            int index = 0;
            dgvCharExtraData.Rows.Clear();
            foreach (KeyValuePair<string, string> kvp in _diStringData)
            {
                dgvCharExtraData.Rows.Add();
                DataGridViewRow row = dgvCharExtraData.Rows[index++];

                row.Cells["colCharExtraID"].Value = kvp.Key;
            }

            SelectRow(dgvCharExtraData, _iIndex);
            LoadDataInfo(_iIndex);
        }
        private void LoadDataGridViewList()
        {
            int index = 0;
            dgvCharExtraData.Rows.Clear();
            foreach (KeyValuePair<string, List<string>> kvp in _diListData)
            {
                dgvCharExtraData.Rows.Add();
                DataGridViewRow row = dgvCharExtraData.Rows[index++];

                row.Cells["colCharExtraID"].Value = kvp.Key;
            }

            SelectRow(dgvCharExtraData, _iIndex);
            LoadDataInfoList(_iIndex);
        }

        private void SelectRow(DataGridView dg, int id)
        {
            dg.Rows[id].Selected = true;
            dg.CurrentCell = dg.Rows[id].Cells[0];
        }

        private void LoadDataInfo(int index)
        {
            string keyValue = dgvCharExtraData.Rows[index].Cells[0].Value.ToString();
            tbCharExtraDataName.Text = keyValue;
            tbCharExtraDataInfo.Text = _diStringData[keyValue];
        }
        private void LoadDataInfoList(int index)
        {
            string keyValue = dgvCharExtraData.Rows[index].Cells[0].Value.ToString();
            tbCharExtraDataName.Text = keyValue;

            dgvEditTags.Rows.Clear();
            foreach (string s in _diListData[keyValue])
            {
                dgvEditTags.Rows.Add(s);
            }
        }

        private void dgvCharExtraData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                if (_eDataMode == DataMode.Dialogue)
                {
                    _diStringData.Remove(_diStringData.ElementAt(_iIndex).Key);
                    _diStringData[tbCharExtraDataName.Text] = tbCharExtraDataInfo.Text;
                    DataGridViewRow row = dgvCharExtraData.Rows[_iIndex];
                    row.Cells["colCharExtraID"].Value = tbCharExtraDataName.Text;

                    _iIndex = e.RowIndex;
                    LoadDataInfo(_iIndex);
                }
                else if (_eDataMode == DataMode.Schedule)
                {
                    _diListData.Remove(_diListData.ElementAt(_iIndex).Key);

                    List<string> listInfo = new List<string>();
                    foreach(DataGridViewRow r in dgvEditTags.Rows)
                    {
                        if (r.Cells[0].Value != null)
                        {
                            listInfo.Add(r.Cells[0].Value.ToString());
                        }
                    }
                    _diListData[tbCharExtraDataName.Text] = listInfo;
                    DataGridViewRow row = dgvCharExtraData.Rows[_iIndex];
                    row.Cells["colCharExtraID"].Value = tbCharExtraDataName.Text;

                    _iIndex = e.RowIndex;
                    LoadDataInfoList(_iIndex);
                }
            }
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            _iIndex = dgvCharExtraData.Rows.Count;
            dgvCharExtraData.Rows.Add();
            SelectRow(dgvCharExtraData, _iIndex);

            DataGridViewRow row = dgvCharExtraData.Rows[_iIndex];
            row.Cells["colCharExtraID"].Value = "New";

            tbCharExtraDataName.Text = "";
            tbCharExtraDataInfo.Text = "";

            tbCharExtraDataName.Focus();
        }
    }
}
