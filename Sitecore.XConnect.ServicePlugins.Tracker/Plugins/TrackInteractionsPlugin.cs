using System;
using Serilog;
using System.Linq;
using Sitecore.Framework.Messaging;
using Sitecore.XConnect.Operations;
using Sitecore.XConnect.Service.Plugins;
using System.Threading.Tasks;

namespace Sitecore.XConnect.ServicePlugins.InteractionsTracker.Plugins
{
    public class TrackInteractionsPlugin : IXConnectServicePlugin, IDisposable
    {
        private XdbContextConfiguration _config;
        //private readonly IMessageBus<CreateDynamicsContactMessageBus> _messageBus;

        public TrackInteractionsPlugin()
        {
            Log.Information("Create {0}", nameof(TrackInteractionsPlugin));
        }

        /// <summary>Subscribes to events current plugin listens to.</summary>
        /// <param name="config">
        ///   A <see cref="T:Sitecore.XConnect.XdbContextConfiguration" /> object that provides access to the configuration settings.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        ///   Argument <paramref name="config" /> is a <b>null</b> reference.
        /// </exception>
        public void Register(XdbContextConfiguration config)
        {
            Log.Information("Register {0}", nameof(TrackInteractionsPlugin));
            _config = config;
            RegisterEvents();
        }

        /// <summary>
        ///   Unsubscribes from events the current plugin listens to.
        /// </summary>
        public void Unregister()
        {
            Log.Information("Unregister {0}", nameof(TrackInteractionsPlugin));
            UnregisterEvents();
        }

        private void RegisterEvents()
        {
            //Subscribe to OperationCompleted event
            _config.OperationCompleted += OnOperationCompleted;
        }

        private void UnregisterEvents()
        {
            //Unsubscribe from OperationCompleted event
            _config.OperationCompleted -= OnOperationCompleted;
        }

        /// <summary>
        /// Handles the event that is generated when an operation completes.
        /// </summary>
        /// <param name="sender">The <see cref="T:System.Object" /> that generated the event.</param>
        /// <param name="xdbOperationEventArgs">A <see cref="T:Sitecore.XConnect.Operations.XdbOperationEventArgs" /> object that provides information about the event.</param>
        private void OnOperationCompleted(object sender, XdbOperationEventArgs xdbOperationEventArgs)
        {
            //Check if no exceptions occurred during executing of the operation. 
            if (xdbOperationEventArgs.Operation.Exception != null)
                return;

            //We need to track only the AddInteractionOperation operation. Trying to cast to a necessary type.
            var operation = xdbOperationEventArgs.Operation as AddInteractionOperation;

            if (operation?.Status == XdbOperationStatus.Succeeded && operation.Entity.Id.HasValue)
            {
                Log.Information("Processing add interaction operation for interaction id = {0} and contact id = {1}", operation.Entity.Events[0].Id, operation.Entity.Contact.Id.Value);

                DataExportService.SendContactInteraction(operation.Entity);
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
            Log.Information("Dispose {0}", nameof(TrackInteractionsPlugin));
            _config = null;
        }
    }
}