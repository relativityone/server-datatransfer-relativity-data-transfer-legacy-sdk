using System;
using System.Collections.Generic;
using Castle.Windsor;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry;
using DataTransfer.Legacy.MassImport.RelEyeTelemetry.MetricsEventsBuilders;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.DataTransfer.Legacy.SDK.ImportExport.V1;
using Relativity.DataTransfer.Legacy.Services.Helpers;
using Relativity.DataTransfer.Legacy.Services.Helpers.BatchCache;
using Relativity.DataTransfer.Legacy.Services.Interceptors;
using Relativity.DataTransfer.Legacy.Services.Metrics;
using Relativity.DataTransfer.Legacy.Services.Observability;
using Relativity.DataTransfer.Legacy.Services.SQL;
using Relativity.Services.Interfaces.LibraryApplication;
using Component = Castle.MicroKernel.Registration.Component;

namespace Relativity.DataTransfer.Legacy.Services.Tests
{
	public abstract class ServicesBaseTests
	{
		protected Mock<IServiceContextFactory> ServiceContextFactoryMock;
		protected Mock<ICommunicationModeStorage> CommunicationModeStorageMock;
		protected Mock<IRelativityPermissionHelper> RelativityPermissionHelperMock;
		protected Mock<IAPILog> LoggerMock;
		protected Mock<IMetricsContext> MetricsMock;
		protected Mock<IRelEyeMetricsService> RelEyeMetricsServiceMock;
		protected Mock<IEventsBuilder> EventsBuilderMock;
		protected Mock<ISnowflakeMetrics> SnowflakeMetricsMock;
		protected WindsorContainer Container;
		protected Mock<ISqlExecutor> SqlExecutorMock;
		protected Mock<ISqlRetryPolicy> SqlRetryPolicyMock;
		protected Mock<ILibraryApplicationManager> LibraryApplicationManager;
		protected Mock<IMetricsPublisher> MetricsPublisherMock;
		protected Mock<ITraceGenerator> TraceGeneratorMock;

		[OneTimeSetUp]
		public void OneTimeSetup()
		{
			ServiceContextFactoryMock = new Mock<IServiceContextFactory>();
			CommunicationModeStorageMock = new Mock<ICommunicationModeStorage>();
			RelativityPermissionHelperMock = new Mock<IRelativityPermissionHelper>();
			LoggerMock = new Mock<IAPILog>();
			MetricsMock = new Mock<IMetricsContext>();
			RelEyeMetricsServiceMock = new Mock<IRelEyeMetricsService>();
			EventsBuilderMock = new Mock<IEventsBuilder>();
			SnowflakeMetricsMock = new Mock<ISnowflakeMetrics>();
			SqlExecutorMock = new Mock<ISqlExecutor>();
			SqlRetryPolicyMock = new Mock<ISqlRetryPolicy>();
			LibraryApplicationManager = new Mock<ILibraryApplicationManager>();
			MetricsPublisherMock = new Mock<IMetricsPublisher>();
			TraceGeneratorMock = new Mock<ITraceGenerator>();

			Container = new WindsorContainer();
			Container.Register(Component.For<ISqlExecutor>().Instance(SqlExecutorMock.Object));
			Container.Register(Component.For<ISqlRetryPolicy>().Instance(SqlRetryPolicyMock.Object));
			Container.Register(Component.For<ISnowflakeMetrics>().Instance(SnowflakeMetricsMock.Object));
			Container.Register(Component.For<IAPILog>().Instance(LoggerMock.Object));
			Container.Register(Component.For<IServiceContextFactory>().Instance(ServiceContextFactoryMock.Object));
			Container.Register(Component.For<ICommunicationModeStorage>().Instance(CommunicationModeStorageMock.Object));
			Container.Register(Component.For<IRelativityPermissionHelper>().Instance(RelativityPermissionHelperMock.Object));
			Container.Register(Component.For<ITraceGenerator>().Instance(TraceGeneratorMock.Object));
			Container.Register(Component.For<ToggleCheckInterceptor>());
			Container.Register(Component.For<PermissionCheckInterceptor>());
			Container.Register(Component.For<LogInterceptor>());
			Container.Register(Component.For<MetricsInterceptor>());
			Container.Register(Component.For<UnhandledExceptionInterceptor>());
			Container.Register(Component.For<DistributedTracingInterceptor>());
			Container.Register(Component.For<IMetricsContext>().Instance(MetricsMock.Object));
			Container.Register(Component.For<Func<IMetricsContext>>().UsingFactoryMethod(x =>
				new Func<IMetricsContext>(Container.Resolve<IMetricsContext>)));
			Container.Register(Component.For<IAuditService>().ImplementedBy<AuditService>());
			Container.Register(Component.For<IRelEyeMetricsService>().Instance(RelEyeMetricsServiceMock.Object).LifestyleTransient());
			Container.Register(Component.For<IEventsBuilder>().Instance(EventsBuilderMock.Object).LifestyleTransient());
			Container.Register(Component.For<IBulkImportService>().ImplementedBy<BulkImportService>());
			Container.Register(Component.For<IBatchResultCache>().ImplementedBy<BatchResultCache>());
			Container.Register(Component.For<ICaseService>().ImplementedBy<CaseService>());
			Container.Register(Component.For<RetryPolicyFactory>());
			Container.Register(Component.For<ICodeService>().ImplementedBy<CodeService>());
			Container.Register(Component.For<IDocumentService>().ImplementedBy<DocumentService>());
			Container.Register(Component.For<IExportService>().ImplementedBy<ExportService>());
			Container.Register(Component.For<IFieldService>().ImplementedBy<FieldService>());
			Container.Register(Component.For<IFileIOService>().ImplementedBy<FileIOService>());
			Container.Register(Component.For<IFolderService>().ImplementedBy<FolderService>());
			Container.Register(Component.For<ILibraryApplicationManager>().Instance(LibraryApplicationManager.Object));
			Container.Register(Component.For<IHealthCheckService>().ImplementedBy<HealthCheckService>());
			Container.Register(Component.For<IEnumerable<IMetricsPublisher>>().Instance(new[] { MetricsPublisherMock.Object }));
			Container.Register(Component.For<IIAPICommunicationModeService>().ImplementedBy<IAPICommunicationModeService>());
			Container.Register(Component.For<IObjectService>().ImplementedBy<ObjectService>());
			Container.Register(Component.For<IObjectTypeService>().ImplementedBy<ObjectTypeService>());
			Container.Register(Component.For<IProductionService>().ImplementedBy<ProductionService>());
			Container.Register(Component.For<IRelativityService>().ImplementedBy<RelativityService>());
			Container.Register(Component.For<ISearchService>().ImplementedBy<SearchService>());
			Container.Register(Component.For<IWebDistributedService>().ImplementedBy<WebDistributedService>());
		}
	}
}