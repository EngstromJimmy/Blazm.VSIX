using EnvDTE;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace BlazmVSIX
{
    public partial class RoutingWindowControl : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ToolWindow1Control"/> class.
        /// </summary>
        public RoutingWindowControl()
        {
            this.InitializeComponent();
            Load();
        }
        private void Load()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var _dte = ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            Array _projects = _dte.ActiveSolutionProjects as Array;
            RoutingDataList.Clear();
            if (_projects.Length != 0 && _projects != null)
            {
                foreach (Project project in _projects)
                {
                    //get the project path
                    string _directoryPath = new FileInfo(project.FullName).DirectoryName;
                    string strInputLine;
                    string[] allFiles = Directory.GetFiles(_directoryPath,"*.razor", SearchOption.AllDirectories);

                    foreach (string file in allFiles)
                    {
                        using (StreamReader sr = new StreamReader(file))
                        {
                            int row = 0;
                            while (sr.Peek() >= 0)
                            {
                                strInputLine = sr.ReadLine();

                                if (strInputLine.Contains("@page"))
                                {
                                    if (!strInputLine.StartsWith("@*"))
                                    {
                                        var content = strInputLine.Replace("@page", "").Replace("\"", "").Trim();
                                        var newItem = new RoutingItem() { Name = file, Content = content };
                                        RoutingDataList.Add(newItem);
                                    }
                                }
                                else if (row > 5)
                                    break;
                                row++;
                            }
                        }
                        
                    }
                }
            }
            else
            {
                RoutingDataList.Add(new RoutingItem() { Name = "No Project in solution or selected" });
            }
            RoutingGrid.ItemsSource = RoutingDataList;
        }

        public ObservableCollection<RoutingItem> RoutingDataList { get; set; } = new ObservableCollection<RoutingItem>();

        public class RoutingItem
        {
            public string Name { get; set; }
            public string Content { get; set; }
            public string Key { get; set; }
            public string Segment { get; set; }
            public string Parameters { get; set; }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            Load();
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "Blazm - Routing");
        }

        private void RoutingGrid_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Ensure row was clicked and not empty space
            var row = ItemsControl.ContainerFromElement((DataGrid)sender,
                                                e.OriginalSource as DependencyObject) as DataGridRow;
            if (row == null) return;
            var item = (RoutingItem)row.DataContext;
            ThreadHelper.ThrowIfNotOnUIThread();
            var _dte = ServiceProvider.GlobalProvider.GetService(typeof(EnvDTE.DTE)) as EnvDTE.DTE;
            _dte.ItemOperations.OpenFile(item.Name);
        }
    }
}