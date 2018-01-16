// Copyright (c) Giovanni Lafratta. All rights reserved.
// Licensed under the MIT license. 
// See the LICENSE file in the project root for more information.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Novacta.Transactions.IO.Tests.Tools
{
    /// <summary>
    /// Verifies conditions about exceptions in 
    /// unit tests using true/false propositions.
    /// </summary>
    public static class ExceptionAssert
    {
        /// <summary>
        /// Determines whether the specified target doesn't throw any 
        /// exception.
        /// </summary>
        /// <param name="target">The target.</param>
        public static void IsNotThrown(
            Action target)
        {
            bool isThrown = false;
            try
            {
                target();
            }
            catch (Exception)
            {
                isThrown = true;
            }

            Assert.IsFalse(isThrown,
                "An unexpected exception has been thrown.");
        }

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
