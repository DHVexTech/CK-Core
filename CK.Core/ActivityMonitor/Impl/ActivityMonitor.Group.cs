#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\ActivityMonitor\Impl\ActivityMonitor.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CK.Core.Impl;

namespace CK.Core
{
    /// <summary>
    /// Concrete implementation of <see cref="IActivityMonitor"/>.
    /// </summary>
    public partial class ActivityMonitor
    {
        /// <summary>
        /// Groups are bound to an <see cref="ActivityMonitor"/> and are linked together from 
        /// the current one to the very first one (a kind of stack).
        /// </summary>
        protected class Group : IActivityLogGroup, IDisposable
        {
            readonly ActivityMonitor _monitor;
            readonly int _index;
            string _text;
            CKTrait _tags;
            DateTime _logTime;
            DateTime _closeLogTime;
            Exception _exception;
            CKExceptionData _exceptionData;
            Func<string> _getConclusion;

            /// <summary>
            /// Initialized a new Group at a given index.
            /// </summary>
            /// <param name="monitor">Monitor.</param>
            /// <param name="index">Index of the group.</param>
            internal protected Group( ActivityMonitor monitor, int index )
            {
                _monitor = monitor;
                _index = index;
            }

            /// <summary>
            /// Initializes or reinitializes this group (if it has been disposed). 
            /// </summary>
            /// <param name="tags">Tags for the group.</param>
            /// <param name="level">The <see cref="GroupLevel"/>.</param>
            /// <param name="text">The <see cref="GroupText"/>.</param>
            /// <param name="getConclusionText">
            /// Optional delegate to call on close to obtain a conclusion text if no 
            /// explicit conclusion is provided through <see cref="IActivityMonitor.CloseGroup"/>.
            /// </param>
            /// <param name="logTimeUtc">Timestamp of the log (must be UTC).</param>
            /// <param name="ex">Optional exception associated to the group.</param>
            internal protected virtual void Initialize( CKTrait tags, LogLevel level, string text, Func<string> getConclusionText, DateTime logTimeUtc, Exception ex )
            {
                SavedMonitorFilter = _monitor.Filter;
                SavedMonitorTags = _monitor.AutoTags;
                // Logs everything when a Group is an error: we then have full details without
                // logging all with Error or Fatal.
                if( level >= LogLevel.Error && _monitor._configuredFilter != LogLevelFilter.Trace ) _monitor.DoSetConfiguredFilter( LogLevelFilter.Trace );
                GroupLevel = level;
                _logTime = logTimeUtc;
                _closeLogTime = DateTime.MinValue;
                _text = text ?? String.Empty;
                _tags = tags ?? ActivityMonitor.EmptyTag;
                _getConclusion = getConclusionText;
                _exception = ex;
            }

            /// <summary>
            /// Gets the tags for the log group.
            /// </summary>
            public CKTrait GroupTags { get { return _tags; } }

            /// <summary>
            /// Gets the log time for the log.
            /// </summary>
            public DateTime LogTimeUtc { get { return _logTime; } }

            /// <summary>
            /// Gets the log time of the group closing.
            /// It is <see cref="DateTime.MinValue"/> when the group is not closed yet.
            /// </summary>
            public DateTime CloseLogTimeUtc 
            { 
                get { return _closeLogTime; } 
                internal set { _closeLogTime = value; } 
            }

            /// <summary>
            /// Gets the <see cref="CKExceptionData"/> that captures exception information 
            /// if it exists. Returns null if no <see cref="P:Exception"/> exists.
            /// </summary>
            public CKExceptionData ExceptionData
            {
                get
                {
                    if( _exceptionData == null && _exception != null )
                    {
                        CKException ckEx = _exception as CKException;
                        if( ckEx != null )
                        {
                            _exceptionData = ckEx.ExceptionData;
                        }
                    }
                    return _exceptionData;
                }
            }

            /// <summary>
            /// Gets or creates the <see cref="CKExceptionData"/> that captures exception information.
            /// If <see cref="P:Exception"/> is null, this returns null.
            /// </summary>
            /// <returns></returns>
            public CKExceptionData EnsureExceptionData()
            {
                return _exceptionData ?? (_exceptionData = CKExceptionData.CreateFrom( _exception ));
            }

            /// <summary>
            /// Get the previous group in its origin monitor. Null if this is a top level group.
            /// </summary>
            public IActivityLogGroup Parent { get { return _index > 0 ? _monitor._groups[_index - 1] : null; } }
            
            /// <summary>
            /// Gets the depth of this group in its origin monitor (1 for top level groups).
            /// </summary>
            public int Depth { get { return _index+1; } }

            /// <summary>
            /// Gets the level of this group.
            /// </summary>
            public LogLevel GroupLevel { get; private set; }
            
            /// <summary>
            /// Gets the text with which this group has been opened. Null if and only if the group is closed.
            /// </summary>
            public string GroupText { get { return _text; } }

            /// <summary>
            /// Gets the associated <see cref="Exception"/> if it exists.
            /// </summary>
            public Exception Exception { get { return _exception; } }

            /// <summary>
            /// Gets or sets the <see cref="IActivityMonitor.Filter"/> that will be restored when group will be closed.
            /// Initialized with the current value of IActivityMonitor.Filter when the group has been opened.
            /// </summary>
            public LogLevelFilter SavedMonitorFilter { get; protected set; }

            /// <summary>
            /// Gets or sets the <see cref="IActivityMonitor.AutoTags"/> that will be restored when group will be closed.
            /// Initialized with the current value of IActivityMonitor.Tags when the group has been opened.
            /// </summary>
            public CKTrait SavedMonitorTags { get; protected set; }

            /// <summary>
            /// Gets whether the <see cref="GroupText"/> is actually the <see cref="Exception"/> message.
            /// </summary>
            public bool IsGroupTextTheExceptionMessage 
            {
                get { return _exception != null && ReferenceEquals( _exception.Message, GroupText ); } 
            }

            /// <summary>
            /// Gets or sets an optional function that will be called on group closing. 
            /// </summary>
            protected Func<string> GetConclusionText 
            { 
                get { return _getConclusion; } 
                set { _getConclusion = value; } 
            }
      
            /// <summary>
            /// Ensures that any groups opened after this one are closed before closing this one.
            /// </summary>
            void IDisposable.Dispose()
            {
                if( _text != null )
                {
                    while( _monitor._current != this ) ((IDisposable)_monitor._current).Dispose();
                    _monitor.CloseGroup( DateTime.UtcNow, null );
                }
            }           

            internal void GroupClosing( ref List<ActivityLogGroupConclusion> conclusions )
            {
                string auto = ConsumeConclusionText();
                if( auto != null )
                {
                    if( conclusions == null ) conclusions = new List<ActivityLogGroupConclusion>();
                    conclusions.Add( new ActivityLogGroupConclusion( TagGetTextConclusion, auto ) );
                }
                Debug.Assert( _getConclusion == null, "Has been consumed." );
            }

            internal void GroupClosed()
            {
                _text = null;
                _exception = null;
            }

            /// <summary>
            /// Calls <see cref="GetConclusionText"/> and sets it to null.
            /// </summary>
            string ConsumeConclusionText()
            {
                string autoText = null;
                if( _getConclusion != null )
                {
                    try
                    {
                        autoText = _getConclusion();
                    }
                    catch( Exception ex )
                    {
                        autoText = String.Format( R.ActivityMonitorErrorWhileGetConclusionText, ex.Message );
                    }
                    _getConclusion = null;
                }
                return autoText;
            }
        }

        IActivityLogGroup IActivityMonitorImpl.CurrentGroup
        {
            get { return _current; }
        }

        /// <summary>
        /// Factory method for <see cref="Group"/> (or any specialized class).
        /// This is may be overriden in advanced scenario where groups may offer different 
        /// behaviors than the default ones.
        /// </summary>
        /// <param name="index">The index (zero based depth) of the group.</param>
        /// <returns>A new group.</returns>
        protected virtual Group CreateGroup( int index )
        {
            return new Group( this, index );
        }

    }
}
