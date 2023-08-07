using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using HPS;

namespace wpf_sandbox
{
    public class ModelBrowser
    {
        public MainWindow Window { get; private set; }

        private WPFComponentTree WPFComponentTree { get; set; }

        public ContextMenu DefaultContextMenu { get; private set; }
        public ContextMenu LayerContextMenu { get; private set; }

        private ComponentTreeViewItem SelectedTreeViewItem { get; set; }

        public ModelBrowser(MainWindow mainWindow)
        {
            Window = mainWindow;

            DefaultContextMenu = new ContextMenu();

            var isolateMenuItem = new MenuItem();
            isolateMenuItem.Header = "Isolate";
            isolateMenuItem.Click += Isolate_Click;
            DefaultContextMenu.Items.Add(isolateMenuItem);

            var showHideMenuItem = new MenuItem();
            showHideMenuItem.Header = "Show/Hide";
            showHideMenuItem.Click += ShowHide_Click;
            DefaultContextMenu.Items.Add(showHideMenuItem);

            var resetVisibilityMenuItem = new MenuItem();
            resetVisibilityMenuItem.Header = "Reset Visibility";
            resetVisibilityMenuItem.Click += ResetVisibility_Click;
            DefaultContextMenu.Items.Add(resetVisibilityMenuItem);

            LayerContextMenu = new ContextMenu();
            var layerToggleMenuItem = new MenuItem();
            layerToggleMenuItem.Header = "Turn OFF";
            layerToggleMenuItem.Click += LayerToggle_Click;
            LayerContextMenu.Items.Add(layerToggleMenuItem);
        }

        public void Init()
        {
            Flush();

            if (Window.CADModel != null)
            {
                WPFComponentTree = new WPFComponentTree(Window.GetSprocketsControl().Canvas, this);
                WPFComponentTree.SetRoot(new WPFComponentTreeItem(WPFComponentTree, Window.CADModel));

                var highlightOptions = new HighlightOptionsKit();
                highlightOptions.SetStyleName("highlight_style").SetNotification(true);
                WPFComponentTree.SetHighlightOptions(highlightOptions);

                // auto expand a couple levels
                var root = WPFComponentTree.TreeView.Items.GetItemAt(0) as ComponentTreeViewItem;
                root.IsExpanded = true;
                var modelGroup = root.Items.GetItemAt(0) as ComponentTreeViewItem;
                modelGroup.IsExpanded = true;
           }
        }

        public void Flush()
        {
            if (WPFComponentTree != null)
            {
                WPFComponentTree.Flush();
				WPFComponentTree.Dispose();
                WPFComponentTree = null;
            }

            InvokeUIAction(delegate()
            {
                Window._modelTreeView.Items.Clear();

                var noModelItem = new TreeViewItem();
                noModelItem.Header = "No Model";
                Window._modelTreeView.Items.Add(noModelItem);
            });
        }

        public void InvokeUIAction(Action action)
        {
            Window.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(action));
        }

        private void LayerToggle_Click(object sender, RoutedEventArgs e)
        {
#if USING_DWG
            if (SelectedTreeViewItem != null)
            {
                DWG.Layer dwg_layer = new DWG.Layer(SelectedTreeViewItem.ComponentTreeItem.GetComponent());
                if (dwg_layer.IsOn())
                    dwg_layer.TurnOff();
                else
                    dwg_layer.TurnOn();

                Window.Update();
            }
#endif
        }

        private void Isolate_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTreeViewItem != null)
            {
                Window.Unhighlight();

                SelectedTreeViewItem.ComponentTreeItem.Isolate();
                Window.ZoomToKeyPath(SelectedTreeViewItem.ComponentTreeItem.GetPath().GetKeyPaths()[0]);

                Window.Update();
            }
        }

        private void ShowHide_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTreeViewItem != null)
            {
                Window.Unhighlight();

                if (SelectedTreeViewItem.ComponentTreeItem.IsHidden())
                {
                    SelectedTreeViewItem.ComponentTreeItem.Show();
                    HPS.Component[] path = { Window.CADModel };
                    if (!new ComponentPath(path).IsHidden(Window.GetSprocketsControl().Canvas))
                        Window.InvalidatePreZoomToKeyPathCamera();
                }
                else
                    SelectedTreeViewItem.ComponentTreeItem.Hide();

                Window.Update();
            }
        }

        private void ResetVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (Window.CADModel != null)
            {
                Window.CADModel.ResetVisibility(Window.GetSprocketsControl().Canvas);
                Window.RestorePreZoomToKeyPathCamera();
                Window.Update();
            }
        }

        public void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedTreeViewItem = GetTreeViewItemUnderCursor(e.Source as DependencyObject);
            if (SelectedTreeViewItem != null)
            {
                SelectedTreeViewItem.ContextMenu = DefaultContextMenu;
                var component = SelectedTreeViewItem.ComponentTreeItem.GetComponent();
                var componentType = component.GetComponentType();
                if (component.HasComponentType(Component.ComponentType.ExchangeComponentMask))
                {
                    var showHideMenuItem = DefaultContextMenu.Items.GetItemAt(1) as MenuItem;
                    if (SelectedTreeViewItem.ComponentTreeItem.IsHidden())
                        showHideMenuItem.Header = "Show";
                    else
                        showHideMenuItem.Header = "Hide";
                }
                else if (componentType == Component.ComponentType.DWGLayer)
                {
#if USING_DWG
                    SelectedTreeViewItem.ContextMenu = LayerContextMenu;
                    var layerToggleMenu = LayerContextMenu.Items.GetItemAt(0) as MenuItem;
                    DWG.Layer dwg_layer = new DWG.Layer(component);
                    if (dwg_layer.IsOn())
                        layerToggleMenu.Header = "Turn OFF";
                    else
                        layerToggleMenu.Header = "Turn ON";
#endif
                }
                else if (componentType == Component.ComponentType.DWGModelFile ||
					componentType == Component.ComponentType.DWGBlockTable ||
					componentType == Component.ComponentType.DWGLayerTable ||
                    componentType == Component.ComponentType.DWGLayout)
					return;
            }
            else
                e.Handled = true;
        }

        private ComponentTreeViewItem GetTreeViewItemUnderCursor(DependencyObject obj)
        {
            while (obj != null && obj.GetType() != typeof(ComponentTreeViewItem))
                obj = VisualTreeHelper.GetParent(obj);
            return obj as ComponentTreeViewItem;
        }
    }

    public class WPFComponentTree : ComponentTree
    {
        public ModelBrowser Browser { get; private set; }

        public TreeView TreeView { get { return Browser.Window._modelTreeView; } }

        public WPFComponentTree(HPS.Canvas canvas, ModelBrowser modelBrowser) : base(canvas)
        {
            Browser = modelBrowser;
        }

        public override void Flush()
        {
            InvokeUIAction(delegate()
            {
                TreeView.Items.Clear();
            });
            base.Flush();
        }

        public void InvokeUIAction(Action action)
        {
            Browser.InvokeUIAction(action);
        }
    }

    public class WPFComponentTreeItem : ComponentTreeItem
    {
        public TreeViewItem TreeViewItem { get; private set; }

        public WPFComponentTreeItem(ComponentTree componentTree, CADModel cadModel) : base(componentTree, cadModel) { }

        public WPFComponentTreeItem(ComponentTree componentTree, Component component, ComponentTree.ItemType type) : base(componentTree, component, type) { }

        public override ComponentTreeItem AddChild(Component component, ComponentTree.ItemType type)
        {
            var child = new WPFComponentTreeItem(GetTree(), component, type);
            var wpfComponentTree = GetWPFComponentTree();
            var treeView = wpfComponentTree.TreeView;

            wpfComponentTree.InvokeUIAction(delegate()
            {
                child.TreeViewItem = new ComponentTreeViewItem(child);
                child.TreeViewItem.FontWeight = FontWeights.Normal;
                SetTreeViewItemIconAndLabel(child, VisibilityState.Shown);

                if (child.HasChildren())
                    child.TreeViewItem.Items.Add(new TreeViewItem());

                if (TreeViewItem == null)
                    treeView.Items.Add(child.TreeViewItem);
                else
                    TreeViewItem.Items.Add(child.TreeViewItem);
            });

            return child;
        }

        public override void Expand()
        {
            bool isExpanded = false;
            GetWPFComponentTree().InvokeUIAction(delegate()
            {
                if (TreeViewItem != null)
                {
                    isExpanded = TreeViewItem.IsExpanded;
                    if (isExpanded == false)
                        TreeViewItem.IsExpanded = true;
                    else
                        TreeViewItem.Items.Clear();
                }
            });
            if (isExpanded || TreeViewItem == null)
                base.Expand();
        }

        public override void Collapse()
        {
            GetWPFComponentTree().InvokeUIAction(delegate()
            {
                if (TreeViewItem != null)
                {
                    TreeViewItem.Items.Clear();
                    if (HasChildren())
                        TreeViewItem.Items.Add(new TreeViewItem());
                }
            });
            base.Collapse();
        }

        public override void OnHighlight(HighlightOptionsKit in_options)
        {
            base.OnHighlight(in_options);

            var wpfComponentTree = GetWPFComponentTree();
            if (wpfComponentTree != null)
            {
                wpfComponentTree.InvokeUIAction(delegate()
                {
                    TreeViewItem.FontWeight = FontWeights.Bold;
                });
            }
        }

        public override void OnUnhighlight(HighlightOptionsKit in_options)
        {
            base.OnUnhighlight(in_options);

            var wpfComponentTree = GetWPFComponentTree();
            if (wpfComponentTree != null)
            {
                wpfComponentTree.InvokeUIAction(delegate()
                {
                    TreeViewItem.FontWeight = FontWeights.Normal;
                });
            }
        }

        public override void OnShow()
        {
            base.OnShow();

            var wpfComponentTree = GetWPFComponentTree();
            if (wpfComponentTree != null)
            {
                wpfComponentTree.InvokeUIAction(delegate()
                {
                    SetTreeViewItemIconAndLabel(this, VisibilityState.Shown);
                });
            }
        }

        public override void OnHide()
        {
            base.OnHide();

            var wpfComponentTree = GetWPFComponentTree();
            if (wpfComponentTree != null && TreeViewItem != null)
            {
                wpfComponentTree.InvokeUIAction(delegate()
                {
                    SetTreeViewItemIconAndLabel(this, VisibilityState.Hidden);
                });
            }
        }

        public WPFComponentTree GetWPFComponentTree() { return (WPFComponentTree)GetTree(); }

        private enum VisibilityState
        {
            Shown,
            Hidden
        }

        static private void SetTreeViewItemIconAndLabel(WPFComponentTreeItem item, VisibilityState state)
        {
            var textBlock = new TextBlock();
            textBlock.Padding = new Thickness(5);
            textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Text = item.GetTitle();

            var image = new System.Windows.Controls.Image();
            image.Source = GetItemIcon(item, state);

            var stack = new StackPanel();
            stack.Orientation = Orientation.Horizontal;
            stack.Children.Add(image);
            stack.Children.Add(textBlock);
            stack.Margin = new Thickness(0, -3, -3, -3);

            item.TreeViewItem.Header = stack;
        }

        static private BitmapImage GetItemIcon(WPFComponentTreeItem item, VisibilityState state)
        {
            string filename = "img_unkn.png";
            ComponentTree.ItemType itemType = item.GetItemType();
            Component.ComponentType componentType = Component.ComponentType.None;
            switch (itemType)
            {
                case ComponentTree.ItemType.ExchangeViewGroup:
                case ComponentTree.ItemType.ExchangeAnnotationViewGroup:
                case ComponentTree.ItemType.ExchangePMIGroup:
                    filename = "dir_mark.png";
                    break;

                case ComponentTree.ItemType.ExchangeModelGroup:
                    filename = "dir_geom.png";
                    break;

                case ComponentTree.ItemType.DWGModelFile:
                case ComponentTree.ItemType.ExchangeModelFile:
                    filename = "il_class.png";
                    break;

                case ComponentTree.ItemType.ExchangeComponent:
                    {
                        Component component = item.GetComponent();
                        componentType = component.GetComponentType();
                        switch (componentType)
                        {
                            case Component.ComponentType.ExchangeProductOccurrence:
                                {
                                    // different icons if this product occurrence has product occurrences as children or not
                                    if (Array.Find(component.GetSubcomponents(), comp => comp.GetComponentType() == Component.ComponentType.ExchangeProductOccurrence) == null)
                                        filename = "dir_product2.png";
                                    else
                                        filename = "dir_product.png";
                                }   break;

                            case Component.ComponentType.ExchangeRISet:
                                filename = "dir_geom.png";
                                break;

                            case Component.ComponentType.ExchangeRIPlane:
                                filename = "img_plan.png";
                                break;

                            case Component.ComponentType.ExchangeRIDirection:
                                filename = "img_direction.png";
                                break;

                            case Component.ComponentType.ExchangeRICoordinateSystem:
                                filename = "img_axis.png";
                                break;

                            case Component.ComponentType.ExchangeRIBRepModel:
                            case Component.ComponentType.ExchangeRIPolyBRepModel:
                                filename = "img_solid.png";
                                break;

                            case Component.ComponentType.ExchangeRICurve:
                                filename = "img_curv.png";
                                break;

                            case Component.ComponentType.ExchangeRIPolyWire:
                                filename = "img_compcurve.png";
                                break;

                            case Component.ComponentType.ExchangeRIPointSet:
                                filename = "img_points_cloud.png";
                                break;

                            case Component.ComponentType.ExchangeView:
                                {
                                    var isAnnotationCapture = new BooleanMetadata(component.GetMetadata("IsAnnotationCapture"));
                                    if (isAnnotationCapture.GetValue())
                                        filename = "cadview.png";
                                    else
                                        filename = "pmiview_productview.png";
                                }   break;

                            case Component.ComponentType.ExchangePMI:
                            case Component.ComponentType.ExchangePMIText:
                            case Component.ComponentType.ExchangePMIRichText:
                                filename = "markup.png";
                                break;

                            case Component.ComponentType.ExchangePMIGDT:
                                filename = "cadtoler.png";
                                break;

                            case Component.ComponentType.ExchangePMIRoughness:
                                filename = "cadrough.png";
                                break;

                            case Component.ComponentType.ExchangePMILineWelding:
                                filename = "cadlinewelding.png";
                                break;

                            case Component.ComponentType.ExchangePMISpotWelding:
                                filename = "cadspotwelding.png";
                                break;

                            case Component.ComponentType.ExchangePMIDatum:
                                filename = "cadrefer.png";
                                break;

                            case Component.ComponentType.ExchangePMIDimension:
                                filename = "dim_distance.png";
                                break;

                            case Component.ComponentType.ExchangePMIBalloon:
                                filename = "cadballoon.png";
                                break;

                            case Component.ComponentType.ExchangePMICoordinate:
                                filename = "dim_coordinate.png";
                                break;

                            case Component.ComponentType.ExchangePMIFastener:
                                filename = "cadFastener.png";
                                break;

                            case Component.ComponentType.ExchangePMILocator:
                                filename = "cadlocator.png";
                                break;

                            case Component.ComponentType.ExchangePMIMeasurementPoint:
                                filename = "cadmeasurementpoint.png";
                                break;

                            case Component.ComponentType.ExchangeDrawingModel:
                                filename = "img_drawing_model.png";
                                break;

                            case Component.ComponentType.ExchangeDrawingSheet:
                                filename = "img_drawing_sheet.png";
                                break;

                            case Component.ComponentType.ExchangeDrawingView:
                                filename = "img_drawing_view.png";
                                break;
                        }
                    }   break;

                case ComponentTree.ItemType.ParasolidModelFile:
                    filename = "il_class.png";
                    break;

                case ComponentTree.ItemType.ParasolidComponent:
                    {
                        switch (item.GetComponent().GetComponentType())
                        {
                            case Component.ComponentType.ParasolidAssembly:
                                filename = "dir_product2.png";
                                break;

                            case Component.ComponentType.ParasolidTopoBody:
                                filename = "img_solid.png";
                                break;

                            case Component.ComponentType.ParasolidInstance:
                                {
                                    Component.ComponentType component_type = item.GetComponent().GetSubcomponents()[0].GetComponentType();
                                    if (component_type == HPS.Component.ComponentType.ParasolidTopoBody)
                                        filename = "img_solid.png";
                                    else if (component_type == HPS.Component.ComponentType.ParasolidAssembly)
                                        filename = "dir_product2.png";
                                } break;
                        }
                    }   break;

                case ComponentTree.ItemType.DWGComponent:
                    {
                        switch (item.GetComponent().GetComponentType())
                        {
                            case Component.ComponentType.DWGBlockTable:
                            case Component.ComponentType.DWGLayerTable:
                            case Component.ComponentType.DWGBlockTableRecord:
                                filename = "dir_geom.png";
                                break;

                            case Component.ComponentType.DWGLayer:
                                filename = "img_drawing_sheet.png";
                                break;

                            case Component.ComponentType.DWGEntity:
                                filename = "img_solid.png";
                                break;

                            case Component.ComponentType.DWGLayout:
                                filename = "cadview.png";
                                break;
                        }
                    } break;
            }

            if (state == VisibilityState.Hidden
                && itemType != ComponentTree.ItemType.ExchangeModelFile
                && itemType != ComponentTree.ItemType.ExchangeViewGroup
                && itemType != ComponentTree.ItemType.ExchangeAnnotationViewGroup
                && itemType != ComponentTree.ItemType.ExchangePMIGroup
                && (itemType != ComponentTree.ItemType.ExchangeComponent || componentType != Component.ComponentType.ExchangeView)
                && itemType != ComponentTree.ItemType.ParasolidModelFile)
                filename = "hidden_" + filename;

            return new BitmapImage(new Uri("pack://application:,,,/Images/" + filename));
        }
    }

    public class ComponentTreeViewItem : TreeViewItem
    {
        public WPFComponentTreeItem ComponentTreeItem { get; private set; }

        public ComponentTreeViewItem(WPFComponentTreeItem componentTreeItem) : base()
        {
            ComponentTreeItem = componentTreeItem;
            ContextMenu = ComponentTreeItem.GetWPFComponentTree().Browser.DefaultContextMenu;
        }

        protected override void OnExpanded(RoutedEventArgs e)
        {
            ComponentTreeItem.Expand();
            base.OnExpanded(e);
        }

        protected override void OnCollapsed(RoutedEventArgs e)
        {
            ComponentTreeItem.Collapse();
            base.OnCollapsed(e);
        }

        protected override void OnSelected(RoutedEventArgs e)
        {
            ComponentTree.ItemType itemType = ComponentTreeItem.GetItemType();
            Component component = ComponentTreeItem.GetComponent();
            Component.ComponentType componentType = component.GetComponentType();
            if ((itemType == ComponentTree.ItemType.ExchangeComponent && componentType != Component.ComponentType.ExchangeFilter)
                || itemType == ComponentTree.ItemType.ParasolidComponent || itemType == ComponentTree.ItemType.DWGComponent)
            {
#if USING_EXCHANGE
                if (componentType == Component.ComponentType.ExchangeView)
                {
                    var capture_path = ComponentTreeItem.GetPath();
                    ComponentTreeItem.GetWPFComponentTree().Browser.Window.ActivateCapture(capture_path);
                }
                else if (componentType == Component.ComponentType.ExchangeDrawingSheet)
                {
                    var sheet = new Exchange.Sheet(component);
                    ComponentTreeItem.GetWPFComponentTree().Browser.Window.AttachView(sheet.Activate());
                }
                else
#elif USING_DWG
                if (componentType == Component.ComponentType.DWGLayout)
                {
                    var layout = new DWG.Layout(component);
                    ComponentTreeItem.GetWPFComponentTree().Browser.Window.ActivateCapture(layout);
                    return;
                }
                else
#endif
                if (ComponentTreeItem.IsHidden() == false)
                {
#if USING_EXCHANGE || USING_PARASOLID || USING_DWG
                    if (componentType != HPS.Component.ComponentType.DWGBlockTable &&
				        componentType != HPS.Component.ComponentType.DWGLayerTable &&
				        componentType != HPS.Component.ComponentType.DWGLayer)
                    {
                        var window = ComponentTreeItem.GetWPFComponentTree().Browser.Window;
                        window.Unhighlight();

                        ComponentTreeItem.Highlight();

                        window.Update();
                    }
#endif
                }
            }

            base.OnSelected(e);
        }
    }
}
