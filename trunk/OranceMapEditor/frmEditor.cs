﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml;

namespace OranceMapEditor
{
    public partial class frmEditor : Form
    {
        //TODO: correct editor, using Render() function
        //TODO: save to xml
        public frmEditor()
        {
            InitializeComponent();
        }
        enum DrawMode
        {
            BRICK,
            MOB,
            CHARACTER
        }
        Bitmap renderBitmap;
        Graphics renderer;
        int[,] matrixBrick;
        int[,] matrixMob;
        DrawMode drawMode;

        private void frmEditor_Load(object sender, EventArgs e)
        {
            matrixBrick = new int[11,13];
            matrixMob = new int[11, 13];
            renderBitmap = new Bitmap(11*52+1,13*42+1);
            renderer = Graphics.FromImage(renderBitmap);
            drawGrid();
            picPreview.Image = renderBitmap;
            drawMode = DrawMode.BRICK;
            cboMobs.Enabled = false;
            {
                DirectoryInfo dir = new DirectoryInfo("Brick");
                foreach (FileInfo item in dir.GetFiles("*.xml"))
                {
                    cboBricks.Items.Add(item.Name);
                }
                if (cboBricks.Items.Count > 0) cboBricks.SelectedIndex = 0;
            }
            {
                DirectoryInfo dir = new DirectoryInfo("Mob");
                foreach (FileInfo item in dir.GetFiles("*.xml"))
                {
                    cboMobs.Items.Add(item.Name);
                }
                if (cboMobs.Items.Count > 0) cboMobs.SelectedIndex = 0;
            }
        }
        void DrawBrick(int x, int y, int id)
        {
            if (x >= matrixBrick.GetLength(0)) return;
            if (y >= matrixBrick.GetLength(1)) return;
            matrixBrick[x, y] = id+1;
            renderer.DrawImage(Crop42((Bitmap)picItemPreview.Image), 
                new Point(x * 52+1, y * 42+1));
        }
        void DrawMob(int x, int y, int id)
        {
            if (x >= matrixMob.GetLength(0)) return;
            if (y >= matrixMob.GetLength(1)) return;
            matrixMob[x, y] = id+1;
            renderer.DrawImage(Crop42((Bitmap)picItemPreview.Image), 
                new Point(x * 52 + 1, y * 42 + 1));
        }
        void DrawCharacter(int x, int y)
        {
            matrixMob[x, y] = -1;
            renderer.FillRectangle(Brushes.DarkGray, 
                new Rectangle(x * 52 + 1, y * 42 + 1, 42, 42));
        }
        private void drawGrid()
        {
            Pen pen = new Pen(Brushes.Black);
            for (int i = 0; i <= 11; i++)
            {
                renderer.DrawLine(pen, new Point(i * 52, 0), new Point(i * 52, 13 * 42));
            }
            for (int i = 0; i <= 13; i++)
            {
                renderer.DrawLine(pen, new Point(0, i * 42), new Point(11 * 52, i * 42));
            }
        }

        private void picPreview_MouseDown(object sender, MouseEventArgs e)
        {
            Point position = new Point(e.X/52, e.Y/42);
            //MessageBox.Show(position.ToString());
            if (drawMode == DrawMode.BRICK)
            {
                DrawBrick(position.X, position.Y, cboBricks.SelectedIndex);
            }
            else if(drawMode== DrawMode.MOB)
            {
                DrawMob(position.X, position.Y, cboMobs.SelectedIndex);
            }
            else if (drawMode == DrawMode.CHARACTER)
            {
                DrawCharacter(position.X, position.Y);
            }
            picPreview.Image = renderBitmap;
        }

        private void btnBrick_Click(object sender, EventArgs e)
        {
            btnBrick.Enabled = false;
            cboBricks.Enabled = true;
            btnCharacter.Enabled = true;
            btnMob.Enabled = true;
            cboMobs.Enabled = false;
            drawMode = DrawMode.BRICK;
            cboBricks_SelectedIndexChanged(sender, e);
        }

        private void btnMob_Click(object sender, EventArgs e)
        {
            btnBrick.Enabled = true;
            cboBricks.Enabled = false;
            btnCharacter.Enabled = true;
            btnMob.Enabled = false;
            cboMobs.Enabled = true;
            drawMode = DrawMode.MOB;
            cboMobs_SelectedIndexChanged(sender, e);
        }

        private void btnCharacter_Click(object sender, EventArgs e)
        {
            btnBrick.Enabled = true;
            cboBricks.Enabled = false;
            btnCharacter.Enabled = false;
            btnMob.Enabled = true;
            cboMobs.Enabled = false;
            drawMode = DrawMode.CHARACTER;
        }

        Bitmap GetImageFromXml(string xml)
        {
            FileInfo info = new FileInfo(xml);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(File.ReadAllText(xml));
            XmlElement root;
            if (doc.FirstChild is XmlElement)
            {
                root = (XmlElement)doc.FirstChild;
            }
            else
            {
                root = (XmlElement)doc.FirstChild.NextSibling;
            }
            XmlElement textureTAG = (XmlElement)root.FirstChild;
            string pngFile = info.DirectoryName + "/" + textureTAG.GetAttribute("file") + ".png";
            Bitmap sourceBitmap = new Bitmap(pngFile);
            int divX = int.Parse(textureTAG.GetAttribute("divide_x"));
            int divY = int.Parse(textureTAG.GetAttribute("divide_y"));
            Bitmap previewBitmap = new Bitmap(sourceBitmap.Width / divX,
                sourceBitmap.Height / divY);
            Graphics g = Graphics.FromImage(previewBitmap);
            g.DrawImage(sourceBitmap,
                new Rectangle(0, 0, previewBitmap.Width, previewBitmap.Height),
                new Rectangle(0, 0, previewBitmap.Width, previewBitmap.Height),
                GraphicsUnit.Pixel);
            return previewBitmap;
        }
        Bitmap Crop42(Bitmap bitmap)
        {
            int w = 51;
            int h = 41;
            Bitmap ret = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(ret);
            g.DrawImage(bitmap, new Rectangle(0, 0, w, h),
                new Rectangle((bitmap.Width - w) / 2, (bitmap.Height - h) / 2, w, h),
                GraphicsUnit.Pixel);
            return ret;
        }
        private void cboBricks_SelectedIndexChanged(object sender, EventArgs e)
        {
            picItemPreview.Image = GetImageFromXml("Brick/"+cboBricks.Text);
        }

        private void cboMobs_SelectedIndexChanged(object sender, EventArgs e)
        {
            picItemPreview.Image = GetImageFromXml("Mob/" + cboMobs.Text);

        }
        Bitmap GetPreviewBitmap()
        {
            return new Bitmap(1, 1);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement mapTAG;
            mapTAG = doc.CreateElement("map");
            for (int i=0; i<11;i++)
            {
                for (int j=0; j < 13; j++)
                {
                    int id = matrixMob[i, j];
                    if (id == 0) continue;
                    if (id == -1)
                    {
                        XmlElement nodeTAG;
                        nodeTAG = doc.CreateElement("node");
                        nodeTAG.SetAttribute("x", i.ToString());
                        nodeTAG.SetAttribute("y", j.ToString());
                        nodeTAG.SetAttribute("node_type", "PLAYER");
                        mapTAG.AppendChild(nodeTAG);
                    }
                    else
                    {
                        string path = cboMobs.Items[id - 1].ToString();
                        XmlElement nodeTAG;
                        nodeTAG = doc.CreateElement("node");
                        nodeTAG.SetAttribute("x", i.ToString());
                        nodeTAG.SetAttribute("y", j.ToString());
                        nodeTAG.SetAttribute("node_type", "MOB");
                        nodeTAG.SetAttribute("file", path);
                        mapTAG.AppendChild(nodeTAG);
                    }
                }
            }
            for (int i = 0; i < 11; i++)
            {
                for (int j = 0; j < 13; j++)
                {
                    int id = matrixBrick[i, j];
                    if (id == 0) continue;
                    string path = cboBricks.Items[id - 1].ToString();
                    XmlElement nodeTAG;
                    nodeTAG = doc.CreateElement("node");
                    nodeTAG.SetAttribute("x", i.ToString());
                    nodeTAG.SetAttribute("y", j.ToString());
                    nodeTAG.SetAttribute("node_type", "BRICK");
                    nodeTAG.SetAttribute("file", path);
                    mapTAG.AppendChild(nodeTAG);

                }
                
            }
            if (dlgSave.ShowDialog() == DialogResult.OK)
            {
                doc.AppendChild(mapTAG);
                XmlTextWriter writer = new XmlTextWriter(dlgSave.FileName, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                doc.Save(writer);
                writer.Close();            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
                
        }
    }
}