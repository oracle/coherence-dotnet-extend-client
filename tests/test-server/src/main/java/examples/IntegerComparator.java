/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;

import com.tangosol.io.pof.PortableObject;
import com.tangosol.io.pof.PofReader;
import com.tangosol.io.pof.PofWriter;

import java.io.IOException;

import java.util.Comparator;

     /**
      * Simple comparator to order integers from highest to lowest.
      */
     public class IntegerComparator
             implements Comparator<Integer>, PortableObject
        {
        // ----- constructors -----------------------------------------------

        /**
         * Default constructor
         */
        public IntegerComparator()
            {
            }

        //----- Compatator interface ---------------------------------------
        /**
         * {@inheritDoc}
         */
        public int compare(Integer o1, Integer o2)
            {
            return -o1.compareTo(o2);
            }

        // ----- PortableObject interface -----------------------------------

        /**
         * {@inheritDoc}
         */
        public void readExternal(PofReader in)
                throws IOException
            {
            }

        /**
         * {@inheritDoc}
         */
        public void writeExternal(PofWriter out)
                throws IOException
            {
            }
        }
