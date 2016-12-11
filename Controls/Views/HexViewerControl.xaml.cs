using Catel.MVVM.Views;
using Controls.ViewModels;
using System.Windows;
namespace Controls.Views
{
    public partial class HexViewerControl
    {

        [ViewToViewModel("TextAreaHeight", MappingType = ViewToViewModelMappingType.ViewToViewModel)]
        public double TextAreaHeight
        {
            get { return (double)GetValue(TextAreaHeightProperty); }
            set { SetValue(TextAreaHeightProperty, value); }
        }

        public static readonly DependencyProperty TextAreaHeightProperty =
            DependencyProperty.Register("TextAreaHeight", typeof(double), typeof(HexViewerControl), new PropertyMetadata(0.0));



        //private HexViewerViewModel HexViewerViewModel
        //{
        //    get
        //    {
        //        return this.ViewModel as HexViewerViewModel;
        //    }
        //}
        public HexViewerControl()
        {
            InitializeComponent();
            //this.Loaded += HexViewerControl_Loaded;
        }

        void HexViewerControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //_UpdateTextAreaSize(this.textArea.ActualHeight);
        }

        private void textArea_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            if (e.HeightChanged)
            {
                this.TextAreaHeight = e.NewSize.Height;
            }
        }

        //private void _UpdateTextAreaSize(double newHeight)
        //{
        //    var viewModel = this.HexViewerViewModel;
        //    if (viewModel != null)
        //    {
        //        viewModel.VerticalTextBoxSize = newHeight;
        //    }
        //}

        private void TextBlock_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            e.Handled = true;
        }
    }
}
