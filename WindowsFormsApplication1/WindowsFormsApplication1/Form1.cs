﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        AAMTGLibrary oMtgLib;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            oMtgLib = new AAMTGLibrary(@"C:\Users\antoi\Source\Repos\AntArt\WindowsFormsApplication1\WindowsFormsApplication1\Resources\AllSets.json");
            foreach (DictionaryEntry oPair in oMtgLib.m_oMtgSets)
            {
                listBox1.Items.Add(oPair.Key);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string sSelectedSet = listBox1.SelectedItem.ToString();
            AAMTGSet oSelectedSet = (AAMTGSet)oMtgLib.m_oMtgSets[sSelectedSet];
            listView1.Items.Clear();
            foreach (AAMTGCard oCard in oSelectedSet.cards)
                listView1.Items.Add(oCard.name);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            AAMTGCard oSelectedCard = null;
            try
            {
                string sSelectedCard;
                if (listView1.SelectedItems.Count > 0)
                {
                    sSelectedCard = listView1.SelectedItems[0].Text;
                    oSelectedCard = oMtgLib.GetCard(listBox1.SelectedItem.ToString(), sSelectedCard);
                    this.Text = oSelectedCard.sImageString;
                    //Clipboard.SetDataObject(listView1.SelectedItems[0].Text);
                    //pictureBox1.ImageLocation = listView1.SelectedItems[0].Text;
                    //pictureBox1.Load(listView1.SelectedItems[0].Text);
                    oSelectedCard.FetchImage();
                    if (oSelectedCard.oImage != null)
                        pictureBox1.Image = oSelectedCard.oImage;
                }
            }
            catch (Exception ex)
            {
                if (oSelectedCard != null)
                    MessageBox.Show(string.Format("Error:{0} card:{1} number:{2} ", ex.Message, oSelectedCard.name, oSelectedCard.number));
                else
                    MessageBox.Show(ex.Message);
            }
        }
    }
}
