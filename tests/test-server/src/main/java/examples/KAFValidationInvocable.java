/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;

import java.io.IOException;
import java.util.Set;

import com.tangosol.io.pof.PofReader;
import com.tangosol.io.pof.PofWriter;
import com.tangosol.io.pof.PortableObject;

import com.tangosol.net.CacheFactory;
import com.tangosol.net.DistributedCacheService;
import com.tangosol.net.Invocable;
import com.tangosol.net.InvocationService;
import com.tangosol.net.Member;
import com.tangosol.net.NamedCache;
import com.tangosol.net.partition.KeyAssociator;
import com.tangosol.net.partition.KeyPartitioningStrategy;

public class KAFValidationInvocable implements Invocable, PortableObject {
	
	// ----- constructors ---------------------------------------------------

	public KAFValidationInvocable() {
		super();
	}

	public void setKeys(Object[] keys) {
		m_keys = keys;
	}

	// ----- Invocable interface --------------------------------------------

	public void init(InvocationService service) {
		NamedCache              cacheTest = CacheFactory.getCache("dist-extend-direct");
		DistributedCacheService dService  = (DistributedCacheService) cacheTest.getCacheService();

		if (dService != null) {
			m_partitioning = dService.getKeyPartitioningStrategy();
			m_associator   = dService.getKeyAssociator();
		}
	}

	public void run() {
		if (m_partitioning != null) {
			int[] result = new int[m_keys.length];
			for (int i = 0; i < m_keys.length; i++) {
				result[i] = m_partitioning.getKeyPartition(m_associator.getAssociatedKey(m_keys[i]));
			}

			m_result = result;
		}
	}

	public Object getResult() {
		return m_result;
	}

	// ----- PortableObject interface ---------------------------------------

	public void readExternal(PofReader in) throws IOException {
		m_keys = in.readObjectArray(0, null);
	}

	public void writeExternal(PofWriter out) throws IOException {
		out.writeObjectArray(0, m_keys);
	}

	// ----- Data members ---------------------------------------------------
	
	private Object[] m_keys;
	private Object m_result;
	private KeyPartitioningStrategy m_partitioning;
	private KeyAssociator m_associator;
}
