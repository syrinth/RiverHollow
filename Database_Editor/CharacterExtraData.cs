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
        int _iIndex = 0;
        Dictionary<string, string> _diData;
        public Dictionary<string, string> Data => _diData;
        public FormCharExtraData(string value, Dictionary<string, string> diCharacterData)
        {
            InitializeComponent();

            _diData = diCharacterData;
            this.Text = value;

            int index = 0;
            dgvCharExtraData.Rows.Clear();
            foreach (KeyValuePair<string, string> kvp in _diData)
            {
                dgvCharExtraData.Rows.Add();
                DataGridViewRow row = dgvCharExtraData.Rows[index++];

                row.Cells["colCharExtraID"].Value = kvp.Key;
            }

            SelectRow(dgvCharExtraData, _iIndex);
            dgvCharExtraData.Focus();

            LoadDataInfo(0);
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
            tbCharExtraDataInfo.Text = _diData[keyValue];
        }

        private void dgvCharExtraData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                if (_diData.Count == _iIndex)
                {
                    dgvCharExtraData.Rows.RemoveAt(_iIndex--);
                }
                _iIndex = e.RowIndex;
                LoadDataInfo(_iIndex);
            }
        }

        private void btnSaveCharacter_Click(object sender, EventArgs e)
        {
            _diData[tbCharExtraDataName.Text] = tbCharExtraDataInfo.Text;
            Close();
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
