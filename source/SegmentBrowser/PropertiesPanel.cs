using Xceed.Wpf.Toolkit.PropertyGrid;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using HPS;

namespace wpf_sandbox
{
    public class PropertiesPanel
    {
        private PropertyGrid propertyGrid { get; set; }
        private IRootProperty rootProperty { get; set; }
        private WPFSceneTreeItem sceneTreeItem { get; set; }
        private Button applyButton { get; set; }
        private HPS.Canvas canvas { get; set; }

        public PropertiesPanel(
            MainWindow win)
        {
            propertyGrid = win.PropertyGrid;
            applyButton = win.PropertiesApply;
            canvas = win.GetSprocketsControl().Canvas;

            Flush();
        }

        public void Flush()
        {
            propertyGrid.SelectedObject = null;
            rootProperty = null;
            sceneTreeItem = null;
            applyButton.IsEnabled = false;
        }

        public void AddProperty(WPFSceneTreeItem sceneTreeItem)
        {
            AddProperty(sceneTreeItem, sceneTreeItem.GetItemType());
        }

        public void AddProperty(WPFSceneTreeItem sceneTreeItem, SceneTree.ItemType itemType)
        {
            Flush();

            Key key = sceneTreeItem.GetKey();
            if (key.Type() == HPS.Type.None)
                return;

            switch (itemType)
            {
                case SceneTree.ItemType.Segment:
                    {
                        HPS.Type keyType = key.Type();
                        if (keyType == HPS.Type.ApplicationWindowKey)
                        {
                            var window = new HPS.ApplicationWindowKey(key);
                            rootProperty = new ApplicationWindowKeyProperty(window);
                            propertyGrid.SelectedObject = rootProperty;
                        }
                        else if (keyType == HPS.Type.OffScreenWindowKey)
                        {
                            var window = new HPS.OffScreenWindowKey(key);
                            rootProperty = new OffScreenWindowKeyProperty(window);
                            propertyGrid.SelectedObject = rootProperty;
                        }
                        else
                        {
                            var segment = new HPS.SegmentKey(key);
                            rootProperty = new SegmentKeyProperty(segment, sceneTreeItem.GetKeyPath());
                            propertyGrid.SelectedObject = rootProperty;
                        }
                    }
                    break;

                case SceneTree.ItemType.AttributeFilter:
                    {
                        HPS.Type keyType = key.Type();
                        if (keyType == HPS.Type.StyleKey)
                        {
                            var style = new HPS.StyleKey(key);
                            rootProperty = new StyleKeyAttributeFilterProperty(style);
                            propertyGrid.SelectedObject = rootProperty;
                        }
                        else if (keyType == HPS.Type.IncludeKey)
                        {
                            var include = new HPS.IncludeKey(key);
                            rootProperty = new IncludeKeyAttributeFilterProperty(include);
                            propertyGrid.SelectedObject = rootProperty;
                        }
                    }
                    break;

                #region Attributes
                case SceneTree.ItemType.Material:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new MaterialMappingKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Camera:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new CameraKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.ModellingMatrix:
                    {
                        if (key.HasType(HPS.Type.SegmentKey))
                        {
                            var segment = new HPS.SegmentKey(key);
                            rootProperty = new SegmentKeyModellingMatrixProperty(segment);
                            propertyGrid.SelectedObject = rootProperty;
                        }
                        else if (key.Type() == HPS.Type.ReferenceKey)
                        {
                            var reference = new HPS.ReferenceKey(key);
                            rootProperty = new ReferenceKeyModellingMatrixProperty(reference);
                            propertyGrid.SelectedObject = rootProperty;
                        }
                    }
                    break;

                case SceneTree.ItemType.UserData:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new SegmentKeyUserDataProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.TextureMatrix:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new SegmentKeyTextureMatrixProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Culling:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new CullingKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.CurveAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new CurveAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.CylinderAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new CylinderAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.EdgeAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new EdgeAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.LightingAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new LightingAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.LineAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new LineAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.MarkerAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new MarkerAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.SurfaceAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new NURBSSurfaceAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Selectability:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new SelectabilityKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.SphereAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new SphereAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Subwindow:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new SubwindowKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.TextAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new TextAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Transparency:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new TransparencyKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Visibility:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new VisibilityKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.VisualEffects:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new VisualEffectsKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Performance:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new PerformanceKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.DrawingAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new DrawingAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.HiddenLineAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new HiddenLineAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.MaterialPalette:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new SegmentKeyMaterialPaletteProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.ContourLine:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new ContourLineKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Condition:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new SegmentKeyConditionProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Bounding:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new BoundingKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.AttributeLock:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new AttributeLockKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.TransformMask:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new TransformMaskKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.ColorInterpolation:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new ColorInterpolationKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.CuttingSectionAttribute:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new CuttingSectionAttributeKitProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Priority:
                    {
                        var segment = new HPS.SegmentKey(key);
                        rootProperty = new SegmentKeyPriorityProperty(segment);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;
                #endregion

                #region Window-only Attributes
                case SceneTree.ItemType.Debugging:
                    {
                        var window = new HPS.WindowKey(key);
                        rootProperty = new DebuggingKitProperty(window);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.PostProcessEffects:
                    {
                        var window = new HPS.WindowKey(key);
                        rootProperty = new PostProcessEffectsKitProperty(window);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.SelectionOptions:
                    {
                        var window = new HPS.WindowKey(key);
                        rootProperty = new SelectionOptionsKitProperty(window);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.UpdateOptions:
                    {
                        var window = new HPS.WindowKey(key);
                        rootProperty = new UpdateOptionsKitProperty(window);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;
                #endregion

                #region Geometry
                case SceneTree.ItemType.CuttingSection:
                    {
                        var geometry = new HPS.CuttingSectionKey(key);
                        rootProperty = new CuttingSectionKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Shell:
                    {
                        var geometry = new HPS.ShellKey(key);
                        rootProperty = new ShellKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Mesh:
                    {
                        var geometry = new HPS.MeshKey(key);
                        rootProperty = new MeshKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Grid:
                    {
                        var geometry = new HPS.GridKey(key);
                        rootProperty = new GridKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.NURBSSurface:
                    {
                        var geometry = new HPS.NURBSSurfaceKey(key);
                        rootProperty = new NURBSSurfaceKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Cylinder:
                    {
                        var geometry = new HPS.CylinderKey(key);
                        rootProperty = new CylinderKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Sphere:
                    {
                        var geometry = new HPS.SphereKey(key);
                        rootProperty = new SphereKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Polygon:
                    {
                        var geometry = new HPS.PolygonKey(key);
                        rootProperty = new PolygonKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Circle:
                    {
                        var geometry = new HPS.CircleKey(key);
                        rootProperty = new CircleKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.CircularWedge:
                    {
                        var geometry = new HPS.CircularWedgeKey(key);
                        rootProperty = new CircularWedgeKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Ellipse:
                    {
                        var geometry = new HPS.EllipseKey(key);
                        rootProperty = new EllipseKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Line:
                    {
                        var geometry = new HPS.LineKey(key);
                        rootProperty = new LineKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.NURBSCurve:
                    {
                        var geometry = new HPS.NURBSCurveKey(key);
                        rootProperty = new NURBSCurveKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.CircularArc:
                    {
                        var geometry = new HPS.CircularArcKey(key);
                        rootProperty = new CircularArcKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.EllipticalArc:
                    {
                        var geometry = new HPS.EllipticalArcKey(key);
                        rootProperty = new EllipticalArcKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.InfiniteLine:
                case SceneTree.ItemType.InfiniteRay:
                    {
                        var geometry = new HPS.InfiniteLineKey(key);
                        rootProperty = new InfiniteLineKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Marker:
                    {
                        var geometry = new HPS.MarkerKey(key);
                        rootProperty = new MarkerKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Text:
                    {
                        var geometry = new HPS.TextKey(key);
                        rootProperty = new TextKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Reference:
                    {
                        // should we do anything in this case?
                    }
                    break;

                case SceneTree.ItemType.DistantLight:
                    {
                        var geometry = new HPS.DistantLightKey(key);
                        rootProperty = new DistantLightKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.Spotlight:
                    {
                        var geometry = new HPS.SpotlightKey(key);
                        rootProperty = new SpotlightKitProperty(geometry);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;
                #endregion

                #region Definitions
                case SceneTree.ItemType.MaterialPaletteDefinition:
                    {
                        var portfolio = new HPS.PortfolioKey(key);
                        HPS.MaterialPaletteDefinition definition;
                        portfolio.ShowMaterialPaletteDefinition(sceneTreeItem.GetTitle(), out definition);
                        rootProperty = new MaterialPaletteDefinitionProperty(definition);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.TextureDefinition:
                    {
                        var portfolio = new HPS.PortfolioKey(key);
                        HPS.TextureDefinition definition;
                        portfolio.ShowTextureDefinition(sceneTreeItem.GetTitle(), out definition);
                        rootProperty = new TextureDefinitionProperty(definition);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.CubeMapDefinition:
                    {
                        var portfolio = new HPS.PortfolioKey(key);
                        HPS.CubeMapDefinition definition;
                        portfolio.ShowCubeMapDefinition(sceneTreeItem.GetTitle(), out definition);
                        rootProperty = new CubeMapDefinitionProperty(definition);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.ImageDefinition:
                    {
                        var portfolio = new HPS.PortfolioKey(key);
                        HPS.ImageDefinition definition;
                        portfolio.ShowImageDefinition(sceneTreeItem.GetTitle(), out definition);
                        rootProperty = new ImageDefinitionProperty(definition);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.LegacyShaderDefinition:
                    {
                        var portfolio = new HPS.PortfolioKey(key);
                        HPS.LegacyShaderDefinition definition;
                        portfolio.ShowLegacyShaderDefinition(sceneTreeItem.GetTitle(), out definition);
                        rootProperty = new LegacyShaderDefinitionProperty(definition);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.LinePatternDefinition:
                    {
                        var portfolio = new HPS.PortfolioKey(key);
                        HPS.LinePatternDefinition definition;
                        portfolio.ShowLinePatternDefinition(sceneTreeItem.GetTitle(), out definition);
                        rootProperty = new LinePatternDefinitionProperty(definition);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.GlyphDefinition:
                    {
                        var portfolio = new HPS.PortfolioKey(key);
                        HPS.GlyphDefinition definition;
                        portfolio.ShowGlyphDefinition(sceneTreeItem.GetTitle(), out definition);
                        rootProperty = new GlyphDefinitionProperty(definition);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;

                case SceneTree.ItemType.ShapeDefinition:
                    {
                        var portfolio = new HPS.PortfolioKey(key);
                        HPS.ShapeDefinition definition;
                        portfolio.ShowShapeDefinition(sceneTreeItem.GetTitle(), out definition);
                        rootProperty = new ShapeDefinitionProperty(definition);
                        propertyGrid.SelectedObject = rootProperty;
                    }
                    break;
                    #endregion
            }

            if (rootProperty != null)
            {
                this.sceneTreeItem = sceneTreeItem;
                applyButton.IsEnabled = true;
            }
        }

        public void UnsetAttribute(WPFSceneTreeItem sceneTreeItem)
        {
            if (!sceneTreeItem.HasItemType(SceneTree.ItemType.Attribute))
                return;

            bool unsetAttribute = true;
            var itemType = sceneTreeItem.GetItemType();
            var segment = new HPS.SegmentKey(sceneTreeItem.GetKey());
            switch (itemType)
            {
                case SceneTree.ItemType.Material:
                    {
                        segment.UnsetMaterialMapping();
                    }
                    break;

                case SceneTree.ItemType.Camera:
                    {
                        segment.UnsetCamera();
                    }
                    break;

                case SceneTree.ItemType.ModellingMatrix:
                    {
                        segment.UnsetModellingMatrix();
                    }
                    break;

                case SceneTree.ItemType.UserData:
                    {
                        segment.UnsetAllUserData();
                    }
                    break;

                case SceneTree.ItemType.TextureMatrix:
                    {
                        segment.UnsetTextureMatrix();
                    }
                    break;

                case SceneTree.ItemType.Culling:
                    {
                        segment.UnsetCulling();
                    }
                    break;

                case SceneTree.ItemType.CurveAttribute:
                    {
                        segment.UnsetCurveAttribute();
                    }
                    break;

                case SceneTree.ItemType.CylinderAttribute:
                    {
                        segment.UnsetCylinderAttribute();
                    }
                    break;

                case SceneTree.ItemType.EdgeAttribute:
                    {
                        segment.UnsetEdgeAttribute();
                    }
                    break;

                case SceneTree.ItemType.LightingAttribute:
                    {
                        segment.UnsetLightingAttribute();
                    }
                    break;

                case SceneTree.ItemType.LineAttribute:
                    {
                        segment.UnsetLineAttribute();
                    }
                    break;

                case SceneTree.ItemType.MarkerAttribute:
                    {
                        segment.UnsetMarkerAttribute();
                    }
                    break;

                case SceneTree.ItemType.SurfaceAttribute:
                    {
                        segment.UnsetNURBSSurfaceAttribute();
                    }
                    break;

                case SceneTree.ItemType.Selectability:
                    {
                        segment.UnsetSelectability();
                    }
                    break;

                case SceneTree.ItemType.SphereAttribute:
                    {
                        segment.UnsetSphereAttribute();
                    }
                    break;

                case SceneTree.ItemType.Subwindow:
                    {
                        segment.UnsetSubwindow();
                    }
                    break;

                case SceneTree.ItemType.TextAttribute:
                    {
                        segment.UnsetTextAttribute();
                    }
                    break;

                case SceneTree.ItemType.Transparency:
                    {
                        segment.UnsetTransparency();
                    }
                    break;

                case SceneTree.ItemType.Visibility:
                    {
                        segment.UnsetVisibility();
                    }
                    break;

                case SceneTree.ItemType.VisualEffects:
                    {
                        segment.UnsetVisualEffects();
                    }
                    break;

                case SceneTree.ItemType.Performance:
                    {
                        segment.UnsetPerformance();
                    }
                    break;

                case SceneTree.ItemType.DrawingAttribute:
                    {
                        segment.UnsetDrawingAttribute();
                    }
                    break;

                case SceneTree.ItemType.HiddenLineAttribute:
                    {
                        segment.UnsetHiddenLineAttribute();
                    }
                    break;

                case SceneTree.ItemType.MaterialPalette:
                    {
                        segment.UnsetMaterialPalette();
                    }
                    break;

                case SceneTree.ItemType.ContourLine:
                    {
                        segment.UnsetContourLine();
                    }
                    break;

                case SceneTree.ItemType.Condition:
                    {
                        segment.UnsetConditions();
                    }
                    break;

                case SceneTree.ItemType.Bounding:
                    {
                        segment.UnsetBounding();
                    }
                    break;

                case SceneTree.ItemType.AttributeLock:
                    {
                        segment.UnsetAttributeLock();
                    }
                    break;

                case SceneTree.ItemType.TransformMask:
                    {
                        segment.UnsetTransformMask();
                    }
                    break;

                case SceneTree.ItemType.ColorInterpolation:
                    {
                        segment.UnsetColorInterpolation();
                    }
                    break;

                case SceneTree.ItemType.CuttingSectionAttribute:
                    {
                        segment.UnsetCuttingSectionAttribute();
                    }
                    break;

                case SceneTree.ItemType.Priority:
                    {
                        segment.UnsetPriority();
                    }
                    break;

                default:
                    {
                        unsetAttribute = false;
                    }
                    break;
            }

            if (unsetAttribute)
            {
                this.sceneTreeItem = sceneTreeItem;
                ReExpandTree();
                canvas.Update();
                Flush();
            }
        }

        public void Apply()
        {
            if (rootProperty != null)
            {
                rootProperty.Apply();
                ReExpandTree();
                canvas.Update();
                Flush();
            }
        }

        private void ReExpandTree()
        {
            if (sceneTreeItem.HasItemType(SceneTree.ItemType.Attribute))
            {
                var attributeGroupItem = GetTreeViewItemParent(sceneTreeItem.GetTreeViewItem());
                var segmentItem = GetTreeViewItemParent(attributeGroupItem);
                segmentItem.sceneTreeItem.ReExpand();
            }
            else if (sceneTreeItem.GetItemType() == SceneTree.ItemType.Segment)
                sceneTreeItem.ReExpand();
        }

        private SceneTreeViewItem GetTreeViewItemParent(SceneTreeViewItem item)
        {

            DependencyObject parent = VisualTreeHelper.GetParent(item);
            while (!(parent is TreeViewItem || parent is TreeView))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return parent as SceneTreeViewItem;
        }

        public static void ExpandProperties(
            System.Collections.IList properties)
        {
            foreach (PropertyItem property in properties)
            {
                if (property.IsExpandable)
                {
                    property.IsExpanded = true;
                    ExpandProperties(property.Properties);
                }
            }
        }
    }
}
