using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using HPS;

namespace wpf_sandbox
{
    public class ConfigurationBrowser
    {
        private MainWindow Window { get; set; }

        private string FileWithConfigurations { get; set; }

#if USING_EXCHANGE
        private Exchange.Configuration[] Configurations { get; set; }
#endif

        public ConfigurationBrowser(MainWindow mainWindow)
        {
            Window = mainWindow;
        }

        public void Init()
        {
#if USING_EXCHANGE
            if (Window.CADModel != null)
            {
                string fileFormat = new StringMetadata(Window.CADModel.GetMetadata("FileFormat")).GetValue();
                if (fileFormat == "SolidWorks" || fileFormat == "CATIA V4" || fileFormat == "I-DEAS")
                {
                    string filename = new StringMetadata(Window.CADModel.GetMetadata("Filename")).GetValue();
                    var exchangeCADModel = new Exchange.CADModel(Window.CADModel);

                    if (filename != FileWithConfigurations)
                    {
                        FileWithConfigurations = filename;

                        if (fileFormat == "SolidWorks")
                            Configurations = exchangeCADModel.GetConfigurations();
                        else
                            Configurations = Exchange.File.GetConfigurations(FileWithConfigurations);

                        if (Configurations != null && Configurations.Length > 0)
                        {
                            Window._configurationTreeView.Items.Clear();

                            var titleItem = new TreeViewItem();
                            titleItem.Header = "Configurations";
                            Window._configurationTreeView.Items.Add(titleItem);

                            foreach (var configuration in Configurations)
                                InsertConfigurationInTree(titleItem, configuration);

                            titleItem.IsExpanded = true;

                            HighlightConfiguration(exchangeCADModel.GetCurrentConfiguration());
                        }
                    }
                    else
                    {
                        UnhighlightAllConfigurations(Window._configurationTreeView.Items.GetItemAt(0) as TreeViewItem);
                        HighlightConfiguration(exchangeCADModel.GetCurrentConfiguration());
                    }
                }
                else
                    Flush();
            }
#else
            Flush();
#endif
        }

        public void Flush()
        {
            FileWithConfigurations = null;

#if USING_EXCHANGE
            Configurations = null;
#endif

            Window._configurationTreeView.Items.Clear();

            var noConfigurationsItem = new TreeViewItem();
            noConfigurationsItem.Header = "No Configurations";
            Window._configurationTreeView.Items.Add(noConfigurationsItem);
        }

#if USING_EXCHANGE
        private void InsertConfigurationInTree(TreeViewItem parent, Exchange.Configuration configuration)
        {
            var child = new TreeViewItem();
            child.Header = configuration.GetName();
            parent.Items.Add(child);

            foreach (var subconfiguration in configuration.GetSubconfigurations())
                InsertConfigurationInTree(child, subconfiguration);
        }

        private void UnhighlightAllConfigurations(TreeViewItem parent)
        {
            foreach (TreeViewItem child in parent.Items)
            {
                child.FontWeight = FontWeights.Normal;
                UnhighlightAllConfigurations(child);
            }
        }

        private void HighlightConfiguration(string[] configuration)
        {
            if (configuration != null && configuration.Length > 0)
            {
                var titleItem = Window._configurationTreeView.Items.GetItemAt(0) as TreeViewItem;
                var selectedItem = FindConfigurationInTree(configuration, titleItem);
                if (selectedItem != null)
                {
                    selectedItem.FontWeight = FontWeights.Bold;
                    (selectedItem.Parent as TreeViewItem).IsExpanded = true;
                }
            }
        }

        private TreeViewItem FindConfigurationInTree(string[] configuration, TreeViewItem parent)
        {
            if (parent.HasItems == false || configuration == null || configuration.Length == 0)
                return null;

            foreach (TreeViewItem child in parent.Items)
            {
                if (child.Header.ToString() == configuration[0])
                {
                    if (configuration.Length == 1)
                        return child;
                    else
                    {
                        var configurationList = configuration.ToList();
                        configurationList.RemoveAt(0);
                        return FindConfigurationInTree(configurationList.ToArray(), child);
                    }
                }
            }

            return null;
        }

        private string[] GetSelectedConfiguration(TreeViewItem selectedItem)
        {
            var selectedConfiguration = new List<string>();
            while (selectedItem != null)
            {
                var header = selectedItem.Header.ToString();
                if (header == "Configurations" || header == "No Configurations")
                    break;

                selectedConfiguration.Add(header);
                selectedItem = selectedItem.Parent as TreeViewItem;
            }

            if (selectedConfiguration.Count == 0)
                return null;
            else
            {
                selectedConfiguration.Reverse();
                return selectedConfiguration.ToArray();
            }
        }
#endif

        public void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
#if USING_EXCHANGE
            if (e.ChangedButton == MouseButton.Left)
            {
                var selectedTreeViewItem = GetTreeViewItemUnderCursor(e.OriginalSource as DependencyObject);
                if (selectedTreeViewItem != null)
                {
                    string[] selectedConfiguration = GetSelectedConfiguration(selectedTreeViewItem);
                    (Window.FileOpenCommand as DemoFileOpenCommand).ImportConfiguration(selectedConfiguration);
                }
                else
                    e.Handled = true;
            }
#endif
        }

        private TreeViewItem GetTreeViewItemUnderCursor(DependencyObject obj)
        {
            while (obj != null && obj.GetType() != typeof(TreeViewItem))
                obj = VisualTreeHelper.GetParent(obj);
            return obj as TreeViewItem;
        }
    }
}
