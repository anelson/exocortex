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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExoCortex
{
    /// <summary>
    /// PageEditor is a cusom control primarily composed of a RichTextBox, which
    /// enables rich text editing features shamelessly ripped off from OneNote.
    /// Unfortunately the stock RichTextBox doesn't have enough sophistication 
    /// in the area of bullets and numbering, and custom enablement of edit commands
    /// based on the state of the control (eg, 'Cut' is enabled when there's nothing to cut,
    /// etc)
    /// </summary>

    public partial class PageEditor : System.Windows.Controls.UserControl
    {
        static PageEditor()
        {
            //Register the command handlers for PageEditor.  Many of these pass through unmodified
            //to the underlying RichTextBox, while others require additional processing.
            RegisterClassCommandBinding(ApplicationCommands.Paste, null, null);
            RegisterClassCommandBinding(ApplicationCommands.Copy, null, null);
            RegisterClassCommandBinding(ApplicationCommands.Cut, null, null);

            RegisterClassCommandBinding(ApplicationCommands.Undo, null, OnCanExecuteUndo);
            RegisterClassCommandBinding(ApplicationCommands.Redo, null, OnCanExecuteRedo);

            RegisterClassCommandBinding(EditingCommands.ToggleBold, null, null);
            RegisterClassCommandBinding(EditingCommands.ToggleItalic, null, null);
            RegisterClassCommandBinding(EditingCommands.ToggleUnderline, null, null);

            RegisterClassCommandBinding(EditingCommands.IncreaseFontSize, null, null);
            RegisterClassCommandBinding(EditingCommands.DecreaseFontSize, null, null);

            RegisterClassCommandBinding(EditingCommands.ToggleBullets, null, null);
            RegisterClassCommandBinding(EditingCommands.ToggleNumbering, null, null);
            RegisterClassCommandBinding(EditingCommands.IncreaseIndentation, null, null);
            RegisterClassCommandBinding(EditingCommands.DecreaseIndentation, null, OnCanExecuteDecreaseIndentation);

            RegisterClassCommandBinding(EditingCommands.AlignLeft, null, null);
            RegisterClassCommandBinding(EditingCommands.AlignCenter, null, null);
            RegisterClassCommandBinding(EditingCommands.AlignRight, null, null);
            RegisterClassCommandBinding(EditingCommands.AlignJustify, null, null);
        }

        /// <summary>
        /// Registers a command binding which specifies how routedUICommand will be handled
        /// when sent to an instance of PageEditor.
        /// </summary>
        /// <param name="routedUICommand"></param>
        /// <param name="executeHandler"></param>
        /// <param name="canExecuteHandler"></param>
        private static void RegisterClassCommandBinding(RoutedUICommand routedUICommand,
            ExecutedRoutedEventHandler executeHandler,
            CanExecuteRoutedEventHandler canExecuteHandler) {
            CommandManager.RegisterClassCommandBinding(typeof(PageEditor),
                CreateCommandBinding(routedUICommand, 
                executeHandler, 
                canExecuteHandler));
        }

        /// <summary>
        /// Creates a CommandBinding object binding a given command to this type, with optional execute and can execute handlers
        /// </summary>
        /// <param name="routedUICommand"></param>
        /// <param name="executeHandler">The Execute handler, or a simple pass-through handler if null</param>
        /// <param name="canExecuteHandler">The CanExecute handler, or a simple pass-through handler if null</param>
        private static CommandBinding CreateCommandBinding(RoutedUICommand routedUICommand, 
            ExecutedRoutedEventHandler executeHandler, 
            CanExecuteRoutedEventHandler canExecuteHandler) {
            if (executeHandler == null) {
                executeHandler = CreatePassThroughExecuteHandler(routedUICommand);
            }
            if (canExecuteHandler == null) {
                canExecuteHandler = CreatePassThroughCanExecuteHandler(routedUICommand);
            }

            return new CommandBinding(routedUICommand,
                executeHandler,
                canExecuteHandler);
        }

        /// <summary>
        /// Creates a Execute command handler which passes the command directly to the rich text box
        /// </summary>
        /// <param name="routedUICommand"></param>
        /// <returns></returns>
        private static ExecutedRoutedEventHandler CreatePassThroughExecuteHandler(RoutedUICommand routedUICommand) {
            return delegate(object sender, ExecutedRoutedEventArgs e) {
                PageEditor target = (PageEditor)sender;
                routedUICommand.Execute(null, target.rtb);
            };
        }

        /// <summary>
        /// Creates a CanExecute command handler which passes the command directly to the rich text box
        /// </summary>
        /// <param name="routedUICommand"></param>
        /// <returns></returns>
        private static CanExecuteRoutedEventHandler CreatePassThroughCanExecuteHandler(RoutedUICommand routedUICommand) {
            return delegate(object sender, CanExecuteRoutedEventArgs e) {
                PageEditor target = (PageEditor)sender;
                e.CanExecute = routedUICommand.CanExecute(null, target.rtb);
            };
        }

        public PageEditor()
        {
            InitializeComponent();
        }

        #region Events
        public event RoutedEventHandler SelectionChanged;
        #endregion

        #region Event Raisers
        protected virtual void OnSelectionChanged(RoutedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }
        #endregion

        #region Public properties

        public TextRange Selection
        {
            get { return rtb.Selection; }
        }

        public bool IsSelectionBold { get { return GetSelectionProperty<FontWeight>(FontWeightProperty) == FontWeights.Bold; } }
        public bool IsSelectionItalics { get { return GetSelectionProperty<FontStyle>(FontStyleProperty) == FontStyles.Italic; } }
        public bool IsSelectionUnderlined { get { return TestSelectionClassProperty(Inline.TextDecorationsProperty, TextDecorations.Underline); } }

        public bool IsSelectionLeftAligned { get { return GetSelectionProperty<TextAlignment>(FlowDocument.TextAlignmentProperty) == TextAlignment.Left; } }
        public bool IsSelectionCenterAligned { get { return GetSelectionProperty<TextAlignment>(FlowDocument.TextAlignmentProperty) == TextAlignment.Center; } }
        public bool IsSelectionRightAligned { get { return GetSelectionProperty<TextAlignment>(FlowDocument.TextAlignmentProperty) == TextAlignment.Right; } }
        public bool IsSelectionJustified { get { return GetSelectionProperty<TextAlignment>(FlowDocument.TextAlignmentProperty) == TextAlignment.Justify; } }

        public bool IsSelectionBulletedList { get { return DoesRangeIncludeList(rtb.Selection, TextMarkerStyle.Disc); } }
        public bool IsSelectionNumberedList { get { return DoesRangeIncludeList(rtb.Selection, TextMarkerStyle.Decimal); } }
        public bool IsSelectionIndented { get { return DoesRangeIncludeIndented(rtb.Selection); } }

        #endregion

        #region Event Handlers
        private void rtb_SelectionChanged(object sender, RoutedEventArgs e)
        {
            OnSelectionChanged(e);
        }
        #endregion

        #region Command Handlers

        private static void OnCanExecuteUndo(object sender, CanExecuteRoutedEventArgs e) {
            //Only allow this if the RTB says so
            PageEditor target = (PageEditor)sender;
            e.CanExecute = target.rtb.CanUndo;
        }

        private static void OnCanExecuteRedo(object sender, CanExecuteRoutedEventArgs e) {
            //Only allow this if the RTB says so
            PageEditor target = (PageEditor)sender;
            e.CanExecute = target.rtb.CanRedo;
        }

        private static void OnCanExecuteDecreaseIndentation(object sender, CanExecuteRoutedEventArgs e) {
            //This only makes sense if the selected text is indented
            PageEditor target = (PageEditor)sender;
            e.CanExecute = target.IsSelectionIndented;
        }

        #endregion

        #region Private methods

        private bool TestSelectionClassProperty<T>(DependencyProperty testProperty, T testValue) where T : class
        {
            return testValue == GetSelectionProperty<T>(testProperty);
        }

        private T GetSelectionProperty<T>(DependencyProperty property) {
            TextRange selection = rtb.Selection;
            Object propVal = selection.GetPropertyValue(property);
            if (propVal is T) {
                return ((T)propVal);
            } else {
                return default(T);
            }
        }

        private bool DoesRangeIncludeList(TextRange range, TextMarkerStyle textMarkerStyle)
        {
            TextPointer start = range.Start;
            TextPointer end = range.End;

            Dictionary<DependencyObject, Object> objectsThatArentsLists = new Dictionary<DependencyObject, Object>();

            while (start != null && start.CompareTo(end) <= 0)
            {
                DependencyObject parent = start.Parent;

                while (parent != null)
                {
                    if (objectsThatArentsLists.ContainsKey(parent))
                    {
                        //Already checked this branch; skip
                        break;
                    }

                    if (parent is List && ((List)parent).MarkerStyle == textMarkerStyle)
                    {
                        //This range *does* include a list of the specified type
                        return true;
                    }

                    objectsThatArentsLists.Add(parent, null);

                    if (parent is FrameworkContentElement)
                    {
                        parent = ((FrameworkContentElement)parent).Parent;
                    }
                    else
                    {
                        parent = null;
                    }
                }

                start = start.GetNextContextPosition(LogicalDirection.Forward);
            }

            return false;
        }

        private bool DoesRangeIncludeIndented(TextSelection range) {
            //The RTB does indentation a little wierd, but for the purposes of detecting it
            //we just have to check the TexteIndent and left Margin properties of each Paragraph element in the 
            //selected range.  A non-zero value for either of these indicates indentation

            TextPointer start = range.Start;
            TextPointer end = range.End;

            Dictionary<Paragraph, Object> parasNotIndented = new Dictionary<Paragraph, Object>();

            while (start != null && start.CompareTo(end) <= 0) {
                Paragraph para = start.Paragraph;

                if (para != null) {
                    if (!parasNotIndented.ContainsKey(para)) {
                        //Check to see if this paragraph is indented
                        if (para.Margin.Left > 0 || para.TextIndent > 0) {
                            //This paragraph is indented
                            return true;
                        }

                        //Else, not indented; add to hash so we don't process again
                        parasNotIndented.Add(para, null);
                    }
                }

                start = start.GetNextContextPosition(LogicalDirection.Forward);
            }

            return false;
        }

        #endregion
    }
}