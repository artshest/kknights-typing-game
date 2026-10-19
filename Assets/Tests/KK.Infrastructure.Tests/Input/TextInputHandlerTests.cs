using System.Collections.Generic;
using KK.Infrastructure.Input;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace KK.Infrastructure.Tests.Input
{
    public class TextInputHandlerTests : InputTestFixture
    {
        private TextInputHandler _handler;
        private List<char> _received;

        public override void Setup()
        {
            base.Setup();
            _received = new List<char>();
        }

        public override void TearDown()
        {
            _handler?.Dispose();
            _handler = null;
            base.TearDown();
        }

        [Test]
        public void TextInput_FromKeyboard_RaisesEvent()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            CreateHandler();

            Type(keyboard, 'a');

            Assert.That(_received, Is.EqualTo(new[] { 'a' }));
        }

        [TestCase('\b')]
        [TestCase('\r')]
        [TestCase('\n')]
        [TestCase('\t')]
        [TestCase('\u001B')]
        public void TextInput_ControlChar_IsIgnored(char controlChar)
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            CreateHandler();

            Type(keyboard, controlChar);

            Assert.That(_received, Is.Empty);
        }

        [Test]
        public void Dispose_ThenTextInput_RaisesNothing()
        {
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            CreateHandler();

            _handler.Dispose();
            Type(keyboard, 'a');

            Assert.That(_received, Is.Empty);
        }

        [Test]
        public void Dispose_ThenKeyboardAdded_DoesNotSubscribe()
        {
            LogAssert.Expect(LogType.Warning, "There is no current Keyboard");
            CreateHandler();

            _handler.Dispose();
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Type(keyboard, 'a');

            Assert.That(_received, Is.Empty);
        }

        [Test]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            InputSystem.AddDevice<Keyboard>();
            CreateHandler();

            _handler.Dispose();

            Assert.DoesNotThrow(() => _handler.Dispose());
        }

        [Test]
        public void Constructor_NoKeyboard_LogsWarning()
        {
            LogAssert.Expect(LogType.Warning, "There is no current Keyboard");

            CreateHandler();
        }

        [Test]
        public void KeyboardAdded_AfterStartWithoutKeyboard_RaisesEvent()
        {
            LogAssert.Expect(LogType.Warning, "There is no current Keyboard");
            CreateHandler();

            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Type(keyboard, 'a');

            Assert.That(_received, Is.EqualTo(new[] { 'a' }));
        }

        [Test]
        public void KeyboardReconnected_RaisesEventOnce()
        {
            Keyboard first = InputSystem.AddDevice<Keyboard>();
            CreateHandler();

            InputSystem.RemoveDevice(first);
            Keyboard second = InputSystem.AddDevice<Keyboard>();
            Type(second, 'a');

            Assert.That(_received, Is.EqualTo(new[] { 'a' }));
        }

        [Test]
        public void ActiveKeyboardRemoved_AnotherConnected_SwitchesToIt()
        {
            Keyboard first = InputSystem.AddDevice<Keyboard>();
            Keyboard second = InputSystem.AddDevice<Keyboard>();
            first.MakeCurrent();
            CreateHandler();

            InputSystem.RemoveDevice(first);
            Type(second, 'a');

            Assert.That(_received, Is.EqualTo(new[] { 'a' }));
        }

        private void CreateHandler()
        {
            _handler = new TextInputHandler();
            _handler.OnTextInput += _received.Add;
        }

        private static void Type(Keyboard keyboard, char ch)
        {
            InputSystem.QueueTextEvent(keyboard, ch);
            InputSystem.Update();
        }
    }
}
