/*
Copyright (c) Quantler B.V., All rights reserved.

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.
*/

using FluentAssertions;
using Quantler.Data.Bars;
using Quantler.Data.Ticks;
using Quantler.Interfaces;
using Quantler.Reflection;
using Quantler.Securities;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Quantler.Tests.Common
{
    public class TestReflectionInvoke
    {
        #region Private Fields

        private Bar _bardata;
        private DataStream _stream;
        private InvokeFactory _sut;
        private Tick _tickdata;
        private bool _touchedbar;
        private bool _touchedtick;
        private Type _type;

        #endregion Private Fields

        #region Public Constructors

        public TestReflectionInvoke()
        {
            _touchedbar = false;
            _type = typeof(Bar);
            _stream = new OHLCBarStream(new SecurityImpl("EURUSD"));
            _sut = new InvokeFactory();
            _bardata = new BarImpl(10m, 11m, 9m, 10.5m, 100, 20120101, 0, "EURUSD", 3600);
            _tickdata = new TickImpl("EURUSD")
            {
                Ask = 10m,
                Bid = 12m,
                Date = 20120101,
                Time = 0
            };
        }

        #endregion Public Constructors

        #region Public Methods

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior
        public void TestInvokeReturn_Default()
        {
            //Arrange
            List<InvokeLinkFunc<Bar, Tick, bool>> OnTest = new List<InvokeLinkFunc<Bar, Tick, bool>>();
            OnTest.Add(new InvokeLinkFunc<Bar, Tick, bool>()
            {
                ParmType = typeof(Bar),
                Action = TestOnMultipleReturn,
                BaseType = typeof(TestReflectionInvoke),
                DataStreams = new[] { _stream },
                ReturnType = typeof(bool),
                SecondParmType = typeof(Tick)
            });

            //Act
            _sut.InvokeAll(OnTest, _bardata, _tickdata);

            //Assert
            _touchedbar.Should().BeTrue();
            _touchedtick.Should().BeTrue();
            OnTest.All(x => x.Result != null && x.Result == true).Should().BeTrue();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior (data void action)
        public void TestInvokeType_Excluding_failed()
        {
            //Arrange
            List<InvokeLinkVoid<Bar>> OnTest = new List<InvokeLinkVoid<Bar>>();
            OnTest.Add(new InvokeLinkVoid<Bar>()
            {
                Action = TestOnbar,
                BaseType = _type,
                DataStreams = new[] { _stream },
                ParmType = _type
            });

            //Act
            _sut.InvokeAllExclude(OnTest, _bardata, _type);

            //Assert
            _touchedbar.Should().BeFalse();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior (data void action)
        public void TestInvokeType_Excluding_success()
        {
            //Arrange
            List<InvokeLinkVoid<Bar>> OnTest = new List<InvokeLinkVoid<Bar>>();
            OnTest.Add(new InvokeLinkVoid<Bar>()
            {
                Action = TestOnbar,
                BaseType = _type,
                DataStreams = new[] { _stream },
                ParmType = _type
            });

            //Act
            _sut.InvokeAllExclude(OnTest, _bardata, typeof(BarImpl));

            //Assert
            _touchedbar.Should().BeTrue();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior filtering the stream used (incorrect stream)
        public void TestInvokeType_FilterStream_Failed()
        {
            //Arrange
            List<InvokeLinkVoid<Bar>> OnTest = new List<InvokeLinkVoid<Bar>>();
            OnTest.Add(new InvokeLinkVoid<Bar>()
            {
                Action = TestOnbar,
                BaseType = _type,
                DataStreams = new[] { _stream },
                ParmType = _type
            });

            //Act
            _sut.InvokeAll(OnTest, _bardata, new OHLCBarStream(new SecurityImpl("AUDUSD")));

            //Assert
            _touchedbar.Should().BeFalse();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior filtering the stream used (correct stream)
        public void TestInvokeType_FilterStream_Success()
        {
            //Arrange
            List<InvokeLinkVoid<Bar>> OnTest = new List<InvokeLinkVoid<Bar>>();
            OnTest.Add(new InvokeLinkVoid<Bar>()
            {
                Action = TestOnbar,
                BaseType = _type,
                DataStreams = new[] { _stream },
                ParmType = _type
            });

            //Act
            _sut.InvokeAll(OnTest, _bardata, _stream);

            //Assert
            _touchedbar.Should().BeTrue();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior filtering the type of data used (incorrect datatype)
        public void TestInvokeType_FilterType_Failed()
        {
            //Arrange
            List<InvokeLinkVoid<Bar>> OnTest = new List<InvokeLinkVoid<Bar>>();
            OnTest.Add(new InvokeLinkVoid<Bar>()
            {
                Action = TestOnbar,
                BaseType = _type,
                DataStreams = new[] { _stream },
                ParmType = _type
            });

            //Act
            _sut.InvokeAll(OnTest, _bardata, _stream, typeof(Tick));

            //Assert
            _touchedbar.Should().BeFalse();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior filtering the type of data used (correct datatype)
        public void TestInvokeType_FilterType_Success()
        {
            //Arrange
            List<InvokeLinkVoid<Bar>> OnTest = new List<InvokeLinkVoid<Bar>>();
            OnTest.Add(new InvokeLinkVoid<Bar>()
            {
                Action = TestOnbar,
                BaseType = _type,
                DataStreams = new[] { _stream },
                ParmType = _type
            });

            //Act
            _sut.InvokeAll(OnTest, _bardata, _stream, _type);

            //Assert
            _touchedbar.Should().BeTrue();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior (data void action)
        public void TestInvokeType_NoFilter()
        {
            //Arrange
            List<InvokeLinkVoid<Bar>> OnTest = new List<InvokeLinkVoid<Bar>>();
            OnTest.Add(new InvokeLinkVoid<Bar>()
            {
                Action = TestOnbar,
                BaseType = _type,
                DataStreams = new[] { _stream },
                ParmType = _type
            });

            //Act
            _sut.InvokeAll(OnTest, _bardata);

            //Assert
            _touchedbar.Should().BeTrue();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior with incorrect filter reference
        public void TestInvokeVoid_FilterType_Failed()
        {
            //Arrange
            List<InvokeLinkVoid> OnTest = new List<InvokeLinkVoid>();
            OnTest.Add(new InvokeLinkVoid()
            {
                Action = TestVoid,
                BaseType = _type,
                DataStreams = new[] { _stream },
                ParmType = _type
            });

            //Act
            _sut.InvokeAll(OnTest, typeof(Tick));

            //Assert
            _touchedbar.Should().BeFalse();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior including a filter
        public void TestInvokeVoid_FilterType_Success()
        {
            //Arrange
            List<InvokeLinkVoid> OnTest = new List<InvokeLinkVoid>();
            OnTest.Add(new InvokeLinkVoid()
            {
                Action = TestVoid,
                BaseType = _type,
                DataStreams = new[] { _stream },
                ParmType = _type
            });

            //Act
            _sut.InvokeAll(OnTest, typeof(Bar));

            //Assert
            _touchedbar.Should().BeTrue();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test when there are no actions to execute
        public void TestInvokeVoid_NoActions()
        {
            //Arrange
            List<InvokeLinkVoid> OnTest = new List<InvokeLinkVoid>();

            //Act
            _sut.InvokeAll(OnTest);

            //Assert
            _touchedbar.Should().BeFalse();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior
        public void TestInvokeVoid_NoFilter()
        {
            //Arrange
            List<InvokeLinkVoid> OnTest = new List<InvokeLinkVoid>();
            OnTest.Add(new InvokeLinkVoid()
            {
                Action = TestVoid,
                BaseType = _type,
                DataStreams = new[] { _stream },
                ParmType = _type
            });

            //Act
            _sut.InvokeAll(OnTest);

            //Assert
            _touchedbar.Should().BeTrue();
        }

        [Fact]
        [Trait("Quantler.Common", "Quantler")]
        //Test correct default behavior
        public void TestInvokeVoidMultiple_NoFilter()
        {
            //Arrange
            List<InvokeLinkVoid<Bar, Tick>> OnTest = new List<InvokeLinkVoid<Bar, Tick>>();
            OnTest.Add(new InvokeLinkVoid<Bar, Tick>()
            {
                Action = TestOnMultiple,
                BaseType = typeof(TestReflectionInvoke),
                ParmType = typeof(Bar),
                DataStreams = new[] { _stream },
                ReturnType = null
            });

            //Act
            _sut.InvokeAll(OnTest, _bardata, _tickdata);

            //Assert
            _touchedbar.Should().BeTrue();
            _touchedtick.Should().BeTrue();
        }

        /// <summary>
        /// Test void action with data
        /// </summary>
        /// <param name="x"></param>
        public void TestOnbar(Bar x)
        {
            _touchedbar = (x.Open + x.High + x.Low + x.Close) == (_bardata.Open + _bardata.High + _bardata.Low + _bardata.Close);
        }

        /// <summary>
        /// Test void action with multiple data points
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void TestOnMultiple(Bar x, Tick y)
        {
            TestOnbar(x);
            TestOntick(y);
        }

        /// <summary>
        /// Test return action with multiple data points
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public bool TestOnMultipleReturn(Bar x, Tick y)
        {
            TestOnbar(x);
            TestOntick(y);

            return _touchedtick && _touchedbar;
        }

        /// <summary>
        /// Test void action with data
        /// </summary>
        /// <param name="x"></param>
        public void TestOntick(Tick x)
        {
            _touchedtick = (x.Ask + x.Bid + x.Time + x.Date) == (_tickdata.Ask + _tickdata.Bid + _tickdata.Time + _tickdata.Date);
        }

        /// <summary>
        /// Test void action
        /// </summary>
        public void TestVoid()
        {
            _touchedbar = true;
        }

        #endregion Public Methods
    }
}