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

namespace Tangosol.Test.V2
{
    [PortableType(1, 2)]
    public class Pet : Animal
    {
        private readonly IEvolvable m_evolvable = new SimpleEvolvable(2);

        public Pet()
        {
        }

        public Pet(String species, String name, int age) : base(species)
        {
            Name = name;
            Age = age;
        }

        public String Name { get; set; }
        public int Age { get; set; }

        public override void ReadExternal(IPofReader reader)
        {
            if (reader.UserTypeId == 1)
            {
                Name = reader.ReadString(0);
                if (reader.VersionId > 1)
                {
                    Age = reader.ReadInt32(1);
                }
            }
            else
            {
                base.ReadExternal(reader);
            }
        }

        public override void WriteExternal(IPofWriter writer)
        {
            if (writer.UserTypeId == 1)
            {
                writer.WriteString(0, Name);
                writer.WriteInt32(1, Age);
            }
            else
            {
                base.WriteExternal(writer);
            }
        }

        public override IEvolvable GetEvolvable(int nTypeId)
        {
            if (nTypeId == 1)
            {
                return m_evolvable;
            }

            return GetEvolvableHolder().GetEvolvable(nTypeId);
        }

        protected bool Equals(Pet pet)
        {
            return base.Equals(pet) 
                && String.Equals(Name, pet.Name) 
                && Age == pet.Age;
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
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ Name.GetHashCode();
                hashCode = (hashCode*397) ^ Age;
                return hashCode;
            }
        }

        public override String ToString()
        {
            return "Pet.v2{" +
                   "name=" + Name +
                   ", age=" + Age + 
                   ", species=" + Species +
                   '}';
        }
    }
}
