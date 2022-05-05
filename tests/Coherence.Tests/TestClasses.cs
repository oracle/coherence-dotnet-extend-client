/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.Net;
using Tangosol.Net.Cache;
using Tangosol.Net.Cache.Support;
using Tangosol.Net.Impl;
using Tangosol.Run.Xml;
using Tangosol.Util;
using Tangosol.Util.Aggregator;
using Tangosol.Util.Extractor;
using Tangosol.Util.Processor;

namespace Tangosol
{
    [Serializable]
    public class Address : IPortableObject
    {
        public string Street;
        public string City;
        public string State;
        public string ZIP;

        public Address()
        {}

        public Address(string street, string city, string state, string zip)
        {
            Street = street;
            City = city;
            State = state;
            ZIP = zip;
        }

        public void ReadExternal(IPofReader reader)
        {
            Street = reader.ReadString(0);
            City = reader.ReadString(1);
            State = reader.ReadString(2);
            ZIP = reader.ReadString(3);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, Street);
            writer.WriteString(1, City);
            writer.WriteString(2, State);
            writer.WriteString(3, ZIP);
        }

        public bool Equals(Address obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.Street, Street) && Equals(obj.City, City) && Equals(obj.State, State) && Equals(obj.ZIP, ZIP);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (Address)) return false;
            return Equals((Address) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = (Street != null ? Street.GetHashCode() : 0);
                result = (result*397) ^ (City != null ? City.GetHashCode() : 0);
                result = (result*397) ^ (State != null ? State.GetHashCode() : 0);
                result = (result*397) ^ (ZIP != null ? ZIP.GetHashCode() : 0);
                return result;
            }
        }
    }

    public class SimpleAddressComparer : IComparer, IPortableObject
    {

        #region IComparer Members

        public int Compare(object o1, object o2)
        {
            if (o1 == null)
            {
                return o2 == null ? 0 : -1;
            }

            if (o2 == null)
            {
                return 1;
            }

            if (((Address)o1).ZIP  == null)
            {
                return (((Address)o2).ZIP == null ? 0 : -1);
            }

            return ((Address)o1).ZIP.CompareTo(((Address)o2).ZIP);
        }

        #endregion

        #region IPortableObject Members

        public void ReadExternal(IPofReader reader)
        { }

        public void WriteExternal(IPofWriter writer)
        { }

        #endregion
    }

    [Serializable]
    public class CustomKeyClass : IPortableObject
    {
        protected object m_o;

        public CustomKeyClass()
        {}

        public CustomKeyClass(object o)
        {
            m_o = o;
        }

        public void ReadExternal(IPofReader reader)
        {
            m_o = reader.ReadObject(0);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteObject(0, m_o);
        }

        public bool Equals(CustomKeyClass o)
        {
            if (ReferenceEquals(null, o))
            {
                return false;
            }
            if (ReferenceEquals(this, o))
            {
                return true;
            }
            return Equals(o.m_o, m_o);
        }

        public override bool Equals(object o)
        {
            if (ReferenceEquals(null, o))
            {
                return false;
            }
            
            return o.GetType() == typeof(CustomKeyClass) ? Equals((CustomKeyClass) o) : false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                object o = m_o;
                return o == null ? 0 : o.GetHashCode();
            }
        }
    }

    [Serializable]
    public class PersonLite
    {
        public string Name;
        public DateTime DOB;

        public PersonLite()
        { }

        public PersonLite(string name, DateTime dob)
        {
            Name = name;
            DOB = dob;
        }

        public bool Equals(PersonLite obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.Name, Name) && obj.DOB.Equals(DOB);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (PersonLite)) return false;
            return Equals((PersonLite) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Name.GetHashCode()*397) ^ DOB.GetHashCode();
            }
        }
    }

    [Serializable]
    public class Person : PersonLite
    {
        private Address address;
        private Person spouse;
        private Person[] children;

        public Person()
        {}

        public Person(string name, DateTime dob) : base(name, dob)
        {}

        public Address Address
        {
            get { return address; }
            set { address = value; }
        }

        public Person Spouse
        {
            get { return spouse; }
            set { spouse = value; }
        }

        public Person[] Children
        {
            get { return children; }
            set { children = value; }
        }

        public bool Equals(Person obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.Name, Name) && obj.DOB.Equals(DOB) &&
                   Equals(obj.address, address) && Equals(obj.spouse, spouse); 
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals((Person) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result*397) ^ (address != null ? address.GetHashCode() : 0);
                result = (result*397) ^ (spouse != null ? spouse.GetHashCode() : 0);
                result = (result*397) ^ (children != null ? children.GetHashCode() : 0);
                return result;
            }
        }
    }

    [Serializable]
    public class PortablePersonLite : PersonLite, IPortableObject
    {
        public PortablePersonLite()
        {}

        public PortablePersonLite(string name, DateTime dob) : base(name, dob)
        {}

        public void ReadExternal(IPofReader reader)
        {
            Name = reader.ReadString(0);
            DOB = reader.ReadDateTime(2);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, Name);
            writer.WriteObject(1, null);
            writer.WriteDateTime(2, DOB);
            writer.WriteObject(3, null);
            writer.WriteArray(4, null, null);
        }
    }

    [Serializable]
    public class SimplePerson : IPortableObject
    {
        private string  ssn;
        private string  firstName;
        private string  lastName;
        private int     year;
        private string  motherId;
        private IList   childrenId;

        public SimplePerson()
        {}

        public SimplePerson(string sSSN, string sFirst, string sLast, int nYear,
                  string sMotherId, string[] asChildrenId)
        {
            SSN         = sSSN;
            FirstName   = sFirst;
            LastName    = sLast;
            Year        = nYear;
            MotherId    = sMotherId;
            ChildrenIds = new ArrayList();
            for (int i = 0; i < asChildrenId.Length; i++)
            {
                ChildrenIds.Add(asChildrenId[i]);
            }
        }

        public string SSN
        {
            get { return ssn; }
            set { ssn = value; }
        }

        public string FirstName
        {
            get { return firstName; }
            set { firstName = value; }
        }

        public string LastName
        {
            get { return lastName; }
            set { lastName = value; }
        }

        public int Year
        {
            get { return year; }
            set { year = value; }
        }

        public string MotherId
        {
            get { return motherId; }
            set { motherId = value; }
        }

        public IList ChildrenIds
        {
            get { return childrenId; }
            set { childrenId = value; }
        }

        public bool Equals(SimplePerson obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj.SSN, SSN) && 
                   Equals(obj.FirstName, FirstName) && Equals(obj.LastName, LastName) &&
                   obj.Year.Equals(Year) &&
                   Equals(obj.MotherId, MotherId) && Equals(obj.ChildrenIds, ChildrenIds); 
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals((SimplePerson) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result*397) ^ (SSN != null ? SSN.GetHashCode() : 0);
                result = (result*397) ^ (FirstName != null ? FirstName.GetHashCode() : 0);
                result = (result*397) ^ (LastName != null ? LastName.GetHashCode() : 0);
                result = (result*397) ^ (MotherId != null ? MotherId.GetHashCode() : 0);
                result = (result*397) ^ (ChildrenIds != null ? ChildrenIds.GetHashCode() : 0);
                return result;
            }
        }

        public void ReadExternal(IPofReader reader)
        {
            SSN         = reader.ReadString(0);
            FirstName   = reader.ReadString(1);
            LastName    = reader.ReadString(2);
            Year        = reader.ReadInt32(3);
            MotherId    = reader.ReadString(4);
            ChildrenIds = (IList) reader.ReadCollection(5, new ArrayList());
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, SSN);
            writer.WriteString(1, FirstName);
            writer.WriteString(2, LastName);
            writer.WriteInt32(3, Year);
            writer.WriteString(4, MotherId);
            writer.WriteCollection(5, ChildrenIds);
        }
    }

    [Serializable]
    public class BadPersonLite : PersonLite, IPortableObject
    {
        public BadPersonLite()
        { }

        public BadPersonLite(string name, DateTime dob)
            : base(name, dob)
        { }

        public void ReadExternal(IPofReader reader)
        {
            DOB = reader.ReadDateTime(2);
            Name = reader.ReadString(0);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, Name);
            writer.WriteObject(1, null);
            writer.WriteDateTime(2, DOB);
            writer.WriteObject(3, null);
            writer.WriteArray(4, null, null);
        }
    }

    [Serializable]
    public class SkippingPersonLite : PersonLite, IPortableObject
    {
        public SkippingPersonLite()
        { }

        public SkippingPersonLite(string name, DateTime dob)
            : base(name, dob)
        { }

        public void ReadExternal(IPofReader reader)
        {
            Name = reader.ReadString(0);
            reader.ReadByteArray(-1);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, Name);
            writer.WriteObject(1, null);
            writer.WriteDateTime(2, DOB);
            writer.WriteObject(3, null);
            writer.WriteArray(4, null, null);
        }
    }

    [Serializable]
    public class PortablePerson : Person, IPortableObject
    {
        public PortablePerson()
        {}

        public PortablePerson(string name, DateTime dob) : base(name, dob)
        {}

        public virtual void ReadExternal(IPofReader reader)
        {
            Name = reader.ReadString(0);
            Address = (Address) reader.ReadObject(1);
            DOB = reader.ReadDateTime(2);
            Spouse = (Person) reader.ReadObject(3);
            Children = (Person[]) reader.ReadArray(4, new PortablePerson[0]);
        }

        public virtual void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, Name);
            writer.WriteObject(1, Address);
            writer.WriteDateTime(2, DOB);
            writer.WriteObject(3, Spouse);
            writer.WriteArray(4, Children, typeof(PortablePerson));
        }
    }

    [Serializable]
    public class EvolvablePortablePerson : PortablePerson, IEvolvablePortableObject
    {
        private Binary m_futureData;
        private int m_dataVersion;

        public EvolvablePortablePerson()
        {}

        public EvolvablePortablePerson(string name, DateTime dob) : base(name, dob)
        {}

        public virtual int ImplVersion
        {
            get { return 1; }
        }

        public virtual int DataVersion
        {
            get { return m_dataVersion; }
            set { m_dataVersion = value; }
        }

        public Binary FutureData
        {
            get { return m_futureData; }
            set { m_futureData = value; }
        }

        public override void ReadExternal(IPofReader reader)
        {
            Name = reader.ReadString(0);
            Address = (Address) reader.ReadObject(1);
            DOB = reader.ReadDateTime(2);
            Spouse = (Person) reader.ReadObject(3);
            Children = (Person[]) reader.ReadArray(4, new EvolvablePortablePerson[0]);
        }

        public override void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, Name);
            writer.WriteObject(1, Address);
            writer.WriteDateTime(2, DOB);
            writer.WriteObject(3, Spouse);
            writer.WriteArray(4, Children, typeof(EvolvablePortablePerson));
        }
    }

    [Serializable]
    public class EvolvablePortablePerson2 : EvolvablePortablePerson
    {
        public string Nationality;
        public Address PlaceOfBirth;

        public EvolvablePortablePerson2()
        {}

        public EvolvablePortablePerson2(string name, DateTime dob) : base(name, dob)
        {}

        public override int ImplVersion
        {
            get { return 2; }
        }

        public override void ReadExternal(IPofReader reader)
        {
            Name = reader.ReadString(0);
            Address = (Address)reader.ReadObject(1);
            DOB = reader.ReadDateTime(2);
            Spouse = (Person)reader.ReadObject(3);
            Children = (Person[]) reader.ReadArray(4, new EvolvablePortablePerson2[0]);
            Nationality = reader.ReadString(5);
            PlaceOfBirth = (Address) reader.ReadObject(6);
        }

        public override void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, Name);
            writer.WriteObject(1, Address);
            writer.WriteDateTime(2, DOB);
            writer.WriteObject(3, Spouse);
            writer.WriteArray(4, Children, typeof(EvolvablePortablePerson2));
            writer.WriteString(5, Nationality);
            writer.WriteObject(6, PlaceOfBirth);
        }
    }

    [Serializable]
    public class PortablePersonReference : Person, IPortableObject
    {
        public PortablePersonReference()
        {}

        public PortablePersonReference(string name, DateTime dob) : base(name, dob)
        {}

        public PortablePersonReference Friend { get; set; }

        public PortablePersonReference[] Siblings { get; set; }

        public void ReadExternal(IPofReader reader)
        {
            Name = reader.ReadString(NAME);
            Address = ((Address) reader.ReadObject(ADDRESS));
            DOB = reader.ReadDateTime(BDATE);
            Spouse = ((Person) reader.ReadObject(SPOUSE));
            Children = ((Person[]) reader.ReadArray(CHILDREN,
                    new PortablePerson[0]));
            Siblings = ((PortablePersonReference[]) reader.ReadArray(
                    SIBLINGS, new PortablePersonReference[0]));
            Friend = ((PortablePersonReference) reader.ReadObject(FRIEND));
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(NAME, Name);
            writer.WriteObject(ADDRESS, Address);
            writer.WriteDateTime(BDATE, DOB);
            writer.WriteObject(SPOUSE, Spouse);
            writer.WriteArray(CHILDREN, Children, typeof(PortablePerson));
            writer.WriteArray(SIBLINGS, Siblings,
                    typeof(PortablePersonReference));
            writer.WriteObject(FRIEND, Friend);
        }

        public const int NAME     = 0;
        public const int ADDRESS  = 1;
        public const int BDATE    = 2;
        public const int SPOUSE   = 3;
        public const int CHILDREN = 4;
        public const int SIBLINGS = 5;
        public const int FRIEND   = 6;
    }

    [Serializable]
    public class Developer : /*IVersionable,*/ IPortableObject
    {
        #region Properties

        /*public string Technology
        {
            get { return m_technology; }
            set { m_technology = value; }
        }*/

       /* public IComparable VersionIndicator
        {
            get { return m_versionIndicator; }
            set { m_versionIndicator = (int) value; }
        }*/

        public string Name
        {
            get { return m_name; }
            set { m_name = value; }
        }

        #endregion

        #region Constructors

        public Developer(string name/*, string tech, Int32 ver*/)
        {
            m_name = name;
            /*m_technology = tech;
            m_versionIndicator = ver;*/
        }

        #endregion

        #region IVersionable interface

        /*public void IncrementVersion()
        {
            m_versionIndicator++;
        }*/

        #endregion

        #region IPortableObject interface

        public void ReadExternal(IPofReader reader)
        {
            m_name = reader.ReadString(0);
            /*m_technology = reader.ReadString(1);
            m_versionIndicator = reader.ReadInt32(2);*/
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, m_name);
            /*writer.WriteString(1, m_technology);
            writer.WriteInt32(2, m_versionIndicator);*/
        }

        #endregion

        #region Data members

        /*private Int32 m_versionIndicator;*/
        private string m_name;
       /* private string m_technology;*/

        #endregion

    }

    [Serializable]
    public class Temperature : IPortableObject, IVersionable
    {
        public Temperature()
        {}

        public Temperature(int value, char grade, int version)
        {
            m_value = value;
            if (grade == 'c' || grade == 'C')
            {
                m_grade = 'C';
            }
            else if(grade == 'f' || grade == 'F')
            {
                m_grade = 'F';
            }
            else
            {
                throw new ArgumentException();
            }
            m_version = version%24;
        }

        public int Value
        {
            get{ return m_value;}
            set{ m_value = value;}
        }

        public char Grade
        {
            get{ return m_grade;}
            set
            {
                 if (value == 'c' || value == 'C')
                {
                    m_grade = 'C';
                }
                else if(value == 'f' || value == 'F')
                {
                    m_grade = 'F';
                }
                else
                {
                    throw new ArgumentException();
                }
            }
        }

        public int Version
        {
            get { return m_version; }
            set { m_version = value; }
        }

        public void ReadExternal(IPofReader reader)
        {
            Value = reader.ReadInt32(0);
            Grade = reader.ReadChar(1);
            Version = reader.ReadInt32(2);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, Value);
            writer.WriteChar(1, Grade);
            writer.WriteInt32(2, Version);

        }

        private int m_value;
        private char m_grade;
        private int m_version;

        public IComparable VersionIndicator
        {
            get { return m_version; }
        }

        public void IncrementVersion()
        {
            m_version = (m_version+1)%24;
        }
    }

    [Serializable]
    public class Score : IVersionable, IPortableObject
    {
        public Score()
        { }

        public Score(byte byteValue, short shortValue, int intValue, long longValue, float floatValue, double doubleValue, decimal decimalValue, RawInt128 int128Value, int version)
        {
            m_byteValue = byteValue;
            m_shortValue = shortValue;
            m_intValue = intValue;
            m_longValue = longValue;
            m_floatValue = floatValue;
            m_doubleValue = doubleValue;
            m_decimalValue = decimalValue;
            m_int128Value = int128Value;

            m_version = version % 24;
        }

        public void ReadExternal(IPofReader reader)
        {
        	m_byteValue = reader.ReadByte(0);
        	m_shortValue = reader.ReadInt16(1);
        	m_intValue = reader.ReadInt32(2);
        	m_longValue = reader.ReadInt64(3);
        	m_floatValue = (float) reader.ReadDouble(4);
        	m_doubleValue = reader.ReadDouble(5);
        	m_decimalValue = reader.ReadDecimal(6);
        	m_int128Value = reader.ReadRawInt128(7);
        }

        public void WriteExternal(IPofWriter writer) 
        {
            writer.WriteByte(0, m_byteValue);
            writer.WriteInt16(1, m_shortValue);
            writer.WriteInt32(2, m_intValue);
            writer.WriteInt64(3, m_longValue);
            writer.WriteDouble(4, m_floatValue);
            writer.WriteDouble(5, m_doubleValue);
            writer.WriteDecimal(6, m_decimalValue);
            writer.WriteRawInt128(7, m_int128Value);
        }

        public int Version
        {
            get { return m_version; }
            set { m_version = value; }
        }

        public byte ByteValue
        {
            get { return m_byteValue; }
            set { m_byteValue = value; }
        }

        public short ShortValue
        {
            get { return m_shortValue; }
            set { m_shortValue = value; }
        }

        public int IntValue
        {
            get { return m_intValue; }
            set { m_intValue = value; }
        }

        public long LongValue
        {
            get { return m_longValue; }
            set { m_longValue = value; }
        }

        public float FloatValue
        {
            get { return m_floatValue; }
            set { m_floatValue = value; }
        }

        public double DoubleValue
        {
            get { return m_doubleValue; }
            set { m_doubleValue = value; }
        }

        public decimal DecimalValue
        {
            get { return m_decimalValue; }
            set { m_decimalValue = value; }
        }

        public RawInt128 RawInt128Value
        {
            get { return m_int128Value; }
            set { m_int128Value = value; }
        }

        private byte m_byteValue;
        private short m_shortValue;
        private int m_intValue;
        private long m_longValue;
        private float m_floatValue;
        private double m_doubleValue;
        private decimal m_decimalValue;
        private RawInt128 m_int128Value;
        private int m_version;

        public IComparable VersionIndicator
        {
            get { return m_version; }
        }

        public void IncrementVersion()
        {
            m_version = (m_version + 1) % 24;
        }
    }

    [Serializable]
    public class BooleanHolder : IPortableObject
    {
        public Boolean Boolean1
        {
            get; set;
        }

        public Boolean Boolean2
        {
            get; set;
        }

        public BooleanHolder()
        {
        }

        public void ReadExternal(IPofReader reader)
        {
            Boolean1 = reader.ReadBoolean(0);
            Boolean2 = reader.ReadBoolean(1);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteBoolean(0, Boolean1);
            writer.WriteBoolean(1, Boolean2);
        }
    }

    public class TestLoader : ICacheLoader
    {
        public object Load(object key)
        {
            return key;
        }

        public IDictionary LoadAll(ICollection keys)
        {
            Hashtable ht = new Hashtable();
            foreach (object o in keys)
            {
                ht.Add(o, null);
            }
            return ht;
        }

        public TestLoader()
        { }

        #region Needed for parameter substitution testing

        private string _s;
        private int _i;
        private bool _b;
        private INamedCache _cache;

        public TestLoader(string s, int i, bool b, INamedCache cache)
        {
            _s = s;
            _i = i;
            _b = b;
            _cache = cache;
        }

        public string StringProperty
        {
            get { return _s; }
        }
        public int IntProperty
        {
            get { return _i; }
        }
        public bool BoolProperty
        {
            get { return _b; }
        }
        public INamedCache CacheProperty
        {
            get { return _cache; }
        }
        #endregion
    }

    public class TestLocalNamedCache : LocalNamedCache, ICacheStore
    {
        public TestLocalNamedCache()
        {}

        public TestLocalNamedCache(int units) : base(units)
        {}

        public TestLocalNamedCache(int units, int expiry) : base(units, expiry)
        {}

        public void Store(object key, object value)
        {}

        public void StoreAll(IDictionary dictionary)
        {}

        public void Erase(object key)
        {}

        public void EraseAll(ICollection keys)
        {}

        public object Load(object key)
        {
            return null;
        }

        public IDictionary LoadAll(ICollection keys)
        {
            return new Hashtable();
        }
    }

    public class EmptyInvocable : AbstractInvocable
    {
        public override void ReadExternal(IPofReader reader)
        {}

        public override void WriteExternal(IPofWriter writer)
        {}
    }

    public class MBeanInvocable : AbstractInvocable
    {
        #region Properties

        public virtual string Value{ get; set; }

        #endregion

        #region IPortableObject implementation

        public override void ReadExternal(IPofReader reader)
        {
            Value = reader.ReadString(0);
        }

        public override void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, Value);
        }

        #endregion
    }

    public class POFObjectInvocable : AbstractInvocable
    {
        #region Properties

        /// <summary>
        /// The POF object
        /// </summary>
        public virtual Object PofObject
        {
            get { return m_pofObject; }
            set { m_pofObject = value; }
        }

        #endregion

        #region IPortableObject implementation

        public override void ReadExternal(IPofReader reader)
        {
            m_pofObject = reader.ReadObject(0);
        }

        public override void WriteExternal(IPofWriter writer)
        {
            writer.WriteObject(0, m_pofObject);
        }

        #endregion

        #region Data members

        private Object m_pofObject;

        #endregion
    }

    public class ProxyStopInvocable : AbstractInvocable
    {
        #region Properties

        public virtual string ProxyServiceName
        { 
            get { return m_proxyServiceName; }
            set { m_proxyServiceName = value; }
        }

        #endregion

        #region IPortableObject implementation

        public override void ReadExternal(IPofReader reader)
        {
            m_proxyServiceName = reader.ReadString(0);
        }

        public override void WriteExternal(IPofWriter writer)
        {
            writer.WriteString(0, m_proxyServiceName);
        }

        #endregion

        #region Data members

        private String m_proxyServiceName;

        #endregion
     }

    public class ReflectionTestType
    {
        public int field = 0;

        public int Property
        {
            get { return m_property; }
            set { m_property = value; }
        }

        public void SetMethod(int val)
        {
            field = val;
        }

        public int GetMethod()
        {
            return 2;
        }

        public int Sum(int number)
        {
            return field + number;
        }

        public InnerTestType InnerMember
        {
            get { return m_inner; }
            set { m_inner = value; }
        }

        private int m_property = 1;
        private InnerTestType m_inner = new InnerTestType();

        public class InnerTestType
        {
            public int field = 1;
        }
    }

    public class TestQueryCacheEntry : IQueryCacheEntry
    {
        public TestQueryCacheEntry(object key, object value)
        {
            m_key = key;
            m_value = value;
        }

        public object Extract(IValueExtractor extractor)
        {
            return InvocableCacheHelper.ExtractFromEntry(extractor, this);   
        }

        public object Key
        {
            get { return m_key; }
        }

        public object Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        private object m_key;
        private object m_value;
    }

    public class TestInvocableCacheEntry : IInvocableCacheEntry
    {
        private object m_key;
        private object m_value;

        public TestInvocableCacheEntry(object key, object value)
        {
            m_key = key;
            m_value = value;
        }

        object IInvocableCacheEntry.Key
        {
            get { return m_key; }
        }

        public object Key
        {
            get { return m_key; }
        }

        object IInvocableCacheEntry.Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public object Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public void SetValue(object value, bool isSynthetic)
        {
            Value = value;
        }

        public bool IsPresent
        {
            get { return true; }
        }

        public object Extract(IValueExtractor extractor)
        {
            return InvocableCacheHelper.ExtractFromEntry(extractor, this);
        }

        public void Update(IValueUpdater updater, object value)
        {
            updater.Update(Value, value);
        }

        public virtual void Remove(bool isSynthetic)
        {}
    }

    public class TestDisposableObject : IDisposable
    {
        // Flag for already disposed
        private bool _alreadyDisposed = false;
        private byte[] buffer = new byte[1024];
        private Stream stream;
        public Stream Stream
        {
            get { return stream; }
            set { stream = value; }
        }

        public TestDisposableObject()
        {
            stream = new MemoryStream(buffer);
        }

        // finalizer:
        // Call the virtual Dispose method.
        ~TestDisposableObject()
        {
            Dispose( false );
        }

        // Implementation of IDisposable.
        // Call the virtual Dispose method.
        // Suppress Finalization.
        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( true );
        }

        // Virtual Dispose method
        protected virtual void Dispose( bool isDisposing )
        {
            // Don't dispose more than once.
            if ( _alreadyDisposed )
                return;
            if ( isDisposing )
            {
                stream.Close();
            }
            // TODO: free unmanaged resources here.

            // Set disposed flag:
            _alreadyDisposed = true;
        }
    }

    public class TestFilterWithArguments : IWrapperStreamFactory, IXmlConfigurable
    {
        private int m_buffer;
        private string m_strategy;
        private IXmlElement config;

        public TestFilterWithArguments(){}

        public TestFilterWithArguments(int buffer, string strategy)
        {
            this.m_buffer = buffer;
            this.m_strategy = strategy;
        }

        public Stream GetInputStream(Stream stream)
        {
            return stream;
        }

        public Stream GetOutputStream(Stream stream)
        {
            return stream;
        }

        public IXmlElement Config
        {
            get { return config; }
            set
            {
                config = value;
                if (config != null)
                {
                    m_buffer   = config.GetSafeElement("buffer").GetInt(1024);
                    m_strategy = config.GetSafeElement("strategy").GetString("default");
                }
            }
        }
    }    

    public class TestCacheListener : CacheListenerSupport.ISynchronousListener
    {
        public int m_inserted = 0;
        public int m_updated = 0;
        public int m_deleted = 0;
        public CacheEventArgs m_evt = null;

        /// <summary>
        /// Invoked when a cache entry has been inserted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the insert
        /// information.
        /// </param>
        public void EntryInserted(CacheEventArgs evt)
        {
            this.m_evt = evt;
            m_inserted++;
        }

        /// <summary>
        /// Invoked when a cache entry has been updated.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the update
        /// information.
        /// </param>
        public void EntryUpdated(CacheEventArgs evt)
        {
            this.m_evt = evt;
            m_updated++;
        }

        /// <summary>
        /// Invoked when a cache entry has been deleted.
        /// </summary>
        /// <param name="evt">
        /// The <see cref="CacheEventArgs"/> carrying the remove
        /// information.
        /// </param>
        public void EntryDeleted(CacheEventArgs evt)
        {
            this.m_evt = evt;
            m_deleted++;
        }

        public override bool Equals(object obj)
        {
            return obj.GetType() == GetType();
        }

        public override int GetHashCode()
        {
            return GetType().GetHashCode();
        }

        #region helper methods

        public int GetActualTotal()
        {
            return m_inserted + m_updated + m_deleted;
        }

        public void ResetActualTotal()
        {
            m_inserted = 0;
            m_updated  = 0;
            m_deleted  = 0;
        }

        ///<summary>
        ///Return the MapEvent received by this MapListener, blocking for 1 second
        /// in the case that an event hasn't been received yet.
        ///</summary>
        ///<return>the CacheEvent received by this MapListener</return>
        public CacheEventArgs WaitForEvent()
        {
            return WaitForEvent(1000L);
        }

        ///<Summary>
        ///Return the CacheEvent received by this MapListener, blocking for the
        ///specified number of milliseconds in the case that an event hasn't been
        ///received yet.
        ///</Summary>
        ///<param name="millis">the number of milliseconds to wait for an event</param>
        ///<return>The MapEvent received by this MapListener.</return>
        public CacheEventArgs WaitForEvent(long millis)
        {
            CacheEventArgs evt = m_evt;
            if (evt == null)
            {
                try
                {
                    Thread.Sleep((int) millis);
                    evt = m_evt;
                }
                catch (ThreadInterruptedException)
                {
                    Thread.CurrentThread.Interrupt();
                }
            }

            ClearEvent();
            return evt;
        }

        /// <summary>
        /// Reset the CacheEvent property.
        /// </summary>
        public void ClearEvent()
        {
            m_evt = null;
        }

        #endregion
    }

    public class TestCacheTriggerEntry : TestInvocableCacheEntry, ICacheTriggerEntry
    {
        public TestCacheTriggerEntry(object key, object value) : base(key, value)
        {
            m_originalValue = value;
        }

        public object OriginalValue
        {
            get { return m_originalValue; }
        }

        public bool IsOriginalPresent
        {
            get { return true; }
        }

        public override void Remove(bool isSynthetic)
        {
            m_isRemoved = true;
        }

        public bool IsRemoved
        {
            get { return m_isRemoved; }
            set { m_isRemoved = value; }
        }

        private object m_originalValue;
        private bool m_isRemoved;
    }

    public class TestAdvancer : PagedEnumerator.IAdvancer
    {
        private object[] m_arr;
        private IEnumerator m_enum;
        
        public TestAdvancer(object[] array)
        {
            m_arr = array;
            m_enum = m_arr.GetEnumerator();
        }

        public ICollection NextPage()
        {
            if (m_enum.MoveNext())
            {
                object o = m_enum.Current;
                return new object[] {o};
            }
            return null;
        }

        public void Reset()
        {
            m_enum.Reset();
        }

        public void Remove(object curr)
        {
            throw new NotImplementedException();
        }
    }

    // KeyAssociate related test classes

    public class Order : IPortableObject
    {
        private int orderId;
        private string name;

        public Order()
        {
        }

        public Order(int orderId, string name)
        {
            this.orderId = orderId;
            this.name = name;
        }

        public int OrderId
        {
            get { return orderId; }
            set { orderId = value; }
        }

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        #region Implementation of IPortableObject

        public void ReadExternal(IPofReader reader)
        {
            orderId = reader.ReadInt32(0);
            name = reader.ReadString(1);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, orderId);
            writer.WriteString(1, name);
        }

        #endregion
    }

    public class OrderKey : IPortableObject, IKeyAssociation
    {
        private int id;

        public OrderKey()
        {
        }

        public OrderKey(int id)
        {
            this.id = id;
        }

        #region IKeyAssociation Members

        public object AssociatedKey
        {
            get { return id; }
        }

        #endregion

        #region Implementation of IPortableObject

        public void ReadExternal(IPofReader reader)
        {
            id = reader.ReadInt32(0);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, id);
        }

        #endregion
    }

    public class Item : IPortableObject
    {
        private int itemId;
        private double sum;

        public Item()
        {
        }

        public Item(int itemId, double sum)
        {
            this.itemId = itemId;
            this.sum = sum;
        }

        public int ItemId
        {
            get { return itemId; }
            set { itemId = value; }
        }

        public double Sum
        {
            get { return sum; }
            set { sum = value; }
        }

        #region Implementation of IPortableObject

        public void ReadExternal(IPofReader reader)
        {
            itemId = reader.ReadInt32(0);
            sum = reader.ReadDouble(1);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, itemId);
            writer.WriteDouble(1, sum);
        }

        #endregion
    }

    public class ItemKey : IPortableObject, IKeyAssociation
    {
        private int id;
        private int parentId;

        public ItemKey()
        {
        }

        public ItemKey(int id, int parentId)
        {
            this.id = id;
            this.parentId = parentId;
        }

        #region IKeyAssociation Members

        public object AssociatedKey
        {
            get { return parentId; }
        }

        #endregion

        #region Implementation of IPortableObject

        public void ReadExternal(IPofReader reader)
        {
            id = reader.ReadInt32(0);
            parentId = reader.ReadInt32(1);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, id);
            writer.WriteInt32(1, parentId);
        }

        #endregion
    }

    public class TestContact : IPortableObject, IComparer
    {
        #region Properties

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName
        {
            get { return m_firstName; }
            set { m_firstName = value; }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName
        {
            get { return m_lastName; }
            set { m_lastName = value; }
        }

        /// <summary>
        /// Gets or sets the home address.
        /// </summary>
        /// <value>
        /// The home address.
        /// </value>
        public ExampleAddress HomeAddress
        {
            get { return m_addrHome; }
            set { m_addrHome = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor (necessary for IPortableObject implementation).
        /// </summary>
        public TestContact()
        {
        }

        /// <summary>
        /// Construct Contact
        /// </summary>
        /// <param name="firstName">
        /// The first name.
        /// </param>
        /// <param name="lastName"> 
        /// The last name.
        /// </param>
        /// <param name="addrHome">
        /// The home address.
        public TestContact(string firstName, string lastName, ExampleAddress addrHome)
        {
            m_firstName       = firstName;
            m_lastName        = lastName;
            m_addrHome        = addrHome;
        }

        #endregion

        #region IPortableObject implementation

        /// <see cref="IPortableObject"/>
        void IPortableObject.ReadExternal(IPofReader reader)
        {
            m_firstName       = reader.ReadString(FIRST_NAME);
            m_lastName        = reader.ReadString(LAST_NAME);
            m_addrHome        = (ExampleAddress)reader.ReadObject(HOME_ADDRESS);
        }

        /// <see cref="IPortableObject"/>
        void IPortableObject.WriteExternal(IPofWriter writer)
        {
            writer.WriteString(FIRST_NAME, m_firstName);
            writer.WriteString(LAST_NAME, m_lastName);
            writer.WriteObject(HOME_ADDRESS, m_addrHome);
        }

        #endregion

        #region IComparable implementation

        public int Compare(object o1, object o2)
        {
            int retValue = 0;
            if (o1 == null)
            {
                return o2 == null ? 0 : -1;
            }

            if (o2 == null)
            {
                return 1;
            }

            TestContact tc1 = (TestContact) o1;
            TestContact tc2 = (TestContact) o2;
            if (tc1.FirstName  == null)
            {
                retValue = (tc2.FirstName == null ? 0 : -1);
            }
            else {
                if (tc2.FirstName == null)
                {
                    return 1;
                }
                else {
                    retValue = tc1.FirstName.CompareTo(tc2.FirstName);
                }
            }
            if (retValue != 0)
            {
                return retValue;
            }

            if (tc1.LastName  == null)
            {
                retValue = (tc2.LastName == null ? 0 : -1);
            }
            else {
                if (tc2.LastName == null)
                {
                    return 1;
                }
                else {
                    retValue = tc1.LastName.CompareTo(tc2.LastName);
                }
            }
            if (retValue != 0)
            {
                return retValue;
            }

            if (tc1.HomeAddress  == null)
            {
                return (tc2.HomeAddress == null ? 0 : -1);
            }
            else {
                if (tc2.HomeAddress == null)
                {
                    return 1;
                }
            }
            return tc1.HomeAddress.Compare(tc1.HomeAddress, tc2.HomeAddress);
        }

        #endregion

        #region Object override methods

        /// <summary>
        /// A string representation of the values of the <b>Contact</b> object.
        /// </summary>
        /// <returns>
        /// A string representation of the Contact. 
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(FirstName)
                    .Append(" ")
                    .Append(LastName)
                    .Append("\nAddresses")
                    .Append("\nHome: ").Append(HomeAddress);
              return sb.ToString();
        }

        #endregion

        #region Constants

        /// <summary>
        /// The POF index for the FirstName property
        /// </summary>
        public const int FIRST_NAME = 0;

        /// <summary>
        /// The POF index for the LastName property
        /// </summary>
        public const int LAST_NAME = 1;

        /// <summary>
        /// The POF index for the HomeAddress property
        /// </summary>
        public const int HOME_ADDRESS = 2;

        #endregion

        #region Data members

        /// <summary>
        /// First name.
        /// </summary>
        private string m_firstName;

        /// <summary>
        /// Last name.
        /// </summary>
        private string m_lastName;

        /// <summary>
        /// Home address.
        /// </summary>
        private ExampleAddress m_addrHome;

        #endregion
    }

    public class KAFValidationInvocable : AbstractInvocable
    {
        private object[] m_keys;

        public object[] Keys
        {
            get { return m_keys; }
            set { m_keys = value; }
        }

        public override void ReadExternal(IPofReader reader)
        { }

        public override void WriteExternal(IPofWriter writer)
        {
            writer.WriteArray(0, m_keys);
        }
    }

    public class TestEvictionPolicy : IEvictionPolicy
    {
        public void EntryTouched(IConfigurableCacheEntry entry)
        { }

        public void RequestEviction(long maximum)
        { }

        public string Name
        {
            get { return GetType().Name; }
        }
    }

    public class TestUnitCalculator : IUnitCalculator
    {
        private int m_unit_size;

        public TestUnitCalculator()
        {
            m_unit_size = 2;
        }

        public TestUnitCalculator(int val)
        {
            m_unit_size = val;
        }
        public int CalculateUnits(object key, object value)
        {
            return m_unit_size;
        }

        public string Name
        {
            get { return GetType().Name; }
        }
    }

    class EventTransformerTestObject : IPortableObject
    {
        private int _ID;
        private String _name;

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }

        public String Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public void ReadExternal(IPofReader reader)
        {
            _ID = reader.ReadInt32(0);
            _name = reader.ReadString(1);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, _ID);
            writer.WriteString(1, _name);
        }
    }

    public class IntegerComparer : IComparer, IPortableObject
    {
        #region Implementation of IComparer

        public int Compare(object a, object b)
        {
            int intA = (int)a;
            int intB = (int)b;

            // descending order
            return -(intA - intB);
        }

        #endregion

        #region Implementation of IPortableObject

        public void ReadExternal(IPofReader reader)
        {
        }

        public void WriteExternal(IPofWriter writer)
        {
        }

        #endregion
    }

    class ProcessorPrintUUIDTimestamp : AbstractClusterProcessor
    {
    }

    /// <summary>
    /// FilterFetcher is a class that supports getting <b>IFilters</b> or
    /// <b>IValueExtractors</b> by using an <b>IInvocationService</b>.
    /// </summary>
    /// <author>Ivan Cikic  2010.03.09</author>
    public class FilterFetcher : AbstractInvocable
    {
        #region Constructors

        /// <summary>
        /// Construct a new FilterFetcher.
        /// </summary>
        public FilterFetcher() 
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Construct a new FilterFetcher that will return an IFilter based on
        /// the given string.
        /// </summary>
        /// <param name="query">
        ///  The string that defines the IFilter.
        /// </param>
        public FilterFetcher(string query) 
            : this(query, null, null)
        {
        }

        /// <summary>
        /// Construct a new FilterFetcher that will return an IFilter based on
        /// the given string. The given flag controls whether an IFilter vs.
        /// an IValueExtractor is retreived.
        /// </summary>
        /// <param name="query">
        /// The string that defines the IFilter.
        /// </param>
        /// <param name="fetchExtractor">
        /// A flag that controles the type of value to retrieve.
        /// </param>
        public FilterFetcher(string query, bool fetchExtractor)
        {
            m_query          = query;
            m_fetchExtractor = fetchExtractor;
        }

        /// <summary>
        /// Construct a new FilterFetcher that will return an IFilter based on
        /// the given string and binding environment.
        /// </summary>
        /// <param name="query">
        /// The string that defines the IFilter.
        /// </param>
        /// <param name="env">
        /// An object[] that specifies the binding environment.
        /// </param>
        public FilterFetcher(string query, object[] env)
            : this(query, env, null)
        {
        }

        /// <summary>
        /// Construct a new FilterFetcher that will return an IFilter based on
        /// the given string and binding environment.
        /// </summary>
        /// <param name="query">The string that defines the IFilter.</param>
        /// <param name="bindings">
        /// A dictionary that specifies the binding environment.
        /// </param>
        public FilterFetcher(string query, IDictionary bindings)
            : this(query, null, bindings)
        {
        }

        /// <summary>
        /// Construct a new FilterFetcher that will return an IFilter based on
        /// the given string and binding environments.
        /// </summary>
        /// <param name="query">
        /// The string that defines the IFilter.
        /// </param>
        /// <param name="env">
        /// An object[] that specifies the binding environment.
        /// </param>
        /// <param name="bindings">
        /// A dictionary that specifies the binding environment.
        /// </param>
        public FilterFetcher(string query, object[] env, IDictionary bindings)
        {
            m_query    = query;
            m_env      = env;
            m_bindings = bindings;
        }

        #endregion

        #region IPorableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void ReadExternal(IPofReader reader)
        {
            m_fetchExtractor = reader.ReadBoolean(0);
            m_query          = reader.ReadString(1);
            m_env            = (object[]) reader.ReadArray(2, null);
            m_bindings       = (IDictionary) reader.ReadDictionary(3, null);
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public override void WriteExternal(IPofWriter writer)
        {
            writer.WriteBoolean(   0, m_fetchExtractor);
            writer.WriteString(    1, m_query);
            writer.WriteArray(     2, m_env);
            writer.WriteDictionary(3, m_bindings);
        }

        #endregion

        #region Data members

        /// <summary>
        /// Flag to control whether to get an IValueExtractor vs. an IFilter.
        /// </summary>
        protected bool m_fetchExtractor;

        /// <summary>
        /// The query string to use.
        /// </summary>
        protected string m_query;

        /// <summary>
        /// An array of bindings.
        /// </summary>
        protected object[] m_env;

        /// <summary>
        /// A dictionary of bindings.
        /// </summary>
        protected IDictionary m_bindings;

        /// <summary>
        /// result
        /// </summary>
        protected Object m_oResult;

        #endregion
    }

    /// <summary>
    /// <b>FilterFactory</b> is a utility class that provides a set of
    /// factory methods that are used for building instances of 
    /// <see cref="IFilter"/> or <see cref="IValueExtractor"/>.
    /// </summary>
    /// <remarks>
    /// <p>
    /// We use an <b>IInvocationService</b> to build the Filters and
    /// ValueExtractors on a java proxy server. This class provides an
    /// example of using the IInvocationService to call <b>QueryHelper</b>.</p>
    /// <p>
    /// The FilterFactory API accepts a String that specifies the creation of
    /// rich Filters in a format that should be familiar to anyone that
    /// understands SQL WHERE clauses. For example the String "street = 'Main'
    /// and state = 'TX'" would create a tree of Filters that is the same as
    /// the following code:
    /// 
    /// <code>
    /// new AndFilter(
    ///     new EqualsFilter("getStreet","Main"),
    ///     new EqualsFilter("getState","TX"));
    /// </code>
    /// 
    /// <see cref="QueryHelper"/>
    /// <p>
    /// The factory methods catch a number of Exceptions from the
    /// implementation stages and subsequently may throw an unchecked 
    /// FilterBuildingException.</p>
    /// </remarks>
    /// <author>Ivan Cikic  2010.03.09</author>
    public class FilterFactory
    {
        #region Constructors

        /// <summary>
        /// Construct a FilterFactory instance.
        /// </summary>
        public FilterFactory()
        {
        }

        /// <summary>
        /// Construct a FilterFactory instance and set the service name of
        /// the IInvocationService.
        /// </summary>
        /// <param name="serviceName">
        /// The name of IInvocationService to use.
        /// </param>
        public FilterFactory(string serviceName)
        {
            m_serviceName = serviceName;
            try
            {
                m_service = (IInvocationService) CacheFactory.GetService(serviceName);
            }
            catch (Exception e)
            {
                Console.WriteLine("\n Exception hooking up to service: " + serviceName);
                Console.WriteLine(e);
                m_service = null;
            }
        }

        #endregion

        #region FilterBuilder API

        /// <summary>
        /// Make a new IFilter from the given string.
        /// </summary>
        /// <param name="s">
        /// String in the Coherence Query Language representing an IFilter.
        /// </param>
        /// <returns>The constructed IFilter.</returns>
        public IFilter CreateFilter(string s)
        {
            return FetchFilter(s, null, null);
        }

        /// <summary>
        /// Make a new IFilter from the given String.
        /// </summary>
        /// <param name="s">
        /// String in the Coherence Query Language representing an IFilter.
        /// </param>
        /// <param name="env">The array of objects to use for bind variables.</param>
        /// <returns>The constructed IFilter</returns>
        public IFilter CreateFilter(string s, object[] env)
        {
            return FetchFilter(s, env, null);
        }

        /// <summary>
        /// Make a new IFilter from the given String.
        /// </summary>
        /// <param name="s">
        /// String in the Coherence Query Language representing an IFilter.
        /// </param>
        /// <param name="bindings">
        /// The dictionary of objects to use for bind variables.
        /// </param>
        /// <returns>The constructed IFilter</returns>
        public IFilter CreateFilter(string s, IDictionary bindings)
        {
            return FetchFilter(s, null, bindings);
        }

        /// <summary>
        /// Make a new IFilter from the given String.
        /// </summary>
        /// <param name="s">
        /// String in the Coherence Query Language representing an IFilter.
        /// </param>
        /// <param name="env">
        /// The array of object to use for bind variables.
        /// </param>
        /// <param name="bindings">
        /// The dictionary of objects to use for bind variables.
        /// </param>
        /// <returns>The constructed IFilter</returns>
        public IFilter CreateFilter(String s, object[] env, IDictionary bindings)
        {
            return FetchFilter(s, env, bindings);
        }

        /// <summary>
        /// Make a new IValueExtracter from the given String.
        /// </summary>
        /// <param name="s">
        /// String in the Coherence Query Language representing a
        /// IValueExtractor.
        /// </param>
        /// <returns>The constructed IValueExtractor.</returns>
        public IValueExtractor CreateExtractor(string s)
        {
            return FetchExtractor(s);
        }

        #endregion

        #region Helper methods

        /// <summary>
        /// Make a new IFilter from the given string using IInvocationService.
        /// </summary>
        /// <param name="s">
        /// String in the Coherence Query Language representing an IFilter.
        /// </param>
        /// <param name="env">
        /// The array of Objects to use for Bind variables.
        /// </param>
        /// <param name="bindings">
        /// The dictionary of objects to use for Bind variables.
        /// </param>
        /// <returns>The constructed IFilter.</returns>
        public IFilter FetchFilter(string s, object[] env, IDictionary bindings)
        {
            if (m_service == null)
            {
                return null;
            }
            IDictionary result = m_service.Query(new FilterFetcher(s, env, bindings), null);
            if (result.Values == null)
                return null;

            IEnumerator enumerator = result.Values.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return (IFilter) enumerator.Current;
            }
            return null;
        }

        /// <summary>
        /// Make a new IValueExtracter from the given String.
        /// </summary>
        /// <param name="s">
        /// String in the Coherence Query Language representing a 
        /// IValueExtractor
        /// </param>
        /// <returns>The constructed IValueExtractor.</returns>
        public IValueExtractor FetchExtractor(string s)
        {
            if (m_service == null)
            {
                return null;
            }
            IDictionary result = m_service.Query(new FilterFetcher(s, true), null);
            if (result.Values == null)
                return null;

            IEnumerator enumerator = result.Values.GetEnumerator();
            if (enumerator.MoveNext())
            {
                return (IValueExtractor) enumerator.Current;
            }
            return null;
        }

        #endregion

        #region Data members

        /// <summary>
        /// The service name to use.
        /// </summary>
        private string m_serviceName;

        /// <summary>
        /// The invocation service.
        /// </summary>
        protected IInvocationService m_service;

        #endregion
    }


    public class ExampleAddress : IPortableObject, IComparer
    {
        #region Properties

        /// <summary>
        /// Gets or sets the first line of the street address.
        /// </summary>
        /// <value>
        /// The first line of the street address.
        /// </value>
        public string Street1
        {
            get { return m_street1; }
            set { m_street1 = value; }
        }

        /// <summary>
        /// Gets or sets the second line of the street address.
        /// </summary>
        /// <value>
        /// The second line of the street address.
        /// </value>
        public string Street2
        {
            get { return m_street2; }
            set { m_street2 = value; }
        }

        /// <summary>
        /// Gets or sets the city name.
        /// </summary>
        /// <value>
        /// The city name.
        /// </value>
        public string City
        {
            get { return m_city; }
            set { m_city = value; }
        }

        /// <summary>
        /// Gets or sets the state name.
        /// </summary>
        /// <value>
        /// The state name
        /// </value>
        public string State
        {
            get { return m_state; }
            set { m_state = value; }
        }

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        /// <value>
        /// The zip code.
        /// </value>
        public string ZipCode
        {
            get { return m_zip; }
            set { m_zip = value; }
        }

        /// <summary>
        /// Gets or sets the country name.
        /// </summary>
        /// <value>
        /// The country name.
        /// </value>
        public string Country
        {
            get { return m_country; }
            set { m_country = value; }
        }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Default constructor (necessary for IPortableObject implementation).
        /// </summary>
        public ExampleAddress()
        {
        }

        /// <summary>
        /// Construct an ExampleAddress.
        /// </summary>
        /// <param name="street1">
        /// The first line of the street address.
        /// </param>
        /// <param name="street2">
        /// The second line of the street address.
        /// </param>
        /// <param name="city">
        /// The city name.
        /// </param>
        /// <param name="state">
        /// The state name.
        /// </param>
        /// <param name="zip">
        /// The zip (postal) code.
        /// </param>
        /// <param name="country">
        /// The country name.
        /// </param>
        public ExampleAddress(string street1, string street2, string city,
            string state, string zip, string country)
        {
            m_street1 = street1;
            m_street2 = street2;
            m_city    = city;
            m_state   = state;
            m_zip     = zip;
            m_country = country;
        }
        
        #endregion

        #region IPortableObject implementation

        /// <see cref="IPortableObject"/>
        void IPortableObject.ReadExternal(IPofReader reader)
        {
            m_street1 = reader.ReadString(STREET_1);
            m_street2 = reader.ReadString(STREET_2);
            m_city    = reader.ReadString(CITY);
            m_state   = reader.ReadString(STATE);
            m_zip     = reader.ReadString(ZIP);
            m_country = reader.ReadString(COUNTRY);
        }

        /// <see cref="IPortableObject"/>
        void IPortableObject.WriteExternal(IPofWriter writer)
        {
            writer.WriteString(STREET_1, m_street1);
            writer.WriteString(STREET_2, m_street2);
            writer.WriteString(CITY, m_city);
            writer.WriteString(STATE, m_state);
            writer.WriteString(ZIP, m_zip);
            writer.WriteString(COUNTRY, m_country);
        }

        #endregion

        #region IComparable implementation

        public int Compare(object o1, object o2)
        {
            int retValue = 0;
            if (o1 == null)
            {
                return o2 == null ? 0 : -1;
            }

            if (o2 == null)
            {
                return 1;
            }

            ExampleAddress ad1 = (ExampleAddress) o1;
            ExampleAddress ad2 = (ExampleAddress) o2;
            if (ad1.Street1  == null)
            {
                retValue = (ad2.Street1 == null ? 0 : -1);
            }
            else {
                if (ad2.Street1 == null)
                {
                    return 1;
                }
                else {
                    retValue = ad1.Street1.CompareTo(ad2.Street1);
                }
            }
            if (retValue != 0)
            {
                return retValue;
            }

            if (ad1.Street2  == null)
            {
                retValue = (ad2.Street2 == null ? 0 : -1);
            }
            else {
                if (ad2.Street2 == null)
                {
                    return 1;
                }
                else {
                    retValue = ad1.Street2.CompareTo(ad2.Street2);
                }
            }
            if (retValue != 0)
            {
                return retValue;
            }
            if (ad1.City  == null)
            {
                retValue = (ad2.City == null ? 0 : -1);
            }
            else {
                if (ad2.City == null)
                {
                    return 1;
                }
                else {
                    retValue = ad1.City.CompareTo(ad2.City);
                }
            }
            if (retValue != 0)
            {
                return retValue;
            }

            if (ad1.State  == null)
            {
                retValue = (ad2.State == null ? 0 : -1);
            }
            else {
                if (ad2.State == null)
                {
                    return 1;
                }
                else {
                    retValue = ad1.State.CompareTo(ad2.State);
                }
            }
            if (retValue != 0)
            {
                return retValue;
            }

            if (ad1.Country  == null)
            {
                retValue = (ad2.Country == null ? 0 : -1);
            }
            else {
                if (ad2.Country == null)
                {
                    return 1;
                }
                else {
                    retValue = ad1.Country.CompareTo(ad2.Country);
                }
            }
            if (retValue != 0)
            {
                return retValue;
            }

            if (ad1.ZipCode  == null)
            {
                retValue = (ad2.ZipCode == null ? 0 : -1);
            }
            else {
                if (ad2.ZipCode == null)
                {
                    return 1;
                }
            }
            return ad1.ZipCode.CompareTo(ad2.ZipCode);
        }

        #endregion

        
        #region Object override methods

        /// <summary>
        /// Equality based on the values of the <b>ExampleAddress</b> properties.
        /// </summary>
        /// <returns>
        /// A bool based on the equality of the <b>ExampleAddress</b> properties.
        /// </returns>
        public override bool Equals(object oThat)
        {
            if (oThat is ExampleAddress)
            {
                ExampleAddress that = (ExampleAddress) oThat;
                return Equals(Street1, that.Street1) &&
                       Equals(Street2, that.Street2) &&
                       Equals(City,    that.City)    &&
                       Equals(ZipCode, that.ZipCode) &&
                       Equals(Country, that.Country);
            }
            return false;
        }

        /// <summary>
        /// Return a string representation of this <b>ExampleAddress</b>.
        /// </summary>
        /// <returns>
        /// A string representation of the address.
        /// </returns>
        public override string ToString()
        {
            return Street1 + "\n" +
                   Street2 + "\n" +
                   City + ", " + State + " " + ZipCode + "\n" +
                   Country;
        }

        /// <summary>
        /// Get a hash value for the <b>ExampleAddress</b> object.
        /// </summary>
        /// <returns>
        /// An integer hash value for this <b>ExampleAddress</b> object.
        /// </returns>
        public override int GetHashCode()
        {
            return (Street1 == null ? 0 : Street1.GetHashCode()) ^
                   (Street2 == null ? 0 : Street2.GetHashCode()) ^
                   (ZipCode == null ? 0 : ZipCode.GetHashCode());
        }

        #endregion

        #region Constants
            
        /// <summary>
        /// The POF index for the Street1 property
        /// </summary>
        public const int STREET_1 = 0;

        /// <summary>
        /// The POF index for the Street2 property
        /// </summary>
        public const int STREET_2 = 1;

        /// <summary>
        /// The POF index for the City property
        /// </summary>
        public const int CITY = 2;

        /// <summary>
        /// The POF index for the State property
        /// </summary>
        public const int STATE = 3;

        /// <summary>
        /// The POF index for the Zip property
        /// </summary>
        public const int ZIP = 4;

        /// <summary>
        /// The POF index for the Country property
        /// </summary>
        public const int COUNTRY = 5;
        
        #endregion
        
        #region Data members

        /// <summary>
        /// First line of street address.
        /// </summary>
        private string m_street1;

        /// <summary>
        /// Second line of street address.
        /// </summary>
        private string m_street2;

        /// <summary>
        /// City.
        /// </summary>
        private string m_city;

        /// <summary>
        /// State or Province.
        /// </summary>
        private string m_state;

        /// <summary>
        /// Zip or other postal code.
        /// </summary>
        private string m_zip;

        /// <summary>
        /// Country.
        /// </summary>
        private string m_country;

    #endregion
    }

    /// Simple TestObject
    class TestObject : IPortableObject
    {

        #region Properties

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <value>
        /// The ID value.
        /// </value>
        public int ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        /// <summary>
        /// Gets or sets the Name.
        /// </summary>
        /// <value>
        /// The Name value.
        /// </value>
        public String Name
        {
            get { return m_name; }
            set { m_name = value; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor (necessary for IPortableObject implementation).
        /// </summary>
        public TestObject()
        {
        }

        /// <summary>
        /// Construct a TestObject.
        /// </summary>
        /// <param name="id">
        /// The object id.
        /// </param>
        /// <param name="name">
        /// The object name.
        /// </param>
        public TestObject(int id, String name)
        {
            this.m_ID = id;
            this.m_name = name;
        }
        #endregion

        #region IPortableObject implementation

        /// <see cref="IPortableObject"/>
        public void ReadExternal(IPofReader reader)
        {
            m_ID = reader.ReadInt32(0);
            m_name = reader.ReadString(1);
        }

        /// <see cref="IPortableObject"/>
        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt32(0, m_ID);
            writer.WriteString(1, m_name);
        }
        #endregion

        #region Data members

        /// <summary>
        /// ID.
        /// </summary>
        private int m_ID;

        /// <summary>
        /// Name.
        /// </summary>
        private String m_name;

        #endregion
    }

    public class SlowProcessor : AbstractProcessor, IPortableObject
    {
        #region Properties

        /// <summary>
        /// Gets or sets the time the processor should take.
        /// </summary>
        /// <value>
        /// The ID value.
        /// </value>
        public long Time
        {
            get { return m_Time; }
            set { m_Time = value; }
        }

        /// <summary>
        /// Gets or sets the value to return from the processor.
        /// </summary>
        /// <value>
        /// The Name value.
        /// </value>
        public String ReturnValue
        {
            get { return m_ReturnValue; }
            set { m_ReturnValue = value; }
        }
        #endregion

        public SlowProcessor()
        { }

        #region AbstractProcessor implementation

        public override object Process(IInvocableCacheEntry entry)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IPortableObject implementation

        public void ReadExternal(IPofReader reader)
        {
            m_Time        = reader.ReadInt64(0);
            m_ReturnValue = reader.ReadString(1);
        }

        public void WriteExternal(IPofWriter writer)
        {
            writer.WriteInt64(0, m_Time);
            writer.WriteString(1, m_ReturnValue);
        }

        #endregion

        #region Data members

        /// <summary>
        /// Time to take to execute.
        /// </summary>
        private long m_Time;

        /// <summary>
        /// The value to return.
        /// </summary>
        private String m_ReturnValue;

        #endregion
    }


    public class SlowAggregator : DoubleAverage
    {
        public SlowAggregator()
            : base(IdentityExtractor.Instance)
        {
        }
    }

    public class TestEntryProcessor : AbstractProcessor, IPortableObject
    {
        #region Constructors

        /// <summary>
        /// Default constructor (necessary for IPortableObject implementation).
        /// </summary>
        public TestEntryProcessor()
                :this(false)
        {
        }

        public TestEntryProcessor(bool fRemove)
        {
            f_fRemoveSynthetic = fRemove;
        }
        #endregion

        #region IEntryProcessor implementation

        public override Object Process(IInvocableCacheEntry entry)
        {
            CacheFactory.Log("entrytype is " + entry.GetType().Name, CacheFactory.LogLevel.Always);
            if (f_fRemoveSynthetic)
            {
                entry.Remove(true);
            }
            else
            {
                entry.SetValue("EPSetValue", true);
            }
            return "OK";
        }

        public override IDictionary ProcessAll(ICollection entries)
        {
            return null;
        }

        #endregion

        #region IPortableObject implementation

        /// <summary>
        /// Restore the contents of a user type instance by reading its state
        /// using the specified <see cref="IPofReader"/> object.
        /// </summary>
        /// <param name="reader">
        /// The <b>IPofReader</b> from which to read the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void ReadExternal(IPofReader reader)
        {
        }

        /// <summary>
        /// Save the contents of a POF user type instance by writing its
        /// state using the specified <see cref="IPofWriter"/> object.
        /// </summary>
        /// <param name="writer">
        /// The <b>IPofWriter</b> to which to write the object's state.
        /// </param>
        /// <exception cref="IOException">
        /// If an I/O error occurs.
        /// </exception>
        public virtual void WriteExternal(IPofWriter writer)
        {
        }

        #endregion
        #region data members

        protected readonly bool f_fRemoveSynthetic;

        #endregion
    }
}