/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;

namespace Tangosol.Run.Xml
{
    /// <summary>
    /// An extension to the IXmlDocument to iterate through all nodes and perform
    /// task.
    /// </summary>
    /// <author>Luk Ho  2015.11.5</author>
    /// <since>Coherence  12.2.1.0.1</since>
    public static class IXmlDocumentExtension
    {
        /// <summary>
        /// Takes an IXmlDocument and iterate through all nodes to perform action
        /// on each node.
        /// </summary>
        /// <param name="doc">The <see cref="IXmlDocument"/>.</param>
        /// <param name="elementVisitor">The action to perform.</param>
        public static void IterateThroughAllNodes(
            this IXmlDocument doc,
            Action<IXmlElement> elementVisitor)
        {
            if (doc != null && elementVisitor != null)
            {
                foreach (IXmlElement node in doc.ElementList)
                {
                    processChildren(node, elementVisitor);
                }
            }
        }

        /// <summary>
        /// Perform the action and recursively iterate through its children.
        /// </summary>
        /// <param name="node">The XML node to iterate through.</param>
        /// <param name="elementVisitor">The action to perform.</param>
        private static void processChildren(
            IXmlElement node,
            Action<IXmlElement> elementVisitor)
        {
            elementVisitor(node);

            foreach (IXmlElement childNode in node.ElementList)
            {
                processChildren(childNode, elementVisitor);
            }
        }
    }
}