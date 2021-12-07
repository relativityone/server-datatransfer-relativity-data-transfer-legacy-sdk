using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using kCura.Data.RowDataGateway;
using Microsoft.VisualBasic.CompilerServices;
using Relativity.Core.AgentJobManagement;
using Relativity.Logging;

namespace Relativity.MassImport.Data.SqlFramework
{
	/// <summary>
	/// 	''' This class is a wrapped around the very useful sp_getapplock and sp_releaseapplock.  
	/// 	''' If you think you want to synclock, you'd probably be better off doing this.
	/// 	''' https://msdn.microsoft.com/en-us/library/ms189823.aspx
	/// 	''' </summary>
	public class AppLock : IDisposable
	{
		private readonly int? _lockTimeoutInMilliseconds;
		private readonly string _resource;
		private readonly Func<BaseContext, bool> _shouldReleaseAppLock;
		private readonly ILog _logger;
		private readonly BaseContext _context;
		private bool _disposed;
		private DateTime? _lockAcquired;
		private DateTime? _lockReleased;

		/// <summary>
		/// 		''' Initializes a new instance of the <see cref="AppLock"/> class.
		/// 		''' </summary>
		/// 		''' <param name="context">
		/// 		''' The SQL context.
		/// 		''' </param>
		/// 		''' <param name="resource">
		/// 		''' The resource name to lock.
		/// 		''' </param>
		public AppLock(
			BaseContext context,
			string resource,
			Func<BaseContext, bool> isTransactionActive,
			Func<BaseContext, bool> shouldReleaseAppLock,
			ILog logger) 
			: this(context, resource, isTransactionActive, shouldReleaseAppLock, logger, (int?)default)
		{
		}

		/// <summary>
		/// 		''' Initializes a new instance of the <see cref="AppLock"/> class.
		/// 		''' </summary>
		/// 		''' <param name="context">
		/// 		''' The SQL context.
		/// 		''' </param>
		/// 		''' <param name="resource">
		/// 		''' The name of the SQL resource to lock.
		/// 		''' </param>
		/// 		''' <param name="lockTimeoutInMilliseconds">
		/// 		''' The SQL lock timeout in milliseconds.
		/// 		''' </param>
		public AppLock(
			BaseContext context, 
			string resource, 
			Func<BaseContext, bool> isTransactionActive, 
			Func<BaseContext, bool> shouldReleaseAppLock,
			ILog logger,
			int lockTimeoutInMilliseconds) 
			: this(context, resource, isTransactionActive, shouldReleaseAppLock, logger, (int?)lockTimeoutInMilliseconds)
		{
		}

		/// <summary>
		/// 		''' Initializes a new instance of the <see cref="AppLock"/> class.
		/// 		''' </summary>
		/// 		''' <param name="context">
		/// 		''' The SQL context.
		/// 		''' </param>
		/// 		''' <param name="resource">
		/// 		''' The name of the SQL resource to lock.
		/// 		''' </param>
		/// 		''' <param name="lockTimeoutInMilliseconds">
		/// 		''' The nullable SQL lock timeout in milliseconds.
		/// 		''' </param>
		private AppLock(
			BaseContext context, 
			string resource, 
			Func<BaseContext,bool> isTransactionActive,
			Func<BaseContext, bool> shouldReleaseAppLock, 
			ILog logger,
			int? lockTimeoutInMilliseconds)
		{
			_context = context;
			_resource = resource;
			_shouldReleaseAppLock = shouldReleaseAppLock;
			_logger = logger;
			_lockTimeoutInMilliseconds = lockTimeoutInMilliseconds;
			if (_context is null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (!isTransactionActive(context))
			{
				throw new ArgumentException("You're required to begin a transaction before calling sp_getapplock.", nameof(context));
			}

			if (string.IsNullOrEmpty(_resource))
			{
				throw new ArgumentException("The resource name must be specified.", nameof(resource));
			}

			if (_lockTimeoutInMilliseconds.HasValue && _lockTimeoutInMilliseconds < -1)
			{
				throw new ArgumentException("The lock timeout cannot be less than -1.", nameof(lockTimeoutInMilliseconds));
			}

			GetAppLock();
		}

		/// <summary>
		/// 		''' Gets the timestamp when the SQL resource lock was acquired.
		/// 		''' </summary>
		/// 		''' <value>
		/// 		''' The timestamp value.
		/// 		''' </value>
		public DateTime? LockAcquired
		{
			get
			{
				return _lockAcquired;
			}
		}

		/// <summary>
		/// 		''' Gets the timestamp when the SQL resource lock was released.
		/// 		''' </summary>
		/// 		''' <value>
		/// 		''' The <see cref="DateTime"/> value.
		/// 		''' </value>
		public DateTime? LockReleased
		{
			get
			{
				return _lockReleased;
			}
		}

		/// <summary>
		/// 		''' Gets the specified SQL lock timeout in milliseconds.
		/// 		''' </summary>
		/// 		''' <value>
		/// 		''' The total number of seconds.
		/// 		''' </value>
		public int? LockTimeoutInMilliseconds
		{
			get
			{
				return _lockTimeoutInMilliseconds;
			}
		}

		/// <summary>
		/// 		''' Gets the name of the SQL resource to lock.
		/// 		''' </summary>
		/// 		''' <value>
		/// 		''' The SQL resource name.
		/// 		''' </value>
		public string Resource
		{
			get
			{
				return _resource;
			}
		}

		private void GetAppLock()
		{
			string sqlText;
			var sqlParameters = new List<SqlParameter>();
			if (_lockTimeoutInMilliseconds.HasValue)
			{
				sqlText =
					@"EXEC @Result = sp_getapplock @Resource = @paramResource, @LockMode = 'Exclusive', @LockTimeout = @lockTimeoutInMilliseconds;
					   SELECT @Result";
				sqlParameters.Add(new SqlParameter("@paramResource", _resource));
				sqlParameters.Add(new SqlParameter("@lockTimeoutInMilliseconds", _lockTimeoutInMilliseconds));
			}
			else
			{
				sqlText = @"EXEC @Result = sp_getapplock @Resource = @paramResource, @LockMode = 'Exclusive';
					   SELECT @Result";
				sqlParameters.Add(new SqlParameter("@paramResource", _resource));
			}

			var returnParameter = new SqlParameter("@Result", SqlDbType.Int);
			returnParameter.Direction = ParameterDirection.Output;
			sqlParameters.Add(returnParameter);

			_context.ExecuteNonQuerySQLStatement(sqlText, sqlParameters);

			int returnedValue = Convert.ToInt32(returnParameter.Value);
			if (returnedValue < 0)
			{
				HandleLockError(returnedValue);
			}

			_lockAcquired = DateTime.Now;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				try
				{

					if (disposing && _shouldReleaseAppLock(_context))
					{
						try
						{
							// The return value is not being checked to avoid possible side effects with existing code.
							const string sqlText = @"EXEC @Result = sp_releaseapplock @Resource = @paramResource;
												   SELECT @Result";

							var sqlParameters = new List<SqlParameter>();
							sqlParameters.Add(new SqlParameter("@paramResource", _resource));
							
							var returnParameter = new SqlParameter("@Result", SqlDbType.Int);
							returnParameter.Direction = ParameterDirection.Output;
							sqlParameters.Add(returnParameter);
							
							_context.ExecuteNonQuerySQLStatement(sqlText, sqlParameters);

							int returnedValue = Convert.ToInt32(returnParameter.Value);
							if (returnedValue < 0)
							{
								HandleReleaseError(returnedValue);
							}

							_lockReleased = DateTime.Now;
						}
						catch (SqlException)
						{
							throw;
						}
						catch (Exception ex)
						{
							throw new ExecuteSQLStatementFailedException($"Failed to release the {_resource} app lock due to an unexpected connection or transaction state.", ex);
						}
					}
				}
				finally
				{
					_disposed = true;
				}
			}
		}

		private void HandleLockError(int returnedValue)
		{
			string errorMessage;
			switch (returnedValue)
			{
				case -1:
					errorMessage = "The lock request timed out.";
					break;
				case -2:
					errorMessage = "The lock request was canceled.";
					break;
				case -3:
					errorMessage = "The lock request was chosen as a deadlock victim.";
					break;
				case -999:
					errorMessage = "Indicates a parameter validation or other call error.";
					break;
				default:
					errorMessage = "Other exception.";
					break;
			}

			throw new ExecuteSQLStatementFailedException($"Failed to acquire app lock for {_resource}. The status of {returnedValue} ({errorMessage}) is returned.");
		}

		private void HandleReleaseError(int returnedValue)
		{
			string errorMessage;
			switch (returnedValue)
			{
				case -999:
					errorMessage = "Indicates parameter validation or other call error.";
					break;
				default:
					errorMessage = "Other exception.";
					break;
			}

			_logger.LogWarning("Failed to release app lock for {_resource}. The status of {returnedValue} ({errorMessage}) is returned. ", _resource, returnedValue, errorMessage);
		}
	}
}