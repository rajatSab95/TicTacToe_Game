using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid;
using System.Collections.Generic;
using HPS;

namespace wpf_sandbox
{
    public enum SegmentBrowserRoot
    {
        Model,
        View,
        Layout,
        Canvas
    }

    public class SegmentBrowser
    {
        public MainWindow win { get; private set; }
        private WPFSceneTree sceneTree { get; set; }
        public PropertiesPanel PropertiesPanel { get; private set; }
        private ContextMenu SegmentContextMenu { get; set; }
        private ContextMenu AttributeContextMenu { get; set; }
        private SceneTreeViewItem SelectedTreeViewItem { get; set; }

        private Dictionary<HPS.SceneTree.ItemType, HPS.Search.Type> searchTypeMap = new Dictionary<SceneTree.ItemType, Search.Type>();

        public SegmentBrowser(MainWindow mainWindow)
        {
            win = mainWindow;
            sceneTree = new WPFSceneTree(win.GetSprocketsControl().Canvas, this);
            PropertiesPanel = new PropertiesPanel(win);

            searchTypeMap.Add(HPS.SceneTree.ItemType.CuttingSectionGroup, HPS.Search.Type.CuttingSection);
            searchTypeMap.Add(HPS.SceneTree.ItemType.ShellGroup, HPS.Search.Type.Shell);
            searchTypeMap.Add(HPS.SceneTree.ItemType.MeshGroup, HPS.Search.Type.Mesh);
            searchTypeMap.Add(HPS.SceneTree.ItemType.GridGroup, HPS.Search.Type.Grid);
            searchTypeMap.Add(HPS.SceneTree.ItemType.NURBSSurfaceGroup, HPS.Search.Type.NURBSSurface);
            searchTypeMap.Add(HPS.SceneTree.ItemType.CylinderGroup, HPS.Search.Type.Cylinder);
            searchTypeMap.Add(HPS.SceneTree.ItemType.SphereGroup, HPS.Search.Type.Sphere);
            searchTypeMap.Add(HPS.SceneTree.ItemType.PolygonGroup, HPS.Search.Type.Polygon);
            searchTypeMap.Add(HPS.SceneTree.ItemType.CircleGroup, HPS.Search.Type.Circle);
            searchTypeMap.Add(HPS.SceneTree.ItemType.CircularWedgeGroup, HPS.Search.Type.CircularWedge);
            searchTypeMap.Add(HPS.SceneTree.ItemType.EllipseGroup, HPS.Search.Type.Ellipse);
            searchTypeMap.Add(HPS.SceneTree.ItemType.LineGroup, HPS.Search.Type.Line);
            searchTypeMap.Add(HPS.SceneTree.ItemType.NURBSCurveGroup, HPS.Search.Type.NURBSCurve);
            searchTypeMap.Add(HPS.SceneTree.ItemType.CircularArcGroup, HPS.Search.Type.CircularArc);
            searchTypeMap.Add(HPS.SceneTree.ItemType.EllipticalArcGroup, HPS.Search.Type.EllipticalArc);
            searchTypeMap.Add(HPS.SceneTree.ItemType.InfiniteLineGroup, HPS.Search.Type.InfiniteLine);
            searchTypeMap.Add(HPS.SceneTree.ItemType.InfiniteRayGroup, HPS.Search.Type.InfiniteRay);
            searchTypeMap.Add(HPS.SceneTree.ItemType.MarkerGroup, HPS.Search.Type.Marker);
            searchTypeMap.Add(HPS.SceneTree.ItemType.TextGroup, HPS.Search.Type.Text);
            searchTypeMap.Add(HPS.SceneTree.ItemType.ReferenceGroup, HPS.Search.Type.Reference);
            searchTypeMap.Add(HPS.SceneTree.ItemType.DistantLightGroup, HPS.Search.Type.DistantLight);
            searchTypeMap.Add(HPS.SceneTree.ItemType.SpotlightGroup, HPS.Search.Type.Spotlight);

            SegmentContextMenu = new ContextMenu();
            var addAttributeMenuItem = new MenuItem();
            addAttributeMenuItem.Header = "Add Attribute";
            string[] menuItemStrings =
            {
                "Material",
                "Camera",
                "Modelling Matrix",
                "Texture Matrix",
                "Culling",
                "Curve Attribute",
                "Cylinder Attribute",
                "Edge Attribute",
                "Lighting Attribute",
                "Line Attribute",
                "Marker Attribute",
                "Surface Attribute",
                "Selectability",
                "Sphere Attribute",
                "Subwindow",
                "Text Attribute",
                "Transparency",
                "Visibility",
                "Visual Effects",
                "Performance",
                "Drawing Attribute",
                "Hidden Line Attribute",
                "Material Palette",
                "Contour Line",
                "Condition",
                "Bounding",
                "Attribute Lock",
                "Transform Mask",
                "Color Interpolation",
                "Cutting Section Attribute",
                "Priority",
            };
            foreach (var item in menuItemStrings)
            {
                var menuItem = new MenuItem();
                menuItem.Header = item;
                menuItem.Click += AddAttribute_Click;
                addAttributeMenuItem.Items.Add(menuItem);
            }
            SegmentContextMenu.Items.Add(addAttributeMenuItem);

            AttributeContextMenu = new ContextMenu();
            var unsetMenuItem = new MenuItem();
            unsetMenuItem.Header = "Unset";
            unsetMenuItem.Click += UnsetAttribute_Click;
            AttributeContextMenu.Items.Add(unsetMenuItem);
        }

        public void SetRoot(SegmentBrowserRoot root)
        {
            WPFSceneTreeItem rootItem = null;

            bool has_canvas = win.GetSprocketsControl().Canvas.Type() != HPS.Type.None;
            bool has_layout = has_canvas && win.GetSprocketsControl().Canvas.GetAttachedLayout().Type() != HPS.Type.None;
            bool has_view = has_layout && win.GetSprocketsControl().Canvas.GetAttachedLayout().GetLayerCount() != 0 &&
                win.GetSprocketsControl().Canvas.GetAttachedLayout().GetFrontView().GetSegmentKey().Type() != HPS.Type.None;
            bool has_model = has_view && win.GetSprocketsControl().Canvas.GetAttachedLayout().GetFrontView().GetAttachedModel().Type() != HPS.Type.None
                && win.GetSprocketsControl().Canvas.GetAttachedLayout().GetFrontView().GetAttachedModel().GetSegmentKey().Type() != HPS.Type.None;

            switch (root)
            {
                case SegmentBrowserRoot.Model:
                    if (has_model)
                        rootItem = new WPFSceneTreeItem(sceneTree, win.GetSprocketsControl().Canvas.GetFrontView().GetAttachedModel());
                    break;
                case SegmentBrowserRoot.View:
                    if (has_view)
                        rootItem = new WPFSceneTreeItem(sceneTree, win.GetSprocketsControl().Canvas.GetFrontView());
                    break;
                case SegmentBrowserRoot.Layout:
                    if (has_layout)
                        rootItem = new WPFSceneTreeItem(sceneTree, win.GetSprocketsControl().Canvas.GetAttachedLayout());
                    break;
                case SegmentBrowserRoot.Canvas:
                default:
                    if (has_canvas)
                        rootItem = new WPFSceneTreeItem(sceneTree, win.GetSprocketsControl().Canvas);
                    break;
            }
            if (rootItem != null)
                sceneTree.SetRoot(rootItem);
            else
                sceneTree.Flush();
            PropertiesPanel.Flush();
        }

        public DispatcherOperation InvokeUIAction(Action action)
        {
            return win.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(action));
        }

        private void AddAttribute_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTreeViewItem.sceneTreeItem.GetItemType() != SceneTree.ItemType.Segment)
                return;

            var menuItem = (MenuItem)sender;
            var header = (string)menuItem.Header;
            var itemType = SceneTree.ItemType.None;
            switch (header)
            {
                case "Material": itemType = SceneTree.ItemType.Material; break;
                case "Camera": itemType = SceneTree.ItemType.Camera; break;
                case "Modelling Matrix": itemType = SceneTree.ItemType.ModellingMatrix; break;
                case "Texture Matrix": itemType = SceneTree.ItemType.TextureMatrix; break;
                case "Culling": itemType = SceneTree.ItemType.Culling; break;
                case "Curve Attribute": itemType = SceneTree.ItemType.CurveAttribute; break;
                case "Cylinder Attribute": itemType = SceneTree.ItemType.CylinderAttribute; break;
                case "Edge Attribute": itemType = SceneTree.ItemType.EdgeAttribute; break;
                case "Lighting Attribute": itemType = SceneTree.ItemType.LightingAttribute; break;
                case "Line Attribute": itemType = SceneTree.ItemType.LineAttribute; break;
                case "Marker Attribute": itemType = SceneTree.ItemType.MarkerAttribute; break;
                case "Surface Attribute": itemType = SceneTree.ItemType.SurfaceAttribute; break;
                case "Selectability": itemType = SceneTree.ItemType.Selectability; break;
                case "Sphere Attribute": itemType = SceneTree.ItemType.SphereAttribute; break;
                case "Subwindow": itemType = SceneTree.ItemType.Subwindow; break;
                case "Text Attribute": itemType = SceneTree.ItemType.TextAttribute; break;
                case "Transparency": itemType = SceneTree.ItemType.Transparency; break;
                case "Visibility": itemType = SceneTree.ItemType.Visibility; break;
                case "Visual Effects": itemType = SceneTree.ItemType.VisualEffects; break;
                case "Performance": itemType = SceneTree.ItemType.Performance; break;
                case "Drawing Attribute": itemType = SceneTree.ItemType.DrawingAttribute; break;
                case "Hidden Line Attribute": itemType = SceneTree.ItemType.HiddenLineAttribute; break;
                case "Material Palette": itemType = SceneTree.ItemType.MaterialPalette; break;
                case "Contour Line": itemType = SceneTree.ItemType.ContourLine; break;
                case "Condition": itemType = SceneTree.ItemType.Condition; break;
                case "Bounding": itemType = SceneTree.ItemType.Bounding; break;
                case "Attribute Lock": itemType = SceneTree.ItemType.AttributeLock; break;
                case "Transform Mask": itemType = SceneTree.ItemType.TransformMask; break;
                case "Color Interpolation": itemType = SceneTree.ItemType.ColorInterpolation; break;
                case "Cutting Section Attribute": itemType = SceneTree.ItemType.CuttingSectionAttribute; break;
                case "Priority": itemType = SceneTree.ItemType.Priority; break;
            }

            PropertiesPanel.AddProperty(SelectedTreeViewItem.sceneTreeItem, itemType);
            SelectedTreeViewItem = null;
        }

        private void UnsetAttribute_Click(object sender, RoutedEventArgs e)
        {
            PropertiesPanel.UnsetAttribute(SelectedTreeViewItem.sceneTreeItem);
            SelectedTreeViewItem = null;
        }

        public void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedTreeViewItem = GetTreeViewItemUnderCursor(e.Source as DependencyObject);
            if (SelectedTreeViewItem != null)
            {
                WPFSceneTreeItem sceneTreeItem = SelectedTreeViewItem.sceneTreeItem;
                var labelStack = (StackPanel)SelectedTreeViewItem.Header;
                if (sceneTreeItem.GetItemType() == HPS.SceneTree.ItemType.Segment)
                    labelStack.ContextMenu = SegmentContextMenu;
                else if (sceneTreeItem.HasItemType(SceneTree.ItemType.Attribute)
                        && sceneTreeItem.GetItemType() != SceneTree.ItemType.Portfolio
                        && !sceneTreeItem.GetKey().HasType(HPS.Type.WindowKey))
                    labelStack.ContextMenu = AttributeContextMenu;
                else
                    labelStack.ContextMenu = null;
            }

            e.Handled = true;
        }

        private bool CanBeDeleted(SceneTreeViewItem item)
        {
            WPFSceneTreeItem WPFTreeItem = item.sceneTreeItem;
            if (WPFTreeItem.GetItemType() == HPS.SceneTree.ItemType.Segment ||
                WPFTreeItem.HasItemType(HPS.SceneTree.ItemType.Geometry) ||
                WPFTreeItem.HasItemType(HPS.SceneTree.ItemType.GeometryGroupMask))
                return true;

            return false;
        }

        private void GetRelatives(TreeViewItem item, out TreeViewItem parent, out TreeViewItem next_sibling, out TreeViewItem prev_sibling)
        {
            parent = item.Parent as TreeViewItem;
            if (parent != null)
            {
                int index = parent.Items.IndexOf(item);
                if (index == 0)
                    prev_sibling = null;
                else
                    prev_sibling = parent.Items.GetItemAt(index - 1) as TreeViewItem;

                if (index == parent.Items.Count - 1)
                    next_sibling = null;
                else
                    next_sibling = parent.Items.GetItemAt(index + 1) as TreeViewItem;
            }
            else
            {
                next_sibling = null;
                prev_sibling = null;
            }
        }

        private void DeleteSelectedItem(TreeViewItem selected_item, TreeViewItem parent, TreeViewItem prev_sibling, TreeViewItem next_sibling)
        {
            if (next_sibling != null)
                next_sibling.IsSelected = true;
            else if (prev_sibling != null)
                prev_sibling.IsSelected = true;
            else
                parent.IsSelected = true;

            DispatcherOperation gui_operation = InvokeUIAction(delegate ()
            {
                 win._segmentTreeView.Items.Remove(selected_item);
               (parent as SceneTreeViewItem).sceneTreeItem.ReExpand();                 
            });
            gui_operation.Wait();

            win.GetSprocketsControl().Canvas.Update();
        }

        public void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Delete)
            {
                SceneTreeViewItem selected_item = win._segmentTreeView.SelectedItem as SceneTreeViewItem;
                if (selected_item != null)
                {
                    WPFSceneTreeItem selectedWPFTreeItem = selected_item.sceneTreeItem;
                    HPS.SceneTree.ItemType itemType = selectedWPFTreeItem.GetItemType();

                    TreeViewItem parent, sibling, prev_sibling;
                    if (itemType == HPS.SceneTree.ItemType.Segment)
                    {
                        GetRelatives(selected_item, out parent, out sibling, out prev_sibling);
                        if (parent == null)
                            return;

                        selectedWPFTreeItem.GetKey().Delete();
                        DeleteSelectedItem(selected_item, parent, sibling, prev_sibling);
                    }
                    else if (selectedWPFTreeItem.HasItemType(HPS.SceneTree.ItemType.Geometry))
                    {
                        GetRelatives(selected_item, out parent, out sibling, out prev_sibling);
                        selectedWPFTreeItem.GetKey().Delete();
                        DeleteSelectedItem(selected_item, parent, sibling, prev_sibling);
                    }
                    else if (selectedWPFTreeItem.HasItemType(HPS.SceneTree.ItemType.GeometryGroupMask))
                    {
                        GetRelatives(selected_item, out parent, out sibling, out prev_sibling);

                        HPS.SegmentKey groupKey = new HPS.SegmentKey(selectedWPFTreeItem.GetKey());
                        HPS.SearchResults results;
                        groupKey.Find(searchTypeMap[selectedWPFTreeItem.GetItemType()], HPS.Search.Space.SegmentOnly, out results);
                        HPS.SearchResultsIterator it = results.GetIterator();
                        while (it.IsValid())
                        {
                            it.GetItem().Delete();
                            it.Next();
                        }

                        DeleteSelectedItem(selected_item, parent, sibling, prev_sibling);
                    }
                }
            }
        }

        private SceneTreeViewItem GetTreeViewItemUnderCursor(DependencyObject obj)
        {
            while (obj != null && obj.GetType() != typeof(SceneTreeViewItem))
                obj = VisualTreeHelper.GetParent(obj);
            return obj as SceneTreeViewItem;
        }
    }

    public class WPFSceneTree : SceneTree
    {
        private SegmentBrowser _segmentBrowser;

        public WPFSceneTree(HPS.Canvas canvas, SegmentBrowser segmentBrowser) : base(canvas)
        {
            _segmentBrowser = segmentBrowser;
        }

        public override void Flush()
        {
            InvokeUIAction(delegate ()
            {
                GetTreeView().Items.Clear();
            });
            base.Flush();
        }

        public TreeView GetTreeView()
        {
            return _segmentBrowser.win._segmentTreeView;
        }

        public PropertiesPanel GetPropertiesPanel()
        {
            return _segmentBrowser.PropertiesPanel;
        }

        public DispatcherOperation InvokeUIAction(Action action)
        {
            return _segmentBrowser.InvokeUIAction(action);
        }
    }

    public class SceneTreeViewItem : TreeViewItem
    {
        public WPFSceneTreeItem sceneTreeItem { get; private set; }

        public SceneTreeViewItem(WPFSceneTreeItem in_sceneTreeItem) : base()
        {
            sceneTreeItem = in_sceneTreeItem;
        }

        protected override void OnExpanded(System.Windows.RoutedEventArgs e)
        {
            if (sceneTreeItem.GetKey().Type() == HPS.Type.None)
            {
                TreeViewItem parent = sceneTreeItem.GetTreeViewItem().Parent as TreeViewItem;
                if (parent == null)
                    sceneTreeItem.GetWPFSceneTree().GetTreeView().Items.Remove(sceneTreeItem.GetTreeViewItem());
                else
                    parent.Items.Remove(sceneTreeItem.GetTreeViewItem());
            }
            else
                sceneTreeItem.Expand();
            base.OnExpanded(e);
        }

        protected override void OnCollapsed(System.Windows.RoutedEventArgs e)
        {
            if (sceneTreeItem.GetKey().Type() == HPS.Type.None)
            {
                TreeViewItem parent = sceneTreeItem.GetTreeViewItem().Parent as TreeViewItem;
                if (parent == null)
                    sceneTreeItem.GetWPFSceneTree().GetTreeView().Items.Remove(sceneTreeItem.GetTreeViewItem());
                else
                    parent.Items.Remove(sceneTreeItem.GetTreeViewItem());
            }
            else
                sceneTreeItem.Collapse();
            base.OnCollapsed(e);
        }

        protected override void OnSelected(System.Windows.RoutedEventArgs e)
        {
            if (sceneTreeItem.GetKey().Type() == HPS.Type.None)
            {
                TreeViewItem parent = sceneTreeItem.GetTreeViewItem().Parent as TreeViewItem;
                if (parent == null)
                    sceneTreeItem.GetWPFSceneTree().GetTreeView().Items.Remove(sceneTreeItem.GetTreeViewItem());
                else
                    parent.Items.Remove(sceneTreeItem.GetTreeViewItem());
            }
            else
                sceneTreeItem.DisplayProperties();
            base.OnSelected(e);
        }
    }

    public class WPFSceneTreeItem : SceneTreeItem
    {
        private SceneTreeViewItem _treeViewItem;

        public WPFSceneTree GetWPFSceneTree()
        {
            return (WPFSceneTree)GetTree();
        }

        public WPFSceneTreeItem(SceneTree sceneTree, Model model) : base(sceneTree, model) { }
        public WPFSceneTreeItem(SceneTree sceneTree, View view) : base(sceneTree, view) { }
        public WPFSceneTreeItem(SceneTree sceneTree, Layout layout) : base(sceneTree, layout) { }
        public WPFSceneTreeItem(SceneTree sceneTree, HPS.Canvas canvas) : base(sceneTree, canvas) { }
        public WPFSceneTreeItem(SceneTree sceneTree, HPS.Key key, SceneTree.ItemType itemType, string title) : base(sceneTree, key, itemType, title) { }

        public SceneTreeViewItem GetTreeViewItem()
        {
            return _treeViewItem;
        }
        public override SceneTreeItem AddChild(HPS.Key in_key, SceneTree.ItemType in_type)
        {
            throw new NotImplementedException();
        }

        public override SceneTreeItem AddChild(HPS.Key in_key, SceneTree.ItemType in_type, string in_title)
        {
            var child = new WPFSceneTreeItem(GetTree(), in_key, in_type, in_title);
            var treeView = GetWPFSceneTree().GetTreeView();

            GetWPFSceneTree().InvokeUIAction(delegate ()
            {
                child._treeViewItem = new SceneTreeViewItem(child);

                SetTreeViewItemIconAndLabel(child, SelectionState.Unselected);

                child._treeViewItem.FontWeight = FontWeights.Normal;

                if (child.HasChildren())
                    child._treeViewItem.Items.Add(new TreeViewItem());

                if (_treeViewItem == null)
                    treeView.Items.Add(child._treeViewItem);
                else
                    _treeViewItem.Items.Add(child._treeViewItem);
            });

            return child;
        }

        public override void Expand()
        {
            bool isExpanded = false;
            DispatcherOperation gui_operation = GetWPFSceneTree().InvokeUIAction(delegate ()
            {
                if (_treeViewItem != null)
                {
                    isExpanded = _treeViewItem.IsExpanded;
                    if (isExpanded == false)
                        _treeViewItem.IsExpanded = true;
                    else
                        _treeViewItem.Items.Clear();
                }
            });


            gui_operation.Wait();
            if (isExpanded || _treeViewItem == null)
                base.Expand();
        }

        public override void Collapse()
        {
            GetWPFSceneTree().InvokeUIAction(delegate ()
            {
                if (_treeViewItem != null)
                {
                    _treeViewItem.Items.Clear();
                    if (HasChildren())
                        _treeViewItem.Items.Add(new TreeViewItem());
                }
            });
            base.Collapse();
        }

        public override void Select()
        {
            GetWPFSceneTree().InvokeUIAction(delegate ()
            {
                if (_treeViewItem != null)
                {
                    SetTreeViewItemIconAndLabel(this, SelectionState.Selected);
                    _treeViewItem.FontWeight = FontWeights.Bold;
                }
            });
            base.Select();
        }

        public override void Unselect()
        {
            GetWPFSceneTree().InvokeUIAction(delegate ()
            {
                if (_treeViewItem != null)
                {
                    SetTreeViewItemIconAndLabel(this, SelectionState.Unselected);
                    _treeViewItem.FontWeight = FontWeights.Normal;
                }
            });
            base.Unselect();
        }

        public void DisplayProperties()
        {
            GetWPFSceneTree().GetPropertiesPanel().AddProperty(this);
        }

        private enum SelectionState
        {
            Selected,
            Unselected
        }

        static private BitmapImage GetItemIcon(SceneTreeItem item, SelectionState state)
        {
            string filename = "";
            SceneTree.ItemType itemType = item.GetItemType();
            switch (itemType)
            {
                case SceneTree.ItemType.Segment:
                    filename = (state == SelectionState.Unselected ? "segment.bmp" : "highlighted_segment.bmp");
                    break;

                case SceneTree.ItemType.AttributeFilter:
                case SceneTree.ItemType.ConditionalExpression:
                case SceneTree.ItemType.NamedStyle:
                case SceneTree.ItemType.SegmentStyle:
                case SceneTree.ItemType.StyleGroup:
                case SceneTree.ItemType.Include:
                case SceneTree.ItemType.IncludeGroup:
                case SceneTree.ItemType.Portfolio:
                case SceneTree.ItemType.PortfolioGroup:
                    filename = "include.bmp";
                    break;

                case SceneTree.ItemType.StaticModelSegment:
                    filename = "static_model.bmp";
                    break;

                case SceneTree.ItemType.Reference:
                    filename = "reference.bmp";
                    break;

                default:
                    {
                        if (item.HasItemType(SceneTree.ItemType.Attribute) || itemType == SceneTree.ItemType.AttributeGroup)
                            filename = "attribute.bmp";
                        else if (item.HasItemType(SceneTree.ItemType.Geometry) || itemType == SceneTree.ItemType.GeometryGroup
                            || item.HasItemType(SceneTree.ItemType.Definition) || item.HasItemType(SceneTree.ItemType.DefinitionGroup))
                            filename = (state == SelectionState.Unselected ? "geometry.bmp" : "highlighted_geometry.bmp");
                        else if (item.HasItemType(SceneTree.ItemType.Group))
                            filename = (state == SelectionState.Unselected ? "group.bmp" : "highlighted_group.bmp");
                    }
                    break;
            }
            var image = new BitmapImage(new Uri("pack://application:,,,/Images/" + filename));
            return image;
        }

        private class SceneTreeViewItemImage : System.Windows.Controls.Image
        {
            protected override void OnMouseDown(System.Windows.Input.MouseButtonEventArgs e)
            {
                var stack = (StackPanel)this.Parent;
                var treeViewItem = (SceneTreeViewItem)stack.Parent;
                var sceneTreeItem = treeViewItem.sceneTreeItem;
                if (sceneTreeItem.IsHighlighted())
                    sceneTreeItem.Unhighlight();
                else
                    sceneTreeItem.Highlight();
                base.OnMouseDown(e);
            }
        }

        static private void SetTreeViewItemIconAndLabel(WPFSceneTreeItem item, SelectionState state)
        {
            StackPanel stack = null;

            if (item._treeViewItem.Header != null)
            {
                stack = (StackPanel)item._treeViewItem.Header;
                if (stack.Children.Count < 2)
                    stack = null;
            }

            if (stack != null)
            {
                // Re-use the existing stack panel.
                var image = (SceneTreeViewItemImage)stack.Children[0];
                image.Source = GetItemIcon(item, state);

                var textBlock = (TextBlock)stack.Children[1];
                textBlock.Text = item.GetTitle();
            }
            else
            {
                var textBlock = new TextBlock();
                textBlock.Padding = new Thickness(5);
                textBlock.VerticalAlignment = VerticalAlignment.Center;
                textBlock.Text = item.GetTitle();

                var image = new SceneTreeViewItemImage();
                image.Source = GetItemIcon(item, state);

                stack = new StackPanel();
                stack.Orientation = Orientation.Horizontal;

                stack.Children.Add(image);
                stack.Children.Add(textBlock);

                stack.Margin = new Thickness(0, -3, -3, -3);

                item._treeViewItem.Header = stack;
            }
        }
    }

    class DemoSegmentBrowserRootCommand : DemoCommand
    {
        public DemoSegmentBrowserRootCommand(MainWindow win) : base(win) { }

        public override void Execute(object parameter)
        {
            // assume model is root by default
            SegmentBrowserRoot root = SegmentBrowserRoot.Model;
            if (_win.RootComboBox.SelectedValue != null)
            {
                if (_win.RootComboBox.SelectedValue.ToString() == "View")
                    root = SegmentBrowserRoot.View;
                else if (_win.RootComboBox.SelectedValue.ToString() == "Layout")
                    root = SegmentBrowserRoot.Layout;
                else if (_win.RootComboBox.SelectedValue.ToString() == "Canvas")
                    root = SegmentBrowserRoot.Canvas;
            }
            _win.SegmentBrowser.SetRoot(root);
        }
    }

    class DemoPropertiesToggleCommand : DemoCommand
    {
        private GridLength _saveSegmentBrowserGridHeight = new GridLength(1, GridUnitType.Star);
        private GridLength _savedHorizontalSplitterHeight = new GridLength(1, GridUnitType.Auto);
        private GridLength _savePropertiesPanelHeight = new GridLength(1, GridUnitType.Star);

        public DemoPropertiesToggleCommand(MainWindow win) : base(win) { }

        public override void Execute(object parameter)
        {
            if (_win.PropertiesCheckBox.IsChecked.Value)
            {
                _win._segmentBrowserAndPropertiesGrid.RowDefinitions[0].Height = _saveSegmentBrowserGridHeight;
                _win._segmentBrowserAndPropertiesGrid.RowDefinitions[1].Height = _savedHorizontalSplitterHeight;
                _win._segmentBrowserAndPropertiesGrid.RowDefinitions[2].Height = _savePropertiesPanelHeight;
            }
            else
            {
                _saveSegmentBrowserGridHeight = _win._segmentBrowserAndPropertiesGrid.RowDefinitions[0].Height;
                _savedHorizontalSplitterHeight = _win._segmentBrowserAndPropertiesGrid.RowDefinitions[1].Height;
                _savePropertiesPanelHeight = _win._segmentBrowserAndPropertiesGrid.RowDefinitions[2].Height;

                _win._segmentBrowserAndPropertiesGrid.RowDefinitions[2].Height = new GridLength(0, GridUnitType.Star);
                _win._segmentBrowserAndPropertiesGrid.RowDefinitions[1].Height = new GridLength(0, GridUnitType.Star);
                _win._segmentBrowserAndPropertiesGrid.RowDefinitions[0].Height = new GridLength(1, GridUnitType.Star);
            }
        }
    }

    class DemoPropertiesApplyCommand : DemoCommand
    {
        public DemoPropertiesApplyCommand(MainWindow win) : base(win) { }

        public override void Execute(object parameter)
        {
            _win.SegmentBrowser.PropertiesPanel.Apply();
        }
    }

    class DemoPropertiesSelectedObjectChangedCommand : DemoCommand
    {
        public DemoPropertiesSelectedObjectChangedCommand(MainWindow win) : base(win) { }

        public override void Execute(object parameter)
        {
            PropertiesPanel.ExpandProperties(_win.PropertyGrid.Properties);
        }
    }

    class DemoPropertiesPropertyValueChangedCommand : DemoCommand
    {
        public DemoPropertiesPropertyValueChangedCommand(MainWindow win) : base(win) { }

        public override void Execute(object parameter)
        {
            var originalSource = parameter as PropertyItem;
            AttributeCollection attributes = originalSource.PropertyDescriptor.Attributes;
            var refreshPropertiesAttribute = (RefreshPropertiesAttribute)attributes[typeof(RefreshPropertiesAttribute)];
            if (refreshPropertiesAttribute.RefreshProperties == RefreshProperties.All)
                PropertiesPanel.ExpandProperties(_win.PropertyGrid.Properties);
        }
    }
}
