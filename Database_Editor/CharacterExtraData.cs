﻿using System;
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

            SelectRow(dgvCharExtraData, 0);
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
                LoadDataInfo(e.RowIndex);
            }
        }

        private void btnSaveCharacter_Click(object sender, EventArgs e)
        {
            _diData[tbCharExtraDataName.Text] = tbCharExtraDataInfo.Text;
            Close();
        }
    }
}
