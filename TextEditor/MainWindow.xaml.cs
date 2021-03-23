using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TextEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string FilePath;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                using FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open);
                TextRange range = new TextRange(rTextBox.Document.ContentStart, rTextBox.Document.ContentEnd);
                range.Load(fileStream, DataFormats.Rtf);
                FilePath = dlg.FileName;
                Title = dlg.FileName;
            }
        }
        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (FilePath != null)
                SaveTo(FilePath);
            else
                SaveAs();
        }
        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAs();
        }
        void SaveAs()
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
            if (dlg.ShowDialog() == true)
            {
                SaveTo(dlg.FileName);
                FilePath = dlg.FileName;
                Title = dlg.FileName;
            }
        }
        void SaveTo(string filePath)
        {
            using FileStream fileStream = new FileStream(filePath, FileMode.Create);
            TextRange range = new TextRange(rTextBox.Document.ContentStart, rTextBox.Document.ContentEnd);
            range.Save(fileStream, DataFormats.Rtf);
        }
        private void PrintCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            PrintDialog pd = new PrintDialog();
            if ((pd.ShowDialog() == true))
            {
                //pd.PrintVisual(rTextBox as Visual, "printing as visual");
                pd.PrintDocument((((IDocumentPaginatorSource)rTextBox.Document).DocumentPaginator), "printing as paginator");
            }
        }

        private void FontFamilyCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FontFamily font = (sender as ComboBox).SelectedItem as FontFamily;
            RTBApplyProperty(rTextBox, TextElement.FontFamilyProperty, font);
        }
        private void FontSizeCB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse((sender as ComboBox).Text, out double fontSize))
                RTBApplyProperty(rTextBox, TextElement.FontSizeProperty, fontSize);
        }

        private void LineSpacingCB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.TryParse((sender as ComboBox).Text, out double lineSpacing))
                if (lineSpacing > 0)
                    RTBApplyProperty(rTextBox, Block.LineHeightProperty, lineSpacing);
        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            Color color = FontColorPicker.SelectedColor ?? Colors.Black;
            RTBApplyProperty(rTextBox, TextElement.ForegroundProperty, new SolidColorBrush(color));
        }

        void RTBApplyProperty(RichTextBox richTextBox, DependencyProperty property, object propertyValue)
        {
            var selection = rTextBox?.Selection;
            if (selection != null && propertyValue != null)
                selection.ApplyPropertyValue(property, propertyValue);
        }

        private void rTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextSelection selection = rTextBox.Selection;
            ToggleBoldTB.IsChecked = (FontWeight)selection.GetPropertyValue(TextElement.FontWeightProperty) == FontWeights.Bold;
            ToggleItalicTB.IsChecked = (FontStyle)selection.GetPropertyValue(TextElement.FontStyleProperty) == FontStyles.Italic;
            ToggleUnderlineTB.IsChecked = (TextDecorationCollection)selection.GetPropertyValue(Inline.TextDecorationsProperty) == TextDecorations.Underline;

            FontFamilyCB.SelectedItem = selection.GetPropertyValue(TextElement.FontFamilyProperty);
            FontSizeCB.SelectedItem = selection.GetPropertyValue(TextElement.FontSizeProperty);
            FontColorPicker.SelectedColor = (selection.GetPropertyValue(TextElement.ForegroundProperty) as SolidColorBrush).Color;

            Paragraph start = selection.Start.Paragraph;
            if (start != null && start.Parent is ListItem)
            {
                TextMarkerStyle markerStyle = ((ListItem)start.Parent).List.MarkerStyle;
                ToggleBulletsTB.IsChecked = markerStyle is >= TextMarkerStyle.Disc and <= TextMarkerStyle.Box;
                ToggleNumberingTB.IsChecked = markerStyle is >= TextMarkerStyle.LowerRoman and <= TextMarkerStyle.Decimal;
            }
            else
            {
                ToggleBulletsTB.IsChecked = false;
                ToggleNumberingTB.IsChecked = false;
            }

            var lineSpacingVal = selection.GetPropertyValue(Block.LineHeightProperty);
            double lineSpacing = lineSpacingVal is Double ? (double)lineSpacingVal : Double.NaN;
            LineSpacingCB.Text = Double.IsNaN(lineSpacing) ? "-" : lineSpacing.ToString();

        }
    }
    public static class AppData
    {
        public static double[] FontSizes = { 8, 9, 10, 10.5, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
        public static double[] LineSpacings = { 1, 3, 6, 9, 12 };
    }
}
