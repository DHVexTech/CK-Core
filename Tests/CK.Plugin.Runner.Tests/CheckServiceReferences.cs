﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CK.Core;
using CK.Context;
using CK.Plugin.Hosting;
using System.Reflection;

namespace CK.Plugin.Runner.Apply
{
    [TestFixture]
    public class CheckServiceReferences : TestBase
    {
        IContext _ctx;
        ISimplePluginRunner _runner;
        PluginRunner _implRunner;
        Guid _implService = new Guid( "{C24EE3EA-F078-4974-A346-B34208221B35}" );

        [SetUp]
        public void Setup()
        {
            _ctx = CK.Context.Context.CreateInstance();
            _runner = _ctx.PluginRunner;
            _implRunner = (PluginRunner)_ctx.PluginRunner;

            Assert.NotNull( _runner );
            Assert.NotNull( _implRunner );
        }

        [SetUp]
        [TearDown]
        public void Teardown()
        {
            TestBase.CleanupTestDir();
        }

        void CheckStartStop( Action beforeStart, Action afterStart, Action beforeStop, Action afterStop, bool startSucceed, bool stopSucceed, params Guid[] idToStart )
        {
            // Set a new user action --> start plugins
            for( int i = 0; i < idToStart.Length; i++ )
                _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStart[i], Config.ConfigUserAction.Started );

            if( beforeStart != null ) beforeStart();

            // So apply the change
            Assert.That( _runner.Apply() == startSucceed );

            if( afterStart != null ) afterStart();

            // Set a new user action --> stop the plugin
            for( int i = 0; i < idToStart.Length; i++ )
                _ctx.ConfigManager.UserConfiguration.LiveUserConfiguration.SetAction( idToStart[i], Config.ConfigUserAction.Stopped );

            if( beforeStop != null ) beforeStop();

            // So apply the change
            Assert.IsTrue( _runner.Apply() == stopSucceed );

            if( afterStop != null ) afterStop();
        }

        void CheckStartStop( Action beforeStart, Action afterStart, Action beforeStop, Action afterStop, params Guid[] idToStart )
        {
            CheckStartStop( beforeStart, afterStart, beforeStop, afterStop, true, true, idToStart );
        }

        #region Check all types of service references with fully implemented service.

        [Test]
        /// <summary>
        /// A plugin needs (MustExistAndRun) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExistAndRun()
        {
            Guid id = new Guid( "{4E69383E-044D-4786-9077-5F8E5B259793}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin that implements the service is still running, we don't care about machine resources yet.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };

            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistAndRun) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExistAndRun()
        {
            Guid id = new Guid( "{457E357D-102D-447D-89B8-DA9C849910C8}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin that implements the service is still running, we don't care about machine resources yet.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };

            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExistTryStart()
        {
            #region Init
            
            Guid id = new Guid( "{58C00B79-D882-4C11-BD90-1F25AD664C67}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin is available ... so we tried to start it.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin that implements the service is still running, we don't care about machine resources yet.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExistTryStart()
        {
            #region Init
            Guid id = new Guid( "{9BBCFE92-7465-4B3B-88D0-3CEF1E2E5580}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true ); 
            #endregion

            #region Asserts
            Action afterStart = () =>
                {
                    // Check if the plugin is started, and if the plugin that implement the required service is started too.
                    Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                    // The plugin is available ... so we tried to start it.
                    Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
                };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin that implements the service is still running, we don't care about machine resources yet.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            }; 
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExist()
        {
            #region Init

            Guid id = new Guid( "{317B5D34-BA84-4A15-92F4-4E791E737EF0}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin is available but we don't need it started.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                // The plugin that implements the service was not running.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExist()
        {
            #region Init
            // PluginNeedsIService_ME
            Guid id = new Guid( "{973B4050-280F-43B0-A9E3-0C4DC9BC2C5F}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_OptionalTryStart()
        {
            #region Init

            Guid id = new Guid( "{ABD53A18-4549-49B8-82C0-9977200F47E9}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_OptionalTryStart()
        {
            #region Init
            Guid id = new Guid( "{CDCE6413-038D-4020-A3E0-51FA755C5E72}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (Optional) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_Optional()
        {
            #region Init

            Guid id = new Guid( "{C78FCB4F-6925-4587-AC98-DA0AE1A977D1}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (Optional) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_Optional()
        {
            #region Init
            Guid id = new Guid( "{FF896081-A15D-4A5C-8030-13544EF09673}" );

            TestBase.CopyPluginToTestDir( "ServiceC.dll" );
            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( _implService ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        #endregion

        #region Check all types of service references with not implemented service.

        [Test]
        /// <summary>
        /// A plugin needs (MustExistAndRun) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExistAndRun_Error()
        {
            Guid id = new Guid( "{4E69383E-044D-4786-9077-5F8E5B259793}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };

            CheckStartStop( null, afterStart, null, afterStop, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistAndRun) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExistAndRun_Error()
        {
            Guid id = new Guid( "{457E357D-102D-447D-89B8-DA9C849910C8}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };

            CheckStartStop( null, afterStart, null, afterStop, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExistTryStart_Error()
        {
            #region Init

            Guid id = new Guid( "{58C00B79-D882-4C11-BD90-1F25AD664C67}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExistTryStart_Error()
        {
            #region Init
            Guid id = new Guid( "{9BBCFE92-7465-4B3B-88D0-3CEF1E2E5580}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_MustExist_Error()
        {
            #region Init

            Guid id = new Guid( "{317B5D34-BA84-4A15-92F4-4E791E737EF0}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_MustExist_Error()
        {
            #region Init
            // PluginNeedsIService_ME
            Guid id = new Guid( "{973B4050-280F-43B0-A9E3-0C4DC9BC2C5F}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action after = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.That( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ), Is.False );
            };
            #endregion

            //Run!
            CheckStartStop( null, after, null, after, false, true, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_OptionalTryStart_Error()
        {
            #region Init

            Guid id = new Guid( "{ABD53A18-4549-49B8-82C0-9977200F47E9}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (MustExistTryStart) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_OptionalTryStart_Error()
        {
            #region Init
            Guid id = new Guid( "{CDCE6413-038D-4020-A3E0-51FA755C5E72}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (Optional) a service implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_Normal_Optional_Error()
        {
            #region Init

            Guid id = new Guid( "{C78FCB4F-6925-4587-AC98-DA0AE1A977D1}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );

            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        [Test]
        /// <summary>
        /// A plugin needs (Optional) a IService{service} implemented by an other plugin.
        /// Check if the plugin that implement the service is auto started to fill the service reference.
        /// </summary>
        public void ServiceReference_IService_Optional_Error()
        {
            #region Init
            Guid id = new Guid( "{FF896081-A15D-4A5C-8030-13544EF09673}" );

            TestBase.CopyPluginToTestDir( "ServiceC.Model.dll" );
            TestBase.CopyPluginToTestDir( "PluginNeedsServiceC.dll" );

            _implRunner.Discoverer.Discover( TestBase.TestFolderDir, true );
            #endregion

            #region Asserts
            Action afterStart = () =>
            {
                // Check if the plugin is started, and if the plugin that implement the required service is started too.
                Assert.IsTrue( _implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            Action afterStop = () =>
            {
                // Check if the plugin is stopped.
                Assert.IsTrue( !_implRunner.IsPluginRunning( _implRunner.Discoverer.FindPlugin( id ) ) );
            };
            #endregion

            //Run!
            CheckStartStop( null, afterStart, null, afterStop, id );
        }

        #endregion
    }
}