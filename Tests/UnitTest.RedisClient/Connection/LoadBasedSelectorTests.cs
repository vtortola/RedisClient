using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vtortola.Redis;
using Moq;
using System.Linq;

namespace UnitTest.RedisClient
{
    [TestClass]
    public class LoadBasedSelectorTests
    {
        private ILoadMeasurable CreateSelector(Int32 load)
        {
            var element = new Mock<ILoadMeasurable>();
            element.Setup(x => x.CurrentLoad).Returns(load);
            return element.Object;
        }

        private DummyLoad[] CreateSelectors(params Int32[] loads)
        {
            return Enumerable.Range(0, loads.Length)
                             .Select(i =>new DummyLoad(i, loads[i]))
                             .ToArray();
        }

        [TestMethod]
        public void CanSelectMinimum()
        {
            var selector = new BasicLoadBasedSelector();

            var elements = CreateSelectors(5, 4, 3, 2, 1);

            var selected = selector.Select(elements);

            Assert.AreEqual(1, selected.CurrentLoad);
            Assert.AreEqual(4, selected.Id);
        }

        [TestMethod]
        public void CanRotateSelectionOnEqualLoad()
        {
            var selector = new BasicLoadBasedSelector();

            var elements = CreateSelectors(5,5,5,5,5);

            Assert.AreEqual(0, selector.Select(elements).Id);
            Assert.AreEqual(1, selector.Select(elements).Id);
            Assert.AreEqual(2, selector.Select(elements).Id);
            Assert.AreEqual(3, selector.Select(elements).Id);
            Assert.AreEqual(4, selector.Select(elements).Id);
            Assert.AreEqual(0, selector.Select(elements).Id);
        }

        [TestMethod]
        public void CanRotateSelectionOnZeroLoads()
        {
            var selector = new BasicLoadBasedSelector();

            var elements = CreateSelectors(0,0,0,0,0);

            Assert.AreEqual(0, selector.Select(elements).Id);
            Assert.AreEqual(1, selector.Select(elements).Id);
            Assert.AreEqual(2, selector.Select(elements).Id);
            Assert.AreEqual(3, selector.Select(elements).Id);
            Assert.AreEqual(4, selector.Select(elements).Id);
            Assert.AreEqual(0, selector.Select(elements).Id);
        }

        [TestMethod]
        public void ChoosesFirstWithZeroLoad()
        {
            var selector = new BasicLoadBasedSelector();

            var elements = CreateSelectors(5, 0, 1, 2, 4);

            var selected = selector.Select(elements);

            Assert.AreEqual(1, selected.Id);
            Assert.IsTrue(elements[0].HasBeenChecked);
            Assert.IsTrue(elements[1].HasBeenChecked);
            Assert.IsFalse(elements[2].HasBeenChecked);
            Assert.IsFalse(elements[3].HasBeenChecked);
            Assert.IsFalse(elements[4].HasBeenChecked);   
        }

        [TestMethod]
        public void CanSafelyOverflow()
        {
            var selector = new BasicLoadBasedSelector(Int32.MaxValue - 10);

            var elements = CreateSelectors(5, 4, 3, 2, 1);

            for(var i= 0; i<20; i++)
            {
                var selected = selector.Select(elements);
            }
        }
    }

    public class DummyLoad : ILoadMeasurable
    {
        Int32 _load;
        public Int32 CurrentLoad
        {
            get
            {
                HasBeenChecked = true;
                return _load;
            }
        }
        public Boolean HasBeenChecked { get; private set; }
        public Int32 Id { get; private set; }
        public DummyLoad(Int32 id, Int32 load)
        {
            _load = load;
            Id = id;
        }
    }

}
