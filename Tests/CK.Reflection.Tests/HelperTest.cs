﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Reflection;

namespace CK.Reflection.Tests
{
    [TestFixture]
    public class HelperTest
    {
        int _aReadOnlyProperty;

        public int AnIntProperty { get { return _aReadOnlyProperty; } }

        [Test]
        public void PropertyInfoThroughLambda()
        {
            {
                // Both types are inferred (holder and property type).
                // Requires an instance of the holder.
                string oneInstance = null;
                PropertyInfo i = Helper.GetPropertyInfo( oneInstance, s => s.Length );
                Assert.That( i.Name, Is.EqualTo( "Length" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( int ) ) );

                Assert.Throws<ArgumentException>( () => Helper.GetPropertyInfo( oneInstance, s => s.IndexOf( 'e' ) ) );
            }
            {
                // Same as before, but default() is used to "obtain" an instance of the holder type.
                // To avoid this, next versions can be used.
                PropertyInfo i = Helper.GetPropertyInfo( default( KeyValuePair<int, long> ), p => p.Value );
                Assert.That( i.Name, Is.EqualTo( "Value" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( long ) ) );
            }
            {
                // This version avoids the instance (but requires the holder type to be specified).
                PropertyInfo i = Helper.GetPropertyInfo<string>( s => s.Length );
                Assert.That( i.Name, Is.EqualTo( "Length" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( int ) ) );
                
                Assert.Throws<ArgumentException>( () => Helper.GetPropertyInfo<string>( s => s.IndexOf( 'e' ) ) );
            }
            {
                // This version avoids the instance (but requires the holder type to be specified),
                // and enables property type checking.
                //
                // PropertyInfo iFail = Helper.GetPropertyInfo<string, byte>( s => s.Length );
                PropertyInfo i = Helper.GetPropertyInfo<string, int>( s => s.Length );
                Assert.That( i.Name, Is.EqualTo( "Length" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( int ) ) );
            }

            {
                // This version uses the closure to capture the reference to the property.
                PropertyInfo i = Helper.GetPropertyInfo( () => AnIntProperty );
                Assert.That( i.Name, Is.EqualTo( "AnIntProperty" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( int ) ) );

                PropertyInfo i2 = Helper.GetPropertyInfo( () => i.Name );
                Assert.That( i2.Name, Is.EqualTo( "Name" ) );
                Assert.That( i2.PropertyType, Is.SameAs( typeof( string ) ) );

                Assert.Throws<ArgumentException>( () => Helper.GetPropertyInfo( () => AppDomain.CurrentDomain.ActivationContext.ApplicationManifestBytes[4] ) );
            }
            {
                // This version uses the closure to capture the reference to the property
                // and enables property type checking.
                
                // PropertyInfo iFail = Helper.GetPropertyInfo<string>( () => AnIntProperty );

                PropertyInfo i = Helper.GetPropertyInfo<int>( () => AnIntProperty );
                Assert.That( i.Name, Is.EqualTo( "AnIntProperty" ) );
                Assert.That( i.PropertyType, Is.SameAs( typeof( int ) ) );

                PropertyInfo i2 = Helper.GetPropertyInfo<string>( () => i.Name );
                Assert.That( i2.Name, Is.EqualTo( "Name" ) );
                Assert.That( i2.PropertyType, Is.SameAs( typeof( string ) ) );
                
                Assert.Throws<ArgumentException>( () => Helper.GetPropertyInfo( () => i2.Name.Clone() ) );

            }
        }

        [Test]
        public void Parameters()
        {
            var bindingJustForTest = System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static;
            MethodInfo m = typeof( HelperTest ).GetMethod( "JustForTest", bindingJustForTest );
            {
                Type[] p = Helper.CreateParametersType( m.GetParameters() );
                Assert.That( p[0] == typeof( int ) );
                Assert.That( p[1] == typeof( HelperTest ) );
                Assert.That( p.Length == 2 );
            }
            {
                Type[] p = Helper.CreateParametersType( m.GetParameters(), 0 );
                Assert.That( p[0] == typeof( int ) );
                Assert.That( p[1] == typeof( HelperTest ) );
                Assert.That( p.Length == 2 );
            }
            {
                Type[] p = Helper.CreateParametersType( m.GetParameters(), 1 );
                Assert.That( p[0] == typeof( HelperTest ) );
                Assert.That( p.Length == 1 );
            }
            {
                Type[] p = Helper.CreateParametersType( m.GetParameters(), typeof( CategoryAttribute ) );
                Assert.That( p[0] == typeof( CategoryAttribute ) );
                Assert.That( p[1] == typeof( int ) );
                Assert.That( p[2] == typeof( HelperTest ) );
                Assert.That( p.Length == 3 );
            }
        }

        delegate void JustForTestDelegate( int i, HelperTest obj );

        static void JustForTest( int i, HelperTest obj )
        {
        }

        interface IExplicit
        {
            bool Start();
        }

        class ImplicitImpl : IExplicit
        {
            public bool Start() { return true; }
        }

        class ExplicitImpl : IExplicit
        {
            bool IExplicit.Start() { return true; }
        }

        class ExplicitImpl2 : IExplicit
        {
            protected ExplicitImpl2( int i )
            {
            }

            bool IExplicit.Start() { return true; }
            public bool Start() { return false; }
        }
        
        class ExplicitImpl3 : ExplicitImpl2
        {
            public ExplicitImpl3()
                : base( 0 )
            {
            }
        }

        [Test]
        public void ExplicitImplementation()
        {
            {
                var c = new ExplicitImpl();
                MethodInfo m0 = c.GetType().GetMethod( "Start", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
                Assert.That( m0, Is.Null, "Start is not found..." );

                MethodInfo m1 = c.GetType().GetMethod( "CK.Reflection.Tests.HelperTest.IExplicit.Start", BindingFlags.Instance | BindingFlags.NonPublic );
                Assert.That( m1, Is.Not.Null );
                Assert.That( m1.Invoke( c, null ), Is.True );
                Assert.That( m1.DeclaringType, Is.EqualTo( c.GetType() ) );

                Assert.That( c.GetType().GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public ).Length > 0, "Default ctor is accessible." );
            }
            {
                var c = new ImplicitImpl();
                MethodInfo m0 = c.GetType().GetMethod( "Start", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
                Assert.That( m0, Is.Not.Null, "Start exists." );

                MethodInfo m1 = c.GetType().GetMethod( "CK.Reflection.Tests.HelperTest.IExplicit.Start", BindingFlags.Instance | BindingFlags.NonPublic );
                Assert.That( m1, Is.Null, "But the explicit does not exist..." );
            }
            {
                ExplicitImpl2 c2 = new ExplicitImpl3();
                MethodInfo m0 = c2.GetType().GetMethod( "Start", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
                Assert.That( m0, Is.Not.Null, "Found the exposed one." );
                Assert.That( m0.Invoke( c2, null ), Is.False );

                MethodInfo m1 = c2.GetType().GetMethod( "CK.Reflection.Tests.HelperTest.IExplicit.Start", BindingFlags.Instance | BindingFlags.NonPublic );
                Assert.That( m1, Is.Not.Null );
                Assert.That( m1.Invoke( c2, null ), Is.True );
                Assert.That( m1.DeclaringType, Is.EqualTo( typeof( ExplicitImpl2 ) ) );

                Assert.That( c2.GetType().GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )[0].GetParameters().Length == 0, "The .ctor() is accessible." );
                Assert.That( typeof( ExplicitImpl2 ).GetConstructors( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public )[0].GetParameters()[0].ParameterType,
                    Is.EqualTo( typeof( int ) ), "The protected .ctor(int) is accessible." );
            }
        }


    }
}