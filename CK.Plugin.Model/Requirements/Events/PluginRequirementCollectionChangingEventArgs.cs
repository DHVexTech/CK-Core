﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Plugin;
using CK.Core;
using System.ComponentModel;

namespace CK.Plugin
{
    /// <summary>
    /// Describes a change that is about to occur in a <see cref="IPluginRequirementCollection"/> and can be <see cref="CancelEventArgs.Cancel"/>ed.
    /// </summary>
    public class PluginRequirementCollectionChangingEventArgs : CancelEventArgs
    {
        /// <summary>
        /// The <see cref="ChangeStatus"/> that synthetizes the change.
        /// </summary>
        public ChangeStatus Action { get; private set; }

        /// <summary>
        /// The source <see cref="IPluginRequirementCollection"/> that is changing.
        /// </summary>
        public IPluginRequirementCollection Collection { get; private set; }

        /// <summary>
        /// The plugin identifier for which a change is occurring. 
        /// It is <see cref="Guid.Empty"/> if the change is a global change (<see cref="IPluginRequirementCollection.Clear"/> is beeing called for instance).
        /// </summary>
        public Guid PluginId { get; private set; }

        /// <summary>
        /// The <see cref="RunningRequirement"/> that is changing.
        /// It is <see cref="RunningRequirement.Optional"/> if the change is a global change (<see cref="IPluginRequirementCollection.Clear"/> is beeing called for instance).
        /// </summary>
        public RunningRequirement Requirement { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="PluginRequirementCollectionChangingEventArgs"/>.
        /// </summary>
        /// <param name="c">The collection that is changing.</param>
        /// <param name="action">The <see cref="ChangeStatus"/>.</param>
        /// <param name="pluginId">The plugin identifier concerned.</param>
        /// <param name="requirement">The <see cref="RunningRequirement"/> of the changed <paramref name="pluginId"/>.</param>
        public PluginRequirementCollectionChangingEventArgs( IPluginRequirementCollection c, ChangeStatus action, Guid pluginId, RunningRequirement requirement )
        {
            Collection = c;
            Action = action;
            PluginId = pluginId;
            Requirement = requirement;
        }
    }

}