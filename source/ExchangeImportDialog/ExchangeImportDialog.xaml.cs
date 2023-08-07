﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using HPS;
using System.Windows.Threading;
using System.Threading;
using wpf_sandbox;

namespace hps_wpf_sandbox.ExchangeImportDialog
{
    public class ImportStatusEventHandler : HPS.EventHandler
    {
        public ImportStatusEventHandler() : base()
	    {
            _progress_dlg = null;
        }

	    public ImportStatusEventHandler(ExchangeImportDialog in_progress_dlg) : base()
	    {
            _progress_dlg = in_progress_dlg;
        }

	    ~ImportStatusEventHandler() { Shutdown(); }

        public override HandleResult Handle(HPS.Event in_event)
        {
            if (_progress_dlg != null)
            {
                var statusMessage = new HPS.ImportStatusEvent(in_event).import_status_message;
                if (!string.IsNullOrEmpty(statusMessage))
                {
                    bool update_message = true;

			        if (statusMessage == "Import and Tessellation")
				        _progress_dlg.SetMessage("Stage 1/3 : Import and Tessellation");
			        else if (statusMessage == "Creating Graphics Database")
				        _progress_dlg.SetMessage("Stage 2/3 : Creating Graphics Database");
			        else if (System.IO.Path.IsPathRooted(statusMessage))
			        {
                        statusMessage = System.IO.Path.GetFileName(statusMessage);
				        _progress_dlg.AddLogEntry(statusMessage);
				        update_message = false;
			        }
			        else
				        update_message = false;

                    if (update_message)
                        _progress_dlg.UpdateMessage();
                }
            }
            return HPS.EventHandler.HandleResult.Handled;
        }

        private ExchangeImportDialog _progress_dlg;
    };

    public partial class ExchangeImportDialog : System.Windows.Window
    {
        private HPS.IONotifier              notifier;
        private MainWindow                  _win;
        private bool                        keep_dialog_open;
        private bool                        success;
        private string                      message;
        private ImportStatusEventHandler    import_event_handler;
        private Queue<string>               import_log;
        private DispatcherTimer             timer;
        private bool                        please_update_message;
        private readonly object             sync_object = new object();

        public ExchangeImportDialog(MainWindow in_main_window, HPS.IONotifier in_notifier)
        {
            InitializeComponent();

            _win = in_main_window;
            notifier = in_notifier;
            import_log = new Queue<string>();
            please_update_message = false;
            success = false;
            keep_dialog_open = false;

            import_event_handler = new ImportStatusEventHandler(this);
            HPS.Database.GetEventDispatcher().Subscribe(import_event_handler, HPS.Object.ClassID<HPS.ImportStatusEvent>());

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += new System.EventHandler(dispatcherTimer_Tick);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 50);   //fire every 50 milli-seconds
            timer.Start();

            m_import_message.Content = "Stage 1/3 : Import and Tessellation";
        }

        private void OnKeepOpenChecked(object sender, RoutedEventArgs e)
        {
            if (m_keep_open_check.IsChecked == true)
                keep_dialog_open = true;
            else
                keep_dialog_open = false;
        }

        private void OnCancelClicked(object sender, RoutedEventArgs e)
        {
            if (success)
                Close();
            else 
            {
                notifier.Cancel();
                timer.Stop();
                Close();
            }
        }

        public void SetMessage(string in_message) 
        { 
            message = in_message; 
        }

        public void AddLogEntry(string in_log_entry)
        {
            lock(sync_object)
            {
                import_log.Enqueue(in_log_entry);                
            }
        }

        public void UpdateMessage()
        {
            please_update_message = true;
        }

        public bool WasSuccessful() 
        { 
            return success; 
        }

        private void PerformInitialUpdate()
        {
#if USING_EXCHANGE
	        m_cancel_button.IsEnabled = false;
            m_import_message.Content = "Stage 3/3 : Performing Initial Update";

	        HPS.CADModel cadModel = (notifier as HPS.Exchange.ImportNotifier).GetCADModel();

	        if (!cadModel.Empty())
	        {
			    _win.CADModel = cadModel;
			    _win.ModelBrowser.Init();
		        cadModel.GetModel().GetSegmentKey().GetPerformanceControl().SetStaticModel(HPS.Performance.StaticModel.Attribute);
                _win.AttachViewWithSmoothTransition(cadModel.ActivateDefaultCapture().FitWorld(),true);
	        }

	        HPS.UpdateNotifier updateNotifier = _win.GetSprocketsControl().Canvas.UpdateWithNotifier(HPS.Window.UpdateType.Exhaustive);
	        updateNotifier.Wait();

	        m_cancel_button.IsEnabled = true;
            m_cancel_button.Content = "Close Dialog";
            m_progress_bar.IsIndeterminate = false;
            m_progress_bar.Value = 100;
            m_import_message.Content = "Import Complete";
	        success = true;
#endif
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try 
		    {
                if (please_update_message && !string.IsNullOrEmpty(message))
                {
                    m_import_message.Content = message;
                    please_update_message = false;
                }

			    //update the import log
			    lock (sync_object)
                {
                    if (import_log.Count() > 0)
			        {
				        foreach (string one_message in import_log)
				        {
                            if (m_import_log.Text != "")
                                m_import_log.AppendText("\n");
					        m_import_log.AppendText("Reading " + one_message);
				        }
				        import_log.Clear();
                        m_import_log.ScrollToEnd();
			        }                    
                }

			    HPS.IOResult status;
			    status = notifier.Status();

			    if (status != HPS.IOResult.InProgress)
			    {
				    HPS.Database.GetEventDispatcher().UnSubscribe(import_event_handler);
                    timer.Stop();

				    if (status == HPS.IOResult.Success)
					    PerformInitialUpdate();

				    if (!keep_dialog_open)
					    Close();
			    }

		    }
		    catch (HPS.IOException)
		    {
			    // notifier not yet created
		    }
        }
    }
}
