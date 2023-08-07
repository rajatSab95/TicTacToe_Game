using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;
using System.IO;
using Microsoft.Windows.Controls.Ribbon;
using System.Windows.Controls;
using System.Drawing.Printing;

namespace wpf_sandbox
{
	/// <summary>
	/// Base class for Demo Commands
	/// </summary>
	abstract class DemoCommand : ICommand
	{
		/// <summary>
		/// MainWindow instance
		/// </summary>
		protected MainWindow _win;

		/// <summary>
		/// Constructs DemoCommand
		/// </summary>
		/// <param name="win">MainWindow instance</param>
		public DemoCommand(MainWindow win)
		{
			if (win == null)
				throw new ArgumentNullException("win");

			_win = win;
		}

		#region ICommand Members

		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// 
		/// DemoCommands are executable by default
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does not require
		/// data to be passed, this object can be set to null.</param>
		/// <returns>true if this command can be executed; otherwise, false.</returns>
		public virtual bool CanExecute(object parameter)
		{
			return true;
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// 
		/// Override to provide command implementation
		/// </summary>
		/// <param name="parameter">Data used by the command. If the command does not require data
		/// to be passed, this object can be set to null.</param>
		public abstract void Execute(object parameter);

		#endregion // ICommand Members
	}

	/// <summary>
	/// New Command handler
	/// </summary>
	class DemoNewCommand : DemoCommand
	{
		public DemoNewCommand(MainWindow win) : base(win) { }

		public override void Execute(object parameter)
		{
			SprocketsWPFControl ctrl = _win.GetSprocketsControl();

			// Restore scene defaults defaults
            ctrl.Canvas.GetWindowKey().GetHighlightControl().UnhighlightEverything();
            _win.CreateNewModel();
			_win.SetupSceneDefaults();
			_win.SetTitle("");

            _win.InitializeBrowsers();

			// Trigger update
			ctrl.Canvas.Update();
		}
	}

	/// <summary>
	/// Exit Command handler
	/// </summary>
	class DemoExitCommand : DemoCommand
	{
		public DemoExitCommand(MainWindow win) : base(win) { }

		public override void Execute(object parameter)
		{
			_win.Close();
		}
	}

	/// <summary>
	/// File loading command handler
	/// </summary>
	class DemoFileOpenCommand : DemoCommand
	{
        private static int max_recent_file_list_length = 5;

		public DemoFileOpenCommand(MainWindow win) : base(win) { }

        private void AddToRecentFiles(string filename)
        {
            List<string> current_files = new List<string>();
            object o;
            for (int i = 0; i < _win.RecentFiles.Items.Count; ++i)
            {
                o = _win.RecentFiles.Items.GetItemAt(i);
                current_files.Add(((o as RibbonGalleryItem).Content as TextBlock).Text);
            }

            if (current_files.Contains(filename))
                return;


            if (_win.RecentFiles.Items.Count > max_recent_file_list_length - 1)
            {

                for (int i = 0; i < _win.RecentFiles.Items.Count - 1; ++i)
                {
                    o = _win.RecentFiles.Items.GetItemAt(i);
                    RibbonGalleryItem item = o as RibbonGalleryItem;
                    o = _win.RecentFiles.Items.GetItemAt(i + 1);
                    RibbonGalleryItem next_item = o as RibbonGalleryItem;
                    item.Content = next_item.Content;
                }
                o = _win.RecentFiles.Items.GetItemAt(max_recent_file_list_length - 1);
                RibbonGalleryItem last_item = o as RibbonGalleryItem;
                var textBlock = new TextBlock();
                textBlock.Text = filename;
                last_item.Content = textBlock;
            }
            else
            {
                RibbonGalleryItem rgi = new RibbonGalleryItem();
                var textBlock = new TextBlock();
                textBlock.Text = filename;
                rgi.Content = textBlock;
                rgi.CheckedBackground = null;
                rgi.CheckedBorderBrush = null;
                rgi.Selected += _win.RecentListItem_IsSelected;
                _win.RecentFiles.Items.Add(rgi);
            }
        }

		/// <summary>
		/// File load delegate for loading file on separate thread
		/// </summary>
		private delegate void FileLoadDelegate(string filename, SprocketsWPFControl control);

		public override void Execute(object parameter)
		{
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            string dialog_filter = "HOOPS Stream Files (.hsf)|*.hsf|" +
                                "Point Cloud Files (*.ptx, *.pts, *.xyz)|*.ptx;*.pts;*.xyz|" +
                                "StereoLithography Files (.stl)|*.stl|" +
                                "Wavefront Files (.obj)|*.obj|";
#if USING_EXCHANGE
            dialog_filter += "All CAD Files (*.3ds, *.3mf, *.3dxml, *.sat, *.sab, *_pd, *.model, *.dlv, *.exp, *.session, *.CATPart, *.CATProduct, *.CATShape, *.CATDrawing" +
                         ", *.cgr, *.dae, *.prt, *.prt.*, *.neu, *.neu.*, *.asm, *.asm.*, *.xas, *.xpr, *.fbx, *.gltf, *.glb, *.arc, *.unv, *.mf1, *.prt, *.pkg, *.ifc, *.ifczip, *.igs, *.iges, *.ipt, *.iam" +
                         ", *.jt, *.kmz, *.nwd, *.prt, *.pdf, *.prc, *.x_t, *.xmt, *.x_b, *.xmt_txt, *.rvt, *.3dm, *.stp, *.step, *.stpz, *.stp.z, *.stl, *.par, *.asm, *.pwd, *.psm" +
                         ", *.sldprt, *.sldasm, *.sldfpp, *.asm, *.u3d, *.vda, *.wrl, *.vrml, *.xv3, *.xv0, *.hsf, *.ptx, *.pts, *.xyz, *.obj)|" +
                         "*.3ds;*.3dxml;*.sat;*.sab;*_pd;*.model;*.dlv;*.exp;*.session;*.catpart;*.catproduct;*.catshape;*.catdrawing" +
                         ";*.cgr;*.dae;*.prt;*.prt.*;*.neu;*.neu.*;*.asm;*.asm.*;*.xas;*.xpr;*.fbx;*.gltf;*.glb;*.arc;*.unv;*.mf1;*.prt;*.pkg;*.ifc;*.ifczip;*.igs;*.iges;*.ipt;*.iam" +
                         ";*.jt;*.kmz;*.prt;*.pdf;*.prc;*.x_t;*.xmt;*.x_b;*.xmt_txt;*.rvt;*.3dm;*.stp;*.step;*.stpz;*.stp.z;*.stl;*.par;*.asm;*.pwd;*.psm" +
                         ";*.sldprt;*.sldasm;*.sldfpp;*.asm;*.u3d;*.vda;*.wrl;*.vrml;*.xv3;*.xv0;*.hsf;*.ptx;*.pts;*.xyz;*.obj|" +
                         "3D Studio Files (*.3ds)|*.3ds|" +
                         "3D Manufacturing Files (*.3mf)|*.3mf|" +
                         "3DXML Files (*.3dxml)|*.3dxml|" +
                         "ACIS SAT Files (*.sat, *.sab)|*.sat;*.sab|" +
                         "CADDS Files (*_pd)|*_pd|" +
                         "CATIA V4 Files (*.model, *.dlv, *.exp, *.session)|*.model;*.dlv;*.exp;*.session|" +
                         "CATIA V5 Files (*.CATPart, *.CATProduct, *.CATShape, *.CATDrawing)|*.catpart;*.catproduct;*.catshape;*.catdrawing|" +
                         "CGR Files (*.cgr)|*.cgr|" +
                         "Collada Files (*.dae)|*.dae|" +
                         "Creo (ProE) Files (*.prt, *.prt.*, *.neu, *.neu.*, *.asm, *.asm.*, *.xas, *.xpr)|*.prt;*.prt.*;*.neu;*.neu.*;*.asm;*.asm.*;*.xas;*.xpr|" +
                         "FBX Files (*.fbx)|*.fbx|" +
                         "GLTF Files (*.gltf, *.glb)|*.gltf;*.glb|" +
                         "I-DEAS Files (*.arc, *.unv, *.mf1, *.prt, *.pkg)|*.arc;*.unv;*.mf1;*.prt;*.pkg|" +
                         "IFC Files (*.ifc, *.ifczip)|*.ifc;*.ifczip|" +
                         "IGES Files (*.igs, *.iges)|*.igs;*.iges|" +
                         "Inventor Files (*.ipt, *.iam)|*.ipt;*.iam|" +
                         "JT Files (*.jt)|*.jt|" +
                         "KMZ Files (*.kmz)|*.kmz|" +
                         "Navisworks Files (*.nwd)|*.nwd|" +
                         "NX (Unigraphics) Files (*.prt)|*.prt|" +
                         "PDF Files (*.pdf)|*.pdf|" +
                         "PRC Files (*.prc)|*.prc|" +
                         "Parasolid Files (*.x_t, *.xmt, *.x_b, *.xmt_txt)|*.x_t;*.xmt;*.x_b;*.xmt_txt|" +
                         "Revit Files (*.rvt)|*.rvt|" +
                         "Rhino Files (*.3dm)|*.3dm|" +
                         "SolidEdge Files (*.par, *.asm, *.pwd, *.psm)|*.par;*.asm;*.pwd;*.psm|" +
                         "SolidWorks Files (*.sldprt, *.sldasm, *.sldfpp, *.asm)|*.sldprt;*.sldasm;*.sldfpp;*.asm|" +
                         "STEP Files (*.stp, *.step, *.stpz, *.stp.z)|*.stp;*.step;*.stpz;*.stp.z|" +
                         "STL Files (*.stl)|*.stl|" +
                         "Universal 3D Files (*.u3d)|*.u3d|" +
                         "VDA Files (*.vda)|*.vda|" +
                         "VRML Files (*.wrl, *.vrml)|*.wrl;*.vrml|" +
                         "XVL Files (*.xv3, *.xv0)|*.xv0;*.xv3|";
#endif

#if USING_PARASOLID
            dialog_filter += "Parasolid Files (*.x_t, *.x_b, *.xmt_txt, *.xmt_bin)|*.x_t;*.x_b;*.xmt_txt;*.xmt_bin|";
#endif

#if USING_DWG
#if !DEBUG
            dialog_filter += "DWG Files (*.dwg, *.dxf)|*.dwg;*.dxf|";
#endif
#endif

            dialog_filter += "All Files (*.*)|*.*";
            dlg.Filter = dialog_filter;
            dlg.DefaultExt = ".hsf";
			dlg.InitialDirectory = Path.GetFullPath(_win.samplesDir);
			Nullable<bool> result = dlg.ShowDialog();
			if (result == true)
				OpenDatasetFile(dlg.FileName);
		}

// 		/// <summary>
// 		/// Helper method to perform UI actions on main thread
// 		/// </summary>
		private void InvokeUIAction(Action action, bool wait)
		{
			if (wait)
				_win.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(action));
			else
				_win.Dispatcher.BeginInvoke(new Action(action));
		}

        private bool DisplayImportProgress(HPS.IONotifier importNotifier)
        {
            bool success = false;
            InvokeUIAction(delegate()
            {
                //show the progress dialog
                _win.GetSprocketsControl().IsEnabled = false;
                var dlg = new ProgressBar(_win, importNotifier);
                dlg.Owner = _win;
                dlg.ShowDialog();

                success = dlg.WasSuccessful();
            }, true);
            return success;
        }

        private bool DisplayImportProgress(ProgressBar dlg)
        {
            bool success = false;
            InvokeUIAction(delegate()
            {
                _win.GetSprocketsControl().IsEnabled = false;
                dlg.Owner = _win;
                dlg.ShowDialog();

                success = dlg.WasSuccessful();
            }, true);
            return success;
        }

        private string GetErrorString(HPS.IOResult status, string filename, string exceptionMessage)
        {
            string errorString = "";
            switch (status)
            {
                case HPS.IOResult.FileNotFound:
                    errorString = "Could not locate file " + filename;
                    break;

                case HPS.IOResult.UnableToOpenFile:
                    errorString = "Unable to open file " + filename;
                    break;

                case HPS.IOResult.InvalidOptions:
                    errorString = "Invalid options: " + exceptionMessage;
                    break;

                case HPS.IOResult.InvalidSegment:
                    errorString = "Invalid segment: " + exceptionMessage;
                    break;

                case HPS.IOResult.UnableToLoadLibraries:
                    errorString = "Unable to load libraries: " + exceptionMessage;
                    break;

                case HPS.IOResult.VersionIncompatibility:
                    errorString = "Version incompatability: " + exceptionMessage;
                    break;

                case HPS.IOResult.InitializationFailed:
                    errorString = "Initialization failed: " + exceptionMessage;
                    break;

                case HPS.IOResult.UnsupportedFormat:
                    errorString = "Unsupported format.";
                    break;

                case HPS.IOResult.Canceled:
                    errorString = "IO canceled.";
                    break;

                case HPS.IOResult.Failure:
                default:
                    errorString = "Error loading file " + filename + ":\n\t" + exceptionMessage;
                    break;
            }
            return errorString;
        }

        private bool ImportHSFFile(string filename, ref HPS.Stream.ImportResultsKit importResults)
        {
            HPS.Stream.ImportOptionsKit importOptions = new HPS.Stream.ImportOptionsKit();
            importOptions.SetSegment(_win.Model.GetSegmentKey());
            importOptions.SetAlternateRoot(_win.Model.GetLibraryKey());
            importOptions.SetPortfolio(_win.Model.GetPortfolioKey());

            HPS.Stream.ImportNotifier importNotifier = new HPS.Stream.ImportNotifier();
            HPS.IOResult status;
            string exceptionMessage = "";
            try
            {
                importNotifier = HPS.Stream.File.Import(filename, importOptions);
                DisplayImportProgress(importNotifier);
                status = importNotifier.Status();

                if (status == HPS.IOResult.Success)
                    importResults = importNotifier.GetResults();
            }
            catch (HPS.IOException ex)
            {
                exceptionMessage = ex.Message;
                status = ex.result;
            }

            if (status != HPS.IOResult.Success)
                MessageBox.Show(GetErrorString(status, filename, exceptionMessage));

            return status == HPS.IOResult.Success;
        }

        private bool ImportSTLFile(string filename)
        {
            HPS.STL.ImportOptionsKit importOptions = HPS.STL.ImportOptionsKit.GetDefault();
            importOptions.SetSegment(_win.Model.GetSegmentKey());

            HPS.STL.ImportNotifier importNotifier = new HPS.STL.ImportNotifier();
            HPS.IOResult status;
            string exceptionMessage = "";
            try
            {
                importNotifier = HPS.STL.File.Import(filename, importOptions);
                DisplayImportProgress(importNotifier);
                status = importNotifier.Status();
            }
            catch (HPS.IOException ex)
            {
                exceptionMessage = ex.Message;
                status = ex.result;
            }

            if (status != HPS.IOResult.Success)
                MessageBox.Show(GetErrorString(status, filename, exceptionMessage));

            return status == HPS.IOResult.Success;
        }

        private bool ImportOBJFile(string filename)
        {
            HPS.OBJ.ImportOptionsKit importOptions = new HPS.OBJ.ImportOptionsKit();
            importOptions.SetSegment(_win.Model.GetSegmentKey());
            importOptions.SetPortfolio(_win.Model.GetPortfolioKey());

            HPS.OBJ.ImportNotifier importNotifier = new HPS.OBJ.ImportNotifier();
            HPS.IOResult status;
            string exceptionMessage = "";
            try
            {
                importNotifier = HPS.OBJ.File.Import(filename, importOptions);
                DisplayImportProgress(importNotifier);
                status = importNotifier.Status();
            }
            catch (HPS.IOException ex)
            {
                exceptionMessage = ex.Message;
                status = ex.result;
            }

            if (status != HPS.IOResult.Success)
                MessageBox.Show(GetErrorString(status, filename, exceptionMessage));

            return status == HPS.IOResult.Success;
		}
		private bool ImportPointCloudFile(string filename)
		{
			HPS.PointCloud.ImportOptionsKit importOptions = new HPS.PointCloud.ImportOptionsKit();
			importOptions.SetSegment(_win.Model.GetSegmentKey());

			HPS.PointCloud.ImportNotifier importNotifier = new HPS.PointCloud.ImportNotifier();
			HPS.IOResult status;
			string exceptionMessage = "";
			try
			{
				importNotifier = HPS.PointCloud.File.Import(filename, importOptions);
				DisplayImportProgress(importNotifier);
				status = importNotifier.Status();
			}
			catch (HPS.IOException ex)
			{
				exceptionMessage = ex.Message;
				status = ex.result;
			}

			if (status != HPS.IOResult.Success)
				MessageBox.Show(GetErrorString(status, filename, exceptionMessage));

			return status == HPS.IOResult.Success;
		}

#if USING_EXCHANGE
        private bool ImportExchangeFile(string filename, HPS.Exchange.ImportOptionsKit importOptions)
        {
            HPS.Exchange.ImportNotifier importNotifier = null;
            HPS.IOResult status;
            string exceptionMessage = "";
            try
            {
                HPS.Exchange.ImportOptionsKit modifiedImportOptions;
                if (importOptions != null)
                    modifiedImportOptions = new HPS.Exchange.ImportOptionsKit(importOptions);
                else
                    modifiedImportOptions = new HPS.Exchange.ImportOptionsKit();
                modifiedImportOptions.SetBRepMode(HPS.Exchange.BRepMode.BRepAndTessellation);

                HPS.Exchange.File.Format format = HPS.Exchange.File.GetFormat(filename);
                string[] configuration;
                HPS.Exchange.Configuration[] allConfigurations;
                if (format == HPS.Exchange.File.Format.CATIAV4
                    && !modifiedImportOptions.ShowConfiguration(out configuration)
                    && (allConfigurations = HPS.Exchange.File.GetConfigurations(filename)).Length > 0)
                {
                    // CATIA V4 files w/ configurations must specify a configuration otherwise nothing will be loaded
                    // So if this file has configurations and no configuration was specified, load the first configuration
                    modifiedImportOptions.SetConfiguration(GetFirstConfiguration(allConfigurations));
                }

                importNotifier = HPS.Exchange.File.Import(filename, modifiedImportOptions);

                bool success = false;
                InvokeUIAction(delegate()
                {
                    //show the progress dialog
                    _win.GetSprocketsControl().IsEnabled = false;
                    var dlg = new hps_wpf_sandbox.ExchangeImportDialog.ExchangeImportDialog(_win, importNotifier);
                    dlg.Owner = _win;
                    dlg.ShowDialog();

                    success = dlg.WasSuccessful();
                }, true);

                status = importNotifier.Status();
            }
            catch (HPS.IOException ex)
            {
                exceptionMessage = ex.Message;
                status = ex.result;
            }

            if (status != HPS.IOResult.Success)
                MessageBox.Show(GetErrorString(status, filename, exceptionMessage));

            return status == HPS.IOResult.Success;
        }

        private string[] GetFirstConfiguration(HPS.Exchange.Configuration[] configurations)
        {
            if (configurations == null || configurations.Length == 0)
                return null;

            var firstConfiguration = new List<string>();
            firstConfiguration.Add(configurations[0].GetName());
            var subconfiguration = GetFirstConfiguration(configurations[0].GetSubconfigurations());
            if (subconfiguration != null)
                firstConfiguration.AddRange(subconfiguration);
            return firstConfiguration.ToArray();
        }

        public void ImportConfiguration(string[] configuration)
        {
            if (_win.CADModel != null)
            {
                string filename = new HPS.StringMetadata(_win.CADModel.GetMetadata("Filename")).GetValue();

                _win.CADModel.Delete();
                _win.CADModel = null;

                var importOptions = new HPS.Exchange.ImportOptionsKit();
                importOptions.SetConfiguration(configuration);

                ImportExchangeFile(filename, importOptions);


                InvokeUIAction(delegate()
                {
                    // make sure the segment browser has a valid root
                    _win.SegmentBrowserRootCommand.Execute(null);

                    _win.ModelBrowser.Init();
                    _win.ConfigurationBrowser.Init();

                    _win.GetSprocketsControl().IsEnabled = true;
                }, false);
            }
        }
#endif

#if USING_PARASOLID
        private bool ImportParasolidFile(string filename, HPS.Parasolid.ImportOptionsKit importOptions)
        {
            HPS.Parasolid.ImportNotifier importNotifier = null;
            HPS.IOResult status;
            string exceptionMessage = "";
            try
            {
                HPS.Parasolid.ImportOptionsKit modifiedImportOptions;
                if (importOptions != null)
                    modifiedImportOptions = new HPS.Parasolid.ImportOptionsKit(importOptions);
                else
                    modifiedImportOptions = new HPS.Parasolid.ImportOptionsKit();
                HPS.Parasolid.Format format;
                if (!modifiedImportOptions.ShowFormat(out format))
                {
                    string ext = System.IO.Path.GetExtension(filename);
                    ext = ext.ToLower();
                    if (ext == ".x_t" || ext == ".xmt_txt")
                        format = HPS.Parasolid.Format.Text;
                    else // assuming not a neutral binary format
                        format = HPS.Parasolid.Format.Binary;
                    modifiedImportOptions.SetFormat(format);
                }

                importNotifier = HPS.Parasolid.File.Import(filename, modifiedImportOptions);
                DisplayImportProgress(importNotifier);
                status = importNotifier.Status();

                if (status == HPS.IOResult.Success)
                    _win.CADModel = importNotifier.GetCADModel();
            }
            catch (HPS.IOException ex)
            {
                exceptionMessage = ex.Message;
                status = ex.result;
            }

            if (status != HPS.IOResult.Success)
                MessageBox.Show(GetErrorString(status, filename, exceptionMessage));

            return status == HPS.IOResult.Success;
        }
#endif

#if USING_DWG
        private bool ImportDWGFile(string filename, HPS.DWG.ImportOptionsKit importOptions)
        {
            HPS.DWG.ImportNotifier importNotifier = null;
            HPS.IOResult status;
            string exceptionMessage = "";
            try
            {
                HPS.DWG.ImportOptionsKit modifiedImportOptions;
                if (importOptions != null)
                    modifiedImportOptions = new HPS.DWG.ImportOptionsKit(importOptions);
                else
                    modifiedImportOptions = new HPS.DWG.ImportOptionsKit();
                
                importNotifier = HPS.DWG.File.Import(filename, modifiedImportOptions);
                DisplayImportProgress(importNotifier);
                status = importNotifier.Status();

                if (status == HPS.IOResult.Success)
                    _win.CADModel = importNotifier.GetCADModel();
            }
            catch (HPS.IOException ex)
            {
                exceptionMessage = ex.Message;
                status = ex.result;
            }

            if (status != HPS.IOResult.Success)
                MessageBox.Show(GetErrorString(status, filename, exceptionMessage));

            return status == HPS.IOResult.Success;
        }
#endif

		/// <summary>
		/// Method for loading the model on a separate thread
		/// </summary>
		public void ThreadedFileLoad(string filename, SprocketsWPFControl control)
		{
            _win.CreateNewModel();

			// Perform file load
            bool loaded = false;
            string ext = System.IO.Path.GetExtension(filename);
            ext = ext.ToLower();

            HPS.Stream.ImportResultsKit importResults = null;
            if (ext == ".hsf")
                loaded = ImportHSFFile(filename, ref importResults);
            else if (ext == ".stl")
                loaded = ImportSTLFile(filename);
            else if (ext == ".obj")
                loaded = ImportOBJFile(filename);
			else if (ext == ".ptx" || ext == ".pts" || ext == ".xyz")
				loaded = ImportPointCloudFile(filename);
#if USING_PARASOLID
            else if (ext == ".x_t" || ext == ".xmt_txt" || ext == ".x_b" || ext == ".xmt_bin")
                loaded = ImportParasolidFile(filename, null);
#endif
#if USING_DWG
#if !DEBUG
            else if (ext == ".dwg" || ext == ".dxf")
                loaded = ImportDWGFile(filename, null);
#endif
#endif
#if USING_EXCHANGE
            else
                loaded = ImportExchangeFile(filename, null);
#else
			else
                MessageBox.Show("This file format is not handled.");
#endif

            InvokeUIAction(delegate()
			{
                // make sure the segment browser has a valid root
                _win.SegmentBrowserRootCommand.Execute(null);

                _win.ConfigurationBrowser.Init();

                if (loaded)
                {
                    if (_win.CADModel == null)
                    {
                        HPS.CameraKit defaultCamera = null;
                        if (importResults == null || !importResults.ShowDefaultCamera(out defaultCamera))
                            control.Canvas.GetFrontView().ComputeFitWorldCamera(out defaultCamera);
                        _win.DefaultCamera = defaultCamera;
                    }

                    // Set new Window title
                    _win.SetTitle("- " + Path.GetFileName(filename));

                    AddToRecentFiles(filename);
                }

				_win.GetSprocketsControl().IsEnabled = true;
			}, false);
		}

        /// <summary>
		/// Loads dataset model and attaches to main view
		/// </summary>
		/// <param name="filename"></param>
		private void OpenDatasetFile(string filename)
		{	
            FileLoadDelegate load;
            load = new FileLoadDelegate(ThreadedFileLoad);

			// Perform asynchronous file load on a separate thread
			load.BeginInvoke(filename, _win.GetSprocketsControl(), null, null);
		}
	}

    /// <summary>
    /// File saving command handler
    /// </summary>
    class DemoFileSaveCommand : DemoCommand
    {
        public DemoFileSaveCommand(MainWindow win) : base(win) { }

        private void InvokeUIAction(Action action, bool wait)
        {
            if (wait)
                _win.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(action));
            else
                _win.Dispatcher.BeginInvoke(new Action(action));
        }

        private bool DisplayExportProgress(HPS.IONotifier notifier)
        {
            bool success = false;
            InvokeUIAction(delegate()
            {
                //show the progress dialog
                _win.GetSprocketsControl().IsEnabled = false;
                var dlg = new ProgressBar(_win, notifier, ProgressBar.Operation.Export);
                dlg.Owner = _win;
                dlg.ShowDialog();

                success = dlg.WasSuccessful();
                _win.GetSprocketsControl().IsEnabled = true;
            }, true);
            return success;
        }

        public override void Execute(object parameter)
        {
            SprocketsWPFControl ctrl = _win.GetSprocketsControl();
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
#if USING_PUBLISH
            dlg.Filter = "2D PDF|*.pdf|Postscript|*.ps|HSF|*.hsf|JPG|*.jpg|PNG|*.png|3D PDF|*.pdf";
#else
            dlg.Filter = "2D PDF|*.pdf|Postscript|*.ps|HSF|*.hsf|JPG|*.jpg|PNG|*.png";
#endif
            dlg.ShowDialog();

            if (dlg.FileName != "")
            {
                //NOTE - FilterIndex is 1 based
                switch (dlg.FilterIndex)
                {
                    case 1:
                        {
                            try
                            {
                                HPS.Hardcopy.File.Export(dlg.FileName,
                                                        HPS.Hardcopy.File.Driver.PDF,
                                                        ctrl.Canvas.GetWindowKey(),
                                                        HPS.Hardcopy.File.ExportOptionsKit.GetDefault());
                            }
                            catch (HPS.IOException e)
                            { MessageBox.Show("HPS::Hardcopy::File::Export threw an exception: " + e.Message); }
                        }
                        break;
                    case 2:
                        {
                            try
                            {
                                HPS.Hardcopy.File.Export(dlg.FileName,
                                                        HPS.Hardcopy.File.Driver.Postscript,
                                                        ctrl.Canvas.GetWindowKey(),
                                                        HPS.Hardcopy.File.ExportOptionsKit.GetDefault());
                            }
                            catch (HPS.IOException e)
                            { MessageBox.Show("HPS::Hardcopy::File::Export threw an exception: " + e.Message); }
                        }
                        break;
                    case 3:
                        {
                            HPS.SegmentKey exportFromHere;
                            HPS.Model model = ctrl.Canvas.GetFrontView().GetAttachedModel();
                            if (model.Type() == HPS.Type.None)
                                exportFromHere = ctrl.Canvas.GetFrontView().GetSegmentKey();
                            else
                                exportFromHere = model.GetSegmentKey();

                            HPS.IOResult status = HPS.IOResult.Failure;
                            try 
                            { 
                                HPS.Stream.ExportNotifier notifier = HPS.Stream.File.Export(dlg.FileName, exportFromHere, new HPS.Stream.ExportOptionsKit());
                                DisplayExportProgress(notifier);
                                status = notifier.Status();
                            }
                            catch (HPS.IOException e)
                            { MessageBox.Show("HPS::Stream::File::Export threw an exception: " + e.Message); }
                            if (status != HPS.IOResult.Success && status != HPS.IOResult.Canceled)
                                MessageBox.Show("HPS.Stream.Export encountered an error.");
                        }
                        break;
                    case 4:
                        {
                            HPS.Image.ExportOptionsKit eok = new HPS.Image.ExportOptionsKit();
                            eok.SetFormat(HPS.Image.Format.Jpeg);

                            try
                            {
                                HPS.Image.File.Export(dlg.FileName,
                                                    ctrl.Canvas.GetWindowKey(),
                                                    eok);
                            }
                            catch (HPS.IOException e)
                            { MessageBox.Show("HPS::Image::File::Export threw an exception: " + e.Message); }
                        }
                        break;
                    case 5:
                        {
                            try
                            {
                                HPS.Image.File.Export(dlg.FileName,
                                                        ctrl.Canvas.GetWindowKey(),
                                                        HPS.Image.ExportOptionsKit.GetDefault());
                            }
                            catch (HPS.IOException e)
                            { MessageBox.Show("HPS::Image::File::Export threw an exception: " + e.Message); }
                        }
                        break;
#if USING_PUBLISH
                    case 6:
                        {
                            try
                            {
                                HPS.SprocketPath sprocket_path = new HPS.SprocketPath(ctrl.Canvas, ctrl.Canvas.GetAttachedLayout(), ctrl.Canvas.GetFrontView(), ctrl.Canvas.GetFrontView().GetAttachedModel());
                                HPS.Publish.ExportOptionsKit export_kit = new HPS.Publish.ExportOptionsKit();
                                HPS.KeyPath [] key_path = {sprocket_path.GetKeyPath()};
                                HPS.Publish.File.ExportPDF(key_path, dlg.FileName, export_kit);
                            }
                            catch (HPS.IOException e)
                            { MessageBox.Show("HPS::Publish::File::Export threw an exception: " + e.Message); }
                        }
                        break;
#endif
                }
            }
        }
    }

    /// <summary>
    /// File saving command handler
    /// </summary>
    class DemoFilePrintCommand : DemoCommand
    {
        public DemoFilePrintCommand(MainWindow win) : base(win) { }

        private void InvokeUIAction(Action action, bool wait)
        {
            if (wait)
                _win.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(action));
            else
                _win.Dispatcher.BeginInvoke(new Action(action));
        }
        
        public override void Execute(object parameter)
        {
            using (var printDialog = new System.Windows.Forms.PrintDialog())
            {
                printDialog.Document = new System.Drawing.Printing.PrintDocument();                                
                printDialog.Document.PrintPage += (sender, e) =>
                {                    
                    var hdc = e.Graphics.GetHdc();
                    try
                    {
                        var options = new HPS.Hardcopy.GDI.ExportOptionsKit();

                        var ioResult = HPS.Hardcopy.GDI.Export(
                            hdc,
                            hdc,
                            _win.GetSprocketsControl().Canvas.GetWindowKey(),
                            options);
                    }
                    catch (HPS.IOException)
                    {

                    }
                    finally
                    {
                        e.Graphics.ReleaseHdc();
                    }
                };

                if (printDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    printDialog.Document.Print();
            }
        }        
    }

    /// <summary>
    /// Orbit Command handler
    /// </summary>
    class DemoOrbitCommand : DemoCommand
	{
		public DemoOrbitCommand(MainWindow win) : base(win) { }

		public override void Execute(object parameter)
		{
			HPS.View view = _win.GetSprocketsControl().Canvas.GetFrontView();
			view.GetOperatorControl().Pop();
			view.GetOperatorControl().Push(new HPS.OrbitOperator(HPS.MouseButtons.ButtonLeft()));
            _win.GetSprocketsControl().Focus();
        }
	}

	/// <summary>
	/// Pan Command handler
	/// </summary>
	class DemoPanCommand : DemoCommand
	{
		public DemoPanCommand(MainWindow win) : base(win) { }

		public override void Execute(object parameter)
		{
			HPS.View view = _win.GetSprocketsControl().Canvas.GetFrontView();
			view.GetOperatorControl().Pop();
			view.GetOperatorControl().Push(new HPS.PanOperator(HPS.MouseButtons.ButtonLeft()));
            _win.GetSprocketsControl().Focus();
        }
	}

	/// <summary>
	/// Zoom Area Command handler
	/// </summary>
	class DemoZoomAreaCommand : DemoCommand
	{
		public DemoZoomAreaCommand(MainWindow win) : base(win) { }

		public override void Execute(object parameter)
		{
			HPS.View view = _win.GetSprocketsControl().Canvas.GetFrontView();
			view.GetOperatorControl().Pop();
			view.GetOperatorControl().Push(new HPS.ZoomBoxOperator(HPS.MouseButtons.ButtonLeft()));
            _win.GetSprocketsControl().Focus();
        }
	}

    /// <summary>
    /// Fly Command handler
    /// </summary>
    class DemoFlyCommand : DemoCommand
    {
        public DemoFlyCommand(MainWindow win) : base(win) { }

        public override void Execute(object parameter)
        {
            HPS.View view = _win.GetSprocketsControl().Canvas.GetFrontView();
            view.GetOperatorControl().Pop();
            view.GetOperatorControl().Push(new HPS.FlyOperator());
            _win.GetSprocketsControl().Focus();
        }
    }

    class DemoHomeCommand : DemoCommand
    {
        public DemoHomeCommand(MainWindow win) : base(win) { }

        public override void Execute(object parameter)
        {
			try
			{
				if (_win.CADModel != null)
					_win.AttachViewWithSmoothTransition(_win.CADModel.ActivateDefaultCapture().FitWorld());
				else if (_win.DefaultCamera != null)
					_win.GetSprocketsControl().Canvas.GetFrontView().SmoothTransition(_win.DefaultCamera);
			}
			catch (HPS.InvalidSpecificationException)
			{
				//SmoothTransition() can throw if there is no model attached
			}
        }
    }

	/// <summary>
	/// Zoom Fit Command handler
	/// </summary>
	class DemoZoomFitCommand : DemoCommand
	{
		public DemoZoomFitCommand(MainWindow win) : base(win) { }

		public override void Execute(object parameter)
		{
			SprocketsWPFControl ctrl = _win.GetSprocketsControl();
            HPS.View frontView = ctrl.Canvas.GetFrontView();
            HPS.CameraKit fitWorldCamera;
            frontView.ComputeFitWorldCamera(out fitWorldCamera);
            frontView.SmoothTransition(fitWorldCamera);
		}
	}

	/// <summary>
	/// Point Select Command handler
	/// </summary>
	class DemoPointSelectCommand : DemoCommand
	{
		public DemoPointSelectCommand(MainWindow win) : base(win) { }

		public override void Execute(object parameter)
		{
			HPS.View view = _win.GetSprocketsControl().Canvas.GetFrontView();
			view.GetOperatorControl().Pop();
			view.GetOperatorControl().Push(new SandboxHighlightOperator(_win));
            _win.GetSprocketsControl().Focus();
        }
	}

	/// <summary>
	/// Area Select Command handler
	/// </summary>
	class DemoAreaSelectCommand : DemoCommand
	{
		public DemoAreaSelectCommand(MainWindow win) : base(win) { }

		public override void Execute(object parameter)
		{
			HPS.View view = _win.GetSprocketsControl().Canvas.GetFrontView();
			view.GetOperatorControl().Pop();
			view.GetOperatorControl().Push(new HPS.HighlightAreaOperator(HPS.MouseButtons.ButtonLeft()));
            _win.GetSprocketsControl().Focus();
        }
	}

	class DemoSegmentBrowserCommand : DemoCommand
	{
		private GridLength _savedSegmentBrowserWidth = new GridLength(225, GridUnitType.Pixel);
		private GridLength _savedVerticalSplitterWidth = new GridLength(1, GridUnitType.Auto);
		private GridLength _savedCanvasPanelWidth = new GridLength(1, GridUnitType.Star);

		public DemoSegmentBrowserCommand(MainWindow win) : base(win) { }

		public override void Execute(object parameter)
		{
			if (_win.SegmentBrowserButton.IsChecked.Value)
			{
				_win._canvasBrowserGrid.ColumnDefinitions[0].Width = _savedSegmentBrowserWidth;
				_win._canvasBrowserGrid.ColumnDefinitions[1].Width = _savedVerticalSplitterWidth;
				_win._canvasBrowserGrid.ColumnDefinitions[2].Width = _savedCanvasPanelWidth;
			}
			else
			{
				_savedSegmentBrowserWidth = _win._canvasBrowserGrid.ColumnDefinitions[0].Width;
				_savedVerticalSplitterWidth = _win._canvasBrowserGrid.ColumnDefinitions[1].Width;
				_savedCanvasPanelWidth = _win._canvasBrowserGrid.ColumnDefinitions[2].Width;

				_win._canvasBrowserGrid.ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
				_win._canvasBrowserGrid.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Star);
				_win._canvasBrowserGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
			}
		}
	}

    class DemoModelBrowserCommand : DemoCommand
    {
        private GridLength _savedCanvasPanelWidth = new GridLength(1, GridUnitType.Star);
        private GridLength _savedVerticalSplitterWidth = new GridLength(1, GridUnitType.Auto);
        private GridLength _savedModelBrowserWidth = new GridLength(225, GridUnitType.Pixel);

        public DemoModelBrowserCommand(MainWindow win) : base(win) { }

        public override void Execute(object parameter)
        {
            if (_win.ModelBrowserButton.IsChecked.Value)
            {
                _win._canvasBrowserGrid.ColumnDefinitions[2].Width = _savedCanvasPanelWidth;
                _win._canvasBrowserGrid.ColumnDefinitions[3].Width = _savedVerticalSplitterWidth;
                _win._canvasBrowserGrid.ColumnDefinitions[4].Width = _savedModelBrowserWidth;
            }
            else
            {
                _savedCanvasPanelWidth = _win._canvasBrowserGrid.ColumnDefinitions[2].Width;
                _savedVerticalSplitterWidth = _win._canvasBrowserGrid.ColumnDefinitions[3].Width;
                _savedModelBrowserWidth = _win._canvasBrowserGrid.ColumnDefinitions[4].Width;

                _win._canvasBrowserGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
                _win._canvasBrowserGrid.ColumnDefinitions[3].Width = new GridLength(0, GridUnitType.Star);
                _win._canvasBrowserGrid.ColumnDefinitions[4].Width = new GridLength(0, GridUnitType.Pixel);
            }
        }
    }
}
