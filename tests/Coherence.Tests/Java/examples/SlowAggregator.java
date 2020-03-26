/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;

import com.tangosol.util.Base;

import com.tangosol.util.aggregator.DoubleAverage;

import com.tangosol.util.extractor.IdentityExtractor;

/**
 * @author jk 2016.03.29
 */
public class SlowAggregator extends DoubleAverage
{
    public SlowAggregator()
    {
        super(IdentityExtractor.INSTANCE);
    }

    @Override
    protected Object finalizeResult(boolean fFinal)
    {
        if (!fFinal)
        {
            Base.sleep(10000L);
        }

        return super.finalizeResult(fFinal);
    }
}