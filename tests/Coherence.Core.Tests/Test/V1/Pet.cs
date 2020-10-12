/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;

using Tangosol.IO;
using Tangosol.IO.Pof;
using Tangosol.IO.Pof.Schema.Annotation;

namespace Tangosol.Test.V1
{
    [PortableType(1, 1)]
    public class Pet : IPortableObject, IEvolvableObject
    {
        private readonly IEvolvable m_evolvable = new SimpleEvolvable(1);
        private readonly EvolvableHolder m_evolvableHolder = new EvolvableHolder();

        public Pet()
        {
        }

        public Pet(String name)
        {
            Name = name;
        }

        public String Name { get; set; }

        public virtual void ReadExternal(IPofReader reader)
        {
            if (reader.UserTypeId == 1)
            {
                Name = reader.ReadString(0);
            }
        }

        public virtual void WriteExternal(IPofWriter writer)
        {
            if (writer.UserTypeId == 1)
            {
                writer.WriteString(0, Name);
            }
        }

        public virtual IEvolvable GetEvolvable(int nTypeId)
        {
            if (nTypeId == 1)
            {
                return m_evolvable;
            }

            return m_evolvableHolder.GetEvolvable(nTypeId);
        }

        public virtual EvolvableHolder GetEvolvableHolder()
        {
            return m_evolvableHolder;
        }

        protected bool Equals(Pet pet)
        {
            return string.Equals(Name, pet.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            Pet pet = obj as Pet;
            return pet != null && Equals(pet);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override String ToString()
        {
            return "Pet.v1{" +
                   "name=" + Name +
                   '}';
        }
    }
}
