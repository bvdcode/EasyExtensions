using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using EasyExtensions.Mediator.Contracts;
using EasyExtensions.Mediator.Pipeline;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace EasyExtensions.Mediator.Registration
{
    /// <summary>
    /// Provides methods for registering MediatR handlers, behaviors, and related services with an IServiceCollection,
    /// including support for generic handler registration limits and registration timeouts.
    /// </summary>
    /// <remarks>This class is intended for advanced scenarios where fine-grained control over MediatR service
    /// registration is required, such as limiting the number of generic handler registrations or enforcing timeouts
    /// during registration. All methods are static and thread-safe. Typically, these methods are used during
    /// application startup to configure dependency injection for MediatR-based applications.</remarks>
    public static class ServiceRegistrar
    {
        private static int MaxGenericTypeParameters;
        private static int MaxTypesClosing;
        private static int MaxGenericTypeRegistrations;
        private static int RegistrationTimeout;

        /// <summary>
        /// Configures the limitations for generic request handler registrations using the specified MediatR service
        /// configuration.
        /// </summary>
        /// <remarks>This method updates the global limitations for generic request handler registrations.
        /// These settings affect how many generic handler types can be registered and the constraints applied during
        /// registration. Changes take effect immediately and may impact subsequent handler registrations.</remarks>
        /// <param name="configuration">The configuration object that specifies the maximum allowed generic type parameters, types closing, generic
        /// type registrations, and registration timeout for generic request handler registrations. Cannot be null.</param>
        public static void SetGenericRequestHandlerRegistrationLimitations(MediatRServiceConfiguration configuration)
        {
            MaxGenericTypeParameters = configuration.MaxGenericTypeParameters;
            MaxTypesClosing = configuration.MaxTypesClosing;
            MaxGenericTypeRegistrations = configuration.MaxGenericTypeRegistrations;
            RegistrationTimeout = configuration.RegistrationTimeout;
        }

        /// <summary>
        /// Registers MediatR handler and related classes with the specified service collection, enforcing a timeout for
        /// the registration process.
        /// </summary>
        /// <remarks>This method enforces a timeout when registering MediatR classes to help prevent
        /// application startup delays due to long-running or stalled registrations. The timeout duration is determined
        /// by the value of the RegistrationTimeout field.</remarks>
        /// <param name="services">The service collection to which MediatR classes will be added. Cannot be null.</param>
        /// <param name="configuration">The configuration settings to use for MediatR registration. Cannot be null.</param>
        /// <exception cref="TimeoutException">Thrown if the registration process does not complete within the configured timeout period.</exception>
        public static void AddMediatRClassesWithTimeout(IServiceCollection services, MediatRServiceConfiguration configuration)
        {
            using var cts = new CancellationTokenSource(RegistrationTimeout);
            try
            {
                AddMediatRClasses(services, configuration, cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException("The generic handler registration process timed out.");
            }
        }

        /// <summary>
        /// Registers MediatR handler and processor classes found in the specified assemblies with the dependency
        /// injection container.
        /// </summary>
        /// <remarks>This method scans the assemblies provided in the configuration for implementations of
        /// MediatR interfaces such as <see cref="IRequestHandler{TRequest,TResponse}"/>, <see
        /// cref="INotificationHandler{TNotification}"/>, and related processor interfaces, and registers them with the
        /// dependency injection container. If <see cref="MediatRServiceConfiguration.AutoRegisterRequestProcessors"/>
        /// is enabled, request pre- and post-processors are also registered. Only concrete, open generic types that
        /// match the expected interface arity are registered. This method is typically called during application
        /// startup to enable MediatR pipeline behaviors and handlers.</remarks>
        /// <param name="services">The service collection to which MediatR handler and processor implementations will be added.</param>
        /// <param name="configuration">The configuration specifying which assemblies to scan and registration options for MediatR services.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the registration process. The default value is <see
        /// cref="CancellationToken.None"/>.</param>
        public static void AddMediatRClasses(IServiceCollection services, MediatRServiceConfiguration configuration, CancellationToken cancellationToken = default)
        {
            var assembliesToScan = configuration.AssembliesToRegister.Distinct().ToArray();

            ConnectImplementationsToTypesClosing(typeof(IRequestHandler<,>), services, assembliesToScan, false, configuration, cancellationToken);
            ConnectImplementationsToTypesClosing(typeof(IRequestHandler<>), services, assembliesToScan, false, configuration, cancellationToken);
            ConnectImplementationsToTypesClosing(typeof(INotificationHandler<>), services, assembliesToScan, true, configuration);
            ConnectImplementationsToTypesClosing(typeof(IStreamRequestHandler<,>), services, assembliesToScan, false, configuration);
            ConnectImplementationsToTypesClosing(typeof(IRequestExceptionHandler<,,>), services, assembliesToScan, true, configuration);
            ConnectImplementationsToTypesClosing(typeof(IRequestExceptionAction<,>), services, assembliesToScan, true, configuration);

            if (configuration.AutoRegisterRequestProcessors)
            {
                ConnectImplementationsToTypesClosing(typeof(IRequestPreProcessor<>), services, assembliesToScan, true, configuration);
                ConnectImplementationsToTypesClosing(typeof(IRequestPostProcessor<,>), services, assembliesToScan, true, configuration);
            }

            var multiOpenInterfaces = new List<Type>
            {
                typeof(INotificationHandler<>),
                typeof(IRequestExceptionHandler<,,>),
                typeof(IRequestExceptionAction<,>)
            };

            if (configuration.AutoRegisterRequestProcessors)
            {
                multiOpenInterfaces.Add(typeof(IRequestPreProcessor<>));
                multiOpenInterfaces.Add(typeof(IRequestPostProcessor<,>));
            }

            foreach (var multiOpenInterface in multiOpenInterfaces)
            {
                var arity = multiOpenInterface.GetGenericArguments().Length;
                var concretions = assembliesToScan
                    .SelectMany(a => a.DefinedTypes)
                    .Where(type => type.FindInterfacesThatClose(multiOpenInterface).Any())
                    .Where(type => type.IsConcrete() && type.IsOpenGeneric())
                    .Where(type => type.GetGenericArguments().Length == arity)
                    .Where(configuration.TypeEvaluator)
                    .ToList();

                foreach (var type in concretions)
                {
                    services.AddTransient(multiOpenInterface, type);
                }
            }
        }

        private static void ConnectImplementationsToTypesClosing(Type openRequestInterface,
            IServiceCollection services,
            IEnumerable<Assembly> assembliesToScan,
            bool addIfAlreadyExists,
            MediatRServiceConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            var concretions = new List<Type>();
            var interfaces = new List<Type>();
            var genericConcretions = new List<Type>();
            var genericInterfaces = new List<Type>();

            var types = assembliesToScan
                .SelectMany(a => a.DefinedTypes)
                .Where(t => !t.ContainsGenericParameters || configuration.RegisterGenericHandlers)
                .Where(t => t.IsConcrete() && t.FindInterfacesThatClose(openRequestInterface).Any())
                .Where(configuration.TypeEvaluator)
                .ToList();

            foreach (var type in types)
            {
                var interfaceTypes = type.FindInterfacesThatClose(openRequestInterface).ToArray();

                if (!type.IsOpenGeneric())
                {
                    concretions.Add(type);

                    foreach (var interfaceType in interfaceTypes)
                    {
                        interfaces.Fill(interfaceType);
                    }
                }
                else
                {
                    genericConcretions.Add(type);
                    foreach (var interfaceType in interfaceTypes)
                    {
                        genericInterfaces.Fill(interfaceType);
                    }
                }
            }

            foreach (var @interface in interfaces)
            {
                var exactMatches = concretions.Where(x => x.CanBeCastTo(@interface)).ToList();
                if (addIfAlreadyExists)
                {
                    foreach (var type in exactMatches)
                    {
                        services.AddTransient(@interface, type);
                    }
                }
                else
                {
                    if (exactMatches.Count > 1)
                    {
                        exactMatches.RemoveAll(m => !IsMatchingWithInterface(m, @interface));
                    }

                    foreach (var type in exactMatches)
                    {
                        services.TryAddTransient(@interface, type);
                    }
                }

                if (!@interface.IsOpenGeneric())
                {
                    AddConcretionsThatCouldBeClosed(@interface, concretions, services);
                }
            }

            foreach (var @interface in genericInterfaces)
            {
                var exactMatches = genericConcretions.Where(x => x.CanBeCastTo(@interface)).ToList();
                AddAllConcretionsThatClose(@interface, exactMatches, services, assembliesToScan, cancellationToken);
            }
        }

        private static bool IsMatchingWithInterface(Type? handlerType, Type handlerInterface)
        {
            if (handlerType == null || handlerInterface == null)
            {
                return false;
            }

            if (handlerType.IsInterface)
            {
                if (handlerType.GenericTypeArguments.SequenceEqual(handlerInterface.GenericTypeArguments))
                {
                    return true;
                }
            }
            else
            {
                return IsMatchingWithInterface(handlerType.GetInterface(handlerInterface.Name), handlerInterface);
            }

            return false;
        }

        private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services)
        {
            foreach (var type in concretions
                         .Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
            {
                try
                {
                    services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
                }
                catch (Exception) { }
            }
        }

        private static (Type Service, Type Implementation) GetConcreteRegistrationTypes(Type openRequestHandlerInterface, Type concreteGenericTRequest, Type openRequestHandlerImplementation)
        {
            var closingTypes = concreteGenericTRequest.GetGenericArguments();

            var concreteTResponse = concreteGenericTRequest.GetInterfaces()
                .FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IRequest<>))
                ?.GetGenericArguments()
                .FirstOrDefault();

            var typeDefinition = openRequestHandlerInterface.GetGenericTypeDefinition();

            var serviceType = concreteTResponse != null ?
                typeDefinition.MakeGenericType(concreteGenericTRequest, concreteTResponse) :
                typeDefinition.MakeGenericType(concreteGenericTRequest);

            return (serviceType, openRequestHandlerImplementation.MakeGenericType(closingTypes));
        }

        private static List<Type>? GetConcreteRequestTypes(Type openRequestHandlerInterface, Type openRequestHandlerImplementation, IEnumerable<Assembly> assembliesToScan, CancellationToken cancellationToken)
        {
            //request generic type constraints       
            var constraintsForEachParameter = openRequestHandlerImplementation
                .GetGenericArguments()
                .Select(x => x.GetGenericParameterConstraints())
                .ToList();

            var typesThatCanCloseForEachParameter = constraintsForEachParameter
                .Select(constraints => assembliesToScan
                    .SelectMany(assembly => assembly.GetTypes())
                    .Where(type => type.IsClass && !type.IsAbstract && constraints.All(constraint => constraint.IsAssignableFrom(type))).ToList()
                ).ToList();

            var requestType = openRequestHandlerInterface.GenericTypeArguments.First();

            if (requestType.IsGenericParameter)
            {
                return null;
            }

            var requestGenericTypeDefinition = requestType.GetGenericTypeDefinition();
            var combinations = GenerateCombinations(requestType, typesThatCanCloseForEachParameter, 0, cancellationToken);
            return combinations.Select(types => requestGenericTypeDefinition.MakeGenericType(types.ToArray())).ToList();
        }

        /// <summary>
        /// Generates all possible combinations by selecting one type from each list of types, producing a list of type
        /// combinations suitable for generic type construction.
        /// </summary>
        /// <remarks>This method is typically used to generate all possible sets of type arguments for a
        /// generic type, given multiple candidate types for each parameter. The number of combinations grows
        /// multiplicatively with the size of the input lists, so use caution with large inputs. The operation can be
        /// cancelled via the provided cancellation token.</remarks>
        /// <param name="requestType">The generic type definition for which combinations are being generated. Used for validation and exception
        /// messages.</param>
        /// <param name="lists">A list of lists, where each inner list contains possible types for a corresponding generic type parameter.
        /// Each combination will select one type from each inner list.</param>
        /// <param name="depth">The current recursion depth. This parameter is used internally and should typically be left at its default
        /// value.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
        /// <returns>A list of combinations, where each combination is a list of types representing one possible selection of
        /// types from the input lists. The result will be empty if any input list is empty.</returns>
        /// <exception cref="ArgumentException">Thrown if the number of generic type parameters exceeds the allowed maximum, if any inner list exceeds the
        /// allowed maximum length, or if the total number of combinations exceeds the allowed maximum.</exception>
        public static List<List<Type>> GenerateCombinations(Type requestType, List<List<Type>> lists, int depth = 0, CancellationToken cancellationToken = default)
        {
            if (depth == 0)
            {
                // Initial checks
                if (MaxGenericTypeParameters > 0 && lists.Count > MaxGenericTypeParameters)
                {
                    throw new ArgumentException($"Error registering the generic type: {requestType.FullName}. The number of generic type parameters exceeds the maximum allowed ({MaxGenericTypeParameters}).");
                }

                foreach (var list in lists)
                {
                    if (MaxTypesClosing > 0 && list.Count > MaxTypesClosing)
                    {
                        throw new ArgumentException($"Error registering the generic type: {requestType.FullName}. One of the generic type parameter's count of types that can close exceeds the maximum length allowed ({MaxTypesClosing}).");
                    }
                }

                // Calculate the total number of combinations
                long totalCombinations = 1;
                foreach (var list in lists)
                {
                    totalCombinations *= list.Count;
                    if (MaxGenericTypeParameters > 0 && totalCombinations > MaxGenericTypeRegistrations)
                    {
                        throw new ArgumentException($"Error registering the generic type: {requestType.FullName}. The total number of generic type registrations exceeds the maximum allowed ({MaxGenericTypeRegistrations}).");
                    }
                }
            }

            if (depth >= lists.Count)
            {
                return new List<List<Type>> { new List<Type>() };
            }

            cancellationToken.ThrowIfCancellationRequested();
            var currentList = lists[depth];
            var childCombinations = GenerateCombinations(requestType, lists, depth + 1, cancellationToken);
            var combinations = new List<List<Type>>();

            foreach (var item in currentList)
            {
                foreach (var childCombination in childCombinations)
                {
                    var currentCombination = new List<Type> { item };
                    currentCombination.AddRange(childCombination);
                    combinations.Add(currentCombination);
                }
            }

            return combinations;
        }

        private static void AddAllConcretionsThatClose(Type openRequestInterface, List<Type> concretions, IServiceCollection services, IEnumerable<Assembly> assembliesToScan, CancellationToken cancellationToken)
        {
            foreach (var concretion in concretions)
            {
                var concreteRequests = GetConcreteRequestTypes(openRequestInterface, concretion, assembliesToScan, cancellationToken);
                if (concreteRequests is null)
                {
                    continue;
                }

                var registrationTypes = concreteRequests
                    .Select(concreteRequest => GetConcreteRegistrationTypes(openRequestInterface, concreteRequest, concretion));

                foreach (var (Service, Implementation) in registrationTypes)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    services.AddTransient(Service, Implementation);
                }
            }
        }

        internal static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GenericTypeArguments;
            var concreteArguments = openConcretion.GenericTypeArguments;
            return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
        }

        private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
        {
            if (pluggedType == null)
            {
                return false;
            }
            if (pluggedType == pluginType)
            {
                return true;
            }

            return pluginType.IsAssignableFrom(pluggedType);
        }

        private static bool IsOpenGeneric(this Type type)
        {
            return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
        }

        internal static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
        {
            return FindInterfacesThatClosesCore(pluggedType, templateType).Distinct();
        }

        private static IEnumerable<Type> FindInterfacesThatClosesCore(Type pluggedType, Type templateType)
        {
            if (pluggedType == null)
            {
                yield break;
            }

            if (!pluggedType.IsConcrete())
            {
                yield break;
            }

            if (templateType.IsInterface)
            {
                foreach (
                    var interfaceType in
                    pluggedType.GetInterfaces()
                        .Where(type => type.IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
                {
                    yield return interfaceType;
                }
            }
            else if (pluggedType.BaseType!.IsGenericType &&
                     (pluggedType.BaseType!.GetGenericTypeDefinition() == templateType))
            {
                yield return pluggedType.BaseType!;
            }

            if (pluggedType.BaseType == typeof(object))
            {
                yield break;
            }

            foreach (var interfaceType in FindInterfacesThatClosesCore(pluggedType.BaseType!, templateType))
            {
                yield return interfaceType;
            }
        }

        private static bool IsConcrete(this Type type)
        {
            return !type.IsAbstract && !type.IsInterface;
        }

        private static void Fill<T>(this IList<T> list, T value)
        {
            if (list.Contains(value))
            {
                return;
            }
            list.Add(value);
        }

        /// <summary>
        /// Registers the required MediatR services and pipeline behaviors into the specified service collection using
        /// the provided configuration.
        /// </summary>
        /// <remarks>This method uses TryAdd and TryAddEnumerable to avoid overwriting existing service
        /// registrations. It registers core MediatR interfaces, notification publishers, and any configured pipeline
        /// behaviors, pre-processors, and post-processors according to the provided configuration.</remarks>
        /// <param name="services">The service collection to which MediatR services and behaviors will be added. Cannot be null.</param>
        /// <param name="serviceConfiguration">The configuration that specifies MediatR implementation types, lifetimes, and pipeline behaviors to
        /// register. Cannot be null.</param>
        public static void AddRequiredServices(IServiceCollection services, MediatRServiceConfiguration serviceConfiguration)
        {
            // Use TryAdd, so any existing ServiceFactory/IMediator registration doesn't get overridden
            services.TryAdd(new ServiceDescriptor(typeof(IMediator), serviceConfiguration.MediatorImplementationType, serviceConfiguration.Lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(ISender), sp => sp.GetRequiredService<IMediator>(), serviceConfiguration.Lifetime));
            services.TryAdd(new ServiceDescriptor(typeof(IPublisher), sp => sp.GetRequiredService<IMediator>(), serviceConfiguration.Lifetime));

            var notificationPublisherServiceDescriptor = serviceConfiguration.NotificationPublisherType != null
                ? new ServiceDescriptor(typeof(INotificationPublisher), serviceConfiguration.NotificationPublisherType, serviceConfiguration.Lifetime)
                : new ServiceDescriptor(typeof(INotificationPublisher), serviceConfiguration.NotificationPublisher);

            services.TryAdd(notificationPublisherServiceDescriptor);

            // Register pre processors, then post processors, then behaviors
            if (serviceConfiguration.RequestExceptionActionProcessorStrategy == RequestExceptionActionProcessorStrategy.ApplyForUnhandledExceptions)
            {
                RegisterBehaviorIfImplementationsExist(services, typeof(RequestExceptionActionProcessorBehavior<,>), typeof(IRequestExceptionAction<,>));
                RegisterBehaviorIfImplementationsExist(services, typeof(RequestExceptionProcessorBehavior<,>), typeof(IRequestExceptionHandler<,,>));
            }
            else
            {
                RegisterBehaviorIfImplementationsExist(services, typeof(RequestExceptionProcessorBehavior<,>), typeof(IRequestExceptionHandler<,,>));
                RegisterBehaviorIfImplementationsExist(services, typeof(RequestExceptionActionProcessorBehavior<,>), typeof(IRequestExceptionAction<,>));
            }

            if (serviceConfiguration.RequestPreProcessorsToRegister.Any())
            {
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>), ServiceLifetime.Transient));
                services.TryAddEnumerable(serviceConfiguration.RequestPreProcessorsToRegister);
            }

            if (serviceConfiguration.RequestPostProcessorsToRegister.Any())
            {
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>), ServiceLifetime.Transient));
                services.TryAddEnumerable(serviceConfiguration.RequestPostProcessorsToRegister);
            }

            foreach (var serviceDescriptor in serviceConfiguration.BehaviorsToRegister)
            {
                services.TryAddEnumerable(serviceDescriptor);
            }

            foreach (var serviceDescriptor in serviceConfiguration.StreamBehaviorsToRegister)
            {
                services.TryAddEnumerable(serviceDescriptor);
            }
        }

        private static void RegisterBehaviorIfImplementationsExist(IServiceCollection services, Type behaviorType, Type subBehaviorType)
        {
            var hasAnyRegistrationsOfSubBehaviorType = services
                .Where(service => !service.IsKeyedService)
                .Select(service => service.ImplementationType)
                .OfType<Type>()
                .SelectMany(type => type.GetInterfaces())
                .Where(type => type.IsGenericType)
                .Select(type => type.GetGenericTypeDefinition())
                .Any(type => type == subBehaviorType);

            if (hasAnyRegistrationsOfSubBehaviorType)
            {
                services.TryAddEnumerable(new ServiceDescriptor(typeof(IPipelineBehavior<,>), behaviorType, ServiceLifetime.Transient));
            }
        }
    }
}