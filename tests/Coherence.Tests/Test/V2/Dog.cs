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
    [PortableType(2, 2)]
    public class Dog : Pet
    {
        private readonly IEvolvable m_evolvable = new SimpleEvolvable(2);

        public Dog()
        {
        }

        public Dog(String name, int age, String breed, Color color)
            : base("Canis lupus familiaris", name, age)
        {
            Breed = breed;
            Color = color;
        }

        public String Breed { get; set; }
        public Color Color { get; set; }

        public override void ReadExternal(IPofReader reader)
        {
            if (reader.UserTypeId == 2)
            {
                Breed = reader.ReadString(0);
                if (reader.VersionId > 1)
                {
                    Color = (Color) reader.ReadObject(1);
                }
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
                writer.WriteObject(1, Color);
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
            return base.Equals(dog) 
                && String.Equals(Breed, dog.Breed) 
                && Color == dog.Color;
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
                int hashCode = base.GetHashCode();
                hashCode = (hashCode*397) ^ Breed.GetHashCode();
                hashCode = (hashCode*397) ^ (int) Color;
                return hashCode;
            }
        }

        public override String ToString()
        {
            return "Dog.v2{" +
                   "name=" + Name + 
                   ", age=" + Age + 
                   ", species=" + Species + 
                   ", breed=" + Breed + 
                   ", color=" + Color + 
                   '}';
        }
    }
}