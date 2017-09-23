using System;
using System.Collections.Generic;
using System.Transactions;

namespace Novacta.Transactions.IO.Tests.Tools
{
    static class TransactionScopeHelper
    {
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
