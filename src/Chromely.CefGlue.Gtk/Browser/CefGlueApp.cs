﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CefGlueApp.cs" company="Chromely Projects">
//   Copyright (c) 2017-2018 Chromely Projects
// </copyright>
// <license>
// See the LICENSE.md file in the project root for more information.
// </license>
// --------------------------------------------------------------------------------------------------------------------

namespace Chromely.CefGlue.Gtk.Browser
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Chromely.CefGlue.Gtk.Browser.Handlers;
    using Chromely.Core;
    using Chromely.Core.Infrastructure;
    using Xilium.CefGlue;

    /// <summary>
    /// The CefGlue app.
    /// </summary>
    public class CefGlueApp : CefApp
    {
        /// <summary>
        /// The render process handler.
        /// </summary>
        private readonly CefRenderProcessHandler mRenderProcessHandler = new CefGlueRenderProcessHandler();

        /// <summary>
        /// Initializes a new instance of the <see cref="CefGlueApp"/> class.
        /// </summary>
        /// <param name="hostConfig">
        /// The host config.
        /// </param>
        public CefGlueApp(ChromelyConfiguration hostConfig)
        {
            HostConfig = hostConfig;
        }

        /// <summary>
        /// Gets or sets the host config.
        /// </summary>
        public ChromelyConfiguration HostConfig { get; set; }

        /// <summary>
        /// The on register custom schemes.
        /// </summary>
        /// <param name="registrar">
        /// The registrar.
        /// </param>
        protected override void OnRegisterCustomSchemes(CefSchemeRegistrar registrar)
        {
            var schemeHandlerObjs = IoC.GetAllInstances(typeof(ChromelySchemeHandler));
            if (schemeHandlerObjs != null)
            {
                var schemeHandlers = schemeHandlerObjs.ToList();

                foreach (var item in schemeHandlers)
                {
                    if (item is ChromelySchemeHandler)
                    {
                        ChromelySchemeHandler handler = (ChromelySchemeHandler)item;
                        if (handler.HandlerFactory is CefSchemeHandlerFactory)
                        {
                            bool isStandardScheme = UrlScheme.IsStandardScheme(handler.SchemeName);
                            if (!isStandardScheme)
                            {
                                registrar.AddCustomScheme(handler.SchemeName, true, false, false, false, true, true);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The on before command line processing.
        /// </summary>
        /// <param name="processType">
        /// The process type.
        /// </param>
        /// <param name="commandLine">
        /// The command line.
        /// </param>
        protected override void OnBeforeCommandLineProcessing(string processType, CefCommandLine commandLine)
        {
            // Get all custom command line argument switches
            if ((HostConfig != null) && (HostConfig.CommandLineArgs != null))
            {
                foreach (var commandArg in HostConfig.CommandLineArgs)
                {
                    commandLine.AppendSwitch(commandArg.Key, commandArg.Value);
                }
            }

            // Currently on linux platform location of locales and pack files are determined
            // incorrectly (relative to main module instead of libcef.so module).
            // Once issue http://code.google.com/p/chromiumembedded/issues/detail?id=668 will be resolved
            // this code can be removed.
            if (CefRuntime.Platform == CefRuntimePlatform.Linux)
            {
                var path = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);

                commandLine.AppendSwitch("resources-dir-path", path);
                commandLine.AppendSwitch("locales-dir-path", Path.Combine(path, "locales"));
            }
        }

        /// <summary>
        /// The get render process handler.
        /// </summary>
        /// <returns>
        /// The <see cref="CefRenderProcessHandler"/>.
        /// </returns>
        protected override CefRenderProcessHandler GetRenderProcessHandler()
        {
            return mRenderProcessHandler;
        }
    }
}
