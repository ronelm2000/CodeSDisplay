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
        IEnumerable<TotalTableValue> totalValues;
        IEnumerable<SpecificValue> specifics;

        // public ICommand RefreshCommand { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CSOWindowControl"/> class.
        /// </summary>
        public CSOWindowControl()
        {
            this.InitializeComponent();
            Summary.Results.OnUpdate += Results_OnUpdate;
            Summary.Results.OnStepUpdate += Results_OnStepUpdate;
            refreshSummaryMenuItem.Command = new RelayCommand( (x) => totalValues != null, Command_Refresh);
            
        }

        private void Results_OnStepUpdate(string filename, CodeSmellSummary summary)
        {
            statusBar.Dispatcher.Invoke(() =>
            {
                statusBar.Text = "Code Smell Added: " + summary.Type.ToString() + " at " + filename.ShortenAbolutePath() + " in " + summary.Location.GetLineSpan().ToString();
            });
        }

        private void Command_Refresh(object obj)
        {
            ReloadTotals();
        }

        internal void Results_OnUpdate()
        {
            // update the tables; use the UI thread.
            tabControl.Dispatcher.Invoke(ReloadTotals);
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Table reloaded.");
            ReloadTotals();

        }

        private void ReloadTotals()
        {
            summaryGrid.ContextMenu.IsOpen = false;
            summaryGrid.ContextMenu.InvalidateVisual();
            summaryGrid.SelectedIndex = -1;
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
            summaryGrid.SelectedIndex = -1;
            summaryGrid.ContextMenu.IsEnabled = true;
            statusBar.Text = "Ready";
            specificGrid.ItemsSource = null;
            lineMapGrid.ItemsSource = null;
            lineMapTabItem.Header = "Line Mapping";
            specificsTabItem.Header = "Specifics";

        }

        private void Grid_Initialized(object sender, EventArgs e)
        {
            ReloadTotals();
        }
        

        private void summaryGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (summaryGrid.SelectedCells.Count > 0)
            {
                var selectedItem = summaryGrid.SelectedItem as TotalTableValue;
                specificGrid.Columns.Clear();
                specifics = Summary.Results.GenerateSpecificTable(selectedItem);
                specificGrid.ItemsSource = specifics;
                tabControl.SelectedIndex = 1;
                specificsTabItem.Header = "Specifics: " + selectedItem.Filename.ShortenAbolutePath();
            }
        }

        private void specificGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (specificGrid.SelectedCells.Count > 0 && summaryGrid.SelectedCells.Count > 0)
            {
                var selectedSpecific = specificGrid.SelectedItem as SpecificValue;
                var selectedSummary = summaryGrid.SelectedItem as TotalTableValue;
                lineMapGrid.Columns.Clear();
                lineMapGrid.ItemsSource = Summary.Results.GenerateLineMapping(selectedSummary, selectedSpecific);
                tabControl.SelectedIndex = 2;
                lineMapTabItem.Header = "Line Map: " + Summary.TypeProper[selectedSpecific.GetCodeSmellType()];
            }
        }



        // TODO LIST
        // Make it so that when the Diagnostic is finished, the CSO Window should activate.
    }
}