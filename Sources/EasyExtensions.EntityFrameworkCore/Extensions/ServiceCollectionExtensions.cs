using Gridify;
using System.Reflection;
using EasyExtensions.Helpers;
using Microsoft.Extensions.DependencyInjection;
using EasyExtensions.EntityFrameworkCore.Repository;
using EasyExtensions.EntityFrameworkCore.Abstractions;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace EasyExtensions.EntityFrameworkCore.Extensions
{
    /// <summary>
    /// <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Sets up Gridify.
        /// </summary>
        public static IServiceCollection AddGridifyMappers(this IServiceCollection services)
        {
            GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
            services.AddGridifyMappers(Assembly.GetCallingAssembly());
            return services;
        }

        /// <summary>
        /// Adds all types that implement <see cref="IRepository"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services"> The <see cref="IServiceCollection"/> instance. </param>
        /// <returns> Current <see cref="IServiceCollection"/> instance. </returns>
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            var repositories = ReflectionHelpers.GetTypesOfInterface<IRepository>();
            foreach (var repository in repositories)
            {
                Type genericType = repository.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRepository<>));
                var descriptor = new ServiceDescriptor(genericType, repository, ServiceLifetime.Scoped);
                services.Add(descriptor);
            }
            return services;
        }
        public static IServiceCollection AddRepositories(this IServiceCollection services, Assembly assembly, Type dbContextType)
        {
            var entityTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.IsPublic && t.IsSubclassOf(typeof(BaseEntity)))
                .ToList();

            foreach (var entityType in entityTypes)
            {
                var repositoryType = typeof(IRepository<>).MakeGenericType(entityType);
                var repositoryImplType = CreateRepositoryType(entityType, dbContextType);
                services.AddScoped(repositoryType, repositoryImplType);
                Console.WriteLine($"Registered repository {repositoryImplType.Name} as {repositoryType.Name}<{repositoryType.GenericTypeArguments[0].Name}>, for entity {entityType.Name}");
            }

            return services;
        }

        private static Type CreateRepositoryType(Type entityType, Type dbContextType)
        {
            var repositoryName = $"{entityType.Name}Repository";
            var typeBuilder = GetTypeBuilder(repositoryName, typeof(BaseRepository<>).MakeGenericType(entityType));

            // Создание конструктора
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { dbContextType });
            var il = constructorBuilder.GetILGenerator();

            // Task<int> SaveChangesAsync()
            var saveChangesMethod = dbContextType.GetMethods()
                .First(m => m.ReturnType == typeof(Task<int>) && m.Name == "SaveChangesAsync");
            // get DbSet<T> property
            var dbSetType = typeof(DbSet<>).MakeGenericType(entityType);
            var dbSetProperty = dbContextType.GetProperties().First(p => p.PropertyType == dbSetType);

            if (saveChangesMethod == null)
            {
                throw new InvalidOperationException($"Method 'SaveChangesAsync' not found in '{dbContextType.Name}'.");
            }

            // Найдите конструктор базового класса
            var baseConstructor = typeof(BaseRepository<>).MakeGenericType(entityType).GetConstructors().First();

            // Генерация IL для конструктора
            il.Emit(OpCodes.Ldarg_0); // this
            il.Emit(OpCodes.Ldarg_1); // context
            il.Emit(OpCodes.Callvirt, dbSetType); // context.Set<entityType>()
            il.Emit(OpCodes.Ldarg_1); // context
            il.Emit(OpCodes.Ldftn, saveChangesMethod);
            il.Emit(OpCodes.Newobj, typeof(Func<Task<int>>).GetConstructor(new[] { typeof(object), typeof(IntPtr) })); // new Func<Task<int>>(context.SaveChangesAsync)
            il.Emit(OpCodes.Call, baseConstructor); // BaseRepository<TEntity>(context.Set<TEntity>(), new Func<Task<int>>(context.SaveChangesAsync))
            il.Emit(OpCodes.Ret);

            return typeBuilder.CreateType();
        }

        private static TypeBuilder GetTypeBuilder(string typeName, Type baseType)
        {
            var typeSignature = typeName;
            var an = new AssemblyName(typeSignature);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
            var tb = moduleBuilder.DefineType(typeSignature, TypeAttributes.Public |
                                                  TypeAttributes.Class |
                                                  TypeAttributes.AutoClass |
                                                  TypeAttributes.AnsiClass |
                                                  TypeAttributes.BeforeFieldInit |
                                                  TypeAttributes.AutoLayout,
                                          baseType);
            return tb;
        }
    }
}