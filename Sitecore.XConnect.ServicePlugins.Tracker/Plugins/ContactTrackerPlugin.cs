using System;
using Serilog;
using Sitecore.Framework.Messaging;
using Sitecore.XConnect;
using Sitecore.XConnect.Operations;
using Sitecore.XConnect.Service.Plugins;

namespace Sitecore.XConnect.ServicePlugins.InteractionsTracker.Plugins
{
    public class ContactTrackerPlugin : IXConnectServicePlugin, IDisposable
    {
        private XdbContextConfiguration _config;

        public ContactTrackerPlugin()
        {
            Log.Information("Create {0}", nameof(ContactTrackerPlugin));
        }

        /// <summary>Subscribes to events the current plugin listens to.</summary>
        /// <param name="config">
        ///   A <see cref="T:Sitecore.XConnect.XdbContextConfiguration" /> object that provides access to the configuration settings.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   Argument <paramref name="config" /> is a <b>null</b> reference.
        /// </exception>
        public void Register(XdbContextConfiguration config)
        {
            Log.Information("Register {0}", nameof(ContactTrackerPlugin));
            _config = config;
            RegisterEvents();
        }

        /// <summary>
        ///   Unsubscribes from events the current plugin listens to.
        /// </summary>
        public void Unregister()
        {
            Log.Information("Unregister {0}", nameof(ContactTrackerPlugin));
            UnregisterEvents();
        }

        private void RegisterEvents()
        {
            //Subscribe OperationCompleted event
            _config.OperationCompleted += OnOperationCompleted;
        }

        private void UnregisterEvents()
        {
            //Unsubscribe OperationCompleted event
            _config.OperationCompleted -= OnOperationCompleted;
        }

        /// <summary>
        /// Handles the event that is generated when an operation completes.
        /// </summary>
        /// <param name="sender">The <see cref="T:System.Object" /> that generated the event.</param>
        /// <param name="xdbOperationEventArgs">A <see cref="T:Sitecore.XConnect.Operations.XdbOperationEventArgs" /> object that provides information about the event.</param>
        private void OnOperationCompleted(object sender, XdbOperationEventArgs xdbOperationEventArgs)
        {
            //Check if no exceptions are occurred during executing the operation. If it is, it will not guarantee that contact was created.
            if (xdbOperationEventArgs.Operation.Exception != null)
                return;

            //We need to track only the AddContactOperation operation. Trying to cast to a necessary type.
            var operation = xdbOperationEventArgs.Operation as AddContactOperation;

            //Checking if it is the necessary operation and if an operation execution status is "Succeeded"
            if ((operation?.Status == XdbOperationStatus.Succeeded && operation.Entity.Id.HasValue)
                || (operation?.Status == XdbOperationStatus.Succeeded))
            {
                Log.Information("Processing add contact creation interaction with contact id = {0}", operation.Entity.Id.Value);
                //Sending a message with an id of newly created contact. 
                DataExportService.SendContactInteraction(Convert.ToString(operation.Entity.Id.Value));
            }
        }

        /// <summary>
        ///   Releases managed and unmanaged resources used by the current class instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   Releases managed and unmanaged resources used by the current class instance.
        /// </summary>
        /// <param name="disposing">
        ///   Indicates whether the current method was called from explicitly or implicitly during finalization.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
            Log.Information("Dispose {0}", nameof(ContactTrackerPlugin));
            _config = null;
        }
    }
}