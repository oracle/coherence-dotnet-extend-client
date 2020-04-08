/*
 * Copyright (c) 2000, 2020, Oracle and/or its affiliates.
 *
 * Licensed under the Universal Permissive License v 1.0 as shown at
 * http://oss.oracle.com/licenses/upl.
 */
﻿using System;
using Tangosol.IO;
using Tangosol.IO.Pof;

namespace Tangosol.Test.V2
{
    public class Animal : IPortableObject, IEvolvableObject
    {
        private IEvolvable m_evolvable;
        private EvolvableHolder m_evolvableHolder;

        public Animal()
        {
        }

        public Animal(String species)
        {
            Species = species;
        }

        public String Species { get; set; }

        public virtual IEvolvable GetEvolvable(int nTypeId)
        {
            if (m_evolvable == null)
            {
                m_evolvable = new SimpleEvolvable(1);
            }
            if (3 == nTypeId)
            {
                return m_evolvable;
            }
            return GetEvolvableHolder().GetEvolvable(nTypeId);
        }

        public virtual EvolvableHolder GetEvolvableHolder()
        {
            if (m_evolvableHolder == null)
            {
                m_evolvableHolder = new EvolvableHolder();
            }
            return m_evolvableHolder;
        }

        public virtual void ReadExternal(IPofReader reader)
        {
            if (reader.UserTypeId == 3)
            {
                Species = reader.ReadString(0);
            }
        }

        public virtual void WriteExternal(IPofWriter writer)
        {
            if (writer.UserTypeId == 3)
            {
                writer.WriteString(0, Species);
            }
        }

        protected bool Equals(Animal animal)
        {
            return String.Equals(Species, animal.Species);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            Animal animal = obj as Animal;
            return animal != null && Equals(animal);
        }

        public override int GetHashCode()
        {
            return Species.GetHashCode();
        }

        public override String ToString()
        {
            return "Animal.v1{" +
                   "species=" + Species +
                   '}';
        }
    }
}