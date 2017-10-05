// Copyright 2015 ThoughtWorks, Inc.
//
// This file is part of Gauge-CSharp.
//
// Gauge-CSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// Gauge-CSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with Gauge-CSharp.  If not, see <http://www.gnu.org/licenses/>.

using Gauge.CSharp.Runner.Models;
using Gauge.Messages;
using Moq;
using NUnit.Framework;

namespace Gauge.CSharp.Runner.UnitTests
{
    [TestFixture]
    public class ValidateProcessorTests
    {
        [SetUp]
        public void Setup()
        {
            var mockMethodScanner = new Mock<IMethodScanner>();
            _mockStepRegistry = new Mock<IStepRegistry>();
            mockMethodScanner.Setup(x => x.GetStepRegistry()).Returns(_mockStepRegistry.Object);
            var mockSandBox = new Mock<ISandbox>();
            _messageProcessorFactory = new MessageProcessorFactory(mockMethodScanner.Object, mockSandBox.Object);
        }

        private MessageProcessorFactory _messageProcessorFactory;
        private Mock<IStepRegistry> _mockStepRegistry;

        [Test]
        public void ShouldGetErrorResponseForStepValidateRequestWhenMultipleStepImplFound()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepValidateRequest);
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0
            };
            var message = new Message
            {
                MessageId = 1,
                MessageType = Message.Types.MessageType.StepValidateRequest,
                StepValidateRequest = request
            };
            _mockStepRegistry.Setup(registry => registry.ContainsStep("step_text_1")).Returns(true);
            _mockStepRegistry.Setup(registry => registry.HasMultipleImplementations("step_text_1")).Returns(true);

            var response = messageProcessor.Process(message);

            Assert.AreEqual(false, response.StepValidateResponse.IsValid);
            Assert.AreEqual(StepValidateResponse.Types.ErrorType.DuplicateStepImplementation,
                response.StepValidateResponse.ErrorType);
            Assert.AreEqual("Multiple step implementations found for : step_text_1",
                response.StepValidateResponse.ErrorMessage);
        }

        [Test]
        public void ShouldGetErrorResponseForStepValidateRequestWhennNoImplFound()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepValidateRequest);
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0
            };
            var message = new Message
            {
                MessageId = 1,
                MessageType = Message.Types.MessageType.StepValidateRequest,
                StepValidateRequest = request
            };

            var response = messageProcessor.Process(message);

            Assert.AreEqual(false, response.StepValidateResponse.IsValid);
            Assert.AreEqual(StepValidateResponse.Types.ErrorType.StepImplementationNotFound,
                response.StepValidateResponse.ErrorType);
            StringAssert.Contains("No implementation found for : step_text_1.",
                response.StepValidateResponse.ErrorMessage);
        }


        [Test]
        public void ShouldGetVaildResponseForStepValidateRequest()
        {
            var messageProcessor = _messageProcessorFactory.GetProcessor(Message.Types.MessageType.StepValidateRequest);
            var request = new StepValidateRequest
            {
                StepText = "step_text_1",
                NumberOfParameters = 0
            };
            var message = new Message
            {
                MessageId = 1,
                MessageType = Message.Types.MessageType.StepValidateRequest,
                StepValidateRequest = request
            };
            _mockStepRegistry.Setup(registry => registry.ContainsStep("step_text_1")).Returns(true);
            _mockStepRegistry.Setup(registry => registry.HasMultipleImplementations("step_text_1")).Returns(false);

            var response = messageProcessor.Process(message);

            Assert.AreEqual(true, response.StepValidateResponse.IsValid);
        }
    }
}