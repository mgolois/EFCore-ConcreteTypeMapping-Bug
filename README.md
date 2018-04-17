# EFCore-ConcreteTypeMapping-Bug

Repo created to show steps to reproduce for the exception below when using EF Core 2.1.0-preview2-final  
issue reference: https://github.com/aspnet/EntityFrameworkCore/issues/11704
```
System.InvalidCastException: Unable to cast object of type 'ConcreteTypeMapping' to type 'Microsoft.EntityFrameworkCore.Storage.RelationalTypeMapping'.
   at Microsoft.EntityFrameworkCore.Metadata.RelationalPropertyAnnotations.get_ColumnType()
   at Microsoft.EntityFrameworkCore.Storage.Internal.SqlServerTypeMapper.GetColumnType(IProperty property)
   at Microsoft.EntityFrameworkCore.Storage.RelationalTypeMapper.FindMapping(IProperty property)
   at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.RelationalTypeMappingConvention.Apply(InternalModelBuilder modelBuilder)
   at Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal.ConventionDispatcher.ImmediateConventionScope.OnModelBuilt(InternalModelBuilder modelBuilder)
   at Microsoft.EntityFrameworkCore.Infrastructure.ModelSource.CreateModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator)
   at System.Lazy`1.ViaFactory(LazyThreadSafetyMode mode)
   at System.Lazy`1.ExecutionAndPublication(LazyHelper executionAndPublication, Boolean useDefaultConstructor)
   at System.Lazy`1.CreateValue(
   at Microsoft.EntityFrameworkCore.Internal.DbContextServices.CreateModel()
   at Microsoft.EntityFrameworkCore.Internal.DbContextServices.get_Model()
   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScoped(ScopedCallSite scopedCallSite, ServiceProviderEngineScope scope)
   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitConstructor(ConstructorCallSite constructorCallSite, ServiceProviderEngineScope scope)
   at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteRuntimeResolver.VisitScoped(ScopedCallSite scopedCallSite, ServiceProviderEngineScope scope)
   at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
   at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService[T](IServiceProvider provider)
   at Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
   at Microsoft.EntityFrameworkCore.DbContext.get_InternalServiceProvider()
   at Microsoft.EntityFrameworkCore.DbContext.get_DbContextDependencies()
   at Microsoft.EntityFrameworkCore.DbContext.get_Model()
   at Microsoft.EntityFrameworkCore.Internal.InternalDbSet`1.get_EntityType()
   at Microsoft.EntityFrameworkCore.Internal.InternalDbSet`1.get_EntityQueryable()
   at Microsoft.EntityFrameworkCore.Internal.InternalDbSet`1.System.Collections.Generic.IEnumerable<TEntity>.GetEnumerator()
   at System.Collections.Generic.List`1.AddEnumerable(IEnumerable`1 enumerable)
   at System.Linq.Enumerable.ToList[TSource](IEnumerable`1 source)
   at WebApplication2.Controllers.HomeController.Index() in C:\Users\gmouelet001\Source\Repos\EFCore-ConcreteTypeMapping-Bug\WebApplication2\Controllers\HomeController.cs:line 13

   ```
