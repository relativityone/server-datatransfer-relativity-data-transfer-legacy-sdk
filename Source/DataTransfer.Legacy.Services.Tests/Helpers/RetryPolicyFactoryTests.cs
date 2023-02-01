using kCura.Data.RowDataGateway;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Relativity.DataTransfer.Legacy.Services.Tests.Helpers
{
	[TestFixture]
    public class RetryPolicyFactoryTests
    {
        private const int SQL_INVALID_STATEMENT_ERROR_NUMBER = 1099; // The ON clause is not valid for this statement.
        private const int SQL_DEADLOCK_ERROR_NUMBER = 1205; // Transaction (Process ID %d) was deadlocked on %.*ls resources with another process and has been chosen as the deadlock victim. Rerun the transaction.

        private Mock<IAPILog> _loggerMock;
        private RetryPolicyFactory _sut;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _sut = new RetryPolicyFactory(_loggerMock.Object);
        }

        [Test]
        public void CreateDeadlockExceptionRetryPolicy_ReturnsPolicy_WhichRetriesOnDeadlockException()
        {
            // arrange
            SqlException deadlockException = SqlExceptionCreator.NewSqlException(SQL_DEADLOCK_ERROR_NUMBER);
            var retryPolicy = _sut.CreateDeadlockExceptionRetryPolicy(maxNumberOfRetries: 3, backoffBase: 0);


            int numberOfCalls = 0;
            int ThrowDeadlockTwice()
            {
                numberOfCalls++;
                if (numberOfCalls < 3)
                {
                    throw deadlockException;
                }

                return numberOfCalls;
            }

            // act
            int actualNumberOfCalls = retryPolicy.Execute(ThrowDeadlockTwice);

            // assert
            int expectedNumberOfCalls = 3;
            Assert.That(actualNumberOfCalls, Is.EqualTo(expectedNumberOfCalls));
            _loggerMock.Verify(x => x.LogWarning(
                deadlockException,
                It.Is<string>(message => message.Contains("Deadlock occured when executing")),
                It.IsAny<object[]>()));
        }

        [Test]
        public void CreateDeadlockExceptionRetryPolicy_ReturnsPolicy_WhichRetriesOnCreateFailedExceptionCausedByDeadlock()
        {
            // arrange
            SqlException deadlockException = SqlExceptionCreator.NewSqlException(SQL_DEADLOCK_ERROR_NUMBER);
            DeadlockException secondException = new DeadlockException("second", deadlockException);
            CreateFailedException thirdException = new CreateFailedException("third", secondException);

            var retryPolicy = _sut.CreateDeadlockExceptionRetryPolicy(maxNumberOfRetries: 3, backoffBase: 0);

            int numberOfCalls = 0;
            int ThrowDeadlockTwice()
            {
                numberOfCalls++;
                if (numberOfCalls < 3)
                {
                    throw thirdException;
                }

                return numberOfCalls;
            }

            // act
            int actualNumberOfCalls = retryPolicy.Execute(ThrowDeadlockTwice);

            // assert
            int expectedNumberOfCalls = 3;
            Assert.That(actualNumberOfCalls, Is.EqualTo(expectedNumberOfCalls));
            _loggerMock.Verify(x => x.LogWarning(
                thirdException,
                It.Is<string>(message => message.Contains("Deadlock occured when executing")),
                It.IsAny<object[]>()));
        }

        [Test]
        public void CreateDeadlockExceptionRetryPolicy_ReturnsPolicy_WhichRetriesOnDeadlockExceptionInTheMiddleOfTheExceptionChain()
        {
            // arrange
            Exception firstException = new Exception("base");
            SqlException deadlockException = SqlExceptionCreator.NewSqlException(firstException, SQL_DEADLOCK_ERROR_NUMBER);
            Exception thirdException = new Exception("third", deadlockException);
            Exception fourthException = new Exception("fourth", thirdException);

            var retryPolicy = _sut.CreateDeadlockExceptionRetryPolicy(maxNumberOfRetries: 3, backoffBase: 0);

            int numberOfCalls = 0;
            int ThrowDeadlockTwice()
            {
                numberOfCalls++;
                if (numberOfCalls < 3)
                {
                    throw fourthException;
                }

                return numberOfCalls;
            }

            // act
            int actualNumberOfCalls = retryPolicy.Execute(ThrowDeadlockTwice);

            // assert
            int expectedNumberOfCalls = 3;
            Assert.That(actualNumberOfCalls, Is.EqualTo(expectedNumberOfCalls));
            _loggerMock.Verify(x => x.LogWarning(
                fourthException,
                It.Is<string>(message => message.Contains("Deadlock occured when executing")),
                It.IsAny<object[]>()));
        }

        [Test]
        public void CreateDeadlockExceptionRetryPolicy_ReturnsPolicy_WhichNotRetryOnNonDeadlockException()
        {
            // arrange
            SqlException nonDeadlockException = SqlExceptionCreator.NewSqlException(SQL_INVALID_STATEMENT_ERROR_NUMBER);

            var retryPolicy = _sut.CreateDeadlockExceptionRetryPolicy(maxNumberOfRetries: 3, backoffBase: 0);

            int numberOfCalls = 0;
            int ThrowDeadlockTwice()
            {
                numberOfCalls++;
                if (numberOfCalls < 3)
                {
                    throw nonDeadlockException;
                }

                return numberOfCalls;
            }

            // act
            Assert.That(() => retryPolicy.Execute(ThrowDeadlockTwice), Throws.Exception.EqualTo(nonDeadlockException));

            // assert
            int expectedNumberOfCalls = 1;
            Assert.That(numberOfCalls, Is.EqualTo(expectedNumberOfCalls));
            _loggerMock.Verify(x => x.LogWarning(nonDeadlockException, "Will not retry - exception was not caused by SQL deadlock."));
        }

        [Test]
        public void CreateDeadlockExceptionAndResultRetryPolicy_ReturnsPolicy_WhichRetriesOnMessageWithDeadlock()
        {
            // arrange
            SqlException deadlockException = SqlExceptionCreator.NewSqlException(SQL_DEADLOCK_ERROR_NUMBER);
            DeadlockException secondException = new DeadlockException("second", deadlockException);
            CreateFailedException thirdException = new CreateFailedException("third", secondException);

            var retryPolicy = _sut.CreateDeadlockExceptionAndResultRetryPolicy(maxNumberOfRetries: 3, backoffBase: 0);

            int numberOfCalls = 0;
            object ReturnDeadlockTwice()
            {
                numberOfCalls++;
                if (numberOfCalls < 3)
                {
                    return thirdException.ToString();
                }

                return numberOfCalls;
            }

            // act
            object actualNumberOfCalls = retryPolicy.Execute(ReturnDeadlockTwice);

            // assert
            int expectedNumberOfCalls = 3;
            Assert.That(actualNumberOfCalls, Is.EqualTo(expectedNumberOfCalls));
            _loggerMock.Verify(x => x.LogWarning(
                null,
                It.Is<string>(message => message.Contains("Deadlock occured when executing")),
                It.IsAny<object[]>()));
        }

        [Test]
        public void CreateDeadlockExceptionAndResultRetryPolicy_ReturnsPolicy_WhichRetriesOnDeadlockExceptionAndMessageWithDeadlock()
        {
            // arrange
            SqlException deadlockException = SqlExceptionCreator.NewSqlException(SQL_DEADLOCK_ERROR_NUMBER);
            DeadlockException secondException = new DeadlockException("second", deadlockException);
            CreateFailedException thirdException = new CreateFailedException("third", secondException);

            var retryPolicy = _sut.CreateDeadlockExceptionAndResultRetryPolicy(maxNumberOfRetries: 3, backoffBase: 0);

            int numberOfCalls = 0;
            object ReturnDeadlockTwice()
            {
                numberOfCalls++;
                if (numberOfCalls < 2)
                {
                    throw thirdException;
                }
                if (numberOfCalls < 3)
                {
                    return thirdException.ToString();
                }

                return numberOfCalls;
            }

            // act
            object actualNumberOfCalls = retryPolicy.Execute(ReturnDeadlockTwice);

            // assert
            int expectedNumberOfCalls = 3;
            Assert.That(actualNumberOfCalls, Is.EqualTo(expectedNumberOfCalls));
            _loggerMock.Verify(x => x.LogWarning(
                thirdException,
                It.Is<string>(message => message.Contains("Deadlock occured when executing")),
                It.IsAny<object[]>()));
            _loggerMock.Verify(x => x.LogWarning(
                null,
                It.Is<string>(message => message.Contains("Deadlock occured when executing")),
                It.IsAny<object[]>()));
        }

        [Test]
        public void CreateDeadlockExceptionAndResultRetryPolicy_ReturnsPolicy_WhichNotRetryOnMessageWithoutDeadlock()
        {
            // arrange
            const string expectedResponse = "response"; 
            var retryPolicy = _sut.CreateDeadlockExceptionAndResultRetryPolicy(maxNumberOfRetries: 3, backoffBase: 0);

            int numberOfCalls = 0;
            object ReturnStringForFirstCallAndIntForSecond()
            {
                numberOfCalls++;
                if (numberOfCalls < 2)
                {
                    return expectedResponse;
                }

                return numberOfCalls;
            }

            // act
            object actualResponse = retryPolicy.Execute(ReturnStringForFirstCallAndIntForSecond);

            // assert
            Assert.That(actualResponse, Is.EqualTo(expectedResponse));
        }

        /// <summary>
        /// Copied from https://stackoverflow.com/a/1387030
        /// </summary>
        private class SqlExceptionCreator
        {
            private static T Construct<T>(params object[] p)
            {
                var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
                return (T)ctors.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
            }

            internal static SqlException NewSqlException(int number = 1)
            {
                SqlErrorCollection collection = CreateErrorCollection(number);

                return typeof(SqlException)
                    .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
                        null,
                        CallingConventions.ExplicitThis,
                        new[] { typeof(SqlErrorCollection), typeof(string) },
                        new ParameterModifier[] { })
                    .Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;
            }

            internal static SqlException NewSqlException(Exception innerException, int number = 1)
            {
                SqlErrorCollection collection = CreateErrorCollection(number);

                return typeof(SqlException)
                    .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
                        null,
                        CallingConventions.ExplicitThis,
                        new[] { typeof(SqlErrorCollection), typeof(string), typeof(Guid), typeof(Exception) },
                        new ParameterModifier[] { })
                    .Invoke(null, new object[] { collection, "7.0.0", Guid.Empty, innerException }) as SqlException;
            }

            private static SqlErrorCollection CreateErrorCollection(int number)
            {
                SqlErrorCollection collection = Construct<SqlErrorCollection>();
                SqlError error = Construct<SqlError>(number, (byte)2, (byte)3, "server name", "error message", "proc", 100);

                typeof(SqlErrorCollection)
                    .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                    .Invoke(collection, new object[] { error });
                return collection;
            }
        }
    }
}
