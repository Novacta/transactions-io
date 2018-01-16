// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Transactions;

namespace Novacta.Transactions.IO.Tests.Tools
{
    /// <summary>
    /// Provides helper methods while using <see cref="TransactionScope"/> 
    /// instances.
    /// </summary>
    static class TransactionScopeHelper
    {
        /// <summary>
        /// Defines a transactional code section in which the given  
        /// managers are enlisted. The section is in turn executed 
        /// inside a <b>try/catch</b> block, and the specified code 
        /// is executed inside the <b>finally</b> and 
        /// <b>catch</b> clauses.
        /// </summary>
        /// <param name="managers">The managers to be enlisted.</param>
        /// <param name="results">A method executed in the <b>finally</b> clause.</param>
        /// <param name="rolledBack">A method executed in the <b>catch</b> clause.</param>
        public static void Using(
            IEnumerable<FileManager> managers,
            Action results,
            Action<Exception> rolledBack)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    foreach (var manager in managers)
                    {
                        manager.EnlistVolatile(EnlistmentOptions.None);
                    }

                    scope.Complete();
                }
            }
            catch (Exception e)
            {
                rolledBack(e);
            }
            finally
            {
                results();
            }
        }
    }
}
