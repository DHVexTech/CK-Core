#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Context\Plugin\PluginConfig\ConfigManager.cs) is part of CiviKey. 
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
* Copyright © 2007-2010, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
namespace CK.Plugin.Config
{
    /// <summary>
    /// System related configuration. 
    /// This is the first level of configuration that applies to all users.
    /// </summary>
    public interface ISystemConfiguration
    {
        /// <summary>
        /// Gets all the <see cref="IUserProfile">user profiles</see> previously used by the system.
        /// </summary>
        IUserProfileCollection UserProfiles { get; }

        /// <summary>
        /// Gets <see cref="IPluginStatus">plugins status</see> configured at the system level.
        /// </summary>
        IPluginStatusCollection PluginsStatus { get; }

        /// <summary>
        /// Gets the host dictionary for System wide configuration.
        /// </summary>
        IObjectPluginConfig HostConfig { get; }

    }
}