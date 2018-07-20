using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Folder_Scanner
{
    public partial class Form2 : Form
    {
        private int LEFT=0, TOP=0;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Location = new Point(LEFT, TOP);
        }

        public void Set_Position(int _Left,int _Top,int _Width,int _Height)
        {
            LEFT = _Left + (_Width - Width) / 2;
            TOP = _Top + (_Height - Height) / 2;
        }
    }
}
