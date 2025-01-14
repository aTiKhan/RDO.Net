﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Collections.Generic;

namespace DevZest.Data.Primitives
{
    [TestClass]
    public class MounterManagerTests
    {
        class TargetType
        {
            static readonly MounterManager<TargetType, PropertyType> s_propertyManager = new MounterManager<TargetType, PropertyType>();

            static TargetType()
            {
                RegisterProperty<TargetType, PropertyType>(x => x.Property1);
                RegisterProperty<TargetType, PropertyType>(x => x.Property2);
            }

            public static void RegisterProperty<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> getter)
                where TTarget : TargetType
                where TProperty : PropertyType, new()
            {
                s_propertyManager.Register<TTarget, TProperty>(getter, a => new TProperty(), null);
            }

            public static Mounter<TTarget, TProperty> RegisterAttachedProperty<TTarget, TProperty>(Expression<Func<TTarget, TProperty>> getter)
                where TTarget : TargetType
                where TProperty : PropertyType, new()
            {
                return s_propertyManager.RegisterAttached<TTarget, TProperty>(getter, a => new TProperty(), null);
            }

            public TargetType()
            {
                foreach (var property in Properties)
                    property.Mount(this);
            }

            public IReadOnlyList<IMounter<TargetType, PropertyType>> Properties
            {
                get { return s_propertyManager.GetAll(this.GetType()); }
            }

            public PropertyType Property1 { get; private set; }

            public PropertyType Property2 { get; private set; }
        }

        class PropertyType
        {
        }

        class TargetTypeDerived : TargetType
        {
            static TargetTypeDerived()
            {
                RegisterProperty<TargetTypeDerived, PropertyTypeDerived>(x => x.Property3);
            }

            public PropertyTypeDerived Property3 { get; private set; }
        }

        class TargetTypeDerived2 : TargetType
        {
            static TargetTypeDerived2()
            {
                RegisterProperty<TargetTypeDerived2, PropertyTypeDerived>(x => x.Property3);
            }

            public PropertyTypeDerived Property3 { get; private set; }
        }

        class PropertyTypeDerived : PropertyType
        {
        }

        class TargetTypeAttached : TargetType
        {
        }

        class Extension
        {
            // It's very interesting the following code does not work under .Net Core 2.0, it works for .Net Framework 4.6.1 though.
            // In .Net Core 2.0, the static field initializer does not get called, you have to initialize in the static constructor.
            //public static readonly Mounter<TargetTypeAttached, PropertyType> _Property3 = TargetType.RegisterAttachedProperty<TargetTypeAttached, PropertyType>(x => GetProperty3(x));
            //public static readonly Mounter<TargetTypeAttached, PropertyType> _Property4 = TargetType.RegisterAttachedProperty<TargetTypeAttached, PropertyType>(x => GetProperty4(x));

            public static readonly Mounter<TargetTypeAttached, PropertyType> _Property3;
            public static readonly Mounter<TargetTypeAttached, PropertyType> _Property4;

            static Extension()
            {
                _Property3 = TargetType.RegisterAttachedProperty<TargetTypeAttached, PropertyType>(x => GetProperty3(x));
                _Property4 = TargetType.RegisterAttachedProperty<TargetTypeAttached, PropertyType>(x => GetProperty4(x));
            }

            public static PropertyType GetProperty3(TargetTypeAttached target)
            {
                return _Property3.GetMember(target);
            }

            public static PropertyType GetProperty4(TargetTypeAttached target)
            {
                return _Property4.GetMember(target);
            }

            public static TargetTypeAttached CreateTarget()
            {
                return new TargetTypeAttached();
            }
        }

        [TestMethod]
        public void MounterManager_RegisterProperty()
        {
            TargetType target = new TargetType();
            Assert.AreEqual(2, target.Properties.Count, "Target object should have 2 properties.");
            VerifyProperty(target.Properties[0], typeof(TargetType), "Property1", typeof(TargetType), typeof(PropertyType));
            VerifyProperty(target.Properties[1], typeof(TargetType), "Property2", typeof(TargetType), typeof(PropertyType));

            Assert.IsNotNull(target.Property1);
            Assert.IsNotNull(target.Property2);
            Assert.AreEqual(target.Property1, target.Properties[0].GetInstance(target));
            Assert.AreEqual(target.Property2, target.Properties[1].GetInstance(target));
        }

        [TestMethod]
        public void MounterManager_RegisterProperty_in_derived_class()
        {
            TargetTypeDerived target = new TargetTypeDerived();
            Assert.AreEqual(3, target.Properties.Count, "Derived target object should have 3 properties.");
            VerifyProperty(target.Properties[0], typeof(TargetType), "Property1", typeof(TargetType), typeof(PropertyType));
            VerifyProperty(target.Properties[1], typeof(TargetType), "Property2", typeof(TargetType), typeof(PropertyType));
            VerifyProperty(target.Properties[2], typeof(TargetTypeDerived), "Property3", typeof(TargetTypeDerived), typeof(PropertyTypeDerived));

            Assert.IsNotNull(target.Property1);
            Assert.IsNotNull(target.Property2);
            Assert.IsNotNull(target.Property3);
            Assert.AreEqual(target.Property1, target.Properties[0].GetInstance(target));
            Assert.AreEqual(target.Property2, target.Properties[1].GetInstance(target));
            Assert.AreEqual(target.Property3, target.Properties[2].GetInstance(target));

            TargetTypeDerived2 target2 = new TargetTypeDerived2();
            Assert.AreEqual(3, target2.Properties.Count);
        }

        [TestMethod]
        public void MounterManager_RegisterAttachedProperty()
        {
            TargetTypeAttached target = Extension.CreateTarget();
            Assert.AreEqual(4, target.Properties.Count, "Derived target object should have 4 properties.");
            VerifyProperty(target.Properties[0], typeof(TargetType), "Property1", typeof(TargetType), typeof(PropertyType));
            VerifyProperty(target.Properties[1], typeof(TargetType), "Property2", typeof(TargetType), typeof(PropertyType));
            VerifyProperty(target.Properties[2], typeof(Extension), "Property3", typeof(TargetTypeAttached), typeof(PropertyType));
            VerifyProperty(target.Properties[3], typeof(Extension), "Property4", typeof(TargetTypeAttached), typeof(PropertyType));

            Assert.IsNotNull(target.Property1);
            Assert.IsNotNull(target.Property2);
            Assert.IsNotNull(Extension.GetProperty3(target));
            Assert.IsNotNull(Extension.GetProperty4(target));
            Assert.AreEqual(target.Property1, target.Properties[0].GetInstance(target));
            Assert.AreEqual(target.Property2, target.Properties[1].GetInstance(target));
            Assert.AreEqual(Extension.GetProperty3(target), target.Properties[2].GetInstance(target));
            Assert.AreEqual(Extension.GetProperty4(target), target.Properties[3].GetInstance(target));
        }

        void VerifyProperty(IMounter<TargetType, PropertyType> property, Type expectedDeclaringType, string expectedName, Type expectedTargetType, Type expectedPropertyType)
        {
            Assert.AreEqual(expectedDeclaringType, property.DeclaringType, string.Format("The owner type should be '{0}'", expectedDeclaringType));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MounterManager_throws_exception_when_register_after_use()
        {
            new TargetType();
            TargetType.RegisterProperty<TargetType, PropertyType>(x => x.Property1);
        }
    }
}
