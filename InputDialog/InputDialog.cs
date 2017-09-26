using System;
using System.Drawing;
using System.Windows.Forms;

namespace InputDialog
{
    public partial class InputDialog : Form
    {
        public InputDialog(string title, string instructions, string[] options)
        {
            //auto generated, has to be untouched
            InitializeComponent();

            //init method params
            Text = title;
            label1.Text = instructions;
            comboBox1.Items.AddRange(options);
            if (options.Length > 0)
                comboBox1.Text = options[0];
        }

        //method to request combobox value
        public string GetSelection()
        {
            return comboBox1.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        //load dialog at mouse position for minimal mouse movement
        private void InputDialog_Load(object sender, EventArgs e)
        {
            var point = new Point(Cursor.Position.X, Cursor.Position.Y);
            Top = point.Y;
            Left = point.X;
        }
    }
}