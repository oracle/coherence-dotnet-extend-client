/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
package examples;

import java.io.IOException;
import java.math.BigDecimal;
import java.math.BigInteger;

import com.tangosol.io.pof.PofReader;
import com.tangosol.io.pof.PofWriter;
import com.tangosol.io.pof.PortableObject;

public class Score implements PortableObject {

    private byte m_byteValue;
    private short m_shortValue;
    private int m_intValue;
    private long m_longValue;
    private float m_floatValue;
    private double m_doubleValue;
    private BigDecimal m_decimalValue;
    private BigInteger m_int128Value;

    	public Score()
    	{ }
    	
		public Score(byte byteValue, short shortValue, int intValue, long longValue, float floatValue, double doubleValue, BigDecimal decimalValue, BigInteger int128Value)
        {
            m_byteValue = byteValue;
            m_shortValue = shortValue;
            m_intValue = intValue;
            m_longValue = longValue;
            m_floatValue = floatValue;
            m_doubleValue = doubleValue;
            m_decimalValue = decimalValue;
            m_int128Value = int128Value;
        }      

		public byte getByteValue(){
			return m_byteValue;
		}
		
		public void setByteValue(byte value){
			m_byteValue = value;
		}
		
		public short getShortValue(){
			return m_shortValue;
		}

		public void setShortValue(short value){
			m_shortValue = value;
		}	

		public int getIntValue(){
			return m_intValue;
		}

		public void setIntValue(int value){
			m_intValue = value;
		}	

		public long getLongValue(){
			return m_longValue;
		}

		public void setLongValue(long value){
			m_longValue = value;
		}	

		public float getFloatValue(){
			return m_floatValue;
		}

		public void setFloatValue(float value){
			m_floatValue = value;
		}

		public double getDoubleValue(){
			return m_doubleValue;
		}

		public void setDoubleValue(double value){
			m_doubleValue = value;
		}
		
		public BigDecimal getBigDecimalValue(){
			return m_decimalValue;
		}

		public void setBigDecimalValue(BigDecimal value){
			m_decimalValue = value;
		}
		
		public BigInteger getBigIntegerValue(){
			return m_int128Value;
		}

		public void setBigIntegerValue(BigInteger value){
			m_int128Value = value;
		}
		
        public void readExternal(PofReader reader) throws IOException
        {
        	m_byteValue = reader.readByte(0);
        	m_shortValue = reader.readShort(1);
        	m_intValue = reader.readInt(2);
        	m_longValue = reader.readLong(3);
        	m_floatValue = reader.readFloat(4);
        	m_doubleValue = reader.readDouble(5);
        	m_decimalValue = reader.readBigDecimal(6);
        	m_int128Value = reader.readBigInteger(7);
        }

        public void writeExternal(PofWriter writer) throws IOException
        {
            writer.writeByte(0, m_byteValue);
            writer.writeShort(1, m_shortValue);
            writer.writeInt(2, m_intValue);
            writer.writeLong(3, m_longValue);
            writer.writeFloat(4, m_floatValue);
            writer.writeDouble(5, m_doubleValue);
            writer.writeBigDecimal(6, m_decimalValue);
            writer.writeBigInteger(7, m_int128Value);
        }
    }
