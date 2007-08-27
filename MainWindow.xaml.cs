using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ExoCortex
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : System.Windows.Window
    {

        public MainWindow()
        {
            InitializeComponent();
        }

        private void editor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            //When the cursor moves to somewhere else in the text box, 
            //the commands (like bold, italic, list, etc) need to update themselves
            tbbBold.IsChecked = editor.IsSelectionBold;
            tbbItalic.IsChecked = editor.IsSelectionItalics;
            tbbUnderline.IsChecked = editor.IsSelectionUnderlined;

            tbbAlignLeft.IsChecked = editor.IsSelectionLeftAligned;
            tbbAlignCenter.IsChecked = editor.IsSelectionCenterAligned;
            tbbAlignRight.IsChecked = editor.IsSelectionRightAligned;
            tbbAlignJustify.IsChecked = editor.IsSelectionJustified;

            tbbBullets.IsChecked = editor.IsSelectionBulletedList;
            tbbNumbers.IsChecked = editor.IsSelectionNumberedList;
        }
    }
}
