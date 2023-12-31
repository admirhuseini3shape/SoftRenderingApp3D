using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace SoftRenderingApp3D.Controls
{
    // Slider

    public partial class Slider : UserControl
    {
        public Slider()
        {
            InitializeComponent();
            textBox1.DataBindings.Add(nameof(TextBox.Text), superSlider1, "Value", false,
                DataSourceUpdateMode.OnPropertyChanged);
        }

        [EditorBrowsable(EditorBrowsableState.Always)]
        [Browsable(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Bindable(true)]
        public override string Text
        {
            get
            {
                return label1.Text;
            }
            set
            {
                label1.Text = value;
            }
        }

        public float Value
        {
            get
            {
                return superSlider1.Value;
            }
            set
            {
                superSlider1.Value = value;
            }
        }

        public float Min
        {
            get
            {
                return superSlider1.Min;
            }
            set
            {
                superSlider1.Min = value;
            }
        }

        public float Max
        {
            get
            {
                return superSlider1.Max;
            }
            set
            {
                superSlider1.Max = value;
            }
        }

        public event EventHandler ValueChanged
        {
            add
            {
                superSlider1.ValueChanged += value;
            }
            remove
            {
                superSlider1.ValueChanged -= value;
            }
        }
    }
}
