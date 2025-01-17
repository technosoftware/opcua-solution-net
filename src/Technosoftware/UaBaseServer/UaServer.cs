#region Copyright (c) 2022-2024 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2022-2024 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com 
//
// The Software is based on the OPC Foundation MIT License. 
// The complete license agreement for that can be found here:
// http://opcfoundation.org/License/MIT/1.00/
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2022-2024 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Threading.Tasks;

using Opc.Ua;
using Technosoftware.UaConfiguration;
using Technosoftware.UaServer;
#endregion

namespace Technosoftware.UaBaseServer
{
    /// <summary>
    ///     Main class for starting up the UA server
    /// </summary>
    public class UaServer : IDisposable
    {
        #region Constructors, Destructor, Initialization
        /// <summary>
        ///     Initializes the object with default values.
        /// </summary>
        public UaServer()
        {
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Frees any unmanaged resources.
        /// </summary>
        public void Dispose()
        {   
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// An overrideable version of the Dispose.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {  
            if (disposing)
            {
                BaseServer.Dispose();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        ///     The used basic OPC UA Server.
        /// </summary>
        public UaBaseServer BaseServer { get; private set; }
        #endregion

        #region Public Methods
        /// <summary>Starts the OPC UA Server.</summary>
        /// <param name="uaServerPlugin">The UA server plugin to use</param>
        /// <param name="configurationSection">The configuration section to use.</param>
        /// <param name="args">The command line arguments</param>
        public async Task StartAsync(object uaServerPlugin, string configurationSection, string[] args)
        {
            await StartAsync(uaServerPlugin, configurationSection, null, null, args).ConfigureAwait(false);
        }

        /// <summary>Starts the OPC UA Server.</summary>
        /// <param name="uaServerPlugin">The UA server plugin to use</param>
        /// <param name="applicationConfiguration">The Application Configuration to use.</param>
        /// <param name="args">The command line arguments</param>
        public async Task StartAsync(object uaServerPlugin, ApplicationConfiguration applicationConfiguration, string[] args)
        {
            await StartAsync(uaServerPlugin, applicationConfiguration, null, null, args).ConfigureAwait(false);
        }

        /// <summary>Starts the OPC UA Server.</summary>
        /// <param name="uaServerPlugin">The UA server plugin to use</param>
        /// <param name="configurationSection">The configuration section to use.</param>
        /// <param name="passwordProvider">The certificate password provider to use.</param>
        /// <param name="certificateValidationEventHandler">The certificate validation event handler to use.</param>
        /// <param name="args">The command line arguments</param>
        public async Task StartAsync(object uaServerPlugin, string configurationSection, CertificatePasswordProvider passwordProvider, CertificateValidationEventHandler certificateValidationEventHandler, string[] args)
        {
            uaServerPlugin_ = (IUaServerPlugin)uaServerPlugin;
            if (uaServerPlugin_ == null)
            {
                throw new ArgumentNullException(nameof(uaServerPlugin));
            }

            try
            {
                if (uaServerPlugin_.OnStartup(args) != StatusCodes.Good)
                {
                    return;
                }

                var application = new ApplicationInstance { ApplicationType = ApplicationType.Server, CertificatePasswordProvider = passwordProvider};

                var useReverseConnect = false;

                // load the application configuration.
                var config = await application.LoadConfigurationAsync(configurationSection);

                if (uaServerPlugin_.OnApplicationConfigurationLoaded(application, config) != StatusCodes.Good)
                {
                    return;
                }

                uaServerPlugin_.OnGetLicenseInformation(out var serialNumber);
                Technosoftware.UaServer.LicenseHandler.Validate(serialNumber);

                // check the application certificate.
                await application.CheckApplicationInstanceCertificateAsync(
                    false, CertificateFactory.DefaultKeySize, CertificateFactory.DefaultLifeTime).ConfigureAwait(false);

                if (!config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    config.CertificateValidator.CertificateValidation += certificateValidationEventHandler;
                }

                // start the server.
                if (uaServerPlugin is IUaReverseConnectServerPlugin uaReverseConnectServerPlugin)
                {
                    useReverseConnect = uaReverseConnectServerPlugin.OnUseReverseConnect();
                }

                if (uaServerPlugin is IUaOptionalServerPlugin uaServerOptionalPlugin)
                {
                    BaseServer = uaServerOptionalPlugin.OnGetServer();
                    if (BaseServer != null)
                    {
                        BaseServer.UaServerPlugin = uaServerPlugin_;
                        BaseServer.UseReverseConnect = useReverseConnect;
                    }
                }
                if (BaseServer == null)
                {
                    BaseServer = new UaBaseServer(uaServerPlugin_, useReverseConnect);
                }
                BaseServer.Application = application;
                BaseServer.Product = Opc.Ua.LicenseHandler.ProductInformation;

                if (!BaseServer.RestartRequired)
                {
                    await application.StartAsync(BaseServer);

                    uaServerPlugin_.OnRunning();
                }
                else
                {
                    if (BaseServer.RestartRequired)
                    {
                        uaServerPlugin_.OnShutdown(ServerState.Failed, "Evaluation expired! You need to restart the server application.",
                            null);
                    }
                }
            }
            catch (Exception e)
            {
                uaServerPlugin_.OnShutdown(ServerState.Failed, e.Message, e);
            }
        }

        /// <summary>Starts the OPC UA Server.</summary>
        /// <param name="uaServerPlugin">The UA server plugin to use</param>
        /// <param name="applicationConfiguration">The Application Configuration to use.</param>
        /// <param name="passwordProvider">The certificate password provider to use.</param>
        /// <param name="certificateValidationEventHandler">The certificate validation event handler to use.</param>
        /// <param name="args">The command line arguments</param>
        public async Task StartAsync(object uaServerPlugin, ApplicationConfiguration applicationConfiguration, CertificatePasswordProvider passwordProvider, CertificateValidationEventHandler certificateValidationEventHandler, string[] args)
        {
            uaServerPlugin_ = (IUaServerPlugin)uaServerPlugin;
            if (uaServerPlugin_ == null)
            {
                throw new ArgumentNullException(nameof(uaServerPlugin));
            }

            try
            {
                if (uaServerPlugin_.OnStartup(args) != StatusCodes.Good)
                {
                    return;
                }

                var application = new ApplicationInstance { ApplicationType = ApplicationType.Server, CertificatePasswordProvider = passwordProvider };

                var useReverseConnect = false;

                // load the application configuration.
                var config = application.ApplicationConfiguration = applicationConfiguration;

                if (uaServerPlugin_.OnApplicationConfigurationLoaded(application, config) != StatusCodes.Good)
                {
                    return;
                }

                uaServerPlugin_.OnGetLicenseInformation(out var serialNumber);
                Technosoftware.UaServer.LicenseHandler.Validate(serialNumber);

                // check the application certificate.
                await application.CheckApplicationInstanceCertificateAsync(
                    false, CertificateFactory.DefaultKeySize, CertificateFactory.DefaultLifeTime).ConfigureAwait(false);

                if (!config.SecurityConfiguration.AutoAcceptUntrustedCertificates)
                {
                    config.CertificateValidator.CertificateValidation += certificateValidationEventHandler;
                }

                // start the server.
                if (uaServerPlugin is IUaReverseConnectServerPlugin uaReverseConnectServerPlugin)
                {
                    useReverseConnect = uaReverseConnectServerPlugin.OnUseReverseConnect();
                }

                if (uaServerPlugin is IUaOptionalServerPlugin uaServerOptionalPlugin)
                {
                    BaseServer = uaServerOptionalPlugin.OnGetServer();
                    if (BaseServer != null)
                    {
                        BaseServer.UaServerPlugin = uaServerPlugin_;
                        BaseServer.UseReverseConnect = useReverseConnect;
                    }
                }
                if (BaseServer == null)
                {
                    BaseServer = new UaBaseServer(uaServerPlugin_, useReverseConnect);
                }
                BaseServer.Application = application;
                BaseServer.Product = Opc.Ua.LicenseHandler.ProductInformation;

                if (!BaseServer.RestartRequired)
                {
                    await application.StartAsync(BaseServer);

                    uaServerPlugin_.OnRunning();
                }
                else
                {
                    if (BaseServer.RestartRequired)
                    {
                        uaServerPlugin_.OnShutdown(ServerState.Failed, "Evaluation expired! You need to restart the server application.", null);
                    }
                }
            }
            catch (Exception e)
            {
                uaServerPlugin_.OnShutdown(ServerState.Failed, e.Message, e);
            }
        }

        /// <summary>
        ///     Stops the UA server.
        /// </summary>
        public void Stop()
        {
            BaseServer.Stop();
        }
        #endregion

        #region Field
        private static IUaServerPlugin uaServerPlugin_;
        #endregion

    }
}
