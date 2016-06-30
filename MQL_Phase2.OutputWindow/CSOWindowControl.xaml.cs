//------------------------------------------------------------------------------
// <copyright file="CSOWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace MQL_Phase2.OutputWindow
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    /// <summary>
    /// Interaction logic for CSOWindowControl.
    /// </summary>
    public partial class CSOWindowControl : UserControl
    {
        IEnumerable<TotalValues> totalValues;
        IEnumerable<SpecificValues> specifics;

        // public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CSOWindowControl"/> class.
        /// </summary>
        public CSOWindowControl()
        {
            this.InitializeComponent();
            Summary.Results.OnUpdate += Results_OnUpdate;
            refreshSummaryMenuItem.Command = new RelayCommand( (x) => totalValues != null, Command_Refresh);
            
        }

        private void Command_Refresh(object obj)
        {
            ReloadTotals();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "CSOWindow");
        }

        internal void Results_OnUpdate()
        {
            // update the tables; use the UI thread.
            summaryGrid.Dispatcher.Invoke(ReloadTotals);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Table reloaded.");
            ReloadTotals();

        }

        private void ReloadTotals()
        {
            summaryGrid.Columns.Clear();
            summaryGrid.ItemsSource = null;
            totalValues = Summary.Results.GenerateTotals();
            summaryGrid.ItemsSource = totalValues;
            if (totalValues.Count() > 0)
            {
                summaryGrid.Columns[0].Header = "File";
                summaryGrid.Columns[1].Header = "Total Code Smells";
                summaryGrid.Columns[2].Header = "Lines of Code";
            }
        }

        private void Grid_Initialized(object sender, EventArgs e)
        {
            ReloadTotals();
        }

        private void summaryGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void summaryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (summaryGrid.SelectedCells.Count > 0)
            {
                var selectedItem = summaryGrid.SelectedItem as TotalValues;
                specificGrid.Columns.Clear();
                specifics = Summary.Results.GenerateSpecificTable(selectedItem);
                specificGrid.ItemsSource = specifics;
                tabControl.SelectedIndex = 1;
                specificsTabItem.Header = "Specifics: " + selectedItem.Filename.ShortenAbolutePath();
            }
        }



        // TODO LIST
        // Make it so that when the Diagnostic is finished, the CSO Window should activate.
    }
}