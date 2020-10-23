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
    [PortableType(2, 1)]
    public class Dog : Pet
    {
        private readonly IEvolvable m_evolvable = new SimpleEvolvable(1);

        public Dog()
        {
        }

        public Dog(String name, String breed)
            : base(name)
        {
            Breed = breed;
        }

        public String Breed { get; set; }

        public override void ReadExternal(IPofReader reader)
        {
            if (reader.UserTypeId == 2)
            {
                Breed = reader.ReadString(0);
            }
            else
            {
                base.ReadExternal(reader);
            }
        }

        public override void WriteExternal(IPofWriter writer)
        {
            if (writer.UserTypeId == 2)
            {
                writer.WriteString(0, Breed);
            }
            else
            {
                base.WriteExternal(writer);
            }
        }

        public override IEvolvable GetEvolvable(int nTypeId)
        {
            if (nTypeId == 2)
            {
                return m_evolvable;
            }
            return base.GetEvolvable(nTypeId);
        }

        protected bool Equals(Dog dog)
        {
            return base.Equals(dog) && String.Equals(Breed, dog.Breed);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            Dog dog = obj as Dog;
            return dog != null && Equals(dog);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ Breed.GetHashCode();
            }
        }

        public override String ToString()
        {
            return "Dog.v1{" +
                   "name=" + Name +
                   ", breed=" + Breed +
                   '}';
        }
    }
}