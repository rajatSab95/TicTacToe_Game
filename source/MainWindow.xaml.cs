using System;
using System.Collections.Generic;
using System.Linq;
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
using Microsoft.Windows.Controls.Ribbon;
using System.Xml.Linq;
using System.ComponentModel;
using System.IO;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace wpf_sandbox
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : RibbonWindow
	{
		/// <summary>
		/// Config file for demo
		/// </summary>
		const string configfile = "hps_wpf_sandbox_config.xml";
		public string samplesDir;
		private XElement _config;

		/// <summary>
		/// Single HPS World instance
		/// </summary>
		private static HPS.World _hpsWorld;

		/// <summary>
		/// Main distant light in Sprockets view.
		/// </summary>
		private HPS.DistantLightKey _mainDistantLight;

        /// MyErrorHandler
        /// </summary>
        public MyErrorHandler _errorHandler;

        /// <summary>
        /// MyWarningHandler
        /// </summary>
        public MyWarningHandler _warningHandler;

        public bool _enableFrameRate;

        private SegmentBrowser _segmentBrowser;
        public SegmentBrowser SegmentBrowser
        {
            get
            {
                if (_segmentBrowser == null)
                {
                    _segmentBrowser = new SegmentBrowser(this);
                }
                return _segmentBrowser;
            }
        }

        private ModelBrowser _modelBrowser;
        public ModelBrowser ModelBrowser
        {
            get
            {
                if (_modelBrowser == null)
                    _modelBrowser = new ModelBrowser(this);
                return _modelBrowser;
            }
        }

        private ConfigurationBrowser _configurationBrowser;
        public ConfigurationBrowser ConfigurationBrowser
        {
            get
            {
                if (_configurationBrowser == null)
                    _configurationBrowser = new ConfigurationBrowser(this);
                return _configurationBrowser;
            }
        }

        public HPS.Model Model { get; set; }

        public HPS.CADModel CADModel { get; set; }

        private HPS.CameraKit PreZoomToKeyPathCamera { get; set; }

        public HPS.CameraKit DefaultCamera { get; set; }

		/// <summary>
		/// MainWindow constructor.
		///
		/// Initialization performed here.
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();

			try
			{
				// Load sandbox config file
				_config = XElement.Load(configfile);

				// Set default data dir
				samplesDir = _config.Element("directories").Element("samples").Value;

                _enableFrameRate = false;

				// Initialize
				InitializeSprockets();

				// Create new scene
                NewCommand.Execute(this);

                // hide the segment and model browsers
                PropertiesCheckBox.IsChecked = false;
                PropertiesToggleCommand.Execute(null);

                SegmentBrowserButton.IsChecked = false;
                SegmentBrowserCommand.Execute(null);

                ModelBrowserButton.IsChecked = false;
                ModelBrowserCommand.Execute(null);

                //restore recent files
                string path = Directory.GetCurrentDirectory();
                path = System.IO.Path.Combine(path, "hps_wpf_sandbox.recent_files");
                if (File.Exists(path))
                {
                    string[] lines = System.IO.File.ReadAllLines(path);
                    foreach (string line in lines)
                    {
                        RibbonGalleryItem rgi = new RibbonGalleryItem();
                        var textBlock = new TextBlock();
                        textBlock.Text = line;
                        rgi.Content = textBlock;
                        rgi.CheckedBackground = null;
                        rgi.CheckedBorderBrush = null;
                        rgi.Selected += RecentListItem_IsSelected;
                        RecentFiles.Items.Add(rgi);
                    }
                }

                CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy,
                                    new ExecutedRoutedEventHandler(DoCopyToClipboard)));
            }
            catch (System.Exception ex)
			{
				string msg = "Error. Caught exception: " + ex.Message;
				MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Exclamation);
				Application.Current.Shutdown(-1);
			}
		}

        public void DoCopyToClipboard(object sender, ExecutedRoutedEventArgs e)
        {
            SprocketsWPFControl ctrl = GetSprocketsControl();
            HPS.Hardcopy.GDI.ExportOptionsKit kit = new HPS.Hardcopy.GDI.ExportOptionsKit();
            HPS.Hardcopy.GDI.ExportClipboard(ctrl.Canvas.GetWindowKey(), kit);
        }

		/// <summary>
		/// Sets window title
		/// </summary>
		/// <param name="title"></param>
		public void SetTitle(String title)
		{
			Title = "WPF Sandbox " + title;
		}

		/// <summary>
		/// Private method used to connect the SprocketsWPFControl to our WPF element
		/// </summary>
		/// <param name="ctrl"></param>
		private void SetSprocketsControl(SprocketsWPFControl ctrl)
		{
			_mainBorder.Child = ctrl;
		}

		/// <summary>
		/// Helper method for retrieving SprocketsWPFControl attached to UI
		/// </summary>
		/// <returns>SprocketsWPFControl attached to _mainBorder.Child</returns>
		public SprocketsWPFControl GetSprocketsControl()
		{
			return _mainBorder.Child as SprocketsWPFControl;
		}

		/// <summary>
		/// Initializes Sprockets and creates SprocketsWPFControl
		/// </summary>
		private void InitializeSprockets()
		{
			// Create and initialize Sprockets World
			string materialsDir = _config.Element("directories").Element("materials").Value;
            string fontsDir = _config.Element("directories").Element("fonts").Value;
            _hpsWorld = new HPS.World(HOOPS_LICENSE.KEY);
			_hpsWorld.SetMaterialLibraryDirectory(materialsDir);
            _hpsWorld.SetFontDirectory(fontsDir);

#if USING_EXCHANGE
            string exchangeInstallDir = System.Environment.GetEnvironmentVariable("HEXCHANGE_INSTALL_DIR");
            if (!String.IsNullOrEmpty(exchangeInstallDir))
            {
                string binFolder;
#if WIN64
                binFolder = "win64_v142";
#else
                binFolder = "win32_v142";
#endif
                _hpsWorld.SetExchangeLibraryDirectory(exchangeInstallDir + "/bin/" + binFolder);
            }
#endif

#if USING_PUBLISH
            string publishInstallDir = System.Environment.GetEnvironmentVariable("HPUBLISH_INSTALL_DIR");
            if (!String.IsNullOrEmpty(publishInstallDir))
                _hpsWorld.SetPublishResourceDirectory(publishInstallDir + "/bin/resource");
#endif

#if USING_PARASOLID
            string PARASOLID_INSTALL_DIR = Environment.GetEnvironmentVariable("PARASOLID_INSTALL_DIR");
            if (PARASOLID_INSTALL_DIR != null)
            {
                string base_string = "/base";
#if _M_X64
                base_string += "64";
#else
                base_string += "32";
#endif

//These platforms are binary compatible, so they share the same directory
#if VS2015 || VS2017
                base_string += "_v140";
#endif
                _hpsWorld.SetParasolidSchemaDirectory(PARASOLID_INSTALL_DIR + base_string + "/schema");
            }
#endif

            // Create and attach Sprockets Control
            SprocketsWPFControl ctrl = new SprocketsWPFControl(HPS.Window.Driver.Default3D, "main");
            ctrl.FileDropped += OnFileDrop;
			SetSprocketsControl(ctrl);

            _errorHandler = new MyErrorHandler();
            _warningHandler = new MyWarningHandler();
        }

        private void OnFileDrop(object sender, MyFileDropEventArgs e)
        {
            DemoFileOpenCommand dfoc = new DemoFileOpenCommand(this);
#if USING_EXCHANGE
            dfoc.ThreadedFileLoad(e.Filename, GetSprocketsControl());
#else
            if (HandledByVisualize(e.Filename)
#if USING_PARASOLID
                || HandledByParasolid(e.Filename)
#endif
#if USING_DWG
                || HandledByDWG(e.Filename)
#endif
                )
                dfoc.ThreadedFileLoad(e.Filename, GetSprocketsControl());
            else
                MessageBox.Show("Unsupported file format");
#endif
        }

		/// <summary>
		/// Sets up defaults for a Sprockets Control attached to the specified WPF border control
		/// </summary>
		public void SetupSceneDefaults()
		{
			// Grab the SprocketsWPFControl from the border element
			SprocketsWPFControl ctrl = GetSprocketsControl();
			if (ctrl == null)
				return;

			// Attach a model
			HPS.View view = ctrl.Canvas.GetFrontView();
            view.AttachModel(Model);

			// Set default operators.  Orbit is on top and will be replaced when the operator is changed
            view.GetOperatorControl()
                .Push(new HPS.MouseWheelOperator(), HPS.Operator.Priority.Low)
                .Push(new HPS.ZoomOperator(HPS.MouseButtons.ButtonMiddle()))
                .Push(new HPS.PanOperator(HPS.MouseButtons.ButtonRight()))
                .Push(new HPS.OrbitOperator(HPS.MouseButtons.ButtonLeft()));

			// Subscribe _errorHandler to handle errors
			HPS.Database.GetEventDispatcher().Subscribe(_errorHandler, HPS.Object.ClassID<HPS.ErrorEvent>());

			// Subscribe _warningHandler to handle warnings
			HPS.Database.GetEventDispatcher().Subscribe(_warningHandler, HPS.Object.ClassID<HPS.WarningEvent>());

			// Make sure the segment browser root is correct.
			SegmentBrowserRootCommand.Execute(null);
		}

		public void InitializeBrowsers()
		{
			// make the model segment the default root
			RootComboBox.SelectedValue = "Model";

            // initialize the model and configuration browser
            ModelBrowser.Init();
            ConfigurationBrowser.Init();
		}

		/// <summary>
		/// Sets the main distant light for the Sprockets View.
		/// </summary>
		/// <param name="light"></param>
		public void SetMainDistantLight(HPS.DistantLightKit light)
		{
			// Delete previous light before inserting new one
			if (_mainDistantLight != null)
				_mainDistantLight.Delete();
			_mainDistantLight = GetSprocketsControl().Canvas.GetFrontView().GetSegmentKey().InsertDistantLight(light);
		}

		/// <summary>
		/// Sets the direction for a camera-relative, colorless, main distant light for the Sprockets View.
		/// </summary>
		/// <param name="lightDirection"></param>
		public void SetMainDistantLight(HPS.Vector? lightDirection = null)
		{
			HPS.DistantLightKit light = new HPS.DistantLightKit();
			light.SetDirection(lightDirection.HasValue ? lightDirection.Value : new HPS.Vector(1, 0, -1.5f));
			light.SetCameraRelative(true);
			SetMainDistantLight(light);
		}

		public void UpdatePlanes()
		{
			HPS.View view = GetSprocketsControl().Canvas.GetFrontView();
			view.SetSimpleShadow(view.GetSimpleShadow());
		}

        public void CreateNewModel()
        {
            if (Model != null)
                Model.Delete();
            Model = HPS.Factory.CreateModel();

            if (CADModel != null)
            {
                CADModel.Delete();
                CADModel = null;
            }

            DefaultCamera = null;
        }

        public void Unhighlight()
        {
            var highlightOptions = new HPS.HighlightOptionsKit();
            highlightOptions.SetStyleName("highlight_style").SetNotification(true);

            var canvas = GetSprocketsControl().Canvas;
            canvas.GetWindowKey().GetHighlightControl().Unhighlight(highlightOptions);

            HPS.Database.GetEventDispatcher().InjectEvent(new HPS.HighlightEvent(HPS.HighlightEvent.Action.Unhighlight, new HPS.SelectionResults(), highlightOptions));
            HPS.Database.GetEventDispatcher().InjectEvent(new HPS.ComponentHighlightEvent(HPS.ComponentHighlightEvent.Action.Unhighlight, canvas, 0, new HPS.ComponentPath(), highlightOptions));
        }

        public void Update()
        {
            GetSprocketsControl().Canvas.Update();
        }

        public void ActivateCapture(HPS.Capture capture)
        {
            HPS.View newView = capture.Activate();
            var newViewSegment = newView.GetSegmentKey();
            HPS.CameraKit newCamera;
            newViewSegment.ShowCamera(out newCamera);

            newCamera.UnsetNearLimit();
            var defaultCameraWithoutNearLimit = HPS.CameraKit.GetDefault().UnsetNearLimit();
            if (newCamera == defaultCameraWithoutNearLimit)
            {
                // there was no camera for this capture - so we'll use the current camera but do a FitWorld on it
                var oldView = GetSprocketsControl().Canvas.GetFrontView();
                HPS.CameraKit oldCamera;
                oldView.GetSegmentKey().ShowCamera(out oldCamera);

                newViewSegment.SetCamera(oldCamera);
                newView.FitWorld();
            }

            AttachViewWithSmoothTransition(newView);
        }

#if USING_EXCHANGE
        public void ActivateCapture(HPS.ComponentPath capture_path)
        {
            HPS.Exchange.Capture capture = new HPS.Exchange.Capture(capture_path.Front());
            HPS.View newView = capture.Activate(capture_path);
            var newViewSegment = newView.GetSegmentKey();
            HPS.CameraKit newCamera;
            newViewSegment.ShowCamera(out newCamera);

            newCamera.UnsetNearLimit();
            var defaultCameraWithoutNearLimit = HPS.CameraKit.GetDefault().UnsetNearLimit();
            if (newCamera == defaultCameraWithoutNearLimit)
            {
                // there was no camera for this capture - so we'll use the current camera but do a FitWorld on it
                var oldView = GetSprocketsControl().Canvas.GetFrontView();
                HPS.CameraKit oldCamera;
                oldView.GetSegmentKey().ShowCamera(out oldCamera);

                newViewSegment.SetCamera(oldCamera);
                newView.FitWorld();
            }

            AttachViewWithSmoothTransition(newView);
        }
#endif

        public void AttachViewWithSmoothTransition(HPS.View newView, bool firstAttach = false)
        {
            HPS.View oldView = GetSprocketsControl().Canvas.GetFrontView();
            HPS.CameraKit oldCamera;
            oldView.GetSegmentKey().ShowCamera(out oldCamera);

            var newViewSegment = newView.GetSegmentKey();
            HPS.CameraKit newCamera;
            newViewSegment.ShowCamera(out newCamera);

            AttachView(newView, firstAttach);

            newViewSegment.SetCamera(oldCamera);
            newView.SmoothTransition(newCamera);
        }

        public void AttachView(HPS.View newView, bool firstAttach = false)
        {
            var canvas = GetSprocketsControl().Canvas;
            if (!firstAttach && CADModel != null)
            {
                CADModel.ResetVisibility(canvas);
                canvas.GetWindowKey().GetHighlightControl().UnhighlightEverything();
            }

            PreZoomToKeyPathCamera = null;

            HPS.View oldView = canvas.GetFrontView();
            canvas.AttachViewAsLayout(newView);

            HPS.Operator[] operators;
            var oldViewOperatorCtrl = oldView.GetOperatorControl();
            var newViewOperatorCtrl = newView.GetOperatorControl();
            oldViewOperatorCtrl.Show(HPS.Operator.Priority.Low, out operators);
            newViewOperatorCtrl.Set(operators, HPS.Operator.Priority.Low);
            oldViewOperatorCtrl.Show(HPS.Operator.Priority.Default, out operators);
            newViewOperatorCtrl.Set(operators, HPS.Operator.Priority.Default);
            oldViewOperatorCtrl.Show(HPS.Operator.Priority.High, out operators);

            SetMainDistantLight();

            oldView.Delete();
        }

        public void ZoomToKeyPath(HPS.KeyPath keyPath)
        {
            HPS.BoundingKit bounding;
            if (keyPath.ShowNetBounding(true, out bounding))
            {
                var frontView = GetSprocketsControl().Canvas.GetFrontView();
                HPS.CameraKit frontViewCamera;
                frontView.GetSegmentKey().ShowCamera(out frontViewCamera);
                PreZoomToKeyPathCamera = frontViewCamera;

                frontView.ComputeFitWorldCamera(bounding, out frontViewCamera);
                frontView.SmoothTransition(frontViewCamera);
            }
        }

        public void RestorePreZoomToKeyPathCamera()
        {
            if (PreZoomToKeyPathCamera != null)
            {
                GetSprocketsControl().Canvas.GetFrontView().SmoothTransition(PreZoomToKeyPathCamera);
                HPS.Database.Sleep(500);

                InvalidatePreZoomToKeyPathCamera();
            }
        }

        public void InvalidatePreZoomToKeyPathCamera()
        {
            PreZoomToKeyPathCamera = null;
        }

		/// <summary>
		/// OnClosed override used for cleaning up HPS and Sprockets resources.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnClosed(EventArgs e)
		{
            string path = Directory.GetCurrentDirectory();
            path = System.IO.Path.Combine(path, "hps_wpf_sandbox.recent_files");
            if (System.IO.File.Exists(path))
                System.IO.File.WriteAllText(path, string.Empty);

            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(File.OpenWrite(path)))
            {
                for (int i = 0; i < RecentFiles.Items.Count; ++i)
                {
                    object o = RecentFiles.Items.GetItemAt(i);
                    RibbonGalleryItem item = o as RibbonGalleryItem;
                    sw.WriteLine((item.Content as TextBlock).Text);
                }
            }

            // Cleanup
            if (GetSprocketsControl() != null)
            {
                if (Model != null)
                {
                    Model.Delete();
                    Model.Dispose();
                    Model = null;
                }
                if (CADModel != null)
                {
                    CADModel.Delete();
                    CADModel.Dispose();
                    CADModel = null;
                }
                GetSprocketsControl().Delete();     // Calls Delete() on View and Canvas
                GetSprocketsControl().Dispose();
                _hpsWorld.Dispose();
                _hpsWorld = null;
            }

            base.OnClosed(e);
		}

        public bool HandledByParasolid(string filename)
        {
            string extension = System.IO.Path.GetExtension(filename);
            extension = extension.ToLower();
            if (extension == ".x_t" ||
                extension == ".x_b" ||
                extension == ".xmt_txt" ||
                extension == ".xmt_bin")
                return true;
            else
                return false;
        }

        public bool HandledByDWG(string filename)
        {
            string extension = System.IO.Path.GetExtension(filename);
            extension = extension.ToLower();
            if (extension == ".dwg" ||
                extension == ".dxf")
                return true;
            else
                return false;
        }

        public bool HandledByVisualize(string filename)
        {
            string extension = System.IO.Path.GetExtension(filename);
            extension = extension.ToLower();
            if (extension == ".hsf" ||
                extension == ".stl" ||
                extension == ".obj")
                return true;
            else
                return false;
        }

        public void RecentListItem_IsSelected(object sender, RoutedEventArgs e)
        {
            RibbonGalleryItem item = sender as RibbonGalleryItem;
            DemoFileOpenCommand file_open = new DemoFileOpenCommand(this);
            string filename = (item.Content as TextBlock).Text;
            file_open.ThreadedFileLoad(filename, GetSprocketsControl());

        }

        #region Demo Commands

        /// Demo Commands
        ///
        /// This section is for ICommands used in MainWindow.xaml
        /// There exists a separate class in DemoCommands.cs for each demo command.
        /// The Command is instantiated when first referenced.

        ///
        /// DemoCommand handles
        ///
        private DemoCommand _newCommand;
		private DemoCommand _exitCommand;
		private DemoCommand _fileOpenCommand;
        private DemoCommand _fileSaveCommand;
        private DemoCommand _filePrintCommand;
        private DemoCommand _orbitCommand;
		private DemoCommand _panCommand;
		private DemoCommand _zoomAreaCommand;
        private DemoCommand _flyCommand;
		private DemoCommand _homeCommand;
		private DemoCommand _zoomFitCommand;
		private DemoCommand _pointSelectCommand;
		private DemoCommand _areaSelectCommand;
		private DemoCommand _segmentBrowserCommand;
        private DemoCommand _modelBrowserCommand;

		private DemoCommand _simpleShadowModeCommand;
		private DemoCommand _smoothModeCommand;
		private DemoCommand _hiddenLineModeCommand;
		private DemoCommand _frameRateModeCommand;
		private DemoCommand _eyeDomeModeCommand;

		private DemoCommand _segmentBrowserRootCommand;
		private DemoCommand _propertiesToggleCommand;
        private DemoCommand _propertiesApplyCommand;
        private DemoCommand _propertiesSelectedObjectChangedCommand;
        private DemoCommand _propertiesPropertyValueChangedCommand;

		private DemoCommand _user1Command;
		private DemoCommand _user2Command;
		private DemoCommand _user3Command;
		private DemoCommand _user4Command;



        ///
        /// Public Demo Command Properties
        ///
        public ICommand NewCommand
		{
			get
			{
				if (_newCommand == null)
					_newCommand = new DemoNewCommand(this);
				return _newCommand;
			}
		}

		public ICommand ExitCommand
		{
			get
			{
				if (_exitCommand == null)
					_exitCommand = new DemoExitCommand(this);
				return _exitCommand;
			}
        }
        public ICommand FileOpenCommand
		{
			get
			{
				if (_fileOpenCommand == null)
					_fileOpenCommand = new DemoFileOpenCommand(this);
				return _fileOpenCommand;
			}
		}

        public ICommand FileSaveCommand
		{
			get
			{
                if (_fileSaveCommand == null)
                    _fileSaveCommand = new DemoFileSaveCommand(this);
                return _fileSaveCommand;
			}
		}

        public ICommand FilePrintCommand
        {
            get
            {
                if (_filePrintCommand == null)
                    _filePrintCommand = new DemoFilePrintCommand(this);
                return _filePrintCommand;
            }
        }

        public ICommand OrbitCommand
		{
			get
			{
				if (_orbitCommand == null)
					_orbitCommand = new DemoOrbitCommand(this);
				return _orbitCommand;
			}
		}

		public ICommand PanCommand
		{
			get
			{
				if (_panCommand == null)
					_panCommand = new DemoPanCommand(this);
				return _panCommand;
			}
		}

		public ICommand ZoomAreaCommand
		{
			get
			{
				if (_zoomAreaCommand == null)
					_zoomAreaCommand = new DemoZoomAreaCommand(this);
				return _zoomAreaCommand;
			}
		}

        public ICommand FlyCommand
        {
            get
            {
                if (_flyCommand == null)
                    _flyCommand = new DemoFlyCommand(this);
                return _flyCommand;
            }
        }

        public ICommand HomeCommand
        {
            get
            {
                if (_homeCommand == null)
                    _homeCommand = new DemoHomeCommand(this);
                return _homeCommand;
            }
        }

		public ICommand ZoomFitCommand
		{
			get
			{
				if (_zoomFitCommand == null)
					_zoomFitCommand = new DemoZoomFitCommand(this);
				return _zoomFitCommand;
			}
		}

		public ICommand PointSelectCommand
		{
			get
			{
				if (_pointSelectCommand == null)
					_pointSelectCommand = new DemoPointSelectCommand(this);
				return _pointSelectCommand;
			}
		}

		public ICommand AreaSelectCommand
		{
			get
			{
				if (_areaSelectCommand == null)
					_areaSelectCommand = new DemoAreaSelectCommand(this);
				return _areaSelectCommand;
			}
		}

		public ICommand SimpleShadowModeCommand
		{
			get
			{
				if (_simpleShadowModeCommand == null)
					_simpleShadowModeCommand = new DemoSimpleShadowModeCommand(this);
				return _simpleShadowModeCommand;
			}
		}

		public ICommand SmoothModeCommand
		{
			get
			{
				if (_smoothModeCommand == null)
                    _smoothModeCommand = new DemoSmoothModeCommand(this);
                return _smoothModeCommand;
			}
		}

		public ICommand HiddenLineModeCommand
		{
			get
			{
				if (_hiddenLineModeCommand == null)
                    _hiddenLineModeCommand = new DemoHiddenLineModeCommand(this);
                return _hiddenLineModeCommand;
			}
		}

		public ICommand FrameRateModeCommand
		{
			get
			{
				if (_frameRateModeCommand == null)
					_frameRateModeCommand = new DemoFrameRateModeCommand(this);
				return _frameRateModeCommand;
			}
		}
		public ICommand EyeDomeModeCommand
		{
			get
			{
				if (_eyeDomeModeCommand == null)
					_eyeDomeModeCommand = new DemoEyeDomeModeCommand(this);
				return _eyeDomeModeCommand;
			}
		}

		public ICommand SegmentBrowserCommand
		{
			get
			{
				if (_segmentBrowserCommand == null)
					_segmentBrowserCommand = new DemoSegmentBrowserCommand(this);
				return _segmentBrowserCommand;
			}
		}

		public ICommand SegmentBrowserRootCommand
		{
			get
			{
				if (_segmentBrowserRootCommand == null)
					_segmentBrowserRootCommand = new DemoSegmentBrowserRootCommand(this);
				return _segmentBrowserRootCommand;
			}
		}

		public ICommand PropertiesToggleCommand
		{
			get
			{
				if (_propertiesToggleCommand == null)
					_propertiesToggleCommand = new DemoPropertiesToggleCommand(this);
				return _propertiesToggleCommand;
			}
		}

        public ICommand PropertiesApplyCommand
        {
            get
            {
                if (_propertiesApplyCommand == null)
                    _propertiesApplyCommand = new DemoPropertiesApplyCommand(this);
                return _propertiesApplyCommand;
            }
        }

        public ICommand PropertiesSelectedObjectChangedCommand
        {
            get
            {
                if (_propertiesSelectedObjectChangedCommand == null)
                    _propertiesSelectedObjectChangedCommand = new DemoPropertiesSelectedObjectChangedCommand(this);
                return _propertiesSelectedObjectChangedCommand;
            }
        }

        public ICommand PropertiesPropertyValueChangedCommand
        {
            get
            {
                if (_propertiesPropertyValueChangedCommand == null)
                    _propertiesPropertyValueChangedCommand = new DemoPropertiesPropertyValueChangedCommand(this);
                return _propertiesPropertyValueChangedCommand;
            }
        }

        public ICommand ModelBrowserCommand
        {
            get
            {
                if (_modelBrowserCommand == null)
                    _modelBrowserCommand = new DemoModelBrowserCommand(this);
                return _modelBrowserCommand;
            }
        }

		public ICommand User1Command
		{
			get
			{
				if (_user1Command == null)
					_user1Command = new DemoUser1Command(this);
				return _user1Command;
			}
		}

		public ICommand User2Command
		{
			get
			{
				if (_user2Command == null)
					_user2Command = new DemoUser2Command(this);
				return _user2Command;
			}
		}
		public ICommand User3Command
		{
			get
			{
				if (_user3Command == null)
					_user3Command = new DemoUser3Command(this);
				return _user3Command;
			}
		}
		public ICommand User4Command
		{
			get
			{
				if (_user4Command == null)
					_user4Command = new DemoUser4Command(this);
				return _user4Command;
			}
		}
#endregion

#region Toolbar ToggleButton Dependency Properties

		/// Toolbar ToggleButton Dependency Properties
		///
		/// This section is for the Toggle Toolbar button bindings.
		/// Dependency properties are used to keep the toggle state of the buttons in sync with these properties.

		/// <summary>
		/// Current Toggle toolbar button
		/// </summary>
		private DependencyProperty CurrentToggleButtonOpProperty = IsCurrentOpOrbitProperty;

		///
		/// Public property wrappers for dependency properties
		///
		public bool IsCurrentOpOrbit
		{
			get { return (bool)GetValue(IsCurrentOpOrbitProperty); }
			set { SetValue(IsCurrentOpOrbitProperty, value); }
		}

		public bool IsCurrentOpPan
		{
			get { return (bool)GetValue(IsCurrentOpPanProperty); }
			set { SetValue(IsCurrentOpPanProperty, value); }
		}

		public bool IsCurrentOpZoomArea
		{
			get { return (bool)GetValue(IsCurrentOpZoomAreaProperty); }
			set { SetValue(IsCurrentOpZoomAreaProperty, value); }
		}

		public bool IsCurrentOpSelectPoint
		{
			get { return (bool)GetValue(IsCurrentOpSelectPointProperty); }
			set { SetValue(IsCurrentOpSelectPointProperty, value); }
		}

		public bool IsCurrentOpSelectArea
		{
			get { return (bool)GetValue(IsCurrentOpSelectAreaProperty); }
			set { SetValue(IsCurrentOpSelectAreaProperty, value); }
		}

        public bool IsCurrentOpFly
        {
            get { return (bool)GetValue(IsCurrentOpFlyProperty); }
            set { SetValue(IsCurrentOpFlyProperty, value); }
        }

		///
		/// Dependency Properties
		///
		private static readonly DependencyProperty IsCurrentOpOrbitProperty = RegisterToggleButtonOp("IsCurrentOpOrbit", true);
		private static readonly DependencyProperty IsCurrentOpPanProperty = RegisterToggleButtonOp("IsCurrentOpPan", false);
		private static readonly DependencyProperty IsCurrentOpZoomAreaProperty = RegisterToggleButtonOp("IsCurrentOpZoomArea", false);
		private static readonly DependencyProperty IsCurrentOpSelectPointProperty = RegisterToggleButtonOp("IsCurrentOpSelectPoint", false);
		private static readonly DependencyProperty IsCurrentOpSelectAreaProperty = RegisterToggleButtonOp("IsCurrentOpSelectArea", false);
        private static readonly DependencyProperty IsCurrentOpFlyProperty = RegisterToggleButtonOp("IsCurrentOpFly", false);

		/// <summary>
		/// Array of all toggle button properties
		/// </summary>
		private static readonly DependencyProperty[] ToggleButtonProperties =
		{
			IsCurrentOpOrbitProperty,
			IsCurrentOpPanProperty,
			IsCurrentOpZoomAreaProperty,
			IsCurrentOpSelectPointProperty,
			IsCurrentOpSelectAreaProperty,
            IsCurrentOpFlyProperty
		};

		/// <summary>
		/// Helper method for registering a dependency property for a toggle toolbar button
		/// </summary>
		/// <param name="name">Name of property</param>
		/// <param name="defaultValue">Default value</param>
		/// <returns>Registered DependencyProperty</returns>
		private static DependencyProperty RegisterToggleButtonOp(string name, bool defaultValue)
		{
			return DependencyProperty.Register(name, typeof(bool), typeof(MainWindow),
				new FrameworkPropertyMetadata(defaultValue, new PropertyChangedCallback(OnIsCurrentOpChanged)));
		}

		/// <summary>
		/// Property changed callback for toggle toolbar buttons.
		///
		/// Ensures that only one toggle toolbar button is selected, and that you cannot deselect the toggle.
		/// </summary>
		private static void OnIsCurrentOpChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
		{
			MainWindow win = obj as MainWindow;

			// Store property which is to be enabled
			if ((bool)args.NewValue == true)
				win.CurrentToggleButtonOpProperty = args.Property;

			// Ensure current property is enabled.
			win.SetValue(win.CurrentToggleButtonOpProperty, true);

			// Disable all other properties
			foreach (DependencyProperty dp in ToggleButtonProperties)
				if (dp != win.CurrentToggleButtonOpProperty)
					win.SetValue(dp, false);
		}

#endregion

#region RenderingMode ToggleButton Dependency Properties

        private DependencyProperty CurrentToggleButtonModeProperty = IsSmoothProperty;

        public bool IsSmooth
        {
            get { return (bool)GetValue(IsSmoothProperty); }
            set { SetValue(IsSmoothProperty, value); }
        }

        public bool IsHidden
        {
            get { return (bool)GetValue(IsHiddenProperty); }
            set { SetValue(IsHiddenProperty, value); }
        }

        private static readonly DependencyProperty IsSmoothProperty = RegisterToggleButtonModes("IsSmooth", true);
        private static readonly DependencyProperty IsHiddenProperty = RegisterToggleButtonModes("IsHidden", false);

        private static readonly DependencyProperty[] ToggleButtonModeProperties =
		{
			IsSmoothProperty,
			IsHiddenProperty,
		};

        private static DependencyProperty RegisterToggleButtonModes(string name, bool defaultValue)
        {
            return DependencyProperty.Register(name, typeof(bool), typeof(MainWindow),
                new FrameworkPropertyMetadata(defaultValue, new PropertyChangedCallback(OnIsCurrentModeChanged)));
        }

        private static void OnIsCurrentModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            MainWindow win = obj as MainWindow;

            // Store property which is to be enabled
            if ((bool)args.NewValue == true)
                win.CurrentToggleButtonModeProperty = args.Property;

            // Ensure current property is enabled.
            win.SetValue(win.CurrentToggleButtonModeProperty, true);

            // Disable all other properties
            foreach (DependencyProperty dp in ToggleButtonModeProperties)
                if (dp != win.CurrentToggleButtonModeProperty)
                    win.SetValue(dp, false);
        }

#endregion

		// doing this since ComboBox doesn't allow command binding
		private void RootComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			SegmentBrowserRootCommand.Execute(sender);
		}

        private void OnModelBrowserPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ModelBrowser.OnPreviewMouseRightButtonDown(sender, e);
        }

        private void OnSegmentBrowserPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            SegmentBrowser.OnPreviewMouseRightButtonDown(sender, e);
        }

        private void OnSegmentBrowserPreviewKeyDown(object sender, KeyEventArgs e)
        {
            SegmentBrowser.OnPreviewKeyDown(sender, e);
        }

        private void OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ConfigurationBrowser.OnMouseDoubleClick(sender, e);
        }

        private void PropertyGrid_SelectedObjectChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            PropertiesSelectedObjectChangedCommand.Execute(sender);
        }

        private void PropertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
        {
            PropertiesPropertyValueChangedCommand.Execute(e.OriginalSource);
        }
    }
}
