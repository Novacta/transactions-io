using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacta.Transactions.IO.Tests.Tools
{
    /// <summary>
    /// Verifies conditions about expected exceptions in 
    /// unit tests using true/false propositions.
    /// </summary>
    public static class ExceptionAssert
    {
        /// <summary>
        /// Determines whether the specified target throws an 
        /// expected exception.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="expectedMessage">The expected message.</param>
        public static void IsThrown(
            Action target,
            Type expectedType,
            string expectedMessage)
        {
            bool isThrown = false;
            string actualMessage = null;
            Type actualType = null;
            try {
                target();
            }
            catch (Exception e) {
                isThrown = true;
                actualType = e.GetType();
                actualMessage = e.Message;
            }

            Assert.IsTrue(isThrown,
                "An expected exception has not been thrown.");
            Assert.AreEqual(expectedMessage, actualMessage);
            Assert.AreEqual(expectedType, actualType);
        }


        /// <summary>
        /// Determines whether the specified target throws an 
        /// expected exception caused by a given inner exception.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <param name="expectedType">The expected type.</param>
        /// <param name="expectedMessage">The expected message.</param>
        /// <param name="expectedInnerType">The expected inner type.</param>
        /// <param name="expectedInnerMessage">The expected inner message.</param>
        public static void IsThrown(
            Action target,
            Type expectedType,
            string expectedMessage,
            Type expectedInnerType,
            string expectedInnerMessage)
        {
            bool isThrown = false;
            bool isInnerThrown = false;
            string actualMessage = null;
            Type actualType = null;
            Type actualInnerType = null;
            string actualInnerMessage = null;
            try {
                target();
            }
            catch (Exception e) {
                isThrown = true;
                if (e.InnerException != null) {
                    isInnerThrown = true;
                    actualInnerMessage = e.InnerException.Message;
                    actualInnerType = e.InnerException.GetType();
                }
                actualType = e.GetType();
                actualMessage = e.Message;
            }

            Assert.IsTrue(isThrown,
                "An expected exception has not been thrown.");
            Assert.IsTrue(isInnerThrown,
                "An expected exception has not been caused by the expected inner one.");
            Assert.AreEqual(expectedMessage, actualMessage);
            Assert.AreEqual(expectedType, actualType);
            Assert.AreEqual(expectedInnerMessage, actualInnerMessage);
            Assert.AreEqual(expectedInnerType, actualInnerType);
        }
    }
}
