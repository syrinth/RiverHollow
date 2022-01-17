using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using static Database_Editor.FrmDBEditor;

namespace Database_Editor
{
    public partial class FormCharExtraData : Form
    {
        enum DataMode { Dialogue, Schedule };
        DataMode _eDataMode;
        int _iIndex = 0;
        List<XMLData> _liStringData;
        public List<XMLData> StringData => _liStringData;

        Dictionary<string, List<string>> _diListData;
        public Dictionary<string, List<string>> ListData => _diListData;
        public FormCharExtraData(string value, List<XMLData> diCharacterData)
        {
            InitializeComponent();

            _liStringData = diCharacterData;
            this.Text = value;
            LoadDataGridViewString();

            dgvCharExtraData.Focus();
            _eDataMode = DataMode.Dialogue;
        }

        public FormCharExtraData(string value, Dictionary<string, List<string>> diCharacterData)
        {
            InitializeComponent();

            _diListData = diCharacterData;
            this.Text = value;
            LoadDataGridViewList();

            dgvCharExtraData.Focus();
            _eDataMode = DataMode.Schedule;
        }

        private void LoadDataGridViewString()
        {
            int index = 0;
            dgvCharExtraData.Rows.Clear();
            foreach (XMLData value in _liStringData)
            {
                dgvCharExtraData.Rows.Add();
                DataGridViewRow row = dgvCharExtraData.Rows[index++];

                row.Cells["colCharExtraID"].Value = value.ID;
                row.Cells["colCharExtraName"].Value = value.Name;
            }

            SelectRow(dgvCharExtraData, _iIndex);
            LoadDataInfo(_liStringData[_iIndex]);
        }
        private void LoadDataGridViewList()
        {
            int index = 0;
            dgvCharExtraData.Rows.Clear();
            foreach (KeyValuePair<string, List<string>> kvp in _diListData)
            {
                dgvCharExtraData.Rows.Add();
                DataGridViewRow row = dgvCharExtraData.Rows[index];

                row.Cells["colCharExtraID"].Value = index++;
                row.Cells["colCharExtraName"].Value = kvp.Key;
            }

            SelectRow(dgvCharExtraData, _iIndex);
            LoadDataInfoList(_iIndex);
        }

        private void SelectRow(DataGridView dg, int id)
        {
            dg.Rows[id].Selected = true;
            dg.CurrentCell = dg.Rows[id].Cells[0];
        }

        private void LoadDataInfo(XMLData data)
        {
            tbCharExtraDataName.Text = data.Name;

            dgvExtraTags.Rows.Clear();

            string[] tags = data.GetTagsString().Split(new char[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in tags)
            {
                if (!s.StartsWith("Type") && !s.StartsWith("Name"))
                {
                    string[] split = s.Split(':');
                    if (s.StartsWith("Text") && split.Length > 1) { tbCharExtraDataInfo.Text = split[1]; }
                    else { dgvExtraTags.Rows.Add(s); }
                }
            }
        }
        private void LoadDataInfoList(int index)
        {
            string keyValue = dgvCharExtraData.Rows[index].Cells[1].Value.ToString();
            tbCharExtraDataName.Text = keyValue;

            dgvExtraTags.Rows.Clear();
            foreach (string s in _diListData[keyValue])
            {
                dgvExtraTags.Rows.Add(s);
            }
        }

        private void dgvCharExtraData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > -1)
            {
                if (_eDataMode == DataMode.Dialogue)
                {
                    SaveDictionaryData();

                    _iIndex = e.RowIndex;
                    LoadDataInfo(_liStringData[_iIndex]);
                }
                else if (_eDataMode == DataMode.Schedule)
                {
                    SaveListData();

                    _iIndex = e.RowIndex;
                    LoadDataInfoList(_iIndex);
                }
            }
        }

        private void btnAddNew_Click(object sender, EventArgs e)
        {
            Save();

            _iIndex = dgvCharExtraData.Rows.Count;
            dgvCharExtraData.Rows.Add();
            SelectRow(dgvCharExtraData, _iIndex);

            string newID = (_iIndex).ToString();
            DataGridViewRow row = dgvCharExtraData.Rows[_iIndex];
            row.Cells["colCharExtraID"].Value = newID;

            tbCharExtraDataName.Text = newID;
            tbCharExtraDataInfo.Text = "";

            tbCharExtraDataName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void FormCharExtraData_FormClosing(object sender, FormClosingEventArgs e)
        {
            Save();
        }

        private void Save()
        {
            if (_eDataMode == DataMode.Dialogue)
            {
                SaveDictionaryData();
            }
            else if (_eDataMode == DataMode.Schedule)
            {
                SaveListData();
            }
        }
        private void SaveDictionaryData()
        {
            DataGridViewRow row = dgvCharExtraData.Rows[_iIndex];

            XMLData data = null;
            if (_iIndex == _liStringData.Count)
            {
                data = new XMLData(_iIndex.ToString(), new Dictionary<string, string>(), FrmDBEditor.TEXTFILE_REF_TAGS, "", XMLTypeEnum.TextFile);
                _liStringData.Add(data);
            }
            else { data = _liStringData[_iIndex]; }

            data.ClearTagInfo();
            data.SetTextData(tbCharExtraDataName.Text);
            data.SetTagInfo("Name", tbCharExtraDataName.Text);
            data.SetTagInfo("Text", tbCharExtraDataInfo.Text);

            foreach (DataGridViewRow r in dgvExtraTags.Rows)
            {
                if (r.Cells[0].Value != null)
                {
                    string[] tagInfo = r.Cells[0].Value.ToString().Split(':');

                    data.SetTagInfo(tagInfo[0], tagInfo.Length > 1 ? tagInfo[1] : string.Empty);
                }
            }
            row.Cells["colCharExtraName"].Value = tbCharExtraDataName.Text;
        }
        private void SaveListData()
        {
            _diListData.Remove(_diListData.ElementAt(_iIndex).Key);

            List<string> listInfo = new List<string>();
            foreach (DataGridViewRow r in dgvExtraTags.Rows)
            {
                if (r.Cells[0].Value != null)
                {
                    listInfo.Add(r.Cells[0].Value.ToString());
                }
            }
            _diListData[tbCharExtraDataName.Text] = listInfo;
            DataGridViewRow row = dgvCharExtraData.Rows[_iIndex];
            row.Cells["colCharExtraName"].Value = tbCharExtraDataName.Text;
        }
    }
}
