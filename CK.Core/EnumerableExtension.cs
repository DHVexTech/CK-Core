﻿#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Core\EnumerableExtension.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CK.Core
{
    /// <summary>
    /// Provides extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Checks whether the enumerable is in strict (no duplicates) ascending order (uses the <see cref="Comparer{T}.Default"/> Compare method).
        /// </summary>
        /// <typeparam name="T">Element type of the enumerable.</typeparam>
        /// <param name="source">This enumerable.</param>
        /// <returns>True if the enumerable is empty or is in strict ascending order.</returns>
        public static bool IsSortedStrict<T>( this IEnumerable<T> source )
        {
            return IsSortedStrict( source, Comparer<T>.Default.Compare );
        }

        /// <summary>
        /// Checks whether the enumerable is in large (duplicates allowed) ascending order (uses the <see cref="Comparer{T}.Default"/> Compare method).
        /// </summary>
        /// <typeparam name="T">Element type of the enumerable.</typeparam>
        /// <param name="source">This enumerable.</param>
        /// <returns>True if the enumerable is empty or is in large ascending order.</returns>
        public static bool IsSortedLarge<T>( this IEnumerable<T> source )
        {
            return IsSortedLarge( source, Comparer<T>.Default.Compare );
        }

        /// <summary>
        /// Checks whether the enumerable is in strict (no duplicates) ascending order based on a comparison function.
        /// </summary>
        /// <typeparam name="T">Element type of the enumerable.</typeparam>
        /// <param name="source">This enumerable.</param>
        /// <param name="comparison">The delegate used to compare elements.</param>
        /// <returns>True if the enumerable is empty or is in strict ascending order.</returns>
        public static bool IsSortedStrict<T>( this IEnumerable<T> source, Comparison<T> comparison )
        {
            if( comparison == null )
                throw new ArgumentNullException( "comparison" );
            using( IEnumerator<T> e = source.GetEnumerator() )
            {
                if( !e.MoveNext() ) return true;
                T prev = e.Current;
                while( e.MoveNext() )
                {
                    T current = e.Current;
                    if( comparison( prev, current ) >= 0 ) return false;
                    prev = current;
                }
            }
            return true;
        }

        /// <summary>
        /// Checks whether the enumerable is in large (duplicates allowed) ascending order based on a comparison function.
        /// </summary>
        /// <typeparam name="T">Element type of the enumerable.</typeparam>
        /// <param name="source">This enumerable.</param>
        /// <param name="comparison">The delegate used to compare elements.</param>
        /// <returns>True if the enumerable is empty or is in large ascending order.</returns>
        public static bool IsSortedLarge<T>( this IEnumerable<T> source, Comparison<T> comparison )
        {
            if( comparison == null )
                throw new ArgumentNullException( "comparison" );
            using( IEnumerator<T> e = source.GetEnumerator() )
            {
                if( !e.MoveNext() ) return true;
                T prev = e.Current;
                while( e.MoveNext() )
                {
                    T current = e.Current;
                    if( comparison( prev, current ) > 0 ) return false;
                    prev = current;
                }
            }
            return true;
        }

    }
}