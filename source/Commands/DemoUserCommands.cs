using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HPS;
namespace wpf_sandbox
{
	/// <summary>
	/// Base class for Demo User Commands
	/// 
	/// Contains helper methods for DemoUserCommands
	/// </summary>
	abstract class DemoUserCommand : DemoCommand
	{
		public DemoUserCommand(MainWindow win)
			: base(win)
		{

		}

		/// <summary>
		/// Returns SprocketsWPFControl attached as a child of the _mainBorder WPF control
		/// </summary>
		/// <returns>SprocketsWPFControl attached to _mainBorder Child</returns>
		protected SprocketsWPFControl GetSprocketsControl()
		{
			return _win.GetSprocketsControl();
		}

		/// <summary>
		/// Returns Canvas contained in SprocketsWPFControl
		/// </summary>
		/// <returns>Main Sprockets.Canvas</returns>
		protected HPS.Canvas GetCanvas()
		{
			// The SprocketsWPFControl contains our Sprockets.Canvas
			return GetSprocketsControl().Canvas;
		}

		/// <summary>
		/// Returns HPS.WindowKey associated with Sprockets.Canvas
		/// </summary>
		/// <returns>Main HPS.WindowKey</returns>
		protected HPS.WindowKey GetWindowKey()
		{
			// The Canvas has direct access to the HPS.WindowKey
			return GetCanvas().GetWindowKey();
		}
	}

	/// <summary>
	/// Demo User Command 1 Handler
	/// </summary>
	class DemoUser1Command : DemoUserCommand
	{
		private bool displayResourceMonitor = false;

		public DemoUser1Command(MainWindow win)
			: base(win)
		{

		}

		public override void Execute(object parameter)
		{
			// Toggle display of resource monitor using the DebuggingControl
			//displayResourceMonitor = !displayResourceMonitor;
			GetWindowKey().GetDebuggingControl().SetResourceMonitor(displayResourceMonitor);


            HPS.Model myModel = HPS.Factory.CreateModel();

            HPS.View myView = GetCanvas().GetAttachedLayout().GetAttachedView();

            // attach the Model to the View
            myView.AttachModel(myModel);

            // you can also get a reference to the Model segment
            HPS.SegmentKey myModelKey = myModel.GetSegmentKey();

          

            myView.GetAxisTriadControl().SetVisibility(true).SetLocation(HPS.AxisTriadControl.Location.BottomRight).SetInteractivity(true);
            


            // enable navigation cube
            myView.GetNavigationCubeControl().SetVisibility(true).SetLocation(HPS.NavigationCubeControl.Location.BottomLeft);
            // Trigger update
            GetCanvas().Update();
		}
	}

	/// <summary>
	/// Demo User Command 2 Handler
	/// </summary>
	class DemoUser2Command : DemoUserCommand
	{
		public DemoUser2Command(MainWindow win)
			: base(win)
		{

		}

		public override void Execute(object parameter)
		{
            // create the Model object
            HPS.Model myModel = HPS.Factory.CreateModel();

            // get a reference to the View
            HPS.View myView = GetCanvas().GetAttachedLayout().GetAttachedView();

            // attach the Model to the View
            myView.AttachModel(myModel);

            // you can also get a reference to the Model segment
            HPS.SegmentKey myModelKey = myModel.GetSegmentKey();

            HPS.CameraKit cameraKit = new HPS.CameraKit();
            cameraKit.SetPosition(new HPS.Point(10, 5, 30)).SetTarget(new HPS.Point(0.5f, -0.4f, 0.8f)).
                SetUpVector(new HPS.Vector(0, 1, 0)).SetField(9.21f, 9.21f).SetProjection(HPS.Camera.Projection.Orthographic);
            myModelKey.SetCamera(cameraKit);

            // create children of the model segment
            HPS.SegmentKey childSegment1 = myModelKey.Subsegment();
            HPS.SegmentKey childSegment2 = myModelKey.Subsegment();

            // create a grandchild segment by calling Subsegment on the child
            HPS.SegmentKey grandchild = childSegment1.Subsegment();

            HPS.SegmentKey cylinderSegment = myModel.GetSegmentKey().Subsegment();
            cylinderSegment.InsertCylinder(new HPS.Point(5, 0, 0.5f), new HPS.Point(5.3f, 1, 0), 0.5f);

            myModel.GetSegmentKey().GetMaterialMappingControl().SetEdgeColor(new HPS.RGBAColor(1, 0, 0));
            cylinderSegment.GetVisibilityControl().SetFaces(false);

            myModel.GetSegmentKey().GetMaterialMappingControl().SetEdgeColor(new HPS.RGBAColor(1, 0, 0));
            cylinderSegment.GetVisibilityControl().SetFaces(false);

            myView.GetAxisTriadControl().SetVisibility(true).SetLocation(HPS.AxisTriadControl.Location.BottomRight);


            //Measurement Operator
            HPS.MeasurementOperator measurementOperator = new HPS.MeasurementOperator();
            MeasurementInsertedHandler measurement_handler = new MeasurementInsertedHandler();

            // enable navigation cube
            myView.GetNavigationCubeControl().SetVisibility(true).SetLocation(HPS.NavigationCubeControl.Location.BottomLeft);
            HPS.Point[] pointArray = new HPS.Point[3];

            // the points specified are the endpoints of the line segments
            pointArray[0] = new HPS.Point(0, 0, 0);
            pointArray[1] = new HPS.Point(0, 100, 0);
            pointArray[2] = new HPS.Point(100, 0, 0);

            // inserts a line according to the points contained in pointArray
            myModelKey.InsertLine(pointArray);
            myModelKey.GetLineAttributeControl().SetWeight(12.0f);
            myModel.GetSegmentKey().GetVisibilityControl().SetEdges(true);
            // Refresh the view to see the changes
            myView.Update();
        }
	}

	/// <summary>
	/// Demo User Command 3 Handler
	/// </summary>
	class DemoUser3Command : DemoUserCommand
	{
		public DemoUser3Command(MainWindow win)
			: base(win)
		{

		}

		public override void Execute(object parameter)
		{

            // create the Model object
            HPS.Model myModel = HPS.Factory.CreateModel();

            // get a reference to the View
            HPS.View myView = GetCanvas().GetAttachedLayout().GetAttachedView();

            // attach the Model to the View
            myView.AttachModel(myModel);

            // you can also get a reference to the Model segment
            HPS.SegmentKey myModelKey = myModel.GetSegmentKey();

           


            myView.GetAxisTriadControl().SetVisibility(true).SetLocation(HPS.AxisTriadControl.Location.BottomRight);

            // enable navigation cube
            myView.GetNavigationCubeControl().SetVisibility(true).SetLocation(HPS.NavigationCubeControl.Location.BottomLeft);
           // myView.GetNavigationCubeControl().SetLocation(HPS.NavigationCubeControl.Location.Custom, new HPS.Rectangle(-0.5f, 0.5f, -0.5f, 0.5f));

            //myModelKey.GetCameraControl().SetNearLimit(12.1f);

            string objFilename = @"D:\Training_Sample (1).obj";

            // importing OBJ file
            HPS.OBJ.ImportOptionsKit objOptionsKit = new HPS.OBJ.ImportOptionsKit();
            objOptionsKit.SetSegment(myModelKey);
            HPS.OBJ.ImportNotifier objNotifier = HPS.OBJ.File.Import(objFilename, objOptionsKit);
            objNotifier.Wait();

            HPS.OrbitOperator orbitOperator = new HPS.OrbitOperator(HPS.MouseButtons.ButtonMiddle());
            myView.GetOperatorControl().Push(orbitOperator); // makes 'orbitOperator' active
            myView.FitWorld();
            var myWindowKey = GetCanvas().GetWindowKey();

            //Selection By Point
            myWindowKey.GetSelectionOptionsControl().SetLevel(HPS.Selection.Level.Entity); // choosing entity-level selection
            myWindowKey.GetSelectionOptionsControl().SetProximity(0.01f);
            myWindowKey.GetSelectionOptionsControl().SetAlgorithm(HPS.Selection.Algorithm.Analytic);

            // the origin will be the selection point
            HPS.Point selectionPoint = new HPS.Point(0, 0, 0);

            HPS.SelectionResults selectionResults = new SelectionResults();
            ulong numSelectedItems = myWindowKey.GetSelectionControl().SelectByPoint(  // this is the actual selection call
                selectionPoint,        // the endpoint of the ray, typically the camera location
                out selectionResults); // the selected geometries are returned to this object

            //Measurement Operator
            HPS.MeasurementOperator measurementOperator = new HPS.MeasurementOperator();
            MeasurementInsertedHandler measurement_handler = new MeasurementInsertedHandler();

            measurementOperator = new HPS.MeasurementOperator();
            measurementOperator.SetMeasurementType(HPS.MeasurementOperator.MeasurementType.Area);
            measurementOperator.SetVertexSnapping(true, 5);
            myView.GetOperatorControl().Push(measurementOperator);

            Database.GetEventDispatcher().Subscribe(measurement_handler, HPS.Object.ClassID<HPS.MeasurementOperator.MeasurementEvent>());


            // Refresh the view to see the changes
            myView.Update();

        }
    }

    

    public class MeasurementInsertedHandler : HPS.EventHandler
    {
        public MeasurementInsertedHandler()
            : base()
        {
        }

        ~MeasurementInsertedHandler()
        {
            Shutdown();
        }

        public override HandleResult Handle(HPS.Event in_event)
        {
            if (in_event.GetClassID() == HPS.Object.ClassID<HPS.MeasurementOperator.MeasurementEvent>())
            {
                HPS.MeasurementOperator.MeasurementEvent measurement_event = new MeasurementOperator.MeasurementEvent(in_event);
                float myValue = measurement_event.measurement_value;
                return HandleResult.Handled;
            }

            return HandleResult.NotHandled;
        }
    };

    /// <summary>
    /// Demo User Command 4 Handler
    /// </summary>
    class DemoUser4Command : DemoUserCommand
	{
		public DemoUser4Command(MainWindow win)
			: base(win)
		{

		}

		public override void Execute(object parameter)
		{

            // create the Model object
            HPS.Model myModel = HPS.Factory.CreateModel();
            HPS.Canvas canvas = GetCanvas();
            // get a reference to the View
            HPS.View myView = GetCanvas().GetAttachedLayout().GetAttachedView();

            // attach the Model to the View
            myView.AttachModel(myModel);

            // you can also get a reference to the Model segment
            HPS.SegmentKey myModelKey = myModel.GetSegmentKey();

            var selectedBlocks = new List<HPS.SegmentKey>();

            myView.GetAxisTriadControl().SetVisibility(true).SetLocation(HPS.AxisTriadControl.Location.BottomRight).SetInteractivity(true);

            

            // enable navigation cube
            myView.GetNavigationCubeControl().SetVisibility(true).SetLocation(HPS.NavigationCubeControl.Location.BottomLeft);

            myModel.GetSegmentKey().GetVisibilityControl().SetEdges(true);
            HPS.ShellKit myShellKit = new HPS.ShellKit();
            myView.FitWorld();


            string objFilename = @"D:\Training_Sample (1).obj";

            // importing OBJ file
            HPS.OBJ.ImportOptionsKit objOptionsKit = new HPS.OBJ.ImportOptionsKit();


            objOptionsKit.SetSegment(myModelKey);
            HPS.OBJ.ImportNotifier objNotifier = HPS.OBJ.File.Import(objFilename, objOptionsKit);
            objNotifier.Wait();

            myModelKey.GetModellingMatrixControl().Rotate(90,0,0);
            
            myModelKey.ShowSubsegments(out SegmentKey[] out_children);
            var BlockDictionary = new Dictionary<int, SegmentKey>();
            List<SegmentKey> Segments = new List<SegmentKey>();
            int count = 0;
            var SegmentList = new List<SegmentKey>();
            foreach(var v in out_children)
            {
                BlockDictionary.Add(count, v);
                Segments.Add(v);
                SegmentList.Add(v);
                count++;
            }
            HPS.WindowKey windowKey = GetCanvas().GetWindowKey();
            
            
            DistanceMeasurementOperator measurementOperator = new DistanceMeasurementOperator(canvas, myModelKey, BlockDictionary, Segments);

            myView.GetOperatorControl().Push(measurementOperator);
            
            myView.FitWorld();
            
            // Trigger update
            GetCanvas().Update();
            myShellKit.UnsetFaceNormals();
        }
        
        };

    public class DistanceMeasurementOperator : Operator
    {
        int step = 2;
        private Canvas canvas;
        private Point startPoint;
        private bool firstPointSelected;
        private bool operatorStarted;
        private HPS.SegmentKey temporaryGeometry;   /* segment used for drawing the sphere outline */
        private HPS.SegmentKey SelectedSegment;
        HPS.Point drawPosition0, drawPosition1;
        HPS.SegmentKey myModelKey;
        HPS.View view;
        public Dictionary<int, SegmentKey> AllSegments;
        public List<SegmentKey> Segments1;
        Dictionary<int, SegmentKey> CrossSegments;
        Dictionary<int, SegmentKey> NoughtSegments;
        private bool isCrossWinner = false;
        private bool isNoughtWinner = false;

        public DistanceMeasurementOperator(Canvas c, SegmentKey modelKey, Dictionary<int, SegmentKey> dict, List<SegmentKey> Segments)
        {
            canvas = c;
            firstPointSelected = true;
            myModelKey = modelKey;
            view = c.GetAttachedLayout().GetAttachedView();
            AllSegments = dict;
            Segments1 = Segments;
            NoughtSegments = new Dictionary<int, SegmentKey>();
            CrossSegments = new Dictionary<int, SegmentKey>();
        }

        /* The OnViewAttached function is executed when the CreateSphere operator is attached to the View
	    * It is responsible for setting up the temporary segment used for drawing the sphere outline */
        public override void OnViewAttached(HPS.View in_attached_view)
        {
            operatorStarted = false;

            HPS.Model model = canvas.GetAttachedLayout().GetAttachedView().GetAttachedModel();
            if (model.Type() == HPS.Type.None)
            {
                model = HPS.Factory.CreateModel();
                canvas.GetAttachedLayout().GetAttachedView().AttachModel(model);
            }

            temporaryGeometry = model.GetSegmentKey().Subsegment();

            temporaryGeometry.GetVisibilityControl().
                SetLines(true).SetMarkers(true).SetFaces(false).SetEdges(true);

            temporaryGeometry.GetMaterialMappingControl().
                SetLineColor(new HPS.RGBAColor(0, 0, 1, 1)).
                SetMarkerColor(new HPS.RGBAColor(1, 0, 0, 1));

            temporaryGeometry.GetMarkerAttributeControl()
                .SetSymbol("plus").SetSize(0.3f);

            temporaryGeometry.GetCameraControl()
                .SetProjection(HPS.Camera.Projection.Stretched)
                .SetPosition(new HPS.Point(0, 0, -1)).SetTarget(new HPS.Point(0, 0, 0))
                .SetUpVector(new HPS.Vector(0, 1, 0)).SetField(2, 2);

            temporaryGeometry.GetDrawingAttributeControl()
                .SetOverlay(HPS.Drawing.Overlay.Default);

            return;
        }

        public override bool OnMouseDown(MouseState in_state)
        {
            if (in_state.GetActiveEvent().CurrentButton.Left())
            {
                Point p1 = in_state.GetLocation();
                
                WindowKey wk = canvas.GetWindowKey();
                SelectionOptionsKit selectionOptions = new SelectionOptionsKit();
                SelectionResults selectionResults = new SelectionResults();
                ulong numberOfSelectedItems;
                selectionOptions.SetLevel(Selection.Level.Segment).SetRelatedLimit(0).SetSorting(Selection.Sorting.Default);



                HPS.SegmentKey mySegment = GetAttachedView().GetAttachedModel().GetSegmentKey().Subsegment();
                HPS.SprocketPath sprkPath = new HPS.SprocketPath(canvas, canvas.GetAttachedLayout(),
                    canvas.GetAttachedLayout().GetAttachedView(),
                    canvas.GetAttachedLayout().GetAttachedView().GetAttachedModel());
                HPS.SprocketPath dd = new HPS.SprocketPath(canvas, canvas.GetAttachedLayout(), canvas.GetAttachedLayout().GetAttachedView(), canvas.GetAttachedLayout().GetAttachedView().GetAttachedModel());

                SelectedSegment = sprkPath.GetCanvas().GetAttachedLayout().GetSegmentKey();
                
                SelectedSegment.ShowSubsegments(out SegmentKey[] out_children);
                
                HPS.KeyPath tpath = sprkPath.GetKeyPath();
                HPS.KeyPath path = new HPS.KeyPath();

                path.Append(mySegment).Append(tpath);
                if (!firstPointSelected)
                {
                    // First point selected (Left mouse button down)
                    drawPosition0 = in_state.GetLocation();

                    path.ConvertCoordinate(HPS.Coordinate.Space.Window, drawPosition0, HPS.Coordinate.Space.World, out startPoint);
                    
                    firstPointSelected = true;
                }
                else
                {
                    Point endPoint;
                    Point drawposition02 = in_state.GetLocation();
                    wk = this.canvas.GetWindowKey();
                    int i = (int) wk.GetSelectionControl().SelectByPoint(drawposition02,out selectionResults);
                    path.ConvertCoordinate(HPS.Coordinate.Space.Window, drawposition02, HPS.Coordinate.Space.World, out endPoint);

                    startPoint.x = endPoint.x-0.5f;
                    startPoint.y = endPoint.y-0.5f;
                    startPoint.z = endPoint.z;
                    endPoint.x += 0.5f;
                    endPoint.y += 0.5f;
                    /* This block of code shows how to define a glyph and a named style 
	                 * 1. For both of these definitions a portfolio is needed, to obtain a portfolio key, 
	                 *		we can use the PortfolioControl, and call its ShowTop method to obtain the key
	                 *		of the portfolio on top of the portfolio stack.
	                 *		NOTE: Remember that this portfolio must be visible by the segment which will use
	                 *		the glyph and styles defined in it 
	                 * 2. To define a glyph, provide a name for the glyph and choose its symbol from the many
	                 *		default symbols provided by Visualize
	                 * 3. To define a named style, first create a segment which contains the attribute for the
	                 *		style, and then call the PortfolioKey.DefineNamedStyle method */
                    HPS.PortfolioKey pKey;
                    myModelKey.GetPortfolioControl().ShowTop(out pKey);
                    
                    HPS.SegmentKey highlightStyle = myModelKey.Subsegment();    
                    
                    pKey.DefineNamedStyle("orangeHighlight", highlightStyle);
                    highlightStyle.GetMaterialMappingControl().SetFaceColor(new HPS.RGBAColor(1, 0.5f, 1, 1)).SetEdgeColor(new HPS.RGBAColor(1, 0.5f, 0, 1));

                    HPS.SegmentKey WinnerStyle = myModelKey.Subsegment();
                    pKey.DefineNamedStyle("WinnerHighlight", WinnerStyle);
                    WinnerStyle.GetMaterialMappingControl().SetFaceColor(new HPS.RGBAColor(0, 1, 0, 1));


                    /* The next block of code inserts the marker in the scene. The marker is as a metaphor for
	                 * a mouse pointer, to show the user where the selection will take place
	                 * 1. Using the MarkerAttributeControl, set the symbol to be used for this marker and its size.
	                 *		The symbol chosen here is the one defined in the portfolio above
	                 * 2. The marker is inserted here using a MarkerKit, which specifies the point in World
	                 *		coordinates where the marker will appear
	                 * 3. Since the marker needs to always be visible, on top of the scene, we set Depth Range
	                 *		on the segment containing it, through its DrawingAttributeControl 
	                 * 4. Finally, since we do not want the marker itself to be selectable, we turn its 
	                 *		selectability off using a SelectabilityControl */
                    HPS.MarkerKit marker = new HPS.MarkerKit();
                    HPS.Point markerPosition = endPoint;
                    marker.SetPoint(startPoint);
                    HPS.SegmentKey geometry = myModelKey.Subsegment();
                    
                    HPS.SegmentKey markerSegment = myModelKey.Subsegment();
                    markerSegment.GetMarkerAttributeControl().SetSymbol("myGlyph").SetSize(3);
                    //markerSegment.InsertMarker(marker);
                    if (step % 2 == 0)
                    {
                        var myKey2 = AllSegments.FirstOrDefault(x => x.Value == markerSegment).Key;
                        markerSegment.GetSelectabilityControl().SetEverything(HPS.Selectability.Value.Off);
                        //int index = FindKeyByValue(Dictionary, markerSegment);
                        //DrawCross(startPoint, endPoint);                    //Drawing Cross on Tile
                    }
                    markerSegment.GetDrawingAttributeControl().SetDepthRange(0.1f, 0.2f);
                    markerSegment.GetVisibilityControl().SetMarkers(true);
                    markerSegment.GetMaterialMappingControl().SetMarkerColor(new HPS.RGBAColor(1, 0, 0, 1));
                    markerSegment.GetSelectabilityControl().SetEverything(HPS.Selectability.Value.Off);

                    canvas.GetAttachedLayout().GetAttachedView().Update();

                    /* Before performing a selection on 'markerPosition' we need to convert its coordinates from
                    * the World frame of reference (in which they were inserted) to the Window frame of reference, which
                    * is used to perform selection.
                    * 
                    * In order to correctly account for cameras and modeling matrices between the window and the segment
                    * in which the geometry resides, we build a key path.
                    * A key path starts from the leaf segment (in this case the segment where the marker is) and contains
                    * a list of segment keys, all the way to the window. Visualize will try to fill in segments automatically
                    * if possible.
                    * 
                    * All include keys need to be included in the key path, in order to provide an unequivocal path
                    * 
                    * Once the KeyPath is ready, we can use its ConvertCoordinate method, to obtain the point we will
                    * use for selection */
                    HPS.Key[] keyArray = new HPS.Key[5];
                    keyArray[0] = markerSegment;
                    keyArray[1] = view.GetAttachedModelIncludeLink();
                    keyArray[2] = canvas.GetAttachedLayout().GetAttachedViewIncludeLink(0);
                    keyArray[3] = canvas.GetAttachedLayoutIncludeLink();
                    keyArray[4] = wk;

                    HPS.KeyPath keyPath = new HPS.KeyPath();
                    keyPath.SetKeys(keyArray);
                    HPS.Point selectionPosition;
                    keyPath.ConvertCoordinate(HPS.Coordinate.Space.World, markerPosition, HPS.Coordinate.Space.Window, out selectionPosition);

                    /* This block of code shows a few selection options as well as the selection procedure itself
                    * 1. In this example we choose the selection level to 'Entity'. 'Segment' and 'Subentity' levels are
                    *		also available
                    * 2. Setting the related selection limit to zero makes it so that we will only get one item from the 
                    *		selection routine
                    * 3. Turning selection sorting on, will make sure that the item closer to the camera will be on top of
                    *		the selection list.
                    *	
                    *	The SelectByPoint method selects under the selectionPosition point using the options we pass to it.
                    *	It is also possible to select by Area, Ray, etc... */
                    selectionOptions = new HPS.SelectionOptionsKit();
                    selectionOptions.SetLevel(HPS.Selection.Level.Segment).SetRelatedLimit(0).SetSorting(HPS.Selection.Sorting.Default);
                    numberOfSelectedItems = wk.GetSelectionControl().SelectByPoint(selectionPosition, selectionOptions, out selectionResults);

                    /*  walk a list of selected items and how to highlight them:
	                 * Use a loop that checks whether SelectionResultsIterator.IsValid() is true to walk the list
	                 * 3. Use SelectionResultsIterator.Next() to advance to the next iterator 
	                 * 4. It is possible to obtain the SelectionItem from the SelectionResultsIterator by using its GetItem() method
	                 * 5. Once we have an item, we can use its ShowPath() method to get a clear path to the instance of the geometry
	                 *		that was selected. For example, here we select a spring. There are two instances of the spring entity
	                 *		in this scene. Using a key path ensures that only the correct one is selected.
	                 * 6. To highlight the selected entity, we first create a HighlightOptionsKit object and set its style to that
	                 *		of the named style we defined above
	                 * 7. The WindowKey.Highlight method can be used to highlight the geometry pointed at by the key path we 
	                 *		obtained, using the highlight options defined in the HighlightOptionsKit object
	                 * */
                    HPS.SelectionResultsIterator it = selectionResults.GetIterator();
                    HPS.HighlightOptionsKit hok = new HPS.HighlightOptionsKit();
                    hok.SetStyleName("orangeHighlight");
                    while (it.IsValid())
                    {
                        HPS.KeyPath selectionPath = new HPS.KeyPath();
                        HPS.SelectionItem selection = it.GetItem();
                        selection.ShowPath(out selectionPath);
                        selection.ShowSelectedItem(out Key out_segmnent);
                        HPS.SegmentKey sg = out_segmnent as SegmentKey;
                        if(Segments1.Contains(sg))
                        {
                            if(step%2==0)
                            {
                                CrossSegments.Add(Segments1.IndexOf(sg), sg);
                                DrawCross(startPoint, endPoint);
                                sg.GetSelectabilityControl().SetEverything(HPS.Selectability.Value.Off);
                                isCrossWinner = CheckWinnerCross();
                                step++;
                            }
                            else
                            {
                                NoughtSegments.Add(Segments1.IndexOf(sg), sg);
                                DrawCircle(startPoint, endPoint);
                                sg.GetSelectabilityControl().SetEverything(HPS.Selectability.Value.Off);
                                isNoughtWinner = CheckWinnerNought();
                                step++;
                            }
                        }
                        wk.GetHighlightControl().Highlight(sg, hok);
                        it.Next();
                    }

                        HPS.HighlightOptionsKit win = new HighlightOptionsKit();
                        win.SetStyleName("WinnerHighlight");
                    if(isCrossWinner)
                    {
                        if (CrossSegments.ContainsKey(0) && CrossSegments.ContainsKey(1) && CrossSegments.ContainsKey(2))
                            {
                                wk.GetHighlightControl().Highlight(CrossSegments[0], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[1], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[2], win);
                            }
                        if (CrossSegments.ContainsKey(5) && CrossSegments.ContainsKey(3) && CrossSegments.ContainsKey(4)) 
                            {
                                wk.GetHighlightControl().Highlight(CrossSegments[5], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[3], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[4], win);
                            }
                        if (CrossSegments.ContainsKey(6) && CrossSegments.ContainsKey(7) && CrossSegments.ContainsKey(8)) 
                            {
                                wk.GetHighlightControl().Highlight(CrossSegments[6], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[7], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[8], win);
                            }
                        if (CrossSegments.ContainsKey(8) && CrossSegments.ContainsKey(5) && CrossSegments.ContainsKey(2)) 
                             {
                                wk.GetHighlightControl().Highlight(CrossSegments[8], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[5], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[2], win);
                             }
                        if (CrossSegments.ContainsKey(7) && CrossSegments.ContainsKey(3) && CrossSegments.ContainsKey(1)) 
                             {
                                wk.GetHighlightControl().Highlight(CrossSegments[7], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[3], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[1], win);
                            }
                        if (CrossSegments.ContainsKey(6) && CrossSegments.ContainsKey(4) && CrossSegments.ContainsKey(0)) 
                             {
                                wk.GetHighlightControl().Highlight(CrossSegments[6], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[4], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[0], win);
                             }
                        if (CrossSegments.ContainsKey(8) && CrossSegments.ContainsKey(3) && CrossSegments.ContainsKey(0))
                            {
                                wk.GetHighlightControl().Highlight(CrossSegments[8], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[3], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[0], win);
                             }
                        if (CrossSegments.ContainsKey(6) && CrossSegments.ContainsKey(3) && CrossSegments.ContainsKey(2))
                            {
                                wk.GetHighlightControl().Highlight(CrossSegments[6], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[3], win);
                                wk.GetHighlightControl().Highlight(CrossSegments[2], win);
                            }
                       //Selection for all the blocks after winning game will be disabled 
                        foreach(var v in Segments1)
                        {
                            v.GetSelectabilityControl().SetEverything(HPS.Selectability.Value.Off);
                        }
                    }
                    if(isNoughtWinner)
                    {
                        if (NoughtSegments.ContainsKey(0) && NoughtSegments.ContainsKey(1) && NoughtSegments.ContainsKey(2))
                        {
                            wk.GetHighlightControl().Highlight(NoughtSegments[0], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[1], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[2], win);
                        }
                        if (NoughtSegments.ContainsKey(5) && NoughtSegments.ContainsKey(3) && NoughtSegments.ContainsKey(4))
                        {
                            wk.GetHighlightControl().Highlight(NoughtSegments[5], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[3], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[4], win);
                        }
                        if (NoughtSegments.ContainsKey(6) && NoughtSegments.ContainsKey(7) && NoughtSegments.ContainsKey(8))
                        {
                            wk.GetHighlightControl().Highlight(NoughtSegments[6], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[7], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[8], win);
                        }
                        if (NoughtSegments.ContainsKey(8) && NoughtSegments.ContainsKey(5) && NoughtSegments.ContainsKey(2))
                        {
                            wk.GetHighlightControl().Highlight(NoughtSegments[8], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[5], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[2], win);
                        }
                        if (NoughtSegments.ContainsKey(7) && NoughtSegments.ContainsKey(3) && NoughtSegments.ContainsKey(1))
                        {
                            wk.GetHighlightControl().Highlight(NoughtSegments[7], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[3], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[1], win);
                        }
                        if (NoughtSegments.ContainsKey(6) && NoughtSegments.ContainsKey(4) && NoughtSegments.ContainsKey(0))
                        {
                            wk.GetHighlightControl().Highlight(NoughtSegments[6], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[4], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[0], win);
                        }
                        if (NoughtSegments.ContainsKey(8) && NoughtSegments.ContainsKey(3) && NoughtSegments.ContainsKey(0))
                        {
                            wk.GetHighlightControl().Highlight(NoughtSegments[8], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[3], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[0], win);
                        }
                        if (NoughtSegments.ContainsKey(6) && NoughtSegments.ContainsKey(3) && NoughtSegments.ContainsKey(2))
                        {
                            wk.GetHighlightControl().Highlight(NoughtSegments[6], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[3], win);
                            wk.GetHighlightControl().Highlight(NoughtSegments[2], win);
                        }
                        //Selection for all the blocks after winning game will be disabled 
                        foreach (var v in Segments1)
                        {
                            v.GetSelectabilityControl().SetEverything(HPS.Selectability.Value.Off);
                        }
                    }

                    view.Update();



                   
                    if(step%2!=0)
                    {
                        CheckWinnerNought();
                    }
                        
                   
                    //firstPointSelected = false;
                }
                    this.canvas.Update();
            }

            return true;
        }

        private bool CheckWinnerCross()
        {
            //Checking Conditions for Winning Player with Cross by checking different senarios
            if(CrossSegments.Count>=3)
                if ((CrossSegments.ContainsKey(0) && CrossSegments.ContainsKey(1) && CrossSegments.ContainsKey(2)) ||
                    (CrossSegments.ContainsKey(5) && CrossSegments.ContainsKey(3) && CrossSegments.ContainsKey(4)) ||
                    (CrossSegments.ContainsKey(6) && CrossSegments.ContainsKey(7) && CrossSegments.ContainsKey(8)) ||
                    (CrossSegments.ContainsKey(8) && CrossSegments.ContainsKey(5) && CrossSegments.ContainsKey(2)) ||
                    (CrossSegments.ContainsKey(7) && CrossSegments.ContainsKey(3) && CrossSegments.ContainsKey(1)) ||
                    (CrossSegments.ContainsKey(6) && CrossSegments.ContainsKey(4) && CrossSegments.ContainsKey(0)) ||
                    (CrossSegments.ContainsKey(8) && CrossSegments.ContainsKey(3) && CrossSegments.ContainsKey(0)) ||
                    (CrossSegments.ContainsKey(6) && CrossSegments.ContainsKey(3) && CrossSegments.ContainsKey(2)))
                    return true;    
            return false;
        }
        private bool CheckWinnerNought()
        {
            //Checking Conditions for Winning Player with Nought by checking different senarios
            if(NoughtSegments.Count>=3)
                if ((NoughtSegments.ContainsKey(0) && NoughtSegments.ContainsKey(1) && NoughtSegments.ContainsKey(2)) ||
                    (NoughtSegments.ContainsKey(6) && NoughtSegments.ContainsKey(7) && NoughtSegments.ContainsKey(8)) ||
                    (NoughtSegments.ContainsKey(8) && NoughtSegments.ContainsKey(5) && NoughtSegments.ContainsKey(2)) ||
                    (NoughtSegments.ContainsKey(7) && NoughtSegments.ContainsKey(3) && NoughtSegments.ContainsKey(1)) ||
                    (NoughtSegments.ContainsKey(5) && NoughtSegments.ContainsKey(3) && NoughtSegments.ContainsKey(4)) ||
                    (NoughtSegments.ContainsKey(6) && NoughtSegments.ContainsKey(4) && NoughtSegments.ContainsKey(0)) ||
                    (NoughtSegments.ContainsKey(8) && NoughtSegments.ContainsKey(3) && NoughtSegments.ContainsKey(0)) ||
                    (NoughtSegments.ContainsKey(6) && NoughtSegments.ContainsKey(3) && NoughtSegments.ContainsKey(2)))
                    return true;
            return false;
        }
        private void DrawLine(Point s, Point e)
        {
            HPS.View attachedView = GetAttachedView();
            //HPS.SegmentKey lineSegment = GetAttachedView().GetAttachedModel().GetSegmentKey();
            HPS.SegmentKey lineSegment = new HPS.SegmentKey(attachedView.GetSegmentKey());
            HPS.Point[] pointArray = new HPS.Point[2];
            pointArray[0] = s;
            pointArray[1] = e;
            
            lineSegment.InsertLine(pointArray);
            lineSegment.GetVisibilityControl().SetLines(true);
            //lineSegment.GetEdgeAttributeControl().SetWeight(2);
          
        }

        private void DrawCircle(Point s, Point e)
        {
            HPS.View attachedView = GetAttachedView();
           
            HPS.SegmentKey circleSegment = new HPS.SegmentKey(attachedView.GetSegmentKey());

            Point centre;
            centre.x = (s.x + e.x) / 2;
            centre.y = (s.y + e.y) / 2;
            centre.z = ((s.z + e.z) / 2)-.12f;

            //circleSegment.InsertCircle(s, centre, e);
            circleSegment.InsertCircle(centre, 0.5f, new HPS.Vector(0, 0, 1));

            circleSegment.GetVisibilityControl().SetEdges(true);
            circleSegment.GetEdgeAttributeControl().SetWeight(2);

        }

        private void DrawCross(Point s, Point e)
        {
            HPS.View attachedView = GetAttachedView();
            //HPS.SegmentKey lineSegment = GetAttachedView().GetAttachedModel().GetSegmentKey();
            HPS.SegmentKey lineSegment = new HPS.SegmentKey(attachedView.GetSegmentKey());
            HPS.Point[] pointArray = new HPS.Point[2];
            HPS.Point[] pointArray2 = new HPS.Point[2];

            Point s1;
            s1.x = s.x;
            s1.y = e.y;
            s1.z = s.z-0.3f;
            s.z -= .3f;
            Point s2;
            s2.x = e.x;
            s2.y = s.y;
            s2.z = e.z-.3f;
            e.z -= .3f;
            pointArray[0] = s;
            pointArray[1] = e;
            pointArray2[0] = s1;
            pointArray2[1] = s2;
            lineSegment.InsertLine(pointArray);
            lineSegment.InsertLine(pointArray2);

            lineSegment.GetVisibilityControl().SetLines(true);
            lineSegment.GetEdgeAttributeControl().SetWeight(2);

        }

        private float ComputeDistance(Point p0, Point p1)
        {
            return (float)Math.Sqrt(Math.Pow(p1.x - p0.x, 2) + Math.Pow(p1.y - p0.y, 2) + Math.Pow(p1.z - p0.z, 2));
        }

        private void ShowDistanceAsText(float distance, Point endPoint)
        {
            HPS.View attachedView = GetAttachedView();
            HPS.SegmentKey textSegment = new HPS.SegmentKey(attachedView.GetSegmentKey());
            HPS.TextKit textKit = new HPS.TextKit();

            textKit.SetPosition(endPoint);
           
            textKit.SetText(distance.ToString());
            textKit.SetColor(new HPS.RGBAColor(1, 0, 0));
            textKit.SetSize(10, HPS.Text.SizeUnits.Pixels);
            textKit.SetAlignment(HPS.Text.Alignment.Center);   // aligns text horizontally relative to its insertion point

            textSegment.InsertText(textKit);
            
        }
    }

}


