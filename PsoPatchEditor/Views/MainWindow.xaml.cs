using System.Windows;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
namespace PsoPatchEditor.Views
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            
            // if the current focused element is textbox then updates the source.
            var focusedElement = Keyboard.FocusedElement as FrameworkElement;
            IInputElement focusedElementLayoutRoot = FocusManager.GetFocusedElement(this.LayoutRoot);

            var probableTextBoxes = new[]
                {
                    focusedElement as TextBox,
                    focusedElementLayoutRoot as TextBox,
                };

            foreach (var tb in probableTextBoxes.Where(x => x != null))
            {
                var expression = tb.GetBindingExpression(TextBox.TextProperty);
                if (expression != null)
                {
                    expression.UpdateSource();
                }
            }
        }
    }
}
